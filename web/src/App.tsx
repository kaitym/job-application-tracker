import { useCallback, useState } from 'react'
import { Dashboard } from './components/Dashboard'
import { LoginScreen } from './components/LoginScreen'
import { clearToken, loadToken, saveToken } from './lib/session'

// Logged in → the shop. Not logged in → the staff door.
export default function App() {
  const [token, setToken] = useState<string | null>(() => loadToken())

  const handleLogin = useCallback((newToken: string, expiresAt: string) => {
    saveToken(newToken, expiresAt)
    setToken(newToken)
  }, [])

  const handleLogout = useCallback(() => {
    clearToken()
    setToken(null)
  }, [])

  return token ? <Dashboard token={token} onLogout={handleLogout} /> : <LoginScreen onLogin={handleLogin} />
}
