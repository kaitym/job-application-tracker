import type { CollectResponse, Job, JobLinkPreview, LoginResponse, NewJob, RecBoard, RefreshResult, Stats, WatchedCompany } from './types'

// Dev: Vite proxies /api to the C# API. Prod: set VITE_API_URL.
const API_URL = import.meta.env.VITE_API_URL ?? '/api'

// An HTTP error with its status code
export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

async function request<T>(path: string, token: string | null, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const response = await fetch(`${API_URL}${path}`, { ...init, headers })

  if (!response.ok) {
    const text = await response.text().catch(() => '')
    throw new ApiError(response.status, text || response.statusText)
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

export function login(username: string, password: string) {
  return request<LoginResponse>('/auth/login', null, {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
}

export function getJobs(token: string) {
  return request<Job[]>('/jobs', token)
}

export function getStats() {
  return request<Stats>('/stats', null)
}

export function createJob(token: string, job: NewJob) {
  return request<Job>('/jobs', token, { method: 'POST', body: JSON.stringify(job) })
}

export function previewLink(token: string, url: string) {
  return request<JobLinkPreview>('/jobs/preview-link', token, { method: 'POST', body: JSON.stringify({ url }) })
}

export function collectXp(token: string) {
  return request<CollectResponse>('/stats/collect', token, { method: 'POST' })
}

export function updateJob(token: string, id: number, job: NewJob) {
  return request<Job>(`/jobs/${id}`, token, { method: 'PUT', body: JSON.stringify(job) })
}

export function getRecs(token: string) {
  return request<RecBoard>('/recs', token)
}

export function refreshRecs(token: string, onlyIfStale: boolean) {
  return request<RefreshResult>(`/recs/refresh?onlyIfStale=${onlyIfStale}`, token, { method: 'POST' })
}

export function addRecToMenu(token: string, id: number) {
  return request<Job>(`/recs/${id}/add`, token, { method: 'POST' })
}

export function tossRec(token: string, id: number) {
  return request<void>(`/recs/${id}/toss`, token, { method: 'POST' })
}

export function addWatchedCompany(token: string, url: string, name: string | null) {
  return request<WatchedCompany>('/recs/companies', token, { method: 'POST', body: JSON.stringify({ url, name }) })
}

export function removeWatchedCompany(token: string, id: number) {
  return request<void>(`/recs/companies/${id}`, token, { method: 'DELETE' })
}
