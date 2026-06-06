/**
 * Insights service — REAL API
 */
import { apiRequest } from "@/shared/api/client";
import type { SpendingInsight } from "@/shared/types/extended";

export async function getSpendingInsights(token: string): Promise<SpendingInsight[]> {
  return apiRequest<SpendingInsight[]>("/api/insights/spending", { token });
}
