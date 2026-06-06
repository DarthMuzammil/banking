/**
 * Unified banking API facade
 *
 * Each domain is independently swappable between mock and real implementations.
 * Change the re-export in the relevant service file when backend endpoints ship.
 */
import * as accountsReal from "./accounts";
import * as accountsService from "./services/accounts";
import * as creditCards from "./services/credit-cards";
import * as insights from "./services/insights";
import * as investments from "./services/investments";
import * as payments from "./services/payments";
import * as settings from "./services/settings";
import * as transactions from "./services/transactions";
import { USE_MOCK_API } from "./config";

// Re-export auth (always real)
export * as auth from "./auth";

export const bankingApi = {
  accounts: USE_MOCK_API
    ? accountsReal // fallback: add mocks/accounts.ts for full offline mode
    : accountsService,
  transactions,
  insights,
  creditCards,
  investments,
  payments,
  settings,
};
