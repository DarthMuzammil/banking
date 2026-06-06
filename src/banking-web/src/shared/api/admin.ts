import { apiRequest } from "./client";

export interface AdminCustomer {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  createdAt: string;
}

export interface AdminAccount {
  id: string;
  accountNumber: string;
  accountType: string;
  balance: number;
  currency: string;
  status: string;
}

export function searchCustomers(
  token: string,
  search?: string,
): Promise<AdminCustomer[]> {
  const query = search ? `?search=${encodeURIComponent(search)}` : "";
  return apiRequest<AdminCustomer[]>(`/api/admin/customers${query}`, { token });
}

export function getCustomerAccounts(
  token: string,
  customerId: string,
): Promise<AdminAccount[]> {
  return apiRequest<AdminAccount[]>(`/api/admin/customers/${customerId}/accounts`, {
    token,
  });
}
