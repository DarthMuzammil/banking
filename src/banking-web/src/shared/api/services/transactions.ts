/**
 * Transactions service — REAL unified API (Sprint 2)
 */
import { apiRequest } from "@/shared/api/client";
import type { TransactionType } from "@/shared/types";
import type { EnrichedTransaction, TransactionCategory } from "@/shared/types/extended";

interface CustomerTransactionsPage {
  skip: number;
  take: number;
  items: Array<{
    id: string;
    accountId: string;
    accountNumber: string;
    type: TransactionType;
    amount: number;
    balanceAfter: number;
    description: string | null;
    createdAt: string;
  }>;
}

const categoryMap: Record<string, TransactionCategory> = {
  deposit: "Income",
  paycheck: "Income",
  transfer: "Transfer",
  bill: "Bills",
  rent: "Bills",
  shop: "Shopping",
  food: "Food",
  transport: "Transport",
};

function inferCategory(description: string | null): TransactionCategory {
  if (!description) return "Other";
  const lower = description.toLowerCase();
  for (const [key, cat] of Object.entries(categoryMap)) {
    if (lower.includes(key)) return cat;
  }
  return "Other";
}

function toEnriched(item: CustomerTransactionsPage["items"][number]): EnrichedTransaction {
  return {
    id: item.id,
    type: item.type,
    amount: item.amount,
    balanceAfter: item.balanceAfter,
    description: item.description,
    createdAt: item.createdAt,
    accountId: item.accountId,
    accountNumber: item.accountNumber,
    category: inferCategory(item.description),
  };
}

export async function getAllTransactions(
  token: string,
  options?: { search?: string; category?: string; accountId?: string },
): Promise<EnrichedTransaction[]> {
  const params = new URLSearchParams();
  if (options?.accountId) params.set("accountId", options.accountId);
  if (options?.search) params.set("search", options.search);
  params.set("skip", "0");
  params.set("take", "100");

  const page = await apiRequest<CustomerTransactionsPage>(
    `/api/transactions?${params.toString()}`,
    { token },
  );

  let enriched = page.items.map(toEnriched);

  if (options?.category && options.category !== "All") {
    enriched = enriched.filter((t) => t.category === options.category);
  }

  return enriched;
}

export async function exportTransactionsCsv(token: string): Promise<string> {
  const rows = await getAllTransactions(token);
  const header = "Date,Description,Account,Category,Type,Amount,Balance After";
  const lines = rows.map((r) =>
    [
      new Date(r.createdAt).toISOString(),
      `"${(r.description ?? "").replace(/"/g, '""')}"`,
      r.accountNumber,
      r.category,
      r.type === 1 ? "Credit" : "Debit",
      r.amount,
      r.balanceAfter,
    ].join(","),
  );
  return [header, ...lines].join("\n");
}
