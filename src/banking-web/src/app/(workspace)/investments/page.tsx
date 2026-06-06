"use client";

import { bankingApi } from "@/shared/api";
import { useAuth } from "@/shared/context/AuthContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Skeleton, SkeletonCard } from "@/shared/components/ui/Skeleton";
import { formatCurrency } from "@/shared/types";

export default function InvestmentsPage() {
  const { token } = useAuth();

  const portfolio = useAsync(() => bankingApi.investments.getPortfolio(token!), [token]);

  return (
    <div className="space-y-10">
      <PageHeader
        title="Investments"
        description="Portfolio overview and performance."
      />

      {portfolio.isLoading ? (
        <Skeleton className="h-20 w-72" />
      ) : portfolio.error ? (
        <Card><p className="text-sm text-error">{portfolio.error}</p></Card>
      ) : (
        <>
          <section>
            <p className="text-sm text-muted">Portfolio value</p>
            <p className="mt-1 text-[40px] font-semibold tracking-tight tabular-nums">
              {formatCurrency(portfolio.data!.totalValue)}
            </p>
            <p className={`mt-2 text-sm font-medium tabular-nums ${portfolio.data!.dayChange >= 0 ? "text-success" : "text-error"}`}>
              {portfolio.data!.dayChange >= 0 ? "+" : ""}{formatCurrency(portfolio.data!.dayChange)} ({portfolio.data!.dayChangePercent}%) today
            </p>
          </section>

          <section className="space-y-4">
            <h2 className="text-lg font-medium text-foreground">Holdings</h2>
            <Card padding="sm" className="overflow-hidden p-0">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-border bg-surface-secondary">
                    <th className="px-5 py-3 font-medium text-muted">Symbol</th>
                    <th className="px-5 py-3 font-medium text-muted">Name</th>
                    <th className="px-5 py-3 font-medium text-muted text-right">Shares</th>
                    <th className="px-5 py-3 font-medium text-muted text-right">Value</th>
                    <th className="px-5 py-3 font-medium text-muted text-right">Change</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {portfolio.data!.holdings.map((h) => (
                    <tr key={h.id} className="transition-colors duration-150 hover:bg-surface-hover">
                      <td className="px-5 py-3.5 font-mono font-medium">{h.symbol}</td>
                      <td className="px-5 py-3.5 text-muted">{h.name}</td>
                      <td className="px-5 py-3.5 text-right tabular-nums">{h.shares}</td>
                      <td className="px-5 py-3.5 text-right font-medium tabular-nums">{formatCurrency(h.value)}</td>
                      <td className={`px-5 py-3.5 text-right tabular-nums ${h.changePercent >= 0 ? "text-success" : "text-error"}`}>
                        {h.changePercent >= 0 ? "+" : ""}{h.changePercent}%
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>
          </section>
        </>
      )}
    </div>
  );
}
