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
