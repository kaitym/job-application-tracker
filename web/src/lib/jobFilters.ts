import type { Job, JobStatus } from '../api/types'

export type SortKey = 'newest' | 'oldest' | 'company' | 'deadline' | 'stage'

export const sortLabels: Record<SortKey, string> = {
  newest: 'Newest first',
  oldest: 'Oldest first',
  company: 'Company A–Z',
  deadline: 'Deadline soonest',
  stage: 'Furthest along',
}

// "all", "orderUp", or one stage
export type StageFilter = 'all' | 'orderUp' | JobStatus

export type ListOptions = {
  search: string
  stage: StageFilter
  sort: SortKey
}

const stageRank: Record<JobStatus, number> = {
  Offer: 0,
  Interviewing: 1,
  Applied: 2,
  Saved: 3,
  Ghosted: 4,
  Rejected: 5,
}

// When the job was applied to (or added, for saved jobs)
function appliedTime(job: Job): number {
  return job.dateApplied ? Date.parse(job.dateApplied) : Number.MAX_SAFE_INTEGER - job.id
}

// Filter and sort the list, "Order up" jobs stay on top
export function filterAndSort(jobs: Job[], { search, stage, sort }: ListOptions): Job[] {
  const query = search.trim().toLowerCase()

  const matches = jobs.filter((job) => {
    if (stage === 'orderUp' && !job.needsAction) return false
    if (stage !== 'all' && stage !== 'orderUp' && job.status !== stage) return false
    if (!query) return true
    return [job.company, job.role, job.location, job.notes ?? ''].some((text) => text.toLowerCase().includes(query))
  })

  const compare = (a: Job, b: Job): number => {
    switch (sort) {
      case 'newest':
        return appliedTime(b) - appliedTime(a) || b.id - a.id
      case 'oldest':
        return appliedTime(a) - appliedTime(b) || a.id - b.id
      case 'company':
        return a.company.localeCompare(b.company)
      case 'deadline': {
        const aTime = a.deadline ? Date.parse(a.deadline) : Infinity
        const bTime = b.deadline ? Date.parse(b.deadline) : Infinity
        return aTime - bTime || a.company.localeCompare(b.company)
      }
      case 'stage':
        return stageRank[a.status] - stageRank[b.status] || a.company.localeCompare(b.company)
    }
  }

  return matches.sort((a, b) => Number(b.needsAction) - Number(a.needsAction) || compare(a, b))
}

// How many jobs are in each filter, for the chip counts
export function countByFilter(jobs: Job[]): Record<StageFilter, number> {
  const counts: Record<StageFilter, number> = {
    all: jobs.length,
    orderUp: 0,
    Saved: 0,
    Applied: 0,
    Interviewing: 0,
    Offer: 0,
    Rejected: 0,
    Ghosted: 0,
  }

  for (const job of jobs) {
    counts[job.status] += 1
    if (job.needsAction) counts.orderUp += 1
  }

  return counts
}
