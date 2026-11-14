import { useEffect, useRef, useState, type FormEvent } from 'react'
import { ApiError, createJob, previewLink } from '../api/client'
import type { JobStatus, NewJob } from '../api/types'
import { todayIso } from '../lib/format'
import { stageLabels, statusOptions } from '../lib/stages'

type Props = {
  token: string
  open: boolean
  onClose: () => void
  onAdded: () => void
  onUnauthorized: () => void
}

type FormState = {
  company: string
  role: string
  location: string
  status: JobStatus
  dateApplied: string
  deadline: string
  link: string
  description: string
  notes: string
  needsAction: boolean
}

const emptyForm = (): FormState => ({
  company: '',
  role: '',
  location: '',
  status: 'Applied',
  dateApplied: todayIso(),
  deadline: '',
  link: '',
  description: '',
  notes: '',
  needsAction: false,
})

const inputClass =
  'w-full rounded-xl border border-line bg-whitewash px-4 py-3 text-base outline-none focus:border-sun focus:ring-2 focus:ring-sun/30'
const labelClass = 'flex flex-col gap-1.5 text-sm font-medium'

// Add a job by hand, or paste a posting link to fill it in
export function AddJobDialog({ token, open, onClose, onAdded, onUnauthorized }: Props) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [pasteUrl, setPasteUrl] = useState('')
  const [reading, setReading] = useState(false)
  const [readMessage, setReadMessage] = useState<{ ok: boolean; text: string } | null>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Open and close the native dialog with the prop
  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  function update<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((current) => ({ ...current, [key]: value }))
  }

  function reset() {
    setForm(emptyForm())
    setPasteUrl('')
    setReadMessage(null)
    setError(null)
  }

  async function handleReadLink() {
    if (!pasteUrl.trim()) return
    setReading(true)
    setReadMessage(null)

    try {
      const preview = await previewLink(token, pasteUrl.trim())
      setForm((current) => ({
        ...current,
        link: preview.link,
        company: preview.company ?? current.company,
        role: preview.role ?? current.role,
        location: preview.location ?? current.location,
        description: preview.description ?? current.description,
      }))

      const found = [preview.company, preview.role, preview.location, preview.description].filter(Boolean).length
      setReadMessage(
        found > 0
          ? { ok: true, text: 'Filled in what I found. Give it a once-over.' }
          : { ok: false, text: 'That page didn’t share its details. Fill it in yourself.' },
      )
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) return onUnauthorized()
      const reason = err instanceof ApiError && err.message ? err.message.replace(/^"|"$/g, '') : 'Couldn’t read that page.'
      setReadMessage({ ok: false, text: `${reason} You can still fill it in yourself.` })
      setForm((current) => ({ ...current, link: current.link || pasteUrl.trim() }))
    } finally {
      setReading(false)
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(null)

    const job: NewJob = {
      company: form.company.trim(),
      role: form.role.trim(),
      location: form.location.trim(),
      status: form.status,
      dateApplied: form.status === 'Saved' ? null : form.dateApplied || null,
      deadline: form.deadline || null,
      link: form.link.trim() || null,
      description: form.description.trim() || null,
      notes: form.notes.trim() || null,
      needsAction: form.needsAction,
    }

    try {
      await createJob(token, job)
      reset()
      onAdded()
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) return onUnauthorized()
      setError('Couldn’t save that job. Check the company and role, then try again.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <dialog
      ref={dialogRef}
      onClose={onClose}
      aria-labelledby="add-job-title"
      className="m-auto w-[min(100%-2rem,40rem)] rounded-3xl border border-line bg-card p-0 text-espresso backdrop:bg-espresso/50"
    >
      <form onSubmit={handleSubmit} className="flex max-h-[85vh] flex-col gap-5 overflow-y-auto p-6 sm:p-8">
        <div className="flex items-start justify-between gap-4">
          <div className="flex flex-col gap-1">
            <h2 id="add-job-title" className="font-display text-3xl">NEW ORDER</h2>
            <p className="font-script text-2xl text-sun">fresh beans on the shelf</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close"
            className="rounded-full p-2 text-muted transition hover:bg-sand hover:text-espresso"
          >
            <svg className="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
              <path d="M6 6l12 12M18 6L6 18" />
            </svg>
          </button>
        </div>

        <section className="flex flex-col gap-2 rounded-2xl bg-sand p-4">
          <label htmlFor="paste-link" className="font-sign text-xs font-bold uppercase tracking-[0.2em]">
            Paste the posting link
          </label>
          <div className="flex flex-col gap-2 sm:flex-row">
            <input
              id="paste-link"
              type="url"
              inputMode="url"
              placeholder="https://…"
              value={pasteUrl}
              onChange={(e) => setPasteUrl(e.target.value)}
              className={inputClass}
            />
            <button
              type="button"
              onClick={handleReadLink}
              disabled={reading || !pasteUrl.trim()}
              className="shrink-0 rounded-full bg-espresso px-5 py-3 font-sign text-xs font-bold uppercase tracking-[0.2em] text-whitewash transition hover:bg-espresso-soft disabled:opacity-50"
            >
              {reading ? 'Reading…' : 'Fill it in'}
            </button>
          </div>
          {readMessage && (
            <p role="status" className={`text-sm ${readMessage.ok ? 'text-espresso' : 'text-sun-dark'}`}>
              {readMessage.text}
            </p>
          )}
        </section>

        <div className="grid gap-4 sm:grid-cols-2">
          <label className={labelClass}>
            Company *
            <input className={inputClass} value={form.company} onChange={(e) => update('company', e.target.value)} required />
          </label>
          <label className={labelClass}>
            Role *
            <input className={inputClass} value={form.role} onChange={(e) => update('role', e.target.value)} required />
          </label>
          <label className={labelClass}>
            Location
            <input className={inputClass} value={form.location} onChange={(e) => update('location', e.target.value)} placeholder="Irvine, CA" />
          </label>
          <label className={labelClass}>
            Stage
            <select className={inputClass} value={form.status} onChange={(e) => update('status', e.target.value as JobStatus)}>
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {stageLabels[status]} ({status})
                </option>
              ))}
            </select>
          </label>
          {form.status !== 'Saved' && (
            <label className={labelClass}>
              Date applied
              <input type="date" className={inputClass} value={form.dateApplied} onChange={(e) => update('dateApplied', e.target.value)} />
            </label>
          )}
          <label className={labelClass}>
            Deadline
            <input type="date" className={inputClass} value={form.deadline} onChange={(e) => update('deadline', e.target.value)} />
          </label>
        </div>

        <label className={labelClass}>
          Link
          <input type="url" className={inputClass} value={form.link} onChange={(e) => update('link', e.target.value)} />
        </label>

        <label className={labelClass}>
          Job description
          <textarea
            rows={5}
            className={inputClass}
            value={form.description}
            onChange={(e) => update('description', e.target.value)}
          />
        </label>

        <label className={labelClass}>
          My notes
          <textarea rows={2} className={inputClass} value={form.notes} onChange={(e) => update('notes', e.target.value)} />
        </label>

        <label className="flex items-center gap-3 text-sm font-medium">
          <input
            type="checkbox"
            checked={form.needsAction}
            onChange={(e) => update('needsAction', e.target.checked)}
            className="h-5 w-5 accent-sun"
          />
          They already responded (Order up!)
        </label>

        {error && (
          <p role="alert" className="rounded-xl bg-sun/10 px-4 py-3 text-sm font-medium text-sun-dark">
            {error}
          </p>
        )}

        <div className="flex flex-wrap justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-line px-5 py-3 font-sign text-xs font-bold uppercase tracking-[0.2em] text-muted transition hover:border-espresso hover:text-espresso"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={saving}
            className="rounded-full bg-sun px-6 py-3 font-sign text-xs font-bold uppercase tracking-[0.2em] text-card transition hover:bg-sun-dark disabled:opacity-60"
          >
            {saving ? 'Saving…' : 'Add to the menu'}
          </button>
        </div>
      </form>
    </dialog>
  )
}
