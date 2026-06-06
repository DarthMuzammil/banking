import { cn } from "@/shared/lib/cn";

type BadgeVariant = "neutral" | "success" | "warning" | "error" | "accent";

const styles: Record<BadgeVariant, string> = {
  neutral: "bg-surface-hover text-muted border border-border",
  success: "bg-success-subtle text-success border border-[rgba(77,171,159,0.2)]",
  warning: "bg-warning-subtle text-warning border border-[rgba(203,145,47,0.2)]",
  error: "bg-error-subtle text-error border border-[rgba(255,115,105,0.2)]",
  accent: "bg-accent-subtle text-accent border border-[rgba(82,156,202,0.25)]",
};

export function Badge({
  children,
  variant = "neutral",
  className,
}: {
  children: React.ReactNode;
  variant?: BadgeVariant;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded px-2 py-0.5 text-[10px] font-medium uppercase tracking-wider",
        styles[variant],
        className,
      )}
    >
      {children}
    </span>
  );
}
