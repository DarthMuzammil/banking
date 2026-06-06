"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { RegisterForm } from "@/features/auth/RegisterForm";
import { useAuth } from "@/shared/context/AuthContext";

export default function RegisterPage() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && isAuthenticated) router.replace("/dashboard");
  }, [isAuthenticated, isLoading, router]);

  if (isLoading || isAuthenticated) return null;
  return <RegisterForm />;
}
