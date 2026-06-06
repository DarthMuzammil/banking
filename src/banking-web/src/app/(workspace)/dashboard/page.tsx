"use client";

import Link from "next/link";
import { bankingApi } from "@/shared/api";
import { useAuth } from "@/shared/context/AuthContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Skeleton, SkeletonCard } from "@/shared/components/ui/Skeleton";
import { EmptyState } from "@/shared/components/ui/EmptyState";
import { Badge } from "@/shared/components/ui/Badge";
import { Button } from "@/shared/components/ui/Button";
import {
  accountTypeLabel,
  formatCurrency,
  transactionTypeLabel,
} from "@/shared/types";
import type { Account } from "@/shared/types";

export default function DashboardPage() {
  const { token } = useAuth();

  const accounts = useAsync(
    () => bankingApi.accounts.getAccounts(token!),
    [token],
    (data) => data.length === 0,
  );

  const insights = useAsync(
    () => bankingApi.insights.getSpendingInsights(token!),
    [token],
  );

  const recentTx = useAsync(
    () => bankingApi.transactions.getAllTransactions(token!, {}),
    [token],
    (data) => data.length === 0,
  );

  const accountList = accounts.data ?? [];
  const totalBalance = accounts.data?.reduce((sum, a) => sum + a.balance, 0) ?? 0;

  return (
    <div className="space-y-12">
      <PageHeader
        title="Overview"
        description="Your financial position at a glance."
      />

      {accounts.isLoading ? (
        <Skeleton className="h-20 w-56" />
      ) : accounts.error ? (
        <Card><p className="text-sm text-error">{accounts.error}</p></Card>
      ) : (
        <section aria-label="Total balance" className="-mt-2">
          <p className="text-xs font-medium uppercase tracking-wider text-subtle">Total balance</p>
          <p className="mt-2 text-[44px] font-semibold leading-none tracking-tight tabular-nums text-foreground">
            {formatCurrency(totalBalance)}
          </p>
          <p className="mt-3 text-sm text-muted">
            Across {accountList.length} account{accountList.length === 1 ? "" : "s"}
          </p>
        </section>
      )}

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-medium text-muted">Accounts</h2>
          <Link href="/accounts">
            <Button variant="ghost" size="sm">View all</Button>
          </Link>
        </div>
        {accounts.isLoading ? (
          <div className="space-y-2">{[1, 2].map((i) => <SkeletonCard key={i} />)}</div>
        ) : accounts.error ? (
          <Card><p className="text-sm text-error">{accounts.error}</p></Card>
        ) : accounts.isEmpty ? (
          <EmptyState
            title="No accounts yet"
            description="Open your first account to start managing money."
            action={<Link href="/accounts"><Button>Open account</Button></Link>}
          />
        ) : (
          <div className="space-y-2">
            {accountList.map((account) => (
              <AccountRow key={account.id} account={account} />
            ))}
          </div>
        )}
      </section>

      <section className="space-y-4">
        <h2 className="text-sm font-medium text-muted">Spending insights</h2>
        {insights.isLoading ? (
          <div className="grid gap-2 sm:grid-cols-3">{[1, 2, 3].map((i) => <SkeletonCard key={i} />)}</div>
        ) : insights.error ? (
          <Card><p className="text-sm text-error">{insights.error}</p></Card>
        ) : insights.isSuccess ? (
          <div className="grid gap-2 sm:grid-cols-3">
            {insights.data!.map((item) => (
              <Card key={item.id} padding="md">
                <p className="text-xs text-subtle">{item.label}</p>
                <p className="mt-2 text-xl font-semibold tabular-nums text-foreground">{formatCurrency(item.amount)}</p>
                <p className="mt-1.5 text-[11px] text-subtle">
                  {item.direction === "flat"
                    ? "No change"
                    : `${item.changePercent > 0 ? "+" : ""}${item.changePercent}% vs last month`}
                </p>
              </Card>
            ))}
          </div>
        ) : null}
      </section>

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-medium text-muted">Recent activity</h2>
          <Link href="/transactions"><Button variant="ghost" size="sm">View all</Button></Link>
        </div>
        {recentTx.isLoading ? (
          <Card padding="md"><Skeleton className="h-28 w-full" /></Card>
        ) : recentTx.error ? (
          <Card><p className="text-sm text-error">{recentTx.error}</p></Card>
        ) : recentTx.isEmpty ? (
          <EmptyState title="No activity" description="Transactions will appear here once you deposit or transfer funds." />
        ) : (
          <Card padding="none" className="overflow-hidden">
            {(recentTx.data ?? []).slice(0, 6).map((tx, i) => (
              <div
                key={tx.id}
                className={`flex items-center justify-between px-4 py-3 transition-colors duration-150 hover:bg-surface-hover ${i > 0 ? "border-t border-border" : ""}`}
              >
                <div className="min-w-0">
                  <p className="truncate text-sm text-foreground">{tx.description ?? transactionTypeLabel(tx.type)}</p>
                  <p className="mt-0.5 text-[11px] text-subtle">
                    {tx.accountNumber} · {new Date(tx.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <p className={`ml-4 shrink-0 text-sm font-medium tabular-nums ${tx.type === 1 ? "text-success" : "text-foreground"}`}>
                  {tx.type === 1 ? "+" : "−"}{formatCurrency(tx.amount)}
                </p>
              </div>
            ))}
          </Card>
        )}
      </section>
    </div>
  );
}

function AccountRow({ account }: { account: Account }) {
  return (
    <Link href={`/accounts/${account.id}`}>
      <div className="group flex items-center justify-between rounded-[6px] border border-transparent px-3 py-3 transition-all duration-200 hover:border-border hover:bg-surface-tertiary">
        <div className="flex items-center gap-3 min-w-0">
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-[6px] bg-surface-elevated text-[11px] font-medium text-muted">
            {account.accountType === 1 ? "CH" : "SV"}
          </div>
          <div className="min-w-0">
            <p className="truncate text-sm font-medium text-foreground">{accountTypeLabel(account.accountType)}</p>
            <p className="truncate font-mono text-[11px] text-subtle">{account.accountNumber}</p>
          </div>
        </div>
        <div className="flex items-center gap-3 shrink-0">
          <p className="text-sm font-medium tabular-nums text-foreground">
            {formatCurrency(account.balance, account.currency)}
          </p>
          <Badge variant="neutral">Active</Badge>
        </div>
      </div>
    </Link>
  );
}
