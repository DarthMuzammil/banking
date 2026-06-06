/**
 * Payments service — REAL API (transfers, bills, scheduled)
 */
import { apiRequest } from "@/shared/api/client";
import type { BillPayment, ScheduledPayment, TransferRequest } from "@/shared/types/extended";

export interface TransferResponse {
  transferId: string;
  reference: string;
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  createdAt: string;
}

export interface PayBillResponse {
  billId: string;
  transactionId: string;
  amount: number;
  reference: string;
  balanceAfter: number;
}

export async function getBillPayments(token: string): Promise<BillPayment[]> {
  return apiRequest<BillPayment[]>("/api/payments/bills", { token });
}

export async function getScheduledPayments(token: string): Promise<ScheduledPayment[]> {
  return apiRequest<ScheduledPayment[]>("/api/payments/scheduled", { token });
}

export async function payBill(
  token: string,
  billId: string,
  accountId: string,
): Promise<PayBillResponse> {
  return apiRequest<PayBillResponse>(`/api/payments/bills/${billId}/pay`, {
    method: "POST",
    token,
    body: JSON.stringify({ accountId }),
  });
}

export async function submitTransfer(
  token: string,
  request: TransferRequest,
  idempotencyKey = crypto.randomUUID(),
): Promise<{ reference: string; status: string }> {
  const result = await apiRequest<TransferResponse>("/api/transfers", {
    method: "POST",
    token,
    idempotencyKey,
    body: JSON.stringify({
      fromAccountId: request.fromAccountId,
      toAccountId: request.toAccountId,
      amount: request.amount,
      description: request.note,
    }),
  });

  return { reference: result.reference, status: "Completed" };
}
