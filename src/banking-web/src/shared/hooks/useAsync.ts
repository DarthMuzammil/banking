"use client";

import { useCallback, useEffect, useState } from "react";

type AsyncStatus = "idle" | "loading" | "success" | "error";

interface UseAsyncResult<T> {
  data: T | null;
  error: string | null;
  status: AsyncStatus;
  isLoading: boolean;
  isSuccess: boolean;
  isEmpty: boolean;
  reload: () => void;
}

export function useAsync<T>(
  fetcher: () => Promise<T>,
  deps: unknown[] = [],
  isEmptyCheck?: (data: T) => boolean,
): UseAsyncResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [tick, setTick] = useState(0);

  const reload = useCallback(() => setTick((n) => n + 1), []);

  useEffect(() => {
    let cancelled = false;
    setStatus("loading");
    setError(null);

    fetcher()
      .then((result) => {
        if (!cancelled) {
          setData(result);
          setStatus("success");
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setData(null);
          setError(err instanceof Error ? err.message : "Something went wrong");
          setStatus("error");
        }
      });

    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tick, ...deps]);

  const isSuccess = status === "success" && data !== null;
  const isEmpty = isSuccess && isEmptyCheck ? isEmptyCheck(data) : false;

  return {
    data,
    error,
    status,
    isLoading: status === "loading" || status === "idle",
    isSuccess,
    isEmpty,
    reload,
  };
}
