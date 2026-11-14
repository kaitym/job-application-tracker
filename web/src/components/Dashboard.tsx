import { useEffect, useState } from 'react'
import { ApiError, getJobs, getStats } from '../api/client'
import type { Job, Stats } from '../api/types'
import { BrewingLoader } from './BrewingLoader'
import { JobCard } from './JobCard'
import { LevelCard } from './LevelCard'
import { StatTiles } from './StatTiles'
import { WaveIcon } from './icons'

type Props = {
  token: string
  onLogout: () => void
}

// The main screen: level, numbers, and every job
export function Dashboard({ token, onLogout }: Props) {
  const [jobs, setJobs] = useState<Job[] | null>(null)
  const [stats, setStats] = useState<Stats | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    Promise.all([getJobs(token), getStats()])
      .then(([jobList, statNumbers]) => {
        if (cancelled) return
        // "Order up" jobs first, then newest
        setJobs([...jobList].sort((a, b) => Number(b.needsAction) - Number(a.needsAction) || b.id - a.id))
        setStats(statNumbers)
      })
      .catch((err) => {
        if (cancelled) return
        if (err instanceof ApiError && err.status === 401) onLogout()
        else setError('Couldn’t reach the shop. Is the API running?')
      })

    return () => {
      cancelled = true
    }
  }, [token, onLogout])

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-8 px-4 py-8 sm:px-6 sm:py-12">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1">
          <span className="font-sign text-xs font-bold uppercase tracking-[0.3em] text-muted">San Clemente, CA</span>
          <h1 className="font-display text-5xl leading-[0.9] text-sun sm:text-6xl">SUN &amp; WAVES</h1>
          <span className="font-script text-2xl sm:text-3xl">coffee &amp; job hunt club</span>
          <WaveIcon className="mt-1 h-4 w-40 text-sun" />
        </div>
        <button
          type="button"
          onClick={onLogout}
          className="rounded-full border border-line px-5 py-2.5 font-sign text-xs font-bold uppercase tracking-[0.2em] text-muted transition hover:border-espresso hover:text-espresso"
        >
          Close up
        </button>
      </header>

      {error && (
        <p role="alert" className="rounded-2xl bg-sun/10 px-5 py-4 font-medium text-sun-dark">
          {error}
        </p>
      )}

      {!error && (!jobs || !stats) && <BrewingLoader />}

      {jobs && stats && (
        <>
          <LevelCard stats={stats} />
          <StatTiles stats={stats} />

          <section aria-labelledby="menu-heading" className="flex flex-col gap-4">
            <h2 id="menu-heading" className="font-sign text-sm font-bold uppercase tracking-[0.25em]">
              On the menu · {jobs.length}
            </h2>

            {jobs.length === 0 ? (
              <div className="flex flex-col items-center gap-2 rounded-3xl border border-dashed border-line px-6 py-14 text-center">
                <span className="font-script text-3xl text-sun">No beans on the shelf yet</span>
                <p className="text-muted">Add your first application to start brewing.</p>
              </div>
            ) : (
              jobs.map((job) => <JobCard key={job.id} job={job} />)
            )}
          </section>
        </>
      )}
    </div>
  )
}
