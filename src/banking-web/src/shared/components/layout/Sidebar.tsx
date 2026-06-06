"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/shared/lib/cn";
import { useAuth } from "@/shared/context/AuthContext";
import { Button } from "@/shared/components/ui/Button";

const navSections = [
  {
    label: "Workspace",
    items: [
      { href: "/dashboard", label: "Overview" },
      { href: "/accounts", label: "Accounts" },
      { href: "/transactions", label: "Transactions" },
    ],
  },
  {
    label: "Money",
    items: [
      { href: "/payments", label: "Payments" },
      { href: "/credit-cards", label: "Credit cards" },
      { href: "/investments", label: "Investments" },
    ],
  },
  {
    label: "Account",
    items: [{ href: "/settings", label: "Settings" }],
  },
];

export function Sidebar() {
  const pathname = usePathname();
  const { customer, isStaff, logout } = useAuth();

  const initials = customer
    ? `${customer.firstName[0] ?? ""}${customer.lastName[0] ?? ""}`.toUpperCase()
    : "?";

  return (
    <aside
      className="flex h-screen w-[var(--sidebar-width)] shrink-0 flex-col border-r border-border bg-surface-secondary"
      aria-label="Main navigation"
    >
      <div className="flex h-[52px] items-center gap-2 px-4">
        <div className="flex h-6 w-6 items-center justify-center rounded-[4px] bg-surface-elevated text-[10px] font-bold text-muted">
          B
        </div>
        <Link href="/dashboard" className="text-sm font-medium text-foreground hover:text-white transition-colors">
          Banking
        </Link>
      </div>

      <nav className="flex-1 overflow-y-auto px-2 py-3">
        {isStaff ? (
          <div className="mb-5">
            <p className="mb-1 px-2 text-[11px] font-medium uppercase tracking-wider text-subtle">
              Staff
            </p>
            <ul className="space-y-0.5">
              <li>
                <Link
                  href="/admin"
                  className={cn(
                    "flex items-center rounded-[6px] px-2 py-1.5 text-sm transition-colors duration-200",
                    pathname === "/admin" || pathname.startsWith("/admin/")
                      ? "bg-surface-active font-medium text-foreground"
                      : "text-muted hover:bg-surface-hover hover:text-foreground",
                  )}
                  aria-current={pathname === "/admin" ? "page" : undefined}
                >
                  Customer lookup
                </Link>
              </li>
            </ul>
          </div>
        ) : null}
        {navSections.map((section) => (
          <div key={section.label} className="mb-5">
            <p className="mb-1 px-2 text-[11px] font-medium uppercase tracking-wider text-subtle">
              {section.label}
            </p>
            <ul className="space-y-0.5">
              {section.items.map((item) => {
                const active =
                  pathname === item.href || pathname.startsWith(`${item.href}/`);
                return (
                  <li key={item.href}>
                    <Link
                      href={item.href}
                      className={cn(
                        "flex items-center rounded-[6px] px-2 py-1.5 text-sm transition-colors duration-200",
                        active
                          ? "bg-surface-active font-medium text-foreground"
                          : "text-muted hover:bg-surface-hover hover:text-foreground",
                      )}
                      aria-current={active ? "page" : undefined}
                    >
                      {item.label}
                    </Link>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>

      <div className="border-t border-border p-3">
        {customer ? (
          <div className="flex items-center gap-2.5 rounded-[6px] px-2 py-2">
            <div
              className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-accent-subtle text-[11px] font-semibold text-accent"
              aria-hidden="true"
            >
              {initials}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-foreground">
                {customer.firstName} {customer.lastName}
              </p>
              <p className="truncate text-[11px] text-subtle">{customer.email}</p>
            </div>
          </div>
        ) : null}
        <Button variant="ghost" size="sm" className="mt-1 w-full justify-start" onClick={logout}>
          Log out
        </Button>
      </div>
    </aside>
  );
}
