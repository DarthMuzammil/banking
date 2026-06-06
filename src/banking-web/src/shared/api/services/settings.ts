/**
 * Settings service — REAL API
 */
import { apiRequest } from "@/shared/api/client";
import type { UserSettings } from "@/shared/types/extended";

export async function getSettings(token: string): Promise<UserSettings> {
  return apiRequest<UserSettings>("/api/settings", { token });
}

export async function updateSettings(
  token: string,
  settings: UserSettings,
): Promise<UserSettings> {
  return apiRequest<UserSettings>("/api/settings", {
    method: "PATCH",
    token,
    body: JSON.stringify({
      email: settings.email,
      firstName: settings.firstName,
      lastName: settings.lastName,
      twoFactorEnabled: settings.twoFactorEnabled,
      notifications: settings.notifications,
    }),
  });
}
