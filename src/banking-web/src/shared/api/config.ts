/**
 * API configuration
 *
 * Swap mock ↔ real per domain by changing the import in each service file.
 * Auth and accounts use the real backend by default.
 * Credit cards, investments, payments, and insights use mocks until backend exists.
 */
export const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5206";

/** Set NEXT_PUBLIC_USE_MOCK_API=true to use mock data for ALL services (offline dev) */
export const USE_MOCK_API = process.env.NEXT_PUBLIC_USE_MOCK_API === "true";
