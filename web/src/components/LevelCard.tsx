import { useState } from 'react'
import type { Stats } from '../api/types'
import { ShackIcon } from './icons'

type Props = {
  stats: Stats
  onCollect: () => Promise<{ collected: number; leveledUp: boolean } | null>
}

// Shop level, XP bar, and the tip jar to collect earned XP
export function LevelCard({ stats, onCollect }: Props) {
  const [collecting, setCollecting] = useState(false)
  const [celebration, setCelebration] = useState<{ collected: number; leveledUp: boolean } | null>(null)

  const span = stats.nextLevelXp ? stats.nextLevelXp - stats.levelStartXp : 1
  const collectedPct = stats.nextLevelXp ? ((stats.collectedXp - stats.levelStartXp) / span) * 100 : 100
  const pendingPct = stats.nextLevelXp ? Math.min(100 - collectedPct, (stats.pendingXp / span) * 100) : 0

  async function handleCollect() {
    setCollecting(true)
    const result = await onCollect()
    setCollecting(false)
    if (result && result.collected > 0) {
      setCelebration(result)
      window.setTimeout(() => setCelebration(null), 3200)
    }
  }

  return (
    <section aria-label="Shop level" className="relative flex flex-col gap-4 overflow-hidden rounded-2xl bg-espresso px-5 py-5 text-whitewash sm:px-6">
      <div className="flex items-center gap-5">
        <ShackIcon className={`h-14 w-14 shrink-0 text-golden ${celebration?.leveledUp ? 'level-bounce' : ''}`} />
        <div className="flex min-w-0 grow flex-col gap-2">
          <span className="font-script text-3xl leading-none">Level {stats.level}</span>
          <div
            className="flex h-3 overflow-hidden rounded-full bg-espresso-soft"
            role="progressbar"
            aria-valuemin={stats.levelStartXp}
            aria-valuemax={stats.nextLevelXp ?? stats.collectedXp}
            aria-valuenow={stats.collectedXp}
            aria-label="Collected XP toward next level"
          >
            <div className="h-3 bg-golden transition-[width] duration-700 ease-out" style={{ width: `${collectedPct}%` }} />
            <div className="tip-stripes h-3 transition-[width] duration-700 ease-out" style={{ width: `${pendingPct}%` }} />
          </div>
        </div>
        <span className="hidden shrink-0 font-sign text-xs font-bold uppercase tracking-[0.15em] text-line sm:block">
          {stats.nextLevelXp ? `${stats.collectedXp} / ${stats.nextLevelXp} XP` : `${stats.collectedXp} XP · max`}
        </span>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <span className="text-sm text-line">
          {stats.pendingXp > 0 ? (
            <>
              <strong className="text-golden">+{stats.pendingXp} XP</strong> in the tip jar
            </>
          ) : (
            'Tip jar’s empty. Go brew something.'
          )}
        </span>
        <button
          type="button"
          onClick={handleCollect}
          disabled={collecting || stats.pendingXp === 0}
          className={`rounded-full bg-golden px-5 py-2.5 font-sign text-xs font-bold uppercase tracking-[0.2em] text-espresso transition hover:bg-whitewash disabled:cursor-not-allowed disabled:opacity-40 ${stats.pendingXp > 0 ? 'tip-wiggle' : ''}`}
        >
          {collecting ? 'Counting tips…' : 'Collect tips'}
        </button>
      </div>

      {celebration && (
        <div aria-live="polite" className="pointer-events-none absolute inset-0 flex items-center justify-center bg-espresso/85">
          <div className="tip-pop flex flex-col items-center gap-1 text-center">
            <span className="font-display text-4xl text-golden">+{celebration.collected} XP</span>
            <span className="font-script text-3xl">
              {celebration.leveledUp ? `Level up! Welcome to level ${stats.level}` : 'Tips collected!'}
            </span>
          </div>
        </div>
      )}
    </section>
  )
}
