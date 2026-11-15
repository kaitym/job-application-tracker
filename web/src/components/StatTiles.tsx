import type { Stats } from '../api/types'
import { BellIcon, BrewIcon, ServedIcon, TasteIcon } from './icons'

// The numbers at a glance
export function StatTiles({ stats }: { stats: Stats }) {
  const tiles = [
    { label: 'Brewed', value: stats.applications, hint: 'applications sent', Icon: BrewIcon },
    { label: 'Responses', value: stats.responses, hint: `${Math.round(stats.responseRate * 100)}% response rate`, Icon: ServedIcon },
    { label: 'Order up', value: stats.needsAction, hint: 'waiting on me', Icon: BellIcon },
    { label: 'Taste tests', value: stats.upcomingInterviews, hint: 'interviews coming up', Icon: TasteIcon },
  ]

  return (
    <section aria-label="Job search numbers" className="grid grid-cols-2 gap-3 sm:grid-cols-4">
      {tiles.map((tile) => (
        <div key={tile.label} className="flex flex-col gap-1 rounded-2xl border border-line bg-card px-4 py-4 transition hover:-translate-y-0.5 hover:border-espresso">
          <span className="flex items-center justify-between gap-2">
            <span className="font-sign text-[11px] font-bold uppercase tracking-[0.2em] text-muted">{tile.label}</span>
            <tile.Icon className="h-5 w-5 text-sun" />
          </span>
          <span className="font-display text-4xl leading-none">{tile.value}</span>
          <span className="text-xs text-muted">{tile.hint}</span>
        </div>
      ))}
    </section>
  )
}
