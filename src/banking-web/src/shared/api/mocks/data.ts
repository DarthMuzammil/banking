import type {
  BillPayment,
  CreditCard,
  CreditCardSpending,
  InvestmentHolding,
  PortfolioSummary,
  ScheduledPayment,
  SpendingInsight,
  UserSettings,
} from "@/shared/types/extended";

export const mockInsights: SpendingInsight[] = [
  { id: "1", label: "Total spending", amount: 2840, changePercent: -4.2, direction: "down" },
  { id: "2", label: "Bills & utilities", amount: 620, changePercent: 1.1, direction: "up" },
  { id: "3", label: "Transfers", amount: 1200, changePercent: 0, direction: "flat" },
];

export const mockCreditCards: CreditCard[] = [
  {
    id: "cc-1",
    name: "Platinum Rewards",
    lastFour: "4821",
    balance: 1842.5,
    limit: 10000,
    dueDate: "2026-06-15",
    minPayment: 55,
    apr: 18.9,
  },
  {
    id: "cc-2",
    name: "Business Card",
    lastFour: "9034",
    balance: 420,
    limit: 5000,
    dueDate: "2026-06-22",
    minPayment: 25,
    apr: 16.4,
  },
];

export const mockCardSpending: CreditCardSpending[] = [
  { category: "Travel", amount: 680, percentage: 30 },
  { category: "Dining", amount: 420, percentage: 19 },
  { category: "Software", amount: 380, percentage: 17 },
  { category: "Office", amount: 290, percentage: 13 },
  { category: "Other", amount: 472, percentage: 21 },
];

export const mockPortfolio: PortfolioSummary = {
  totalValue: 48250,
  dayChange: 312.4,
  dayChangePercent: 0.65,
  holdings: [
    { id: "h1", symbol: "VTI", name: "Total Stock Market", shares: 120, value: 28500, changePercent: 0.8 },
    { id: "h2", symbol: "BND", name: "Total Bond Market", shares: 200, value: 15200, changePercent: 0.1 },
    { id: "h3", symbol: "VXUS", name: "International Stocks", shares: 85, value: 4550, changePercent: -0.3 },
  ] satisfies InvestmentHolding[],
};

export const mockBills: BillPayment[] = [
  { id: "b1", payee: "Electric Company", amount: 142.3, dueDate: "2026-06-12", status: "Due" },
  { id: "b2", payee: "Internet Provider", amount: 79.99, dueDate: "2026-06-18", status: "Scheduled" },
  { id: "b3", payee: "Insurance", amount: 210, dueDate: "2026-06-01", status: "Paid" },
];

export const mockScheduled: ScheduledPayment[] = [
  { id: "s1", payee: "Rent", amount: 1800, frequency: "Monthly", nextDate: "2026-07-01", accountId: "" },
  { id: "s2", payee: "Savings transfer", amount: 500, frequency: "Monthly", nextDate: "2026-06-15", accountId: "" },
];

export function buildMockSettings(
  firstName: string,
  lastName: string,
  email: string,
): UserSettings {
  return {
    email,
    firstName,
    lastName,
    twoFactorEnabled: false,
    notifications: { transactions: true, security: true, marketing: false },
  };
}

const delay = (ms = 400) => new Promise((r) => setTimeout(r, ms));

export async function mockFetch<T>(data: T, ms?: number): Promise<T> {
  await delay(ms);
  return data;
}
