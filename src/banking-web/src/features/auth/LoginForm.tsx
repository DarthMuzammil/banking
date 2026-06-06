"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { ApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/context/AuthContext";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";

export function LoginForm() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    try {
      await login(email, password);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to sign in.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="animate-fade-in">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold tracking-tight text-foreground">Sign in</h1>
        <p className="mt-2 text-sm text-muted">Access your accounts and transactions</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {error ? (
          <p className="rounded-[6px] border border-[rgba(255,115,105,0.3)] bg-error-subtle px-3 py-2.5 text-sm text-error" role="alert">
            {error}
          </p>
        ) : null}
        <Input label="Email" type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <Input label="Password" type="password" autoComplete="current-password" required value={password} onChange={(e) => setPassword(e.target.value)} />
        <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>Continue</Button>
      </form>

      <p className="mt-8 text-center text-sm text-muted">
        New here?{" "}
        <Link href="/register" className="font-medium text-accent hover:text-[var(--accent-hover)] transition-colors">
          Create an account
        </Link>
      </p>
    </div>
  );
}
