"use client";

import { AuthProvider } from "@/shared/context/AuthContext";
import { ToastProvider } from "@/shared/context/ToastContext";

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <ToastProvider>{children}</ToastProvider>
    </AuthProvider>
  );
}
