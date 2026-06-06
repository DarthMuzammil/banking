/**
 * Credit cards service — REAL API
 */
import { apiRequest } from "@/shared/api/client";
import type { CreditCard, CreditCardSpending } from "@/shared/types/extended";

export async function getCreditCards(token: string): Promise<CreditCard[]> {
  return apiRequest<CreditCard[]>("/api/credit-cards", { token });
}

export async function getCardSpending(
  token: string,
  cardId: string,
): Promise<CreditCardSpending[]> {
  return apiRequest<CreditCardSpending[]>(`/api/credit-cards/${cardId}/spending`, { token });
}
