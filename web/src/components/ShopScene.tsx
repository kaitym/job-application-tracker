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
const sunDark = '#7e2a16'
const golden = '#e9a65a'
const sage = '#9db39a'
const leaf = '#6f8f6a'
const wood = '#c9965f'
const woodDark = '#8a5a2e'

// Where each piece sits, for the "under construction" tape and cone
const areas: Record<number, { x: number; y: number; w: number; h: number }> = {
  1: { x: 40, y: 92, w: 130, h: 150 },
  2: { x: 104, y: 136, w: 48, h: 40 },
  3: { x: 205, y: 70, w: 200, h: 172 },
  4: { x: 400, y: 104, w: 64, h: 140 },
  5: { x: 150, y: 80, w: 70, h: 180 },
  6: { x: 214, y: 120, w: 182, h: 34 },
  7: { x: 286, y: 236, w: 84, h: 30 },
  8: { x: 238, y: 146, w: 104, h: 24 },
  9: { x: 246, y: 170, w: 88, h: 30 },
  10: { x: 245, y: 32, w: 150, h: 34 },
}

// The beach shop: unlocked pieces in full color, the rest greyed out,
// and the next one under construction
export function ShopScene({ level, celebrate }: Props) {
  const next = level < shopLevels.length ? level + 1 : null

  const piece = (unlockAt: number, children: ReactNode, key?: string) => {
    const unlocked = unlockAt <= level
    const pop = unlocked && celebrate === unlockAt
    return (
      <g
        key={key ?? unlockAt}
        className={pop ? 'shop-pop' : undefined}
        filter={unlocked ? 'url(#lift)' : 'url(#locked)'}
        opacity={unlocked ? 1 : 0.38}
      >
        {children}
      </g>
    )
  }

  const unlockedNames = shopLevels.filter((s) => s.level <= level).map((s) => s.title.toLowerCase())

  return (
    <svg
      viewBox="0 0 480 270"
      role="img"
      aria-label={`Your shop so far: ${unlockedNames.join(', ')}. ${next ? `Under construction: ${shopLevels[next - 1].title.toLowerCase()}.` : 'Complete!'}`}
      className="h-auto w-full"
    >
      <defs>
        <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#fbf6ec" />
          <stop offset="1" stopColor="#f6e2c2" />
        </linearGradient>
        <linearGradient id="ocean" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#a9c9bf" />
          <stop offset="1" stopColor="#86ad9f" />
        </linearGradient>
        <linearGradient id="sandFill" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#f2e6cf" />
          <stop offset="1" stopColor="#e3cfa8" />
        </linearGradient>
        <pattern id="grain" width="14" height="10" patternUnits="userSpaceOnUse">
          <circle cx="2" cy="3" r="0.8" fill={woodDark} opacity="0.25" />
          <circle cx="9" cy="7" r="0.7" fill={woodDark} opacity="0.2" />
          <circle cx="12" cy="2" r="0.5" fill={cream} opacity="0.7" />
        </pattern>
        <pattern id="planks" width="96" height="10" patternUnits="userSpaceOnUse">
          <rect width="96" height="10" fill={wood} />
          <path d="M0 9.5h96M30 0v10M74 0v10" stroke={woodDark} strokeWidth="1" opacity="0.55" />
          <path d="M8 4c6 1 10-1 16 0M50 6c5-1 9 1 14 0" stroke={woodDark} strokeWidth="0.6" opacity="0.35" fill="none" />
        </pattern>
        <pattern id="siding" width="180" height="11" patternUnits="userSpaceOnUse">
          <rect width="180" height="11" fill={cream} />
          <rect y="8.5" width="180" height="2.5" fill="#e4d7c2" />
        </pattern>
        <pattern id="shingles" width="16" height="10" patternUnits="userSpaceOnUse">
          <rect width="16" height="10" fill="#3a2d25" />
          <path d="M0 10a8 8 0 0 1 16 0" fill="#4a3a30" stroke="#22190f" strokeWidth="0.8" />
        </pattern>
        <pattern id="caution" width="12" height="12" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
          <rect width="12" height="12" fill={golden} />
          <rect width="6" height="12" fill={ink} />
        </pattern>
        <linearGradient id="glass" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0" stopColor="#ffffff" stopOpacity="0.9" />
          <stop offset="0.5" stopColor="#dfeae6" stopOpacity="0.7" />
          <stop offset="1" stopColor="#ffffff" stopOpacity="0.9" />
        </linearGradient>
        <linearGradient id="chrome" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0" stopColor="#8f8a86" />
          <stop offset="0.35" stopColor="#f3f1ee" />
          <stop offset="0.7" stopColor="#b9b4ae" />
          <stop offset="1" stopColor="#7a746f" />
        </linearGradient>
        <linearGradient id="interior" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#3b2c22" />
          <stop offset="1" stopColor="#1f1712" />
        </linearGradient>
        <linearGradient id="trunk" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0" stopColor="#7d5634" />
          <stop offset="1" stopColor="#a67c52" />
        </linearGradient>
        <radialGradient id="bulbGlow">
          <stop offset="0" stopColor={golden} stopOpacity="0.9" />
          <stop offset="1" stopColor={golden} stopOpacity="0" />
        </radialGradient>
        <radialGradient id="sunGlow">
          <stop offset="0.55" stopColor="#f3c27a" />
          <stop offset="1" stopColor="#f3c27a" stopOpacity="0" />
        </radialGradient>
        <filter id="lift" x="-10%" y="-10%" width="120%" height="130%">
          <feDropShadow dx="0" dy="2.5" stdDeviation="1.6" floodColor={ink} floodOpacity="0.22" />
        </filter>
        <filter id="locked">
          <feColorMatrix type="saturate" values="0" />
        </filter>
        <filter id="neonGlow" x="-30%" y="-60%" width="160%" height="220%">
          <feGaussianBlur stdDeviation="2.4" result="blur" />
          <feMerge>
            <feMergeNode in="blur" />
            <feMergeNode in="blur" />
            <feMergeNode in="SourceGraphic" />
          </feMerge>
        </filter>
      </defs>

      {/* Sky, clouds, ocean, sand */}
      <rect width="480" height="270" fill="url(#sky)" />
      {piece(
        10,
        <>
          <circle cx="62" cy="52" r="34" fill="url(#sunGlow)" />
          <circle cx="62" cy="52" r="20" fill={golden} stroke={ink} strokeWidth="2" />
          <path d="M62 22v-8M62 90v-8M32 52h-8M100 52h-8M41 31l-6-6M89 79l-6-6M41 73l-6 6M89 25l-6 6" stroke={ink} strokeWidth="2" strokeLinecap="round" />
          <path d="M150 40q6-6 12 0q6-6 12 0M182 58q5-5 10 0q5-5 10 0" fill="none" stroke={ink} strokeWidth="1.8" strokeLinecap="round" />
        </>,
        'sun',
      )}
      <g fill={cream} opacity="0.85">
        <path d="M330 52c0-8 10-12 16-7 3-9 18-9 21 0 9-2 14 6 10 12h-47z" />
        <path d="M200 30c0-6 8-9 12-5 2-7 14-7 16 0 7-1 10 5 8 9h-36z" opacity="0.7" />
      </g>
      <rect y="146" width="480" height="44" fill="url(#ocean)" />
      <path
        d="M0 156c12-5 24 5 36 0s24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0 24 5 36 0"
        fill="none"
        stroke={cream}
        strokeWidth="1.6"
        opacity="0.8"
      />
      <path d="M20 172h22M120 166h14M300 176h20M420 168h18" stroke={cream} strokeWidth="1.4" strokeLinecap="round" opacity="0.7" />
      <path d="M0 186c60-6 120-4 180-2s160 6 300-2V270H0z" fill="url(#sandFill)" />
      <path d="M0 186c60-6 120-4 180-2s160 6 300-2V270H0z" fill="url(#grain)" />
      <path d="M0 186c60-6 120-4 180-2s160 6 300-2" fill="none" stroke={cream} strokeWidth="2.5" opacity="0.9" />

      <g fill="none" stroke={ink} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        {/* Level 5 (back): palm tree */}
        {piece(
          5,
          <>
            <ellipse cx="186" cy="243" rx="20" ry="4" fill={ink} opacity="0.15" stroke="none" />
            <path d="M178 242c4-40 2-90-10-150l9-1c13 58 14 110 11 151z" fill="url(#trunk)" />
            {[110, 128, 146, 164, 182, 200, 218, 232].map((y, i) => (
              <path key={y} d={`M${171 + i * 1.3} ${y}q6 3 11 0`} fill="none" strokeWidth="1.2" />
            ))}
            <path d="M172 92c-14-14-38-16-52-4 16-2 34 2 48 10z" fill={leaf} />
            <path d="M172 92c-6-20-24-30-42-28 16 6 28 16 38 30z" fill={sage} />
            <path d="M174 92c8-18 28-24 44-18-16 2-30 10-40 22z" fill={leaf} />
            <path d="M174 92c18-6 38 2 46 16-16-8-32-10-44-6z" fill={sage} />
            <path d="M173 94c-4 14-14 24-26 30 6-12 14-22 22-32z" fill={leaf} />
            <path d="M174 94c8 10 12 22 10 34-4-12-8-22-14-32z" fill={sage} />
            <circle cx="170" cy="96" r="4" fill={woodDark} />
            <circle cx="178" cy="97" r="4" fill={woodDark} />
          </>,
          'palm',
        )}

        {/* Level 3: the shack */}
        {piece(
          3,
          <>
            <path d="M215 242h182l26 10H240z" fill={ink} opacity="0.13" stroke="none" />
            <rect x="215" y="120" width="182" height="122" fill="url(#siding)" />
            <path d="M200 124l106-54 106 54z" fill="url(#shingles)" />
            <path d="M200 124l106-54 106 54" fill="none" strokeWidth="2.5" />
            <rect x="198" y="122" width="216" height="7" rx="2" fill={ink} />
            <path d="M215 129h182" stroke="#d8c9b0" strokeWidth="5" opacity="0.6" />
            <rect x="245" y="160" width="90" height="48" rx="2" fill="url(#interior)" />
            <path d="M252 190h76" stroke={woodDark} strokeWidth="2" />
            <path d="M258 190v-6h6v6M270 190v-8h7v8M300 190v-6h6v6M314 190v-9h8v9" fill={golden} strokeWidth="1.2" />
            <rect x="238" y="206" width="104" height="8" rx="2" fill="url(#planks)" />
            <rect x="256" y="219" width="68" height="15" rx="2" fill={sand} />
            <path d="M262 214v5M318 214v5" strokeWidth="1.5" />
            <text x="290" y="230" textAnchor="middle" fontSize="8.5" fontWeight="700" letterSpacing="1.4" fill={ink} stroke="none" fontFamily="Fraunces, serif">
              ORDER HERE
            </text>
            <rect x="356" y="166" width="30" height="76" rx="2" fill={sand} />
            <rect x="361" y="172" width="20" height="28" rx="1" fill="none" strokeWidth="1.4" />
            <rect x="361" y="206" width="20" height="28" rx="1" fill="none" strokeWidth="1.4" />
            <circle cx="381" cy="204" r="2" fill={golden} />
            <path d="M226 242v-14h14v14" fill={sun} strokeWidth="1.6" />
            <path d="M233 228c-4-8-2-14 0-16 2 4 4 10 0 16zM233 228c-8-4-10-10-8-14 4 2 8 8 8 14zM233 228c6-6 12-6 14-4-2 4-8 6-14 4z" fill={leaf} strokeWidth="1.2" />
          </>,
          'shack',
        )}

        {/* Level 8: striped awning */}
        {piece(
          8,
          <>
            <path d="M240 148h100l-4 10h-92z" fill={sunDark} />
            {[0, 1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
              <path key={i} d={`M${242 + i * 10.6} 156h10.6v6a5.3 5.3 0 0 1-10.6 0z`} fill={i % 2 === 0 ? sun : cream} strokeWidth="1.5" />
            ))}
            <path d="M244 160h92" stroke={ink} strokeWidth="3" opacity="0.15" />
          </>,
          'awning',
        )}

        {/* Level 9: neon OPEN sign */}
        {piece(
          9,
          <g filter="url(#neonGlow)">
            <rect x="262" y="168" width="56" height="18" rx="9" fill="none" stroke={golden} strokeWidth="1.8" />
            <text x="290" y="182" textAnchor="middle" fontSize="12" letterSpacing="2" fill="#ffd79a" stroke="none" fontFamily="'Bowlby One', sans-serif">
              OPEN
            </text>
          </g>,
          'neon',
        )}

        {/* Level 6: string lights */}
        {piece(
          6,
          <>
            <path d="M214 130q91 26 182 0" fill="none" strokeWidth="1.4" />
            {[232, 254, 276, 298, 320, 342, 364, 384].map((x, i) => {
              const y = 134 + [3, 7, 10, 11, 11, 10, 7, 3][i]
              return (
                <g key={x}>
                  <circle cx={x} cy={y + 2} r="8" fill="url(#bulbGlow)" stroke="none" />
                  <path d={`M${x} ${y - 3}v2`} strokeWidth="1.2" />
                  <ellipse cx={x} cy={y + 2} rx="3" ry="4" fill="#ffd79a" strokeWidth="1.2" />
                </g>
              )
            })}
          </>,
          'lights',
        )}

        {/* Level 10: name on the roof */}
        {piece(
          10,
          <>
            <path d="M270 70v-8M340 70v-8" strokeWidth="2.5" />
            <rect x="246" y="36" width="148" height="26" rx="4" fill={sun} />
            <rect x="250" y="40" width="140" height="18" rx="2" fill="none" stroke={cream} strokeWidth="1" opacity="0.7" />
            <text x="320" y="53.5" textAnchor="middle" fontSize="11" letterSpacing="0.8" fill={cream} stroke="none" fontFamily="'Bowlby One', sans-serif">
              GROUNDS FOR HIRE
            </text>
          </>,
          'roofSign',
        )}

        {/* Level 4: surfboards */}
        {piece(
          4,
          <>
            <path d="M398 243h64l10 6h-70z" fill={ink} opacity="0.14" stroke="none" />
            <path d="M406 242c-8-36-4-90 14-130 12 32 12 92 3 130z" fill={golden} />
            <path d="M409 180c4 1 10 1 14 0" stroke={sun} strokeWidth="5" />
            <path d="M414 236c-2-34 0-80 6-118" fill="none" strokeWidth="1" opacity="0.5" />
            <path d="M426 242c-5-34 0-76 16-110 10 30 8 80 0 110z" fill={sun} />
            <path d="M430 196c5 1 12 1 16-1" stroke={cream} strokeWidth="4" />
            <path d="M445 242c-3-28 3-62 14-88 7 26 5 64-2 88z" fill={cream} />
            <path d="M448 212c4 1 9 0 12-2" stroke={sage} strokeWidth="4" />
          </>,
          'boards',
        )}

        {/* Level 1: coffee cart */}
        {piece(
          1,
          <>
            <ellipse cx="102" cy="246" rx="62" ry="6" fill={ink} opacity="0.16" stroke="none" />
            <path d="M102 176V100" strokeWidth="3" />
            <path d="M44 112q58-48 116 0z" fill={sun} />
            <path d="M102 76l-36 36h14zM102 76l22 36h14z" fill={cream} strokeWidth="1.5" />
            <path d="M44 112q7.25 8 14.5 0t14.5 0 14.5 0 14.5 0 14.5 0 14.5 0 14.5 0 14.5 0" fill={sun} />
            <circle cx="102" cy="75" r="3" fill={golden} />
            <rect x="52" y="174" width="102" height="8" rx="2" fill={cream} />
            <rect x="54" y="182" width="98" height="46" rx="3" fill="url(#planks)" />
            <rect x="68" y="192" width="44" height="26" rx="2" fill={ink} />
            <text x="90" y="210" textAnchor="middle" fontSize="13" fill={cream} stroke="none" fontFamily="Yellowtail, cursive">
              coffee
            </text>
            <path d="M152 196h20v-8" strokeWidth="2.5" />
            {[74, 132].map((cx) => (
              <g key={cx}>
                <circle cx={cx} cy="234" r="12" fill={cream} strokeWidth="2.5" />
                <path d={`M${cx - 9} 234h18M${cx} 225v18M${cx - 6} 228l12 12M${cx + 6} 228l-12 12`} strokeWidth="1" opacity="0.6" />
                <circle cx={cx} cy="234" r="3" fill={golden} strokeWidth="1.5" />
              </g>
            ))}
            <rect x="62" y="150" width="15" height="24" rx="2" fill="url(#glass)" />
            <rect x="63" y="162" width="13" height="11" fill="#6b4226" stroke="none" opacity="0.85" />
            <path d="M60 150h19M69.5 150v-7M65 143h9" strokeWidth="1.6" />
            <path d="M77 155c5 0 5 12 0 12" fill="none" strokeWidth="1.6" />
            <path d="M65 153v16" stroke={cream} strokeWidth="1.5" opacity="0.9" />
            <path d="M84 166h10l-1 8h-8zM86 158h10l-1 8h-8z" fill={cream} strokeWidth="1.4" />
          </>,
          'cart',
        )}

        {/* Level 2: espresso machine */}
        {piece(
          2,
          <>
            <rect x="108" y="144" width="40" height="30" rx="4" fill="url(#chrome)" />
            <rect x="106" y="139" width="44" height="6" rx="2" fill={ink} />
            <circle cx="120" cy="153" r="4" fill={cream} strokeWidth="1.4" />
            <path d="M120 153l2-2" strokeWidth="1" />
            <rect x="130" y="160" width="12" height="4" rx="1" fill={ink} />
            <path d="M142 162h8" strokeWidth="2.5" />
            <path d="M132 166h8l-1 6h-6z" fill={cream} strokeWidth="1.3" />
            <path className="steam" d="M136 134c-2-4 2-5 0-9" fill="none" strokeWidth="1.4" opacity="0.6" />
            <path className="steam" style={{ animationDelay: '0.5s' }} d="M142 134c-2-4 2-5 0-9" fill="none" strokeWidth="1.4" opacity="0.6" />
          </>,
          'espresso',
        )}

        {/* Level 5 (front): beach chairs */}
        {piece(
          5,
          <>
            <ellipse cx="190" cy="262" rx="34" ry="3" fill={ink} opacity="0.14" stroke="none" />
            <path d="M160 262l12-28h20l-8 18h-18" fill="none" strokeWidth="2" />
            <path d="M172 234h20l-8 18h-18z" fill={sun} strokeWidth="1.6" />
            <path d="M176 234l-8 18M182 234l-8 18M188 234l-8 18" stroke={cream} strokeWidth="2.2" />
            <path d="M196 262l12-28h20l-8 18h-18" fill="none" strokeWidth="2" />
            <path d="M208 234h20l-8 18h-18z" fill={sage} strokeWidth="1.6" />
            <path d="M212 234l-8 18M218 234l-8 18M224 234l-8 18" stroke={cream} strokeWidth="2.2" />
          </>,
          'chairs',
        )}

        {/* Level 7: shop dog */}
        {piece(
          7,
          <>
            <ellipse cx="326" cy="264" rx="40" ry="4" fill={ink} opacity="0.15" stroke="none" />
            <path d="M292 262c0-14 16-20 34-20s30 6 30 16c0 5-5 6-12 6h-46c-4 0-6-1-6-2z" fill={golden} />
            <path d="M306 246c6-2 14-2 18 2-6 4-14 4-18-2z" fill={wood} stroke="none" />
            <circle cx="358" cy="250" r="11" fill={golden} />
            <path d="M352 241c-6 0-10 8-6 14 3-2 6-8 6-14z" fill={woodDark} />
            <path d="M361 250h4M356 254q2 2 4 0" strokeWidth="1.5" />
            <circle cx="368" cy="253" r="1.6" fill={ink} stroke="none" />
            <path d="M292 258c-8-2-12-8-10-14 4 2 6 6 10 8" fill={golden} />
            <path d="M372 266h18l-2-6h-14z" fill={sage} strokeWidth="1.5" />
          </>,
          'dog',
        )}

        {/* Under construction: tape, cone, and sign over the next piece */}
        {next && <Construction level={next} />}
      </g>
    </svg>
  )
}

// Caution tape across the next piece, a cone, and a "LV n" sign
function Construction({ level }: { level: number }) {
  const a = areas[level]
  const cx = a.x + a.w / 2
  const cy = a.y + a.h / 2
  const coneX = Math.min(a.x + a.w + 4, 462)
  const coneY = Math.min(a.y + a.h + 2, 262)

  return (
    <g className="construction">
      <g transform={`rotate(-8 ${cx} ${cy})`}>
        <rect x={a.x - 12} y={cy - 6} width={a.w + 24} height="12" fill="url(#caution)" strokeWidth="1.2" />
      </g>
      <g transform={`rotate(6 ${cx} ${cy + 14})`}>
        <rect x={a.x - 6} y={cy + 8} width={a.w + 12} height="9" fill="url(#caution)" strokeWidth="1" opacity="0.9" />
      </g>
      <path d={`M${coneX - 9} ${coneY}l6-22h6l6 22z`} fill={sun} />
      <path d={`M${coneX - 6.5} ${coneY - 9}h13`} stroke={cream} strokeWidth="3" />
      <rect x={coneX - 12} y={coneY - 1} width="24" height="4" rx="1" fill={sun} strokeWidth="1.5" />
      <g transform={`translate(${Math.max(a.x - 4, 6)} ${Math.max(a.y - 18, 4)})`}>
        <rect width="46" height="16" rx="3" fill={ink} stroke="none" />
        <text x="23" y="11.5" textAnchor="middle" fontSize="8" letterSpacing="1" fill={golden} stroke="none" fontFamily="'Bowlby One', sans-serif">
          LV {level}
        </text>
      </g>
    </g>
  )
}
