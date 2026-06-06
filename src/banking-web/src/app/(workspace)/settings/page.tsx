"use client";

import { FormEvent, useState, useEffect } from "react";
import { bankingApi } from "@/shared/api";
import { useAuth } from "@/shared/context/AuthContext";
import { useToast } from "@/shared/context/ToastContext";
import { useAsync } from "@/shared/hooks/useAsync";
import { PageHeader } from "@/shared/components/ui/PageHeader";
import { Card } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { Skeleton } from "@/shared/components/ui/Skeleton";
import type { UserSettings } from "@/shared/types/extended";

export default function SettingsPage() {
  const { token } = useAuth();
  const { toast } = useToast();
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<UserSettings | null>(null);

  const settings = useAsync(
    () => bankingApi.settings.getSettings(token!),
    [token],
  );

  useEffect(() => {
    if (settings.data) setForm(settings.data);
  }, [settings.data]);

  async function handleSave(e: FormEvent) {
    e.preventDefault();
    if (!token || !form) return;
    setSaving(true);
    try {
      await bankingApi.settings.updateSettings(token, form);
      toast("Settings saved", "success");
    } catch {
      toast("Could not save settings", "error");
    } finally {
      setSaving(false);
    }
  }

  if (settings.isLoading || !form) {
    return <Skeleton className="h-96 w-full" />;
  }

  if (settings.error) {
    return <Card><p className="text-sm text-error">{settings.error}</p></Card>;
  }

  return (
    <div className="space-y-10">
      <PageHeader
        title="Settings"
        description="Personal information, security, and notification preferences."
      />

      <form onSubmit={handleSave} className="space-y-8">
        <Card>
          <h2 className="text-lg font-medium text-foreground">Personal information</h2>
          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            <Input label="First name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
            <Input label="Last name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
            <div className="sm:col-span-2">
              <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
            </div>
          </div>
        </Card>

        <Card>
          <h2 className="text-lg font-medium text-foreground">Security</h2>
          <label className="mt-6 flex items-center justify-between">
            <div>
              <p className="text-sm font-medium">Two-factor authentication</p>
              <p className="mt-0.5 text-xs text-subtle">Add an extra layer of security</p>
            </div>
            <input
              type="checkbox"
              checked={form.twoFactorEnabled}
              onChange={(e) => setForm({ ...form, twoFactorEnabled: e.target.checked })}
              className="h-4 w-4 rounded border-border accent-foreground"
            />
          </label>
        </Card>

        <Card>
          <h2 className="text-lg font-medium text-foreground">Notifications</h2>
          <div className="mt-6 space-y-4">
            {(
              [
                ["transactions", "Transaction alerts"],
                ["security", "Security notifications"],
                ["marketing", "Product updates"],
              ] as const
            ).map(([key, label]) => (
              <label key={key} className="flex items-center justify-between">
                <span className="text-sm">{label}</span>
                <input
                  type="checkbox"
                  checked={form.notifications[key]}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      notifications: { ...form.notifications, [key]: e.target.checked },
                    })
                  }
                  className="h-4 w-4 rounded border-border accent-foreground"
                />
              </label>
            ))}
          </div>
        </Card>

        <Button type="submit" isLoading={saving}>Save changes</Button>
      </form>
    </div>
  );
}
