"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { ApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/context/AuthContext";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";

export function RegisterForm() {
  const { register } = useAuth();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    try {
      await register(email, password, firstName, lastName);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to create account.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="animate-fade-in">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold tracking-tight text-foreground">Create account</h1>
        <p className="mt-2 text-sm text-muted">Start with a secure personal workspace</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {error ? (
          <p className="rounded-[6px] border border-[rgba(255,115,105,0.3)] bg-error-subtle px-3 py-2.5 text-sm text-error" role="alert">
            {error}
          </p>
        ) : null}
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="First name" required value={firstName} onChange={(e) => setFirstName(e.target.value)} />
          <Input label="Last name" required value={lastName} onChange={(e) => setLastName(e.target.value)} />
        </div>
        <Input label="Email" type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <Input label="Password" type="password" autoComplete="new-password" required minLength={8} hint="At least 8 characters" value={password} onChange={(e) => setPassword(e.target.value)} />
        <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>Create account</Button>
      </form>

      <p className="mt-8 text-center text-sm text-muted">
        Already have an account?{" "}
        <Link href="/login" className="font-medium text-accent hover:text-[var(--accent-hover)] transition-colors">
          Sign in
        </Link>
      </p>
    </div>
  );
}
