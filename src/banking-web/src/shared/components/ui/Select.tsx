import type { SelectHTMLAttributes } from "react";
import { cn } from "@/shared/lib/cn";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  error?: string;
  options: { value: string; label: string }[];
}

export function Select({ label, error, options, id, className, ...props }: SelectProps) {
  const selectId = id ?? label.toLowerCase().replace(/\s+/g, "-");

  return (
    <div className="space-y-1.5">
      <label htmlFor={selectId} className="block text-xs font-medium text-muted">
        {label}
      </label>
      <select
        id={selectId}
        className={cn(
          "h-9 w-full appearance-none rounded-[6px] border bg-surface-secondary px-3 text-sm text-foreground transition-colors duration-200",
          "hover:border-border-strong focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent-subtle",
          error ? "border-error" : "border-border",
          className,
        )}
        {...props}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value} className="bg-surface-tertiary">
            {opt.label}
          </option>
        ))}
      </select>
      {error ? <p className="text-[11px] text-error" role="alert">{error}</p> : null}
    </div>
  );
}
