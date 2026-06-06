import type { ButtonHTMLAttributes } from "react";
import { cn } from "@/shared/lib/cn";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
}

const variants = {
  primary:
    "bg-accent text-white hover:bg-[var(--accent-hover)] shadow-[inset_0_0_0_1px_rgba(255,255,255,0.08)] active:scale-[0.98]",
  secondary:
    "bg-surface-tertiary border border-border text-foreground hover:bg-surface-elevated hover:border-border-strong active:scale-[0.98]",
  ghost:
    "bg-transparent text-muted hover:bg-surface-hover hover:text-foreground",
  danger:
    "bg-error-subtle text-error hover:bg-[rgba(255,115,105,0.2)]",
};

const sizes = {
  sm: "h-7 px-2.5 text-xs",
  md: "h-8 px-3.5 text-sm",
  lg: "h-9 px-4 text-sm",
};

export function Button({
  variant = "primary",
  size = "md",
  isLoading = false,
  className,
  children,
  disabled,
  ...props
}: ButtonProps) {
  return (
    <button
      className={cn(
        "inline-flex items-center justify-center gap-2 rounded-[6px] font-medium transition-all duration-200",
        "disabled:pointer-events-none disabled:opacity-40",
        variants[variant],
        sizes[size],
        className,
      )}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <span className="inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-r-transparent" />
      ) : null}
      {isLoading ? "Working…" : children}
    </button>
  );
}
