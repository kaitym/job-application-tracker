import type { JobStatus } from '../api/types'
import type { ReactNode } from 'react'
import { sortLabels, type SortKey, type StageFilter } from '../lib/jobFilters'
import { stageLabels } from '../lib/stages'
import { BellIcon, SearchIcon, StageIcon } from './icons'

type Props = {
  search: string
  stage: StageFilter
  sort: SortKey
  counts: Record<StageFilter, number>
  onSearch: (value: string) => void
  onStage: (value: StageFilter) => void
  onSort: (value: SortKey) => void
}

const stageOrder: JobStatus[] = ['Saved', 'Applied', 'Interviewing', 'Offer', 'Rejected', 'Ghosted']

// Search box, stage chips, and sort menu for the job list
export function JobFilters({ search, stage, sort, counts, onSearch, onStage, onSort }: Props) {
  const chip = (value: StageFilter, label: string, icon: ReactNode) => {
    const active = stage === value
    const count = counts[value]
    if (value !== 'all' && count === 0 && !active) return null

    return (
      <button
        key={value}
        type="button"
        onClick={() => onStage(active && value !== 'all' ? 'all' : value)}
        aria-pressed={active}
        className={`flex shrink-0 items-center gap-1.5 rounded-full border px-3.5 py-2 text-sm font-medium transition active:scale-95 ${
          active ? 'border-espresso bg-espresso text-whitewash' : 'border-line bg-card text-muted hover:border-espresso hover:text-espresso'
        }`}
      >
        {icon}
        {label}
        <span className={`rounded-full px-1.5 text-xs font-bold ${active ? 'bg-espresso-soft' : 'bg-sand'}`}>{count}</span>
      </button>
    )
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-col gap-2 sm:flex-row">
        <label className="relative grow">
          <span className="sr-only">Search jobs</span>
          <SearchIcon className="pointer-events-none absolute top-1/2 left-3.5 h-5 w-5 -translate-y-1/2 text-muted" />
          <input
            type="search"
            value={search}
            onChange={(e) => onSearch(e.target.value)}
            placeholder="Search company, role, city…"
            className="w-full rounded-full border border-line bg-card py-2.5 pr-4 pl-11 outline-none focus:border-sun focus:ring-2 focus:ring-sun/30"
          />
        </label>
        <label className="flex items-center gap-2 text-sm text-muted">
          <span className="font-sign text-xs font-bold tracking-[0.15em] uppercase">Sort</span>
          <select
            value={sort}
            onChange={(e) => onSort(e.target.value as SortKey)}
            className="rounded-full border border-line bg-card px-4 py-2.5 text-espresso outline-none focus:border-sun focus:ring-2 focus:ring-sun/30"
          >
            {(Object.keys(sortLabels) as SortKey[]).map((key) => (
              <option key={key} value={key}>
                {sortLabels[key]}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="no-scrollbar -mx-1 flex gap-2 overflow-x-auto px-1 pb-1" role="group" aria-label="Filter by stage">
        {chip('all', 'Everything', null)}
        {chip('orderUp', 'Order up', <BellIcon className="h-4 w-4" />)}
        {stageOrder.map((status) => chip(status, stageLabels[status], <StageIcon status={status} className="h-4 w-4" />))}
      </div>
    </div>
  )
}
