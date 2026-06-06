"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { searchCustomers, getCustomerAccounts, type AdminCustomer, type AdminAccount } from "@/shared/api/admin";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { formatCurrency } from "@/shared/types";
import { ApiError } from "@/shared/api/client";

export default function AdminPage() {
  const { token, isStaff, isLoading } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [query, setQuery] = useState("");
  const [customers, setCustomers] = useState<AdminCustomer[]>([]);
  const [selected, setSelected] = useState<AdminCustomer | null>(null);
  const [accounts, setAccounts] = useState<AdminAccount[]>([]);
  const [searching, setSearching] = useState(false);
  const [loadingAccounts, setLoadingAccounts] = useState(false);

  useEffect(() => {
    if (!isLoading && !isStaff) {
      router.replace("/dashboard");
    }
  }, [isLoading, isStaff, router]);

  if (!isLoading && !isStaff) {
    return null;
  }

  async function handleSearch(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    setSearching(true);
    setSelected(null);
    setAccounts([]);
    try {
      const results = await searchCustomers(token, query);
      setCustomers(results);
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Search failed", "error");
    } finally {
      setSearching(false);
    }
  }

  async function handleSelectCustomer(customer: AdminCustomer) {
    if (!token) return;
    setSelected(customer);
    setLoadingAccounts(true);
    try {
      const results = await getCustomerAccounts(token, customer.id);
      setAccounts(results);
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Could not load accounts", "error");
      setAccounts([]);
    } finally {
      setLoadingAccounts(false);
    }
  }

  return (
    <div className="space-y-8">
      <PageHeader
        title="Admin"
        description="Staff customer lookup — search by email or name."
      />

      <Card>
        <form onSubmit={handleSearch} className="flex flex-wrap items-end gap-3">
          <div className="min-w-[240px] flex-1">
            <Input
              label="Search"
              placeholder="Email or name"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </div>
          <Button type="submit" isLoading={searching}>
            Search
          </Button>
        </form>
      </Card>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <h2 className="text-sm font-medium text-foreground">Customers</h2>
          <ul className="mt-4 divide-y divide-border">
            {customers.length === 0 ? (
              <li className="py-4 text-sm text-muted">No results yet.</li>
            ) : (
              customers.map((customer) => (
                <li key={customer.id}>
                  <button
                    type="button"
                    onClick={() => handleSelectCustomer(customer)}
                    className="flex w-full items-start justify-between gap-3 py-3 text-left hover:bg-surface-hover rounded-[6px] px-2 -mx-2"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">
                        {customer.firstName} {customer.lastName}
                      </p>
                      <p className="text-xs text-subtle">{customer.email}</p>
                    </div>
                    <span className="text-xs text-muted">{customer.role}</span>
                  </button>
                </li>
              ))
            )}
          </ul>
        </Card>

        <Card>
          <h2 className="text-sm font-medium text-foreground">
            {selected ? `${selected.firstName} ${selected.lastName}'s accounts` : "Accounts"}
          </h2>
          {loadingAccounts ? (
            <p className="mt-4 text-sm text-muted">Loading…</p>
          ) : (
            <ul className="mt-4 divide-y divide-border">
              {!selected ? (
                <li className="py-4 text-sm text-muted">Select a customer to view accounts.</li>
              ) : accounts.length === 0 ? (
                <li className="py-4 text-sm text-muted">No accounts found.</li>
              ) : (
                accounts.map((account) => (
                  <li key={account.id} className="py-3">
                    <div className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-sm font-medium text-foreground">
                          {account.accountType} · {account.accountNumber}
                        </p>
                        <p className="text-xs text-subtle">{account.status}</p>
                      </div>
                      <p className="text-sm font-medium text-foreground">
                        {formatCurrency(account.balance, account.currency)}
                      </p>
                    </div>
                  </li>
                ))
              )}
            </ul>
          )}
        </Card>
      </div>
    </div>
  );
}
