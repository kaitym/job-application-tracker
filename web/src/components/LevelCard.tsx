import type { Stats } from '../api/types'
import { ShackIcon } from './icons'

// Shop level and XP progress
export function LevelCard({ stats }: { stats: Stats }) {
  const progress = stats.nextLevelXp ? Math.min(100, Math.round((stats.xp / stats.nextLevelXp) * 100)) : 100

  return (
    <section aria-label="Shop level" className="flex items-center gap-5 rounded-2xl bg-espresso px-6 py-5 text-whitewash">
      <ShackIcon className="h-14 w-14 shrink-0 text-golden" />
      <div className="flex min-w-0 grow flex-col gap-2">
        <span className="font-script text-3xl leading-none">Level {stats.level}</span>
        <div
          className="h-2.5 rounded-full bg-espresso-soft"
          role="progressbar"
          aria-valuemin={0}
          aria-valuemax={stats.nextLevelXp ?? stats.xp}
          aria-valuenow={stats.xp}
          aria-label="XP toward next level"
        >
          <div className="h-2.5 rounded-full bg-golden" style={{ width: `${progress}%` }} />
        </div>
      </div>
      <span className="shrink-0 font-sign text-xs font-bold uppercase tracking-[0.15em] text-line">
        {stats.nextLevelXp ? `${stats.xp} / ${stats.nextLevelXp} XP` : `${stats.xp} XP · max`}
      </span>
    </section>
  )
}
