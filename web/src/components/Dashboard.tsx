import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { ApiError, collectXp, getJobs, getStats, updateJob } from '../api/client'
import type { Job, NewJob, Stats } from '../api/types'
import { toRequest } from '../lib/format'
import { countByFilter, filterAndSort, type SortKey, type StageFilter } from '../lib/jobFilters'
import { Awning } from './Awning'
import { BrewingLoader } from './BrewingLoader'
import { JobCard } from './JobCard'
import { JobFilters } from './JobFilters'
import { JobFormDialog } from './JobFormDialog'
import { LevelCard } from './LevelCard'
import { ShopCard } from './ShopCard'
import { StageGuide } from './StageGuide'
import { StatTiles } from './StatTiles'
import { Toast } from './Toast'
import { PalmIcon, SurfboardIcon, WaveIcon } from './icons'

type Props = {
  token: string
  onLogout: () => void
}

// Which job form is open: none, a new job, or editing one
type FormState = { mode: 'closed' } | { mode: 'add' } | { mode: 'edit'; job: Job }

// The main screen: level, guide, numbers, and the menu of jobs
export function Dashboard({ token, onLogout }: Props) {
  const [jobs, setJobs] = useState<Job[] | null>(null)
  const [stats, setStats] = useState<Stats | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState<FormState>({ mode: 'closed' })
  const [search, setSearch] = useState('')
  const [stage, setStage] = useState<StageFilter>('all')
  const [sort, setSort] = useState<SortKey>('newest')
  const [toast, setToast] = useState<string | null>(null)
  const [celebrateLevel, setCelebrateLevel] = useState<number | null>(null)
  const toastTimer = useRef<number | undefined>(undefined)

  const showToast = useCallback((message: string) => {
    setToast(message)
    window.clearTimeout(toastTimer.current)
    toastTimer.current = window.setTimeout(() => setToast(null), 3000)
  }, [])

  // Fetch jobs and stats
  const load = useCallback(
    (isCancelled: () => boolean = () => false) =>
      Promise.all([getJobs(token), getStats()])
        .then(([jobList, statNumbers]) => {
          if (isCancelled()) return
          setJobs(jobList)
          setStats(statNumbers)
          setError(null)
        })
        .catch((err) => {
          if (isCancelled()) return
          if (err instanceof ApiError && err.status === 401) onLogout()
          else setError('Couldn’t reach the shop. Is the API running?')
        }),
    [token, onLogout],
  )

  useEffect(() => {
    let cancelled = false
    void load(() => cancelled)
    return () => {
      cancelled = true
    }
  }, [load])

  const visibleJobs = useMemo(() => (jobs ? filterAndSort(jobs, { search, stage, sort }) : []), [jobs, search, stage, sort])
  const counts = useMemo(() => countByFilter(jobs ?? []), [jobs])

  // Refresh the numbers and say how many tips a change earned
  async function refreshStats(message: string) {
    const before = stats?.pendingXp ?? 0
    try {
      const fresh = await getStats()
      setStats(fresh)
      const earned = fresh.pendingXp - before
      showToast(earned > 0 ? `${message} · +${earned} XP in the tip jar` : message)
    } catch {
      showToast(message)
    }
  }

  async function handleQuickUpdate(job: Job, changes: Partial<NewJob>, message: string) {
    try {
      const saved = await updateJob(token, job.id, toRequest(job, changes))
      setJobs((current) => current?.map((j) => (j.id === saved.id ? saved : j)) ?? null)
      await refreshStats(message)
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) return onLogout()
      showToast('Couldn’t save that. Try again?')
    }
  }

  async function handleSaved(saved: Job, wasNew: boolean) {
    setForm({ mode: 'closed' })
    setJobs((current) => {
      if (!current) return [saved]
      return wasNew ? [saved, ...current] : current.map((j) => (j.id === saved.id ? saved : j))
    })
    await refreshStats(wasNew ? `${saved.company} added to the menu` : `${saved.company} updated`)
  }

  async function handleCollect() {
    try {
      const result = await collectXp(token)
      setStats(result.stats)
      const leveledUp = result.stats.level > result.previousLevel
      if (leveledUp) {
        setCelebrateLevel(result.stats.level)
        window.setTimeout(() => setCelebrateLevel(null), 4000)
      }
      return { collected: result.collected, leveledUp }
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) onLogout()
      return null
    }
  }

  const filtering = search.trim() !== '' || stage !== 'all'

  return (
    <div className="flex min-h-screen flex-col">
      <Awning />

      <div className="mx-auto flex w-full max-w-3xl flex-col gap-7 px-4 py-8 sm:px-6 sm:py-10">
        <header className="flex flex-wrap items-end justify-between gap-4">
          <div className="flex items-end gap-3">
            <SurfboardIcon className="hidden h-24 w-8 shrink-0 -rotate-6 text-golden sm:block" />
            <div className="flex flex-col gap-1">
              <h1 className="font-display text-5xl leading-[0.9] text-sun sm:text-6xl">
                GROUNDS
                <br /> FOR HIRE
              </h1>
              <span className="font-script text-2xl sm:text-3xl">coffee &amp; job hunt club</span>
              <WaveIcon className="mt-1 h-4 w-40 text-sun" />
            </div>
            <PalmIcon className="hidden h-16 w-16 shrink-0 text-sage sm:block" />
          </div>
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={() => setForm({ mode: 'add' })}
              className="rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-card uppercase transition hover:bg-sun-dark active:scale-95"
            >
              + Add a job
            </button>
            <button
              type="button"
              onClick={onLogout}
              className="rounded-full border border-line px-5 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-muted uppercase transition hover:border-espresso hover:text-espresso"
            >
              Close up
            </button>
          </div>
        </header>

        {error && (
          <p role="alert" className="rounded-2xl bg-sun/10 px-5 py-4 font-medium text-sun-dark">
            {error}
          </p>
        )}

        {!error && (!jobs || !stats) && <BrewingLoader />}

        {jobs && stats && (
          <>
            <LevelCard stats={stats} onCollect={handleCollect} />
            <ShopCard stats={stats} celebrate={celebrateLevel} />
            <StatTiles stats={stats} />
            <StageGuide />

            <section aria-labelledby="menu-heading" className="flex flex-col gap-4">
              <div className="flex items-center justify-between gap-4 rounded-2xl bg-espresso px-5 py-4 text-whitewash">
                <h2 id="menu-heading" className="font-sign text-sm font-bold tracking-[0.3em] uppercase">
                  Today’s menu
                </h2>
                <span className="font-script text-2xl text-golden">
                  {filtering ? `${visibleJobs.length} of ${jobs.length}` : `${jobs.length} on the board`}
                </span>
              </div>

              {jobs.length > 0 && (
                <JobFilters
                  search={search}
                  stage={stage}
                  sort={sort}
                  counts={counts}
                  onSearch={setSearch}
                  onStage={setStage}
                  onSort={setSort}
                />
              )}

              {jobs.length === 0 ? (
                <div className="flex flex-col items-center gap-3 rounded-3xl border border-dashed border-line px-6 py-14 text-center">
                  <span className="font-script text-3xl text-sun">No beans on the shelf yet</span>
                  <p className="text-muted">Add your first application to start brewing.</p>
                  <button
                    type="button"
                    onClick={() => setForm({ mode: 'add' })}
                    className="mt-2 rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-card uppercase transition hover:bg-sun-dark"
                  >
                    + Add a job
                  </button>
                </div>
              ) : visibleJobs.length === 0 ? (
                <div className="flex flex-col items-center gap-2 rounded-3xl border border-dashed border-line px-6 py-10 text-center">
                  <span className="font-script text-2xl text-sun">Nothing like that on the menu</span>
                  <button
                    type="button"
                    onClick={() => {
                      setSearch('')
                      setStage('all')
                    }}
                    className="text-sm font-medium text-sun underline-offset-4 hover:underline"
                  >
                    Clear filters
                  </button>
                </div>
              ) : (
                <ul className="flex flex-col gap-3">
                  {visibleJobs.map((job) => (
                    <li key={job.id}>
                      <JobCard job={job} onEdit={(j) => setForm({ mode: 'edit', job: j })} onQuickUpdate={handleQuickUpdate} />
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
        )}
      </div>

      {form.mode !== 'closed' && (
        <JobFormDialog
          key={form.mode === 'edit' ? form.job.id : 'new'}
          token={token}
          editing={form.mode === 'edit' ? form.job : null}
          onClose={() => setForm({ mode: 'closed' })}
          onSaved={handleSaved}
          onUnauthorized={onLogout}
        />
      )}

      <Toast message={toast} />
    </div>
  )
}
