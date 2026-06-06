import { cn } from "@/shared/lib/cn";

interface EmptyStateProps {
  title: string;
  description: string;
  action?: React.ReactNode;
  className?: string;
}

export function EmptyState({ title, description, action, className }: EmptyStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center rounded-lg border border-dashed border-border bg-surface-secondary/50 px-8 py-14 text-center",
        className,
      )}
    >
      <p className="text-sm font-medium text-foreground">{title}</p>
      <p className="mt-2 max-w-sm text-sm leading-relaxed text-muted">{description}</p>
      {action ? <div className="mt-6">{action}</div> : null}
    </div>
  );
}
