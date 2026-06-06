"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { bankingApi } from "@/shared/api";
import { ApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { Skeleton } from "@/shared/components/ui/Skeleton";
import { EmptyState } from "@/shared/components/ui/EmptyState";
import {
  accountTypeLabel,
  formatCurrency,
  transactionTypeLabel,
} from "@/shared/types";

export default function AccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { token } = useAuth();
  const { toast } = useToast();

  const [depositAmount, setDepositAmount] = useState("");
  const [depositDescription, setDepositDescription] = useState("");
  const [depositing, setDepositing] = useState(false);

  const [withdrawAmount, setWithdrawAmount] = useState("");
  const [withdrawDescription, setWithdrawDescription] = useState("");
  const [withdrawing, setWithdrawing] = useState(false);

  const account = useAsync(
    () => bankingApi.accounts.getAccount(token!, id),
    [token, id],
  );

  const transactions = useAsync(
    () => bankingApi.accounts.getTransactions(token!, id),
    [token, id],
    (d) => d.items.length === 0,
  );

  async function handleDeposit(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    const parsed = Number.parseFloat(depositAmount);
    if (Number.isNaN(parsed) || parsed <= 0) {
      toast("Enter a valid amount", "error");
      return;
    }
    setDepositing(true);
    try {
      const result = await bankingApi.accounts.deposit(
        token,
        id,
        parsed,
        depositDescription || undefined,
      );
      toast(`Deposited ${formatCurrency(result.amount)}`, "success");
      setDepositAmount("");
      setDepositDescription("");
      account.reload();
      transactions.reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Deposit failed", "error");
    } finally {
      setDepositing(false);
    }
  }

  async function handleWithdraw(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    const parsed = Number.parseFloat(withdrawAmount);
    if (Number.isNaN(parsed) || parsed <= 0) {
      toast("Enter a valid amount", "error");
      return;
    }
    setWithdrawing(true);
    try {
      const result = await bankingApi.accounts.withdraw(
        token,
        id,
        parsed,
        withdrawDescription || undefined,
      );
      toast(`Withdrew ${formatCurrency(result.amount)}`, "success");
      setWithdrawAmount("");
      setWithdrawDescription("");
      account.reload();
      transactions.reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Withdrawal failed", "error");
    } finally {
      setWithdrawing(false);
    }
  }

  if (account.isLoading) {
    return <Skeleton className="h-48 w-full" />;
  }

  if (account.error || !account.data) {
    return (
      <EmptyState
        title="Account not found"
        description={account.error ?? "This account may not exist or you may not have access."}
        action={
          <Link href="/accounts">
            <Button variant="secondary">Back to accounts</Button>
          </Link>
        }
      />
    );
  }

  const acc = account.data;

  return (
    <div className="space-y-10">
      <Link
        href="/accounts"
        className="inline-flex text-sm text-subtle hover:text-accent transition-colors duration-200"
      >
        ← Accounts
      </Link>

      <PageHeader
        title={accountTypeLabel(acc.accountType)}
        description={acc.accountNumber}
      />

      <section className="-mt-2">
        <p className="text-xs font-medium uppercase tracking-wider text-subtle">
          Available balance
        </p>
        <p className="mt-2 text-[44px] font-semibold leading-none tracking-tight tabular-nums text-foreground">
          {formatCurrency(acc.balance, acc.currency)}
        </p>
      </section>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="space-y-6">
          <Card>
            <h2 className="text-sm font-medium text-foreground">Deposit</h2>
            <form onSubmit={handleDeposit} className="mt-5 space-y-4">
              <Input
                label="Amount"
                type="number"
                min="0.01"
                step="0.01"
                required
                value={depositAmount}
                onChange={(e) => setDepositAmount(e.target.value)}
              />
              <Input
                label="Note"
                value={depositDescription}
                onChange={(e) => setDepositDescription(e.target.value)}
                placeholder="Optional"
              />
              <Button type="submit" isLoading={depositing}>
                Deposit funds
              </Button>
            </form>
          </Card>

          <Card>
            <h2 className="text-sm font-medium text-foreground">Withdraw</h2>
            <form onSubmit={handleWithdraw} className="mt-5 space-y-4">
              <Input
                label="Amount"
                type="number"
                min="0.01"
                step="0.01"
                max={acc.balance}
                required
                value={withdrawAmount}
                onChange={(e) => setWithdrawAmount(e.target.value)}
              />
              <Input
                label="Note"
                value={withdrawDescription}
                onChange={(e) => setWithdrawDescription(e.target.value)}
                placeholder="Optional"
              />
              <Button type="submit" variant="secondary" isLoading={withdrawing}>
                Withdraw funds
              </Button>
            </form>
          </Card>
        </div>

        <div className="space-y-4">
          <h2 className="text-sm font-medium text-muted">Activity</h2>
          {transactions.isLoading ? (
            <Skeleton className="h-40 w-full" />
          ) : transactions.isEmpty ? (
            <EmptyState
              title="No transactions"
              description="Deposits and transfers will show here."
            />
          ) : transactions.error ? (
            <Card>
              <p className="text-sm text-error">{transactions.error}</p>
            </Card>
          ) : (
            <Card padding="none" className="overflow-hidden">
              {transactions.data!.items.map((tx, i) => (
                <div
                  key={tx.id}
                  className={`flex items-center justify-between px-4 py-3 transition-colors hover:bg-surface-hover ${i > 0 ? "border-t border-border" : ""}`}
                >
                  <div>
                    <p className="text-sm font-medium">
                      {tx.description ?? transactionTypeLabel(tx.type)}
                    </p>
                    <p className="mt-0.5 text-xs text-subtle">
                      {new Date(tx.createdAt).toLocaleString()}
                    </p>
                  </div>
                  <div className="text-right">
                    <p
                      className={`text-sm font-medium tabular-nums ${tx.type === 1 ? "text-success" : ""}`}
                    >
                      {tx.type === 1 ? "+" : "−"}
                      {formatCurrency(tx.amount)}
                    </p>
                    <p className="text-xs text-subtle">
                      {formatCurrency(tx.balanceAfter)}
                    </p>
                  </div>
                </div>
              ))}
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
