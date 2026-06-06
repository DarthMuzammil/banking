"use client";

import { createContext, useCallback, useContext, useMemo, useState } from "react";
import { cn } from "@/shared/lib/cn";

type ToastVariant = "success" | "error" | "info";

interface Toast {
  id: string;
  message: string;
  variant: ToastVariant;
}

interface ToastContextValue {
  toast: (message: string, variant?: ToastVariant) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const variantStyles: Record<ToastVariant, string> = {
  success: "border-[rgba(77,171,159,0.3)] bg-surface-elevated text-success",
  error: "border-[rgba(255,115,105,0.3)] bg-surface-elevated text-error",
  info: "border-border bg-surface-elevated text-foreground",
};

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const toast = useCallback((message: string, variant: ToastVariant = "info") => {
    const id = crypto.randomUUID();
    setToasts((prev) => [...prev, { id, message, variant }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 3200);
  }, []);

  const value = useMemo(() => ({ toast }), [toast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div
        className="pointer-events-none fixed bottom-6 right-6 z-[100] flex flex-col gap-2"
        aria-live="polite"
        aria-relevant="additions"
      >
        {toasts.map((t) => (
          <div
            key={t.id}
            className={cn(
              "animate-fade-in pointer-events-auto rounded-[6px] border px-4 py-3 text-sm font-medium shadow-[0_8px_32px_rgba(0,0,0,0.4)]",
              variantStyles[t.variant],
            )}
            role="status"
          >
            {t.message}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within ToastProvider");
  return ctx;
}
