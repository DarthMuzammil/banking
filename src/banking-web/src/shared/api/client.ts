const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5206";

export class ApiError extends Error {
  constructor(
    public status: number,
    public code: string,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface ApiErrorBody {
  error?: string;
  code?: string;
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit & { token?: string | null; idempotencyKey?: string } = {},
): Promise<T> {
  const { token, idempotencyKey, headers: customHeaders, ...rest } = options;

  const headers = new Headers(customHeaders);
  headers.set("Content-Type", "application/json");
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }
  if (idempotencyKey) {
    headers.set("Idempotency-Key", idempotencyKey);
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...rest,
    headers,
  });

  if (!response.ok) {
    const body: ApiErrorBody = await response.json().catch(() => ({}));
    throw new ApiError(
      response.status,
      body.code ?? "UNKNOWN",
      body.error ?? response.statusText,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}
