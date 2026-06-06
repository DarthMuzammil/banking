"use client";

import { FormEvent, useState } from "react";
import { bankingApi } from "@/shared/api";
import { ApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { Select } from "@/shared/components/ui/Select";
import { SkeletonCard } from "@/shared/components/ui/Skeleton";
import { Badge } from "@/shared/components/ui/Badge";
import { formatCurrency } from "@/shared/types";

export default function PaymentsPage() {
  const { token } = useAuth();
  const { toast } = useToast();
  const [fromId, setFromId] = useState("");
  const [toId, setToId] = useState("");
  const [amount, setAmount] = useState("");
  const [note, setNote] = useState("");
  const [transferring, setTransferring] = useState(false);
  const [payingBillId, setPayingBillId] = useState<string | null>(null);

  const accounts = useAsync(() => bankingApi.accounts.getAccounts(token!), [token]);
  const bills = useAsync(() => bankingApi.payments.getBillPayments(token!), [token]);
  const scheduled = useAsync(() => bankingApi.payments.getScheduledPayments(token!), [token]);

  const accountOptions = (accounts.data ?? []).map((a) => ({
    value: a.id,
    label: `${a.accountNumber} (${formatCurrency(a.balance)})`,
  }));

  async function handleTransfer(e: FormEvent) {
    e.preventDefault();
    if (!token || !fromId || !toId) return;
    const parsed = Number.parseFloat(amount);
    if (Number.isNaN(parsed) || parsed <= 0) {
      toast("Enter a valid amount", "error");
      return;
    }
    setTransferring(true);
    try {
      const result = await bankingApi.payments.submitTransfer(token, {
        fromAccountId: fromId,
        toAccountId: toId,
        amount: parsed,
        note,
      });
      toast(`Transfer ${result.reference} completed`, "success");
      setAmount("");
      setNote("");
      accounts.reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Transfer failed", "error");
    } finally {
      setTransferring(false);
    }
  }

  async function handlePayBill(billId: string, amount: number) {
    if (!token) return;
    const defaultAccount = accounts.data?.[0];
    if (!defaultAccount) {
      toast("No account available to pay from", "error");
      return;
    }
    setPayingBillId(billId);
    try {
      const result = await bankingApi.payments.payBill(token, billId, defaultAccount.id);
      toast(`Paid ${formatCurrency(amount)} — ref ${result.reference}`, "success");
      bills.reload();
      accounts.reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Payment failed", "error");
    } finally {
      setPayingBillId(null);
    }
  }

  return (
    <div className="space-y-10">
      <PageHeader
        title="Payments"
        description="Transfer between accounts, pay bills, and manage scheduled payments."
      />

      <Card>
        <h2 className="text-lg font-medium text-foreground">Transfer money</h2>
        <p className="mt-1 text-sm text-muted">Move funds between your accounts instantly.</p>
        <form onSubmit={handleTransfer} className="mt-6 grid gap-4 sm:grid-cols-2">
          <Select label="From" value={fromId} onChange={(e) => setFromId(e.target.value)} options={[{ value: "", label: "Select account" }, ...accountOptions]} />
          <Select label="To" value={toId} onChange={(e) => setToId(e.target.value)} options={[{ value: "", label: "Select account" }, ...accountOptions]} />
          <Input label="Amount" type="number" min="0.01" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} />
          <Input label="Note" value={note} onChange={(e) => setNote(e.target.value)} placeholder="Optional" />
          <div className="sm:col-span-2">
            <Button type="submit" isLoading={transferring}>Send transfer</Button>
          </div>
        </form>
      </Card>

      <section className="space-y-4">
        <h2 className="text-lg font-medium text-foreground">Bill payments</h2>
        {bills.isLoading ? <SkeletonCard /> : (
          <Card padding="sm" className="divide-y divide-border overflow-hidden p-0">
            {bills.data!.map((bill) => (
              <div key={bill.id} className="flex items-center justify-between px-5 py-4">
                <div>
                  <p className="text-sm font-medium">{bill.payee}</p>
                  <p className="mt-0.5 text-xs text-subtle">Due {new Date(bill.dueDate).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-4">
                  <p className="text-sm font-medium tabular-nums">{formatCurrency(bill.amount)}</p>
                  {bill.status === "Due" && (
                    <Button
                      size="sm"
                      variant="secondary"
                      isLoading={payingBillId === bill.id}
                      onClick={() => handlePayBill(bill.id, bill.amount)}
                    >
                      Pay
                    </Button>
                  )}
                  <Badge variant={bill.status === "Paid" ? "success" : bill.status === "Due" ? "warning" : "accent"}>
                    {bill.status}
                  </Badge>
                </div>
              </div>
            ))}
          </Card>
        )}
      </section>

      <section className="space-y-4">
        <h2 className="text-lg font-medium text-foreground">Scheduled payments</h2>
        {scheduled.isLoading ? <SkeletonCard /> : (
          <Card padding="sm" className="divide-y divide-border overflow-hidden p-0">
            {scheduled.data!.map((item) => (
              <div key={item.id} className="flex items-center justify-between px-5 py-4">
                <div>
                  <p className="text-sm font-medium">{item.payee}</p>
                  <p className="mt-0.5 text-xs text-subtle">{item.frequency} · Next {new Date(item.nextDate).toLocaleDateString()}</p>
                </div>
                <p className="text-sm font-medium tabular-nums">{formatCurrency(item.amount)}</p>
              </div>
            ))}
          </Card>
        )}
      </section>
    </div>
  );
}
