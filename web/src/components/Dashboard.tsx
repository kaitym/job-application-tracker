import { useCallback, useEffect, useState } from 'react'
import { ApiError, collectXp, getJobs, getStats } from '../api/client'
import type { Job, Stats } from '../api/types'
import { AddJobDialog } from './AddJobDialog'
import { BrewingLoader } from './BrewingLoader'
import { JobCard } from './JobCard'
import { LevelCard } from './LevelCard'
import { StageGuide } from './StageGuide'
import { StatTiles } from './StatTiles'
import { WaveIcon } from './icons'

type Props = {
  token: string
  onLogout: () => void
}

// "Order up" jobs first, then newest
function sortJobs(jobs: Job[]): Job[] {
  return [...jobs].sort((a, b) => Number(b.needsAction) - Number(a.needsAction) || b.id - a.id)
}

// The main screen: level, guide, numbers, and every job
export function Dashboard({ token, onLogout }: Props) {
  const [jobs, setJobs] = useState<Job[] | null>(null)
  const [stats, setStats] = useState<Stats | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [adding, setAdding] = useState(false)

  // Fetch jobs and stats, then hand them to whoever asked
  const load = useCallback(
    (isCancelled: () => boolean = () => false) =>
      Promise.all([getJobs(token), getStats()])
        .then(([jobList, statNumbers]) => {
          if (isCancelled()) return
          setJobs(sortJobs(jobList))
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

  async function handleCollect() {
    try {
      const result = await collectXp(token)
      setStats(result.stats)
      return { collected: result.collected, leveledUp: result.stats.level > result.previousLevel }
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) onLogout()
      return null
    }
  }

  function handleAdded() {
    setAdding(false)
    void load()
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-8 px-4 py-8 sm:px-6 sm:py-12">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="font-display text-5xl leading-[0.9] text-sun sm:text-6xl">SUN &amp; WAVES</h1>
          <span className="font-script text-2xl sm:text-3xl">coffee &amp; job hunt club</span>
          <WaveIcon className="mt-1 h-4 w-40 text-sun" />
        </div>
        <div className="flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => setAdding(true)}
            className="rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold uppercase tracking-[0.2em] text-card transition hover:bg-sun-dark"
          >
            + Add a job
          </button>
          <button
            type="button"
            onClick={onLogout}
            className="rounded-full border border-line px-5 py-2.5 font-sign text-xs font-bold uppercase tracking-[0.2em] text-muted transition hover:border-espresso hover:text-espresso"
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
          <StageGuide />
          <StatTiles stats={stats} />

          <section aria-labelledby="menu-heading" className="flex flex-col gap-4">
            <h2 id="menu-heading" className="font-sign text-sm font-bold uppercase tracking-[0.25em]">
              On the menu · {jobs.length}
            </h2>

            {jobs.length === 0 ? (
              <div className="flex flex-col items-center gap-3 rounded-3xl border border-dashed border-line px-6 py-14 text-center">
                <span className="font-script text-3xl text-sun">No beans on the shelf yet</span>
                <p className="text-muted">Add your first application to start brewing.</p>
                <button
                  type="button"
                  onClick={() => setAdding(true)}
                  className="mt-2 rounded-full bg-sun px-5 py-2.5 font-sign text-xs font-bold uppercase tracking-[0.2em] text-card transition hover:bg-sun-dark"
                >
                  + Add a job
                </button>
              </div>
            ) : (
              jobs.map((job) => <JobCard key={job.id} job={job} />)
            )}
          </section>
        </>
      )}

      <AddJobDialog
        token={token}
        open={adding}
        onClose={() => setAdding(false)}
        onAdded={handleAdded}
        onUnauthorized={onLogout}
      />
    </div>
  )
}
