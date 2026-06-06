"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { bankingApi } from "@/shared/api";
import { ApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Select } from "@/shared/components/ui/Select";
import { SkeletonCard } from "@/shared/components/ui/Skeleton";
import { EmptyState } from "@/shared/components/ui/EmptyState";
import { Badge } from "@/shared/components/ui/Badge";
import { accountTypeLabel, formatCurrency } from "@/shared/types";

export default function AccountsPage() {
  const { token } = useAuth();
  const { toast } = useToast();
  const [accountType, setAccountType] = useState("Checking");
  const [creating, setCreating] = useState(false);

  const { data, error, isLoading, isEmpty, reload } = useAsync(
    () => bankingApi.accounts.getAccounts(token!),
    [token],
    (d) => d.length === 0,
  );

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    setCreating(true);
    try {
      const created = await bankingApi.accounts.createAccount(
        token,
        accountType as "Checking" | "Savings",
      );
      toast(`Account ${created.accountNumber} created`, "success");
      reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Could not create account", "error");
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className="space-y-10">
      <PageHeader
        title="Accounts"
        description="Savings, checking, and deposit accounts in one place."
      />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">{[1, 2].map((i) => <SkeletonCard key={i} />)}</div>
      ) : error ? (
        <Card><p className="text-sm text-error">{error}</p></Card>
      ) : isEmpty ? (
        <EmptyState title="No accounts" description="Create a checking or savings account to get started." />
      ) : (
        <div className="space-y-1">
          {(data ?? []).map((account) => (
            <Link key={account.id} href={`/accounts/${account.id}`}>
              <div className="group flex items-center justify-between rounded-[6px] border border-transparent px-3 py-4 transition-all duration-200 hover:border-border hover:bg-surface-tertiary">
                <div className="flex items-center gap-3">
                  <div className="flex h-9 w-9 items-center justify-center rounded-[6px] bg-surface-elevated text-[11px] font-medium text-muted">
                    {account.accountType === 1 ? "CH" : "SV"}
                  </div>
                  <div>
                    <p className="text-sm font-medium text-foreground">{accountTypeLabel(account.accountType)}</p>
                    <p className="font-mono text-[11px] text-subtle">{account.accountNumber}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="text-right">
                    <p className="text-sm font-semibold tabular-nums">{formatCurrency(account.balance, account.currency)}</p>
                    <p className="text-[11px] text-subtle">Opened {new Date(account.createdAt).toLocaleDateString()}</p>
                  </div>
                  <Badge>Active</Badge>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}

      <Card>
        <h2 className="text-sm font-medium text-foreground">Open new account</h2>
        <p className="mt-1 text-sm text-muted">Balance starts at zero.</p>
        <form onSubmit={handleCreate} className="mt-6 flex flex-col gap-4 sm:flex-row sm:items-end">
          <div className="flex-1">
            <Select
              label="Account type"
              value={accountType}
              onChange={(e) => setAccountType(e.target.value)}
              options={[
                { value: "Checking", label: "Checking — everyday spending" },
                { value: "Savings", label: "Savings — grow your balance" },
              ]}
            />
          </div>
          <Button type="submit" isLoading={creating}>Create account</Button>
        </form>
      </Card>
    </div>
  );
}
