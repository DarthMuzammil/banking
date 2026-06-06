"use client";

import { Sidebar } from "./Sidebar";
import { useRequireAuth } from "@/shared/hooks/useRequireAuth";

export function WorkspaceShell({ children }: { children: React.ReactNode }) {
  const { isLoading } = useRequireAuth();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-surface">
        <p className="text-sm text-muted">Loading workspace…</p>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen bg-surface">
      <Sidebar />
      <main className="min-w-0 flex-1 overflow-y-auto">
        <div className="mx-auto max-w-[720px] px-8 py-10 md:px-12 md:py-12 animate-fade-in">
          {children}
        </div>
      </main>
    </div>
  );
}
