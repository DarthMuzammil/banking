import { apiRequest } from "./client";
import type { AuthResponse, RegisterResponse } from "@/shared/types";

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export function login(payload: LoginPayload): Promise<AuthResponse> {
  return apiRequest<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function refresh(refreshToken: string): Promise<AuthResponse> {
  return apiRequest<AuthResponse>("/api/auth/refresh", {
    method: "POST",
    body: JSON.stringify({ refreshToken }),
  });
}

export function register(payload: RegisterPayload): Promise<RegisterResponse> {
  return apiRequest<RegisterResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}
