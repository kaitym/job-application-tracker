import type { Job, NewJob } from '../api/types'

// "2026-11-10" -> "Nov 10"
export function shortDate(value: string | null): string | null {
  if (!value) return null
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, month - 1, day).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

// Today as "2026-11-14" in local time
export function todayIso(): string {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

// Turn a job back into what PUT /jobs/{id} expects, with some changes

export function toRequest(job: Job, changes: Partial<NewJob> = {}): NewJob {
  return {
    company: job.company,
    role: job.role,
    location: job.location,
    status: job.status,
    dateApplied: job.dateApplied,
    link: job.link,
    notes: job.notes,
    deadline: job.deadline,
    needsAction: job.needsAction,
    description: job.description,
    ...changes,
  }
}

// "2026-11-12T..." -> "2d ago"
export function timeAgo(value: string | null): string | null {
  if (!value) return null
  const minutes = Math.round((Date.now() - Date.parse(value)) / 60000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.round(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.round(hours / 24)
  return days < 30 ? `${days}d ago` : `${Math.round(days / 30)}mo ago`
}
