import { apiRequest } from "./client";
import type {
  Account,
  CreateAccountResponse,
  DepositResponse,
  TransactionsPage,
  WithdrawResponse,
} from "@/shared/types";

export function getAccounts(token: string): Promise<Account[]> {
  return apiRequest<Account[]>("/api/accounts", { token });
}

export function getAccount(token: string, accountId: string): Promise<Account> {
  return apiRequest<Account>(`/api/accounts/${accountId}`, { token });
}

export function createAccount(
  token: string,
  accountType: "Checking" | "Savings",
): Promise<CreateAccountResponse> {
  return apiRequest<CreateAccountResponse>("/api/accounts", {
    method: "POST",
    token,
    body: JSON.stringify({ accountType }),
  });
}

function newIdempotencyKey(): string {
  return crypto.randomUUID();
}

export function deposit(
  token: string,
  accountId: string,
  amount: number,
  description?: string,
  idempotencyKey = newIdempotencyKey(),
): Promise<DepositResponse> {
  return apiRequest<DepositResponse>(`/api/accounts/${accountId}/deposits`, {
    method: "POST",
    token,
    idempotencyKey,
    body: JSON.stringify({ amount, description }),
  });
}

export function withdraw(
  token: string,
  accountId: string,
  amount: number,
  description?: string,
  idempotencyKey = newIdempotencyKey(),
): Promise<WithdrawResponse> {
  return apiRequest<WithdrawResponse>(`/api/accounts/${accountId}/withdrawals`, {
    method: "POST",
    token,
    idempotencyKey,
    body: JSON.stringify({ amount, description }),
  });
}

export function getTransactions(
  token: string,
  accountId: string,
  skip = 0,
  take = 20,
): Promise<TransactionsPage> {
  return apiRequest<TransactionsPage>(
    `/api/accounts/${accountId}/transactions?skip=${skip}&take=${take}`,
    { token },
  );
}
