import type { Stats } from '../api/types'
import { nextShopLevel, shopLevel, shopLevels } from '../lib/shop'
import { ShopScene } from './ShopScene'
import { ConeIcon } from './icons'

type Props = {
  stats: Stats
  celebrate: number | null
}

// Dev only: ?shop=7 previews the shop at another level
function previewLevel(): number | null {
  if (!import.meta.env.DEV) return null
  const value = Number(new URLSearchParams(window.location.search).get('shop'))
  return value >= 1 && value <= shopLevels.length ? value : null
}

// The shop picture, what's next, and every upgrade on the way
export function ShopCard({ stats, celebrate }: Props) {
  const current = shopLevel(stats.level)
  const next = nextShopLevel(stats.level)
  const toGo = stats.nextLevelXp ? Math.max(0, stats.nextLevelXp - stats.collectedXp) : 0
  const readyToLevel = stats.nextLevelXp !== null && stats.collectedXp + stats.pendingXp >= stats.nextLevelXp

  // How far into this level, collected vs waiting in the jar
  const span = stats.nextLevelXp ? stats.nextLevelXp - stats.levelStartXp : 1
  const collectedPct = stats.nextLevelXp ? ((stats.collectedXp - stats.levelStartXp) / span) * 100 : 100
  const progress = {
    collected: collectedPct,
    pending: stats.nextLevelXp ? Math.min(100 - collectedPct, (stats.pendingXp / span) * 100) : 0,
  }

  return (
    <section aria-labelledby="shop-heading" className="flex flex-col gap-4 overflow-hidden rounded-3xl border border-line bg-card">
      <div className="flex flex-wrap items-end justify-between gap-2 px-5 pt-5 sm:px-6">
        <div className="flex flex-col">
          <h2 id="shop-heading" className="font-sign text-xs font-bold tracking-[0.25em] text-muted uppercase">
            Your shop
          </h2>
          <span className="font-script text-3xl leading-tight text-sun">{current.title}</span>
        </div>
        <span className="text-sm text-muted">
          Level {stats.level} of {shopLevels.length}
        </span>
      </div>

      <div className="bg-whitewash px-2 pt-2">
        <ShopScene level={previewLevel() ?? stats.level} celebrate={celebrate} />
      </div>

      <div className="flex flex-col gap-4 px-5 pb-5 sm:px-6">
        {next ? (
          <div className="relative flex gap-4 overflow-hidden rounded-2xl border border-espresso/15 bg-sand px-4 py-4">
            <span aria-hidden="true" className="caution-strip absolute inset-x-0 top-0 h-1.5" />
            <ConeIcon className="mt-1 h-10 w-10 shrink-0" />
            <div className="flex min-w-0 grow flex-col gap-2">
              <span className="font-sign text-xs font-bold tracking-[0.2em] text-sun uppercase">
                Under construction · Level {next.level}
              </span>
              <span className="text-sm">
                <strong className="font-display text-base uppercase">{next.title}</strong>
                <span className="text-muted"> · {next.unlock}</span>
              </span>
              <div
                className="flex h-2.5 overflow-hidden rounded-full bg-line"
                role="progressbar"
                aria-label={`Progress toward ${next.title}`}
                aria-valuemin={stats.levelStartXp}
                aria-valuemax={stats.nextLevelXp ?? 0}
                aria-valuenow={stats.collectedXp}
              >
                <div className="h-2.5 bg-sun transition-[width] duration-700" style={{ width: `${progress.collected}%` }} />
                <div className="tip-stripes h-2.5 transition-[width] duration-700" style={{ width: `${progress.pending}%` }} />
              </div>
              <span className="text-xs text-muted">
                {readyToLevel ? (
                  <strong className="text-sun-dark">Enough tips in the jar. Collect them to finish building!</strong>
                ) : (
                  `${toGo} XP to go${stats.pendingXp > 0 ? ` (${stats.pendingXp} waiting in the tip jar)` : ''}`
                )}
              </span>
            </div>
          </div>
        ) : (
          <p className="rounded-2xl bg-golden/20 px-4 py-3 text-sm font-medium">The shop is complete. Beachfront café, sunset, your name on the roof. 🌅</p>
        )}

        <ol className="no-scrollbar -mx-1 flex gap-2 overflow-x-auto px-1 pb-1" aria-label="Shop upgrades">
          {shopLevels.map((step) => {
            const done = step.level <= stats.level
            const isNext = step.level === stats.level + 1
            return (
              <li
                key={step.level}
                title={`${step.title}: ${step.unlock}`}
                className={`flex shrink-0 flex-col items-center gap-1 rounded-2xl px-3 py-2 text-center text-xs ${
                  done ? 'bg-espresso text-whitewash' : isNext ? 'border border-dashed border-sun text-sun-dark' : 'border border-line text-muted'
                }`}
              >
                <span className="font-sign font-bold">Lv {step.level}</span>
                <span className="max-w-20 leading-tight">{step.title}</span>
                <span className="sr-only">{done ? 'unlocked' : 'locked'}</span>
              </li>
            )
          })}
        </ol>
      </div>
    </section>
  )
}
