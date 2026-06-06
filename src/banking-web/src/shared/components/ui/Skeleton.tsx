import { cn } from "@/shared/lib/cn";

export function Skeleton({ className }: { className?: string }) {
  return (
    <div
      className={cn("animate-skeleton rounded-[6px] bg-surface-elevated", className)}
      aria-hidden="true"
    />
  );
}

export function SkeletonCard() {
  return (
    <div className="rounded-lg border border-border bg-surface-tertiary p-5 space-y-3">
      <Skeleton className="h-3 w-24" />
      <Skeleton className="h-8 w-40" />
      <Skeleton className="h-3 w-32" />
    </div>
  );
}
