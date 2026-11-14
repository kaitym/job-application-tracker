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
export const stageGuide: { name: string; meaning: string; xp: string }[] = [
  { name: 'Beans picked', meaning: 'Saved a job I want, haven’t applied yet', xp: '+5' },
  { name: 'Brewing', meaning: 'Applied, waiting to hear back', xp: '+15' },
  { name: 'Order up!', meaning: 'They responded, it’s my move', xp: '+25' },
  { name: 'Taste test', meaning: 'Interviewing', xp: '+50' },
  { name: 'Served', meaning: 'Got an offer', xp: '+200' },
  { name: 'Spilled', meaning: 'Rejected. It happens to every barista', xp: '+5' },
  { name: 'Gone cold', meaning: 'No reply in 30+ days (probably ghosted)', xp: '' },
]

export const statusOptions: JobStatus[] = ['Saved', 'Applied', 'Interviewing', 'Offer', 'Rejected', 'Ghosted']
