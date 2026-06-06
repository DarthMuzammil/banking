export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-surface px-6 py-12">
      <div className="mb-8 flex items-center gap-2">
        <div className="flex h-8 w-8 items-center justify-center rounded-[6px] bg-surface-tertiary text-xs font-bold text-muted">
          B
        </div>
        <span className="text-sm font-medium text-foreground">Banking</span>
      </div>
      <div className="w-full max-w-[400px]">{children}</div>
    </div>
  );
}
