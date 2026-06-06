"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { useRouter } from "next/navigation";
import * as authApi from "@/shared/api/auth";
import { getRoleFromJwt } from "@/shared/lib/jwt";
import type { CustomerRole, CustomerSummary } from "@/shared/types";

const TOKEN_KEY = "banking_token";
const REFRESH_TOKEN_KEY = "banking_refresh_token";
const CUSTOMER_KEY = "banking_customer";

interface AuthContextValue {
  token: string | null;
  refreshToken: string | null;
  customer: CustomerSummary | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  isStaff: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    firstName: string,
    lastName: string,
  ) => Promise<void>;
  logout: () => void;
  refreshSession: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function withRole(customer: CustomerSummary, token: string | null): CustomerSummary {
  if (customer.role) {
    return customer;
  }

  const role = getRoleFromJwt(token);
  return role ? { ...customer, role } : customer;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [customer, setCustomer] = useState<CustomerSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function hydrate() {
      const storedToken = localStorage.getItem(TOKEN_KEY);
      const storedRefresh = localStorage.getItem(REFRESH_TOKEN_KEY);
      const storedCustomer = localStorage.getItem(CUSTOMER_KEY);

      if (!storedToken || !storedCustomer) {
        if (!cancelled) setIsLoading(false);
        return;
      }

      let parsedCustomer = withRole(
        JSON.parse(storedCustomer) as CustomerSummary,
        storedToken,
      );

      if (!parsedCustomer.role && storedRefresh) {
        try {
          const response = await authApi.refresh(storedRefresh);
          if (!cancelled) {
            localStorage.setItem(TOKEN_KEY, response.token);
            localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
            localStorage.setItem(CUSTOMER_KEY, JSON.stringify(response.customer));
            setToken(response.token);
            setRefreshToken(response.refreshToken);
            setCustomer(response.customer);
            setIsLoading(false);
          }
          return;
        } catch {
          // Fall back to stored session below.
        }
      }

      if (!cancelled) {
        if (parsedCustomer.role) {
          localStorage.setItem(CUSTOMER_KEY, JSON.stringify(parsedCustomer));
        }
        setToken(storedToken);
        setRefreshToken(storedRefresh);
        setCustomer(parsedCustomer);
        setIsLoading(false);
      }
    }

    void hydrate();

    return () => {
      cancelled = true;
    };
  }, []);

  const persistSession = useCallback(
    (newToken: string, newRefreshToken: string, newCustomer: CustomerSummary) => {
      localStorage.setItem(TOKEN_KEY, newToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, newRefreshToken);
      localStorage.setItem(CUSTOMER_KEY, JSON.stringify(newCustomer));
      setToken(newToken);
      setRefreshToken(newRefreshToken);
      setCustomer(newCustomer);
    },
    [],
  );

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login({ email, password });
      persistSession(response.token, response.refreshToken, response.customer);
      router.push(response.customer.role === "Staff" ? "/admin" : "/dashboard");
    },
    [persistSession, router],
  );

  const register = useCallback(
    async (
      email: string,
      password: string,
      firstName: string,
      lastName: string,
    ) => {
      await authApi.register({ email, password, firstName, lastName });
      const response = await authApi.login({ email, password });
      persistSession(response.token, response.refreshToken, response.customer);
      router.push("/dashboard");
    },
    [persistSession, router],
  );

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(CUSTOMER_KEY);
    setToken(null);
    setRefreshToken(null);
    setCustomer(null);
    router.push("/login");
  }, [router]);

  const refreshSession = useCallback(async () => {
    const storedRefresh = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!storedRefresh) {
      return false;
    }

    try {
      const response = await authApi.refresh(storedRefresh);
      persistSession(response.token, response.refreshToken, response.customer);
      return true;
    } catch {
      logout();
      return false;
    }
  }, [logout, persistSession]);

  const value = useMemo(
    () => ({
      token,
      refreshToken,
      customer,
      isLoading,
      isAuthenticated: Boolean(token),
      isStaff:
        customer?.role === "Staff" ||
        getRoleFromJwt(token) === "Staff",
      login,
      register,
      logout,
      refreshSession,
    }),
    [token, refreshToken, customer, isLoading, login, register, logout, refreshSession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
