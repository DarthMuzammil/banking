import type { CustomerRole } from "@/shared/types";

const ROLE_CLAIM_KEYS = [
  "role",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
] as const;

export function getRoleFromJwt(token: string | null): CustomerRole | null {
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const decoded = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/"))) as Record<
      string,
      unknown
    >;

    for (const key of ROLE_CLAIM_KEYS) {
      const value = decoded[key];
      if (value === "Staff" || value === "Customer") {
        return value;
      }
    }
  } catch {
    return null;
  }

  return null;
}
