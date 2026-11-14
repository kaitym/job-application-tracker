// Keep the login token between visits (falls back to memory if storage is blocked)
const KEY = 'sun-waves-token'

export function loadToken(): string | null {
  try {
    const raw = localStorage.getItem(KEY)
    if (!raw) return null
    const { token, expiresAt } = JSON.parse(raw) as { token: string; expiresAt: string }
    return new Date(expiresAt) > new Date() ? token : null
  } catch {
    return null
  }
}

export function saveToken(token: string, expiresAt: string) {
  try {
    localStorage.setItem(KEY, JSON.stringify({ token, expiresAt }))
  } catch {
    // Private mode etc: stay logged in for this tab only
  }
}

export function clearToken() {
  try {
    localStorage.removeItem(KEY)
  } catch {
    // Nothing to clear
  }
}
