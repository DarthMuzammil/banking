"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/shared/context/AuthContext";

export function useRequireAuth() {
  const { isAuthenticated, isLoading, token } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isAuthenticated, isLoading, router]);

  return { token, isLoading, isAuthenticated };
}
