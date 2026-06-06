import { cn } from "@/shared/lib/cn";

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hover?: boolean;
  padding?: "sm" | "md" | "lg" | "none";
}

const paddingMap = { none: "", sm: "p-4", md: "p-5", lg: "p-6" };

export function Card({ children, className, hover = false, padding = "md" }: CardProps) {
  return (
    <div
      className={cn(
        "rounded-lg border border-border bg-surface-tertiary",
        paddingMap[padding],
        hover &&
          "cursor-pointer transition-all duration-200 hover:border-border-strong hover:bg-surface-elevated",
        className,
      )}
    >
      {children}
    </div>
  );
}
