import type { Job } from '../api/types'
import { shortDate } from '../lib/format'
import { BellIcon } from './icons'
import { StagePipeline } from './StagePipeline'

// One application
export function JobCard({ job }: { job: Job }) {
  const applied = shortDate(job.dateApplied)
  const deadline = shortDate(job.deadline)

  return (
    <article className="flex flex-col gap-4 rounded-3xl border border-line bg-card p-5 sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex min-w-0 flex-col gap-1">
          <h3 className="font-display text-2xl break-words uppercase">{job.company}</h3>
          <p className="text-muted">
            {job.role} · {job.location}
          </p>
        </div>
        {job.needsAction && (
          <span className="flex items-center gap-2 rounded-full bg-sun px-3.5 py-2 font-sign text-xs font-bold uppercase tracking-[0.15em] text-card">
            <BellIcon className="h-4 w-4" />
            Order up
          </span>
        )}
      </div>

      <StagePipeline status={job.status} />

      {(applied || deadline || job.link) && (
        <div className="flex flex-wrap gap-x-6 gap-y-1 text-sm text-muted">
          {applied && (
            <span>
              <strong className="text-espresso">Applied</strong> {applied}
            </span>
          )}
          {deadline && (
            <span>
              <strong className="text-espresso">Deadline</strong> {deadline}
            </span>
          )}
          {job.link && (
            <a href={job.link} target="_blank" rel="noreferrer" className="font-medium text-sun underline-offset-4 hover:underline">
              View posting
            </a>
          )}
        </div>
      )}

      {job.notes && <p className="text-sm text-muted">{job.notes}</p>}

      {job.description && (
        <details className="group rounded-2xl bg-whitewash px-4 py-3">
          <summary className="cursor-pointer font-sign text-xs font-bold uppercase tracking-[0.2em] text-muted group-open:text-espresso">
            Job description
          </summary>
          <p className="mt-3 text-sm whitespace-pre-line text-muted">{job.description}</p>
        </details>
      )}
    </article>
  )
}
