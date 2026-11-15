import { useEffect, useRef, useState, type FormEvent } from 'react'
import { ApiError, addWatchedCompany, removeWatchedCompany } from '../api/client'
import type { WatchedCompany } from '../api/types'
import { timeAgo } from '../lib/format'

type Props = {
  token: string
  companies: WatchedCompany[]
  onChanged: () => void
  onClose: () => void
  onUnauthorized: () => void
}

const inputClass =
  'w-full rounded-xl border border-line bg-whitewash px-4 py-3 text-base outline-none focus:border-sun focus:ring-2 focus:ring-sun/30'

// Companies the bulletin board keeps an eye on
export function WatchlistDialog({ token, companies, onChanged, onClose, onUnauthorized }: Props) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const [url, setUrl] = useState('')
  const [name, setName] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (dialog && !dialog.open) dialog.showModal()
  }, [])

  async function handleAdd(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await addWatchedCompany(token, url.trim(), name.trim() || null)
      setUrl('')
      setName('')
      onChanged()
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) return onUnauthorized()
      setError(err instanceof ApiError && err.message ? err.message.replace(/^"|"$/g, '') : 'Couldn’t add that company.')
    } finally {
      setBusy(false)
    }
  }

  async function handleRemove(id: number) {
    try {
      await removeWatchedCompany(token, id)
      onChanged()
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) onUnauthorized()
    }
  }

  return (
    <dialog
      ref={dialogRef}
      onClose={onClose}
      aria-labelledby="watchlist-title"
      className="m-auto w-[min(100%-2rem,36rem)] rounded-3xl border border-line bg-card p-0 text-espresso backdrop:bg-espresso/50"
    >
      <div className="flex max-h-[85vh] flex-col gap-5 overflow-y-auto p-6 sm:p-8">
        <div className="flex items-start justify-between gap-4">
          <div className="flex flex-col gap-1">
            <h2 id="watchlist-title" className="font-display text-3xl">WATCHLIST</h2>
            <p className="font-script text-2xl text-sun">companies I’m keeping an eye on</p>
          </div>
          <button type="button" onClick={onClose} aria-label="Close" className="rounded-full p-2 text-muted transition hover:bg-sand hover:text-espresso">
            <svg className="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
              <path d="M6 6l12 12M18 6L6 18" />
            </svg>
          </button>
        </div>

        <form onSubmit={handleAdd} className="flex flex-col gap-3 rounded-2xl bg-sand p-4">
          <label className="flex flex-col gap-1.5 text-sm font-medium">
            Careers page link
            <input
              type="url"
              required
              placeholder="https://boards.greenhouse.io/pinterest"
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              className={inputClass}
            />
          </label>
          <label className="flex flex-col gap-1.5 text-sm font-medium">
            Name (optional)
            <input placeholder="Pinterest" value={name} onChange={(e) => setName(e.target.value)} className={inputClass} />
          </label>
          <p className="text-xs text-muted">
            Works with Greenhouse, Lever, Ashby, and SmartRecruiters careers pages. Workday and LinkedIn don’t share their listings.
          </p>
          {error && (
            <p role="alert" className="rounded-xl bg-sun/10 px-4 py-3 text-sm font-medium text-sun-dark">
              {error}
            </p>
          )}
          <button
            type="submit"
            disabled={busy}
            className="self-end rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-card uppercase transition hover:bg-sun-dark disabled:opacity-60"
          >
            {busy ? 'Checking their board…' : 'Watch them'}
          </button>
        </form>

        {companies.length === 0 ? (
          <p className="text-center text-muted">No companies yet. Add one above.</p>
        ) : (
          <ul className="flex flex-col divide-y divide-dashed divide-line">
            {companies.map((company) => (
              <li key={company.id} className="flex items-center justify-between gap-3 py-3">
                <div className="flex min-w-0 flex-col">
                  <span className="font-bold">{company.name}</span>
                  <span className="text-xs text-muted">
                    {company.board} · {company.lastCheckedAt ? `checked ${timeAgo(company.lastCheckedAt)}` : 'not checked yet'}
                  </span>
                  {company.lastError && <span className="text-xs text-sun-dark">{company.lastError}</span>}
                </div>
                <button
                  type="button"
                  onClick={() => handleRemove(company.id)}
                  className="shrink-0 rounded-full border border-line px-3 py-1.5 text-xs font-bold text-muted transition hover:border-espresso hover:text-espresso"
                >
                  Remove
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </dialog>
  )
}
