import { useState } from 'react'
import { stageGuide } from '../lib/stages'

const KEY = 'sun-waves-guide-open'

function loadOpen(): boolean {
  try {
    return localStorage.getItem(KEY) !== 'false'
  } catch {
    return true
  }
}

// "How the shop works": what each stage means and what it's worth
export function StageGuide() {
  const [open, setOpen] = useState(loadOpen)

  function toggle() {
    const next = !open
    setOpen(next)
    try {
      localStorage.setItem(KEY, String(next))
    } catch {
      // Not saved, that's fine
    }
  }

  return (
    <section className="rounded-3xl border border-line bg-card">
      <h2>
        <button
          type="button"
          onClick={toggle}
          aria-expanded={open}
          aria-controls="stage-guide"
          className="flex w-full items-center justify-between gap-4 px-5 py-4 text-left sm:px-6"
        >
          <span className="font-sign text-sm font-bold uppercase tracking-[0.25em]">How the shop works</span>
          <svg
            className={`h-5 w-5 text-muted transition-transform ${open ? 'rotate-180' : ''}`}
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            aria-hidden="true"
          >
            <path d="M6 9l6 6 6-6" />
          </svg>
        </button>
      </h2>

      {open && (
        <div id="stage-guide" className="flex flex-col gap-4 px-5 pb-5 sm:px-6 sm:pb-6">
          <p className="text-sm text-muted">
            Every application moves through the shop like a cup of coffee. Moving forward earns tips (XP); collect them to level up the shop.
          </p>
          <dl className="grid gap-x-6 gap-y-3 sm:grid-cols-2">
            {stageGuide.map((stage) => (
              <div key={stage.name} className="flex items-baseline justify-between gap-3 border-b border-dashed border-line pb-2">
                <div className="flex flex-col">
                  <dt className="font-bold">{stage.name}</dt>
                  <dd className="text-sm text-muted">{stage.meaning}</dd>
                </div>
                {stage.xp && <span className="shrink-0 font-sign text-xs font-bold tracking-[0.1em] text-sun">{stage.xp} XP</span>}
              </div>
            ))}
          </dl>
          <p className="text-xs text-muted">Each job earns each step once, so no farming tips by flipping statuses.</p>
        </div>
      )}
    </section>
  )
}
