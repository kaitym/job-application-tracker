import type { JobStatus } from '../api/types'

// Coffee names for each real status (the API keeps the real ones)
export const stageLabels: Record<JobStatus, string> = {
  Saved: 'Beans picked',
  Applied: 'Brewing',
  Interviewing: 'Taste test',
  Offer: 'Served',
  Rejected: 'Spilled',
  Ghosted: 'Gone cold',
}

// The happy path, in order
export const pipeline: JobStatus[] = ['Saved', 'Applied', 'Interviewing', 'Offer']

// Where a job sits on the happy path (-1 if it fell off)
export function pipelineIndex(status: JobStatus): number {
  return pipeline.indexOf(status)
}

export function isEnded(status: JobStatus): boolean {
  return status === 'Rejected' || status === 'Ghosted'
}

// What each stage means, for the guide at the top
export const stageGuide: { name: string; meaning: string; xp: string; status: JobStatus | null }[] = [
  { name: 'Beans picked', meaning: 'Saved a job I want, haven’t applied yet', xp: '+5', status: 'Saved' },
  { name: 'Brewing', meaning: 'Applied, waiting to hear back', xp: '+15', status: 'Applied' },
  { name: 'Order up!', meaning: 'They responded, it’s my move', xp: '+25', status: null },
  { name: 'Taste test', meaning: 'Interviewing', xp: '+50', status: 'Interviewing' },
  { name: 'Served', meaning: 'Got an offer', xp: '+200', status: 'Offer' },
  { name: 'Spilled', meaning: 'Rejected. It happens to every barista', xp: '+5', status: 'Rejected' },
  { name: 'Gone cold', meaning: 'No reply in 30+ days (probably ghosted)', xp: '', status: 'Ghosted' },
]

export const statusOptions: JobStatus[] = ['Saved', 'Applied', 'Interviewing', 'Offer', 'Rejected', 'Ghosted']

// The next step on the happy path, and its XP
export const nextStage: Partial<Record<JobStatus, { status: JobStatus; xp: number }>> = {
  Saved: { status: 'Applied', xp: 15 },
  Applied: { status: 'Interviewing', xp: 50 },
  Interviewing: { status: 'Offer', xp: 200 },
}

// Background tint for each stage's icon bubble
export const stageTint: Record<JobStatus, string> = {
  Saved: 'bg-sand text-espresso',
  Applied: 'bg-golden/30 text-espresso',
  Interviewing: 'bg-sage/40 text-espresso',
  Offer: 'bg-sun text-card',
  Rejected: 'bg-line text-muted',
  Ghosted: 'bg-line text-muted',
}
