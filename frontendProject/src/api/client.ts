export interface ApiFieldError {
  field: string;
  message: string;
}

export class ApiError extends Error {
  readonly code: string;
  readonly status: number;
  readonly errors: ApiFieldError[];

  constructor(status: number, code: string, message: string, errors: ApiFieldError[] = []) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.errors = errors;
  }

  fieldError(field: string): string | undefined {
    return this.errors.find((e) => e.field.toLowerCase() === field.toLowerCase())?.message;
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(url, {
      ...init,
      headers: {
        "Content-Type": "application/json",
        ...(init?.headers ?? {})
      }
    });
  } catch {
    throw new ApiError(0, "NETWORK_ERROR", "Сервер недоступен. Проверьте, что API запущен.");
  }

  const text = await response.text();
  const body = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new ApiError(
      response.status,
      body?.code ?? "INTERNAL_ERROR",
      body?.message ?? "Не удалось выполнить запрос.",
      body?.errors ?? []
    );
  }

  return body as T;
}

export const api = {
  get: <T>(url: string) => request<T>(url),
  put: <T>(url: string, body: unknown) => request<T>(url, { method: "PUT", body: JSON.stringify(body) }),
  post: <T>(url: string, body: unknown) => request<T>(url, { method: "POST", body: JSON.stringify(body) }),
  delete: <T>(url: string) => request<T>(url, { method: "DELETE" })
};
