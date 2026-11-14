import type { CollectResponse, Job, JobLinkPreview, LoginResponse, NewJob, Stats } from './types'

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
