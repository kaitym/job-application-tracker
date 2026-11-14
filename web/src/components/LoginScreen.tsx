import { useState, type FormEvent } from 'react'
import { ApiError, login } from '../api/client'
import { WaveIcon } from './icons'

type Props = {
  onLogin: (token: string, expiresAt: string) => void
}

// Owner login
export function LoginScreen({ onLogin }: Props) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setError(null)

    try {
      const result = await login(username, password)
      onLogin(result.token, result.expiresAt)
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) setError('That username or password didn’t match.')
      else if (err instanceof ApiError && err.status === 429) setError('Too many tries. Wait a minute and try again.')
      else setError('Couldn’t reach the shop. Is the API running?')
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-md flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-1">
        <h1 className="font-display text-6xl leading-[0.9] text-sun">
          SUN &amp;
          <br />
          WAVES
        </h1>
        <span className="font-script text-3xl">coffee &amp; job hunt club</span>
        <WaveIcon className="mt-2 h-4 w-40 text-sun" />
      </header>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-3xl border border-line bg-card p-6">
        <h2 className="font-sign text-sm font-bold uppercase tracking-[0.2em]">Staff only</h2>

        <label className="flex flex-col gap-1.5 text-sm font-medium">
          Username
          <input
            className="rounded-xl border border-line bg-whitewash px-4 py-3 text-base outline-none focus:border-sun focus:ring-2 focus:ring-sun/30"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
            required
          />
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium">
          Password
          <input
            type="password"
            className="rounded-xl border border-line bg-whitewash px-4 py-3 text-base outline-none focus:border-sun focus:ring-2 focus:ring-sun/30"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
            required
          />
        </label>

        {error && (
          <p role="alert" className="rounded-xl bg-sun/10 px-4 py-3 text-sm font-medium text-sun-dark">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={busy}
          className="mt-2 rounded-full bg-sun px-6 py-3 font-sign text-sm font-bold uppercase tracking-[0.2em] text-card transition hover:bg-sun-dark disabled:opacity-60"
        >
          {busy ? 'Opening up…' : 'Open the shop'}
        </button>
      </form>
    </main>
  )
}
