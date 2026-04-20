const API_URL = "https://localhost:7269/api/auth";

export interface Response {
  isSuccess: boolean;
  email: string;
  username: string;
}

export async function login(email: string, password: string): Promise<Response | null> {
  const res = await fetch(`${API_URL}/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    credentials: "include",
    body: JSON.stringify({ email, password })
  });

  if (!res.ok) {
    throw new Error("Login failed");
  }else
  {
    return res.json();
  }
}

export async function register(email: string, password: string, username: string): Promise<Response | null> {
  const res = await fetch(`${API_URL}/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    credentials: "include",
    body: JSON.stringify({ email, password, username })
  });

  if (!res.ok) {
    throw new Error("Register failed");
  }else
  {
    return res.json();
  }
}

export async function logout() {
  const res = await fetch(`${API_URL}/logout`, {
    method: "POST",
    credentials: "include"
  });

  if (!res.ok) {
    throw new Error("Logout failed");
  }else
  {
    return true;
  }
}

export async function refresh(): Promise<Response | null> {
  const res = await fetch(`${API_URL}/refresh`, {
    method: "POST",
    credentials: "include"
  });

  if (!res.ok) return null;

  return await res.json();
}