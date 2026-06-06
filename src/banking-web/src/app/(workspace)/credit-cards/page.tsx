"use client";

import { bankingApi } from "@/shared/api";
import { useAuth } from "@/shared/context/AuthContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { SkeletonCard } from "@/shared/components/ui/Skeleton";
import { formatCurrency } from "@/shared/types";

export default function CreditCardsPage() {
  const { token } = useAuth();

  const cards = useAsync(() => bankingApi.creditCards.getCreditCards(token!), [token]);

  const firstCardId = cards.data?.[0]?.id;
  const spending = useAsync(
    () =>
      firstCardId
        ? bankingApi.creditCards.getCardSpending(token!, firstCardId)
        : Promise.resolve([]),
    [token, firstCardId],
    (d) => d.length === 0,
  );

  return (
    <div className="space-y-10">
      <PageHeader
        title="Credit cards"
        description="Balances, due dates, and spending breakdown."
      />

      {cards.isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">{[1, 2].map((i) => <SkeletonCard key={i} />)}</div>
      ) : cards.error ? (
        <Card><p className="text-sm text-error">{cards.error}</p></Card>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {cards.data!.map((card) => {
            const utilization = (card.balance / card.limit) * 100;
            return (
              <Card key={card.id}>
                <p className="text-sm text-muted">{card.name}</p>
                <p className="mt-1 font-mono text-xs text-subtle">•••• {card.lastFour}</p>
                <p className="mt-6 text-[32px] font-semibold tracking-tight tabular-nums">
                  {formatCurrency(card.balance)}
                </p>
                <p className="mt-1 text-sm text-subtle">of {formatCurrency(card.limit)} limit</p>
                <div className="mt-4 h-1 overflow-hidden rounded-full bg-surface">
                  <div
                    className="h-full rounded-full bg-accent transition-all duration-250"
                    style={{ width: `${Math.min(utilization, 100)}%` }}
                  />
                </div>
                <div className="mt-6 grid grid-cols-2 gap-4 border-t border-border pt-4 text-sm">
                  <div>
                    <p className="text-subtle">Due date</p>
                    <p className="mt-0.5 font-medium">{new Date(card.dueDate).toLocaleDateString()}</p>
                  </div>
                  <div>
                    <p className="text-subtle">Min payment</p>
                    <p className="mt-0.5 font-medium tabular-nums">{formatCurrency(card.minPayment)}</p>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      )}

      <section className="space-y-4">
        <h2 className="text-lg font-medium text-foreground">Spending breakdown</h2>
        {spending.isLoading ? <SkeletonCard /> : spending.error ? (
          <Card><p className="text-sm text-error">{spending.error}</p></Card>
        ) : spending.data && spending.data.length > 0 ? (
          <Card padding="sm" className="space-y-4 p-5">
            {spending.data.map((item) => (
              <div key={item.category}>
                <div className="flex justify-between text-sm">
                  <span className="text-muted">{item.category}</span>
                  <span className="font-medium tabular-nums">{formatCurrency(item.amount)}</span>
                </div>
                <div className="mt-2 h-1 overflow-hidden rounded-full bg-surface">
                  <div
                    className="h-full rounded-full bg-accent/80"
                    style={{ width: `${item.percentage}%` }}
                  />
                </div>
              </div>
            ))}
          </Card>
        ) : (
          <Card><p className="text-sm text-muted">No spending data for this card.</p></Card>
        )}
      </section>
    </div>
  );
}
