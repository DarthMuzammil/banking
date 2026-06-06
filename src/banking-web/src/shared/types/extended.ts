import type { Account, Transaction } from "./index";

export type TransactionCategory =
  | "Income"
  | "Transfer"
  | "Bills"
  | "Shopping"
  | "Food"
  | "Transport"
  | "Other";

export interface EnrichedTransaction extends Transaction {
  accountId: string;
  accountNumber: string;
  category: TransactionCategory;
}

export interface SpendingInsight {
  id: string;
  label: string;
  amount: number;
  changePercent: number;
  direction: "up" | "down" | "flat";
}

export interface CreditCard {
  id: string;
  name: string;
  lastFour: string;
  balance: number;
  limit: number;
  dueDate: string;
  minPayment: number;
  apr: number;
}

export interface CreditCardSpending {
  category: string;
  amount: number;
  percentage: number;
}

export interface InvestmentHolding {
  id: string;
  symbol: string;
  name: string;
  shares: number;
  value: number;
  changePercent: number;
}

export interface PortfolioSummary {
  totalValue: number;
  dayChange: number;
  dayChangePercent: number;
  holdings: InvestmentHolding[];
}

export interface ScheduledPayment {
  id: string;
  payee: string;
  amount: number;
  frequency: "Weekly" | "Monthly" | "Quarterly";
  nextDate: string;
  accountId: string;
}

export interface BillPayment {
  id: string;
  payee: string;
  amount: number;
  dueDate: string;
  status: "Due" | "Paid" | "Scheduled";
}

export interface TransferRequest {
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  note?: string;
}

export interface UserSettings {
  email: string;
  firstName: string;
  lastName: string;
  twoFactorEnabled: boolean;
  notifications: {
    transactions: boolean;
    security: boolean;
    marketing: boolean;
  };
}

export interface AccountWithMeta extends Account {
  label: string;
}
