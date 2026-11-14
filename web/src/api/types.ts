// Shapes the C# API sends back

export type JobStatus = 'Saved' | 'Applied' | 'Interviewing' | 'Offer' | 'Rejected' | 'Ghosted'

export type Job = {
  id: number
  company: string
  role: string
  location: string
  status: JobStatus
  dateApplied: string | null
  link: string | null
  notes: string | null
  deadline: string | null
  needsAction: boolean
  respondedAt: string | null
}

export type Stats = {
  totalJobs: number
  byStatus: Record<JobStatus, number>
  applications: number
  responses: number
  responseRate: number
  needsAction: number
  upcomingInterviews: number
  xp: number
  level: number
  nextLevelXp: number | null
}

export type LoginResponse = {
  token: string
  expiresAt: string
}
