import { useId, useState } from 'react'
import type { Job, NewJob } from '../api/types'
import { shortDate, todayIso } from '../lib/format'
import { isEnded, nextStage, stageLabels, stageTint } from '../lib/stages'
import { BellIcon, ChevronIcon, SpilledIcon, StageIcon } from './icons'
import { StagePipeline } from './StagePipeline'

type Props = {
  job: Job
  onEdit: (job: Job) => void
  onQuickUpdate: (job: Job, changes: Partial<NewJob>, message: string) => Promise<void>
}

// One application: a compact row that opens up for details and quick actions
export function JobCard({ job, onEdit, onQuickUpdate }: Props) {
  const [open, setOpen] = useState(false)
  const [busy, setBusy] = useState(false)
  const detailsId = useId()

  const applied = shortDate(job.dateApplied)
  const deadline = shortDate(job.deadline)
  const next = nextStage[job.status]

  async function quick(changes: Partial<NewJob>, message: string) {
    setBusy(true)
    await onQuickUpdate(job, changes, message)
    setBusy(false)
  }

  return (
    <article
      className={`rounded-3xl border bg-card transition hover:-translate-y-0.5 hover:shadow-[0_6px_0_0_rgba(43,33,27,0.08)] ${
        job.needsAction ? 'border-sun' : 'border-line'
      }`}
    >
      <h3>
        <button
          type="button"
          onClick={() => setOpen(!open)}
          aria-expanded={open}
          aria-controls={detailsId}
          className="flex w-full items-center gap-4 px-4 py-4 text-left sm:px-5"
        >
          <span className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${stageTint[job.status]}`}>
            <StageIcon status={job.status} className="h-7 w-7" />
          </span>
          <span className="flex min-w-0 grow flex-col">
            <span className="font-display text-xl leading-tight break-words uppercase">{job.company}</span>
            <span className="truncate text-sm text-muted">
              {job.role}
              {job.location ? ` · ${job.location}` : ''}
            </span>
          </span>
          <span className="hidden shrink-0 flex-col items-end gap-1 sm:flex">
            {job.needsAction ? (
              <span className="flex items-center gap-1.5 rounded-full bg-sun px-2.5 py-1 font-sign text-[11px] font-bold tracking-[0.15em] text-card uppercase">
                <BellIcon className="bell-ring h-3.5 w-3.5" />
                Order up
              </span>
            ) : (
              <span className="font-sign text-[11px] font-bold tracking-[0.15em] text-muted uppercase">{stageLabels[job.status]}</span>
            )}
            {applied && <span className="text-xs text-muted">{applied}</span>}
          </span>
          {job.needsAction && <BellIcon className="bell-ring h-5 w-5 shrink-0 text-sun sm:hidden" />}
          <ChevronIcon className={`h-5 w-5 shrink-0 text-muted transition-transform duration-300 ${open ? 'rotate-180' : ''}`} />
        </button>
      </h3>

      <div id={detailsId} className={`grid transition-[grid-template-rows] duration-300 ease-out ${open ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]'}`}>
        <div className="overflow-hidden" inert={!open}>
          <div className="flex flex-col gap-4 border-t border-dashed border-line px-4 pt-4 pb-5 sm:px-5">
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
                <summary className="cursor-pointer font-sign text-xs font-bold tracking-[0.2em] text-muted uppercase group-open:text-espresso">
                  Job description
                </summary>
                <p className="mt-3 text-sm whitespace-pre-line text-muted">{job.description}</p>
              </details>
            )}

            <div className="flex flex-wrap gap-2">
              {next && (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() =>
                    quick(
                      {
                        status: next.status,
                        dateApplied: job.dateApplied ?? (next.status === 'Applied' ? todayIso() : null),
                      },
                      `${job.company} moved to ${stageLabels[next.status]}`,
                    )
                  }
                  className="flex items-center gap-2 rounded-full bg-espresso px-4 py-2.5 text-sm font-bold text-whitewash transition hover:bg-espresso-soft active:scale-95 disabled:opacity-50"
                >
                  <StageIcon status={next.status} className="h-4 w-4" />
                  Move to {stageLabels[next.status]}
                  <span className="rounded-full bg-golden px-2 text-xs text-espresso">up to +{next.xp} XP</span>
                </button>
              )}

              {!isEnded(job.status) && (
                <button
                  type="button"
                  disabled={busy}
                  aria-pressed={job.needsAction}
                  onClick={() =>
                    quick(
                      { needsAction: !job.needsAction },
                      job.needsAction ? `${job.company}: handled it` : `${job.company}: order up! They wrote back`,
                    )
                  }
                  className={`flex items-center gap-2 rounded-full border px-4 py-2.5 text-sm font-bold transition active:scale-95 disabled:opacity-50 ${
                    job.needsAction ? 'border-sun bg-sun/10 text-sun-dark' : 'border-line text-muted hover:border-espresso hover:text-espresso'
                  }`}
                >
                  <BellIcon className="h-4 w-4" />
                  {job.needsAction ? 'Handled it' : 'They wrote back'}
                </button>
              )}

              {!isEnded(job.status) && (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => quick({ status: 'Rejected', needsAction: false }, `${job.company} spilled. Onto the next pour`)}
                  className="flex items-center gap-2 rounded-full border border-line px-4 py-2.5 text-sm font-bold text-muted transition hover:border-espresso hover:text-espresso active:scale-95 disabled:opacity-50"
                >
                  <SpilledIcon className="h-4 w-4" />
                  Spilled
                </button>
              )}

              <button
                type="button"
                onClick={() => onEdit(job)}
                className="ml-auto rounded-full border border-line px-4 py-2.5 font-sign text-xs font-bold tracking-[0.2em] text-muted uppercase transition hover:border-espresso hover:text-espresso active:scale-95"
              >
                Edit
              </button>
            </div>
          </div>
        </div>
      </div>
    </article>
  )
}
