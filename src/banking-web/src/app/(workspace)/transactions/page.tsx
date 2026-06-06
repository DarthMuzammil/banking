"use client";

import { useMemo, useState } from "react";
import { bankingApi } from "@/shared/api";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Input } from "@/shared/components/ui/Input";
import { Select } from "@/shared/components/ui/Select";
import { Button } from "@/shared/components/ui/Button";
import { Skeleton } from "@/shared/components/ui/Skeleton";
import { EmptyState } from "@/shared/components/ui/EmptyState";
import { Badge } from "@/shared/components/ui/Badge";
import { formatCurrency, transactionTypeLabel } from "@/shared/types";
import type { TransactionCategory } from "@/shared/types/extended";

const categories = ["All", "Income", "Transfer", "Bills", "Shopping", "Food", "Transport", "Other"];

export default function TransactionsPage() {
  const { token } = useAuth();
  const { toast } = useToast();
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("All");
  const [accountFilter, setAccountFilter] = useState("");

  const accounts = useAsync(() => bankingApi.accounts.getAccounts(token!), [token]);

  const transactions = useAsync(
    () =>
      bankingApi.transactions.getAllTransactions(token!, {
        search: search || undefined,
        category: category !== "All" ? category : undefined,
        accountId: accountFilter || undefined,
      }),
    [token, search, category, accountFilter],
    (d) => d.length === 0,
  );

  const accountOptions = useMemo(
    () => [
      { value: "", label: "All accounts" },
      ...(accounts.data?.map((a) => ({ value: a.id, label: a.accountNumber })) ?? []),
    ],
    [accounts.data],
  );

  async function handleExport() {
    if (!token) return;
    try {
      const csv = await bankingApi.transactions.exportTransactionsCsv(token);
      const blob = new Blob([csv], { type: "text/csv" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `transactions-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
      toast("Export downloaded", "success");
    } catch {
      toast("Export failed", "error");
    }
  }

  return (
    <div className="space-y-8">
      <PageHeader
        title="Transactions"
        description="Search, filter, and export your activity across all accounts."
        action={<Button variant="secondary" size="sm" onClick={handleExport}>Export CSV</Button>}
      />

      <Card>
        <div className="grid gap-4 sm:grid-cols-3">
          <Input
            label="Search"
            placeholder="Description or account…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <Select
            label="Category"
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            options={categories.map((c) => ({ value: c, label: c }))}
          />
          <Select
            label="Account"
            value={accountFilter}
            onChange={(e) => setAccountFilter(e.target.value)}
            options={accountOptions}
          />
        </div>
      </Card>

      {transactions.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : transactions.error ? (
        <Card><p className="text-sm text-error">{transactions.error}</p></Card>
      ) : transactions.isEmpty ? (
        <EmptyState title="No transactions found" description="Try adjusting your filters or make a deposit to see activity." />
      ) : (
        <Card padding="none" className="overflow-hidden">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-border bg-surface-secondary">
                <th className="px-5 py-3 font-medium text-muted">Date</th>
                <th className="px-5 py-3 font-medium text-muted">Description</th>
                <th className="px-5 py-3 font-medium text-muted">Account</th>
                <th className="px-5 py-3 font-medium text-muted">Category</th>
                <th className="px-5 py-3 font-medium text-muted text-right">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {(transactions.data ?? []).map((tx) => (
                <tr key={tx.id} className="transition-colors duration-150 hover:bg-surface-hover">
                  <td className="px-5 py-3.5 text-subtle whitespace-nowrap">{new Date(tx.createdAt).toLocaleDateString()}</td>
                  <td className="px-5 py-3.5 font-medium">{tx.description ?? transactionTypeLabel(tx.type)}</td>
                  <td className="px-5 py-3.5 font-mono text-xs text-subtle">{tx.accountNumber}</td>
                  <td className="px-5 py-3.5"><Badge variant="neutral">{tx.category as TransactionCategory}</Badge></td>
                  <td className={`px-5 py-3.5 text-right font-medium tabular-nums ${tx.type === 1 ? "text-success" : ""}`}>
                    {tx.type === 1 ? "+" : "−"}{formatCurrency(tx.amount)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  );
}
