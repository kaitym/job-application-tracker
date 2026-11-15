import type { Stats } from '../api/types'
import { nextShopLevel, shopLevel, shopLevels } from '../lib/shop'
import { ShopScene } from './ShopScene'

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
          <p className="rounded-2xl border border-dashed border-sun/50 bg-sun/5 px-4 py-3 text-sm">
            <span className="font-sign text-xs font-bold tracking-[0.2em] text-sun uppercase">Next up · Level {next.level}</span>
            <br />
            <strong>{next.title}:</strong> {next.unlock.toLowerCase()}.{' '}
            <span className="text-muted">
              {readyToLevel ? 'You have enough tips. Collect them to unlock it!' : `${toGo} XP to go.`}
            </span>
          </p>
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
