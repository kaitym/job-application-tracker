import type { ReactNode } from 'react'
import { shopLevels } from '../lib/shop'

type Props = {
  level: number
  celebrate?: number | null // level whose piece just unlocked (pops in)
}

const ink = '#2b211b'
const cream = '#fffdf8'
const sand = '#efe7da'
const sun = '#a8391f'
const golden = '#e9a65a'
const sage = '#9db39a'

// The beach shop, built up one piece per level
export function ShopScene({ level, celebrate }: Props) {
  // Unlocked pieces show; the next one is a dashed ghost
  const piece = (unlockAt: number, children: ReactNode) => {
    if (unlockAt > level + 1) return null
    const ghost = unlockAt === level + 1
    const pop = celebrate === unlockAt
    return (
      <g key={unlockAt} className={ghost ? 'shop-ghost' : pop ? 'shop-pop' : undefined}>
        {children}
      </g>
    )
  }

  const unlocked = shopLevels.filter((s) => s.level <= level).map((s) => s.title.toLowerCase())

  return (
    <svg
      viewBox="0 0 400 225"
      role="img"
      aria-label={`Your shop so far: ${unlocked.join(', ')}`}
      className="h-auto w-full"
    >
      <g stroke={ink} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        {/* Level 10: the sun comes out */}
        {piece(
          10,
          <>
            <circle cx="42" cy="44" r="18" fill={golden} />
            <path d="M42 14v-6M42 80v-6M12 44H6M78 44h-6M21 23l-4-4M67 69l-4-4M21 65l-4 4M67 19l-4 4" strokeWidth="1.8" />
          </>,
        )}

        {/* Always: ocean and sand */}
        <path d="M0 140h400v25H0z" fill={sage} fillOpacity="0.45" stroke="none" />
        <path d="M0 146c10-5 20 5 30 0s20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0 20 5 30 0" fill="none" stroke={cream} strokeWidth="1.5" />
        <path d="M0 165h400v60H0z" fill={sand} stroke="none" />
        <path d="M0 165h400" />

        {/* Level 3: the shack */}
        {piece(
          3,
          <>
            <path d="M178 98h150v100H178z" fill={cream} />
            <path d="M168 98l85-38 85 38z" fill={ink} />
            <path d="M200 124h74v40h-74z" fill={ink} />
            <path d="M196 164h82" strokeWidth="3" />
            <path d="M202 171h70v13h-70z" fill={sand} />
            <text x="237" y="180.5" textAnchor="middle" fontSize="8" fontWeight="700" letterSpacing="1.2" fill={ink} stroke="none" fontFamily="Fraunces, serif">
              ORDER HERE
            </text>
            <path d="M292 132h26v66h-26z" fill={sand} />
            <circle cx="312" cy="166" r="1.6" fill={ink} />
          </>,
        )}

        {/* Level 8: striped awning over the window */}
        {piece(
          8,
          <>
            {[0, 1, 2, 3, 4, 5, 6, 7].map((i) => (
              <path key={i} d={`M${194 + i * 11} 112h11v8a5.5 5.5 0 0 1-11 0z`} fill={i % 2 === 0 ? sun : cream} />
            ))}
          </>,
        )}

        {/* Level 9: neon OPEN sign */}
        {piece(
          9,
          <text x="237" y="149" textAnchor="middle" fontSize="15" fontWeight="700" letterSpacing="2" fill={golden} stroke="none" fontFamily="'Bowlby One', sans-serif" className="neon">
            OPEN
          </text>,
        )}

        {/* Level 6: string lights along the roof */}
        {piece(
          6,
          <>
            <path d="M176 99q77 22 154 0" fill="none" strokeWidth="1.5" />
            {[192, 212, 232, 253, 274, 294, 314].map((x, i) => (
              <circle key={x} cx={x} cy={102 + [3, 7, 9, 10, 9, 7, 3][i]} r="3" fill={golden} strokeWidth="1.2" />
            ))}
          </>,
        )}

        {/* Level 10: name on the roof */}
        {piece(
          10,
          <>
            <path d="M214 54v-7M292 54v-7" />
            <path d="M188 28h130v19H188z" fill={sun} />
            <text x="253" y="41.5" textAnchor="middle" fontSize="9" letterSpacing="0.6" fill={cream} stroke="none" fontFamily="'Bowlby One', sans-serif">
              GROUNDS FOR HIRE
            </text>
          </>,
        )}

        {/* Level 4: surfboards leaning on the wall */}
        {piece(
          4,
          <>
            <path d="M336 200c-6-30-3-70 10-96 9 24 9 66 2 96z" fill={golden} />
            <path d="M352 200c-4-28 0-62 12-84 8 22 6 58-1 84z" fill={sun} />
            <path d="M367 200c-2-24 2-52 12-70 6 20 4 48-2 70z" fill={cream} />
          </>,
        )}

        {/* Level 5: palm and beach chairs */}
        {piece(
          5,
          <>
            <path d="M152 198c2-34 0-64-8-92" fill="none" strokeWidth="3" />
            <path d="M144 106c-12-10-28-10-38 0 12-2 24 0 34 6" fill={sage} />
            <path d="M144 106c-4-14-16-22-30-22 12 4 20 12 26 22" fill={sage} />
            <path d="M144 106c6-12 20-18 32-14-12 2-22 8-28 16" fill={sage} />
            <path d="M144 106c14-4 28 2 34 12-12-6-24-6-32-2" fill={sage} />
            <path d="M160 214l10-22h18l-6 14h-16" fill={cream} />
            <path d="M172 214l4-8M186 214l-4-8" />
            <path d="M120 214l10-22h18l-6 14h-16" fill={golden} fillOpacity="0.6" />
            <path d="M132 214l4-8M146 214l-4-8" />
          </>,
        )}

        {/* Level 1: coffee cart with umbrella and French press */}
        {piece(
          1,
          <>
            <path d="M72 158V96" />
            <path d="M34 106q38-34 76 0z" fill={sun} />
            <path d="M34 106q19 6 38 0 19 6 38 0" fill="none" />
            <path d="M30 158h84v34H30z" fill={cream} />
            <path d="M114 166h14" />
            <circle cx="46" cy="198" r="8" fill={cream} />
            <circle cx="98" cy="198" r="8" fill={cream} />
            <path d="M40 140h11v18H40z" fill={cream} />
            <path d="M45.5 134v6M42 134h7" />
          </>,
        )}

        {/* Level 2: espresso machine on the cart */}
        {piece(
          2,
          <>
            <path d="M80 136h26v22H80z" fill={sand} />
            <path d="M84 136v-5h18v5" fill="none" />
            <path d="M93 144v4" />
            <path d="M89 150h8v8h-8z" fill={cream} strokeWidth="1.5" />
          </>,
        )}

        {/* Level 7: shop dog napping in the sand */}
        {piece(
          7,
          <>
            <path d="M232 214c0-10 12-14 26-14s24 4 24 12c0 4-4 6-10 6h-34c-4 0-6-2-6-4z" fill={golden} fillOpacity="0.75" />
            <circle cx="286" cy="205" r="9" fill={golden} fillOpacity="0.75" />
            <path d="M282 197c-4 0-7 6-4 10" fill={sun} fillOpacity="0.6" />
            <path d="M289 205h3M285 206.5q1.5 1.5 3 0" strokeWidth="1.5" />
            <path d="M232 210c-6-2-10-6-10-10" fill="none" />
          </>,
        )}
      </g>
    </svg>
  )
}
