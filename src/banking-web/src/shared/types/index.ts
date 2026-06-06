export type AccountType = 1 | 2;
export type AccountStatus = 1 | 2;
export type TransactionType = 1 | 2;

export type CustomerRole = "Customer" | "Staff";

export interface CustomerSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role?: CustomerRole;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  customer: CustomerSummary;
}

export interface RegisterResponse {
  customerId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface Account {
  id: string;
  accountNumber: string;
  accountType: AccountType;
  balance: number;
  currency: string;
  status: AccountStatus;
  createdAt: string;
}

export interface CreateAccountResponse {
  accountId: string;
  accountNumber: string;
  accountType: AccountType;
  balance: number;
  currency: string;
}

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
  description: string | null;
  createdAt: string;
}

export interface TransactionsPage {
  accountId: string;
  skip: number;
  take: number;
  items: Transaction[];
}

export interface DepositResponse {
  transactionId: string;
  accountId: string;
  amount: number;
  balanceAfter: number;
  description: string | null;
  createdAt: string;
}

export interface WithdrawResponse {
  transactionId: string;
  accountId: string;
  amount: number;
  balanceAfter: number;
  description: string | null;
  createdAt: string;
}

export function accountTypeLabel(type: AccountType): string {
  return type === 1 ? "Checking" : "Savings";
}

export function transactionTypeLabel(type: TransactionType): string {
  return type === 1 ? "Credit" : "Debit";
}

export function formatCurrency(amount: number, currency = "USD"): string {
  return new Intl.NumberFormat("en-US", { style: "currency", currency }).format(amount);
}
