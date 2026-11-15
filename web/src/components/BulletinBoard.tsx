import { useCallback, useEffect, useState } from 'react'
import { ApiError, addRecToMenu, getRecs, refreshRecs, tossRec } from '../api/client'
import type { Job, JobRec, RecBoard } from '../api/types'
import { shortDate, timeAgo } from '../lib/format'
import { WatchlistDialog } from './WatchlistDialog'

type Props = {
  token: string
  onAddedJob: (job: Job) => void
  onUnauthorized: () => void
  onToast: (message: string) => void
}

const tilts = ['-rotate-2', 'rotate-1', '-rotate-1', 'rotate-2', 'rotate-0', '-rotate-1']
const pinColors = ['bg-sun', 'bg-golden', 'bg-sage', 'bg-espresso']
const regionLabel: Record<number, string> = { 3: 'Orange County', 2: 'San Diego', 1: 'SF / Remote' }

// Cork board of job recs found on watched companies' boards
export function BulletinBoard({ token, onAddedJob, onUnauthorized, onToast }: Props) {
  const [board, setBoard] = useState<RecBoard | null>(null)
  const [checking, setChecking] = useState(false)
  const [showAll, setShowAll] = useState(false)
  const [watchlistOpen, setWatchlistOpen] = useState(false)
  const [busyId, setBusyId] = useState<number | null>(null)

  const handleError = useCallback(
    (err: unknown) => {
      if (err instanceof ApiError && err.status === 401) onUnauthorized()
      else onToast('Couldn’t reach the bulletin board.')
    },
    [onUnauthorized, onToast],
  )

  const load = useCallback(() => getRecs(token).then(setBoard).catch(handleError), [token, handleError])

  // Check the boards if it's been 12+ hours, then show what's pinned
  useEffect(() => {
    let cancelled = false
    refreshRecs(token, true)
      .catch(() => null)
      .then(() => getRecs(token))
      .then((result) => {
        if (!cancelled) setBoard(result)
      })
      .catch((err) => {
        if (!cancelled) handleError(err)
      })
    return () => {
      cancelled = true
    }
  }, [token, handleError])

  async function checkNow() {
    setChecking(true)
    try {
      const result = await refreshRecs(token, false)
      await load()
      onToast(result.added > 0 ? `${result.added} new pin${result.added === 1 ? '' : 's'} on the board` : 'No new pins this time')
    } catch (err) {
      handleError(err)
    } finally {
      setChecking(false)
    }
  }

  async function handleAdd(rec: JobRec) {
    setBusyId(rec.id)
    try {
      const job = await addRecToMenu(token, rec.id)
      setBoard((current) => current && { ...current, recs: current.recs.filter((r) => r.id !== rec.id), newCount: current.newCount - 1 })
      onAddedJob(job)
    } catch (err) {
      handleError(err)
    } finally {
      setBusyId(null)
    }
  }

  async function handleToss(rec: JobRec) {
    setBusyId(rec.id)
    try {
      await tossRec(token, rec.id)
      setBoard((current) => current && { ...current, recs: current.recs.filter((r) => r.id !== rec.id), newCount: current.newCount - 1 })
    } catch (err) {
      handleError(err)
    } finally {
      setBusyId(null)
    }
  }

  const recs = board?.recs ?? []
  const visible = showAll ? recs : recs.slice(0, 6)

  return (
    <section aria-labelledby="board-heading" className="flex flex-col gap-3">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div className="flex flex-col">
          <h2 id="board-heading" className="font-sign text-sm font-bold tracking-[0.3em] uppercase">
            Bulletin board
          </h2>
          <span className="text-sm text-muted">
            {board?.lastRefreshedAt ? `Fresh pins from your watchlist · checked ${timeAgo(board.lastRefreshedAt)}` : 'Fresh pins from your watchlist'}
          </span>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setWatchlistOpen(true)}
            className="rounded-full border border-line bg-card px-4 py-2 text-sm font-bold text-muted transition hover:border-espresso hover:text-espresso active:scale-95"
          >
            Watchlist{board ? ` (${board.companies.length})` : ''}
          </button>
          <button
            type="button"
            onClick={checkNow}
            disabled={checking || !board || board.companies.length === 0}
            className="rounded-full bg-espresso px-4 py-2 text-sm font-bold text-whitewash transition hover:bg-espresso-soft active:scale-95 disabled:opacity-50"
          >
            {checking ? 'Checking…' : 'Check now'}
          </button>
        </div>
      </div>

      <div className="cork rounded-3xl p-5 sm:p-7">
        {!board ? (
          <p className="py-8 text-center font-script text-2xl text-espresso">Checking the boards…</p>
        ) : board.companies.length === 0 ? (
          <div className="mx-auto flex max-w-sm -rotate-1 flex-col items-center gap-3 rounded-lg bg-card px-6 py-6 text-center shadow-md">
            <span className="font-script text-2xl text-sun">Nothing pinned yet</span>
            <p className="text-sm text-muted">Add companies to your watchlist and new-grad roles near you get pinned here automatically.</p>
            <button
              type="button"
              onClick={() => setWatchlistOpen(true)}
              className="rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-card uppercase transition hover:bg-sun-dark"
            >
              + Watch a company
            </button>
          </div>
        ) : recs.length === 0 ? (
          <div className="mx-auto max-w-sm rotate-1 rounded-lg bg-card px-6 py-6 text-center shadow-md">
            <span className="font-script text-2xl text-sun">No new pins right now</span>
            <p className="text-sm text-muted">I’ll keep checking your {board.companies.length} companies twice a day.</p>
          </div>
        ) : (
          <ul className="grid gap-5 sm:grid-cols-2">
            {visible.map((rec, i) => (
              <li key={rec.id} className={`pin-card relative flex flex-col gap-2 rounded-md bg-card px-4 pt-6 pb-4 shadow-md ${tilts[i % tilts.length]}`}>
                <span aria-hidden="true" className={`absolute top-2 left-1/2 h-3.5 w-3.5 -translate-x-1/2 rounded-full shadow ${pinColors[i % pinColors.length]}`} />
                <div className="flex items-start justify-between gap-2">
                  <span className="font-display text-lg leading-tight break-words uppercase">{rec.company}</span>
                  {regionLabel[rec.priority] && (
                    <span className="shrink-0 rounded-full bg-sand px-2 py-0.5 text-[11px] font-bold text-muted">{regionLabel[rec.priority]}</span>
                  )}
                </div>
                <a href={rec.url} target="_blank" rel="noreferrer" className="font-medium leading-snug underline-offset-4 hover:text-sun hover:underline">
                  {rec.title}
                </a>
                <span className="text-xs text-muted">
                  {rec.location}
                  {rec.postedAt ? ` · posted ${shortDate(rec.postedAt.slice(0, 10))}` : ''}
                </span>
                <div className="mt-1 flex gap-2">
                  <button
                    type="button"
                    disabled={busyId === rec.id}
                    onClick={() => handleAdd(rec)}
                    className="flex items-center gap-1.5 rounded-full bg-espresso px-3 py-2 text-xs font-bold text-whitewash transition hover:bg-espresso-soft active:scale-95 disabled:opacity-50"
                  >
                    Add to menu <span className="rounded-full bg-golden px-1.5 text-espresso">+5 XP</span>
                  </button>
                  <button
                    type="button"
                    disabled={busyId === rec.id}
                    onClick={() => handleToss(rec)}
                    className="rounded-full border border-line px-3 py-2 text-xs font-bold text-muted transition hover:border-espresso hover:text-espresso active:scale-95 disabled:opacity-50"
                  >
                    Toss
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}

        {recs.length > 6 && (
          <div className="mt-5 flex justify-center">
            <button
              type="button"
              onClick={() => setShowAll(!showAll)}
              className="rounded-full bg-card px-5 py-2 text-sm font-bold shadow transition hover:bg-whitewash"
            >
              {showAll ? 'Show fewer' : `Show all ${recs.length} pins`}
            </button>
          </div>
        )}
      </div>

      {watchlistOpen && board && (
        <WatchlistDialog
          token={token}
          companies={board.companies}
          onChanged={() => void load()}
          onClose={() => setWatchlistOpen(false)}
          onUnauthorized={onUnauthorized}
        />
      )}
    </section>
  )
}
