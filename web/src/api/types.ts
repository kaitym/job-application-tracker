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
  description: string | null
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
  collectedXp: number
  pendingXp: number
  level: number
  levelStartXp: number
  nextLevelXp: number | null
}

export type LoginResponse = {
  token: string
  expiresAt: string
}

export type CollectResponse = {
  collected: number
  previousLevel: number
  stats: Stats
}

export type JobLinkPreview = {
  link: string
  company: string | null
  role: string | null
  location: string | null
  description: string | null
}

export type NewJob = {
  company: string
  role: string
  location: string
  status: JobStatus
  dateApplied: string | null
  link: string | null
  notes: string | null
  deadline: string | null
  needsAction: boolean
  description: string | null
}

export type JobBoard = 'Greenhouse' | 'Lever' | 'Ashby' | 'SmartRecruiters'

export type WatchedCompany = {
  id: number
  name: string
  board: JobBoard
  slug: string
  createdAt: string
  lastCheckedAt: string | null
  lastError: string | null
}

export type JobRec = {
  id: number
  watchedCompanyId: number
  externalId: string
  company: string
  title: string
  location: string
  url: string
  postedAt: string | null
  foundAt: string
  priority: number
  state: 'New' | 'Added' | 'Tossed'
  jobId: number | null
}

export type RecBoard = {
  recs: JobRec[]
  newCount: number
  lastRefreshedAt: string | null
  companies: WatchedCompany[]
}

export type RefreshResult = {
  ran: boolean
  checked: number
  added: number
  errors: string[]
}
