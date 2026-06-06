import type { InputHTMLAttributes } from "react";
import { cn } from "@/shared/lib/cn";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: string;
  error?: string;
}

export function Input({ label, hint, error, id, className, ...props }: InputProps) {
  const inputId = id ?? label.toLowerCase().replace(/\s+/g, "-");

  return (
    <div className="space-y-1.5">
      <label htmlFor={inputId} className="block text-xs font-medium text-muted">
        {label}
      </label>
      <input
        id={inputId}
        className={cn(
          "h-9 w-full rounded-[6px] border bg-surface-secondary px-3 text-sm text-foreground transition-colors duration-200",
          "placeholder:text-subtle",
          "hover:border-border-strong",
          "focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent-subtle",
          error ? "border-error" : "border-border",
          className,
        )}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined}
        {...props}
      />
      {hint && !error ? (
        <p id={`${inputId}-hint`} className="text-[11px] text-subtle">{hint}</p>
      ) : null}
      {error ? (
        <p id={`${inputId}-error`} className="text-[11px] text-error" role="alert">{error}</p>
      ) : null}
    </div>
  );
}
