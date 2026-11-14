import { CupIcon } from './icons'

// Shown while waiting on the API (a free host can take ~1 min to wake up)
export function BrewingLoader({ message = 'Warming up the espresso machine…' }: { message?: string }) {
  return (
    <div role="status" className="flex flex-col items-center gap-3 py-16 text-muted">
      <CupIcon className="h-16 w-16 text-espresso" />
      <p className="font-script text-3xl text-sun">{message}</p>
    </div>
  )
}
