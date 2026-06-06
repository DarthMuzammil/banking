/**
 * Investments service — REAL API
 */
import { apiRequest } from "@/shared/api/client";
import type { PortfolioSummary } from "@/shared/types/extended";

export async function getPortfolio(token: string): Promise<PortfolioSummary> {
  return apiRequest<PortfolioSummary>("/api/investments/portfolio", { token });
}
