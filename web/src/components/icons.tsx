import type { ReactNode } from 'react'
import type { JobStatus } from '../api/types'

// Line-drawn coffee shop icons (stand-ins until we draw our own)

type IconProps = { className?: string }

function Line({ className, children, viewBox = '0 0 24 24' }: IconProps & { children: ReactNode; viewBox?: string }) {
  return (
    <svg
      className={className}
      viewBox={viewBox}
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {children}
    </svg>
  )
}

// Beans picked: a coffee bean
export function BeanIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <ellipse cx="12" cy="12" rx="6.5" ry="9" transform="rotate(30 12 12)" />
      <path d="M15.5 5.5c-3 2.5-1 6-3.5 8.5s-2.5 3.5-3.5 4.5" />
    </Line>
  )
}

// Brewing: a pour-over dripping into a cup
export function BrewIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M5 4h14l-4.5 6h-5z" />
      <path d="M12 10v2" />
      <path d="M12 14.5v.5" />
      <path d="M6 16h12l-1.5 5h-9z" />
    </Line>
  )
}

// Order up: a counter bell
export function BellIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M5 17a7 7 0 0 1 14 0" />
      <path d="M3 17h18" />
      <path d="M12 8V6" />
      <path d="M10 6h4" />
      <path d="M4 20h16" />
    </Line>
  )
}

// Taste test: a cup with a spoon
export function TasteIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M4 10h13v4a6 6 0 0 1-6 6h-1a6 6 0 0 1-6-6z" />
      <path d="M17 11.5h1.5a2.5 2.5 0 0 1 0 5H16" />
      <path d="M14 3l-3 9" />
      <ellipse cx="14.5" cy="3" rx="1.2" ry="0.8" transform="rotate(20 14.5 3)" />
    </Line>
  )
}

// Served: a cup on a saucer with a little sparkle
export function ServedIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M5 11h11v3a5 5 0 0 1-5 5h-1a5 5 0 0 1-5-5z" />
      <path d="M16 12h1.5a2 2 0 0 1 0 4H15.5" />
      <path d="M3 21h16" />
      <path d="M10 3v3M8.5 4.5h3" />
      <path d="M15 6v2M14 7h2" />
    </Line>
  )
}

// Spilled: a tipped-over cup and a puddle
export function SpilledIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M4 7l9-3 2.5 8-9 3z" />
      <path d="M8.2 14.5c1 2.5 4 3.5 7 2.5s4.5-1 5.8.5" />
      <path d="M3 20.5c3-1 6 1 9 0s6-1 9 0" />
    </Line>
  )
}

// Gone cold: an iced cup
export function ColdIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M6 7h12l-1.5 14h-9z" />
      <path d="M5 7h14" />
      <path d="M13 7l2-5" />
      <rect x="9" y="10" width="3" height="3" rx="0.5" />
      <rect x="12.5" y="13.5" width="3" height="3" rx="0.5" />
    </Line>
  )
}

// One icon per stage
export function StageIcon({ status, className }: IconProps & { status: JobStatus }) {
  switch (status) {
    case 'Saved':
      return <BeanIcon className={className} />
    case 'Applied':
      return <BrewIcon className={className} />
    case 'Interviewing':
      return <TasteIcon className={className} />
    case 'Offer':
      return <ServedIcon className={className} />
    case 'Rejected':
      return <SpilledIcon className={className} />
    case 'Ghosted':
      return <ColdIcon className={className} />
  }
}

export function ShackIcon({ className }: IconProps) {
  return (
    <Line className={className} viewBox="0 0 64 64">
      <path d="M8 44h40" strokeWidth="2.5" />
      <path d="M14 44V24h28v20" strokeWidth="2.5" />
      <path d="M10 24l18-12 18 12" strokeWidth="2.5" />
      <path d="M24 44V32h8v12" strokeWidth="2.5" />
      <path d="M50 44c2-8 2-18 0-28" strokeWidth="2.5" />
      <path d="M50 16c3 2 5 6 5 10" strokeWidth="2.5" />
    </Line>
  )
}

export function CupIcon({ className }: IconProps) {
  return (
    <Line className={className} viewBox="0 0 64 64">
      <path d="M16 26h28l-4 26H20z" strokeWidth="3" />
      <path d="M44 31c8 0 8 12 0 12" strokeWidth="3" />
      <path className="steam" d="M24 18c-2-4 2-5 0-9" strokeWidth="3" />
      <path className="steam" style={{ animationDelay: '0.4s' }} d="M31 18c-2-4 2-5 0-9" strokeWidth="3" />
      <path className="steam" style={{ animationDelay: '0.8s' }} d="M38 18c-2-4 2-5 0-9" strokeWidth="3" />
    </Line>
  )
}

export function WaveIcon({ className }: IconProps) {
  return (
    <Line className={className} viewBox="0 0 120 16">
      <path d="M2 10c8-6 12 6 20 0s12 6 20 0 12 6 20 0 12 6 20 0 12 6 20 0 12 6 16 2" strokeWidth="2.5" />
    </Line>
  )
}

// A surfboard leaning on the shop
export function SurfboardIcon({ className }: IconProps) {
  return (
    <Line className={className} viewBox="0 0 24 64">
      <path d="M12 2c6 10 7 34 4 56-1 3-7 3-8 0C5 36 6 12 12 2z" />
      <path d="M12 6v52" />
      <path d="M9 50l-4 6M15 50l4 6" />
    </Line>
  )
}

// A palm frond
export function PalmIcon({ className }: IconProps) {
  return (
    <Line className={className} viewBox="0 0 48 48">
      <path d="M24 46c0-12-1-22-4-30" />
      <path d="M20 16c-6-4-13-3-17 1 6-1 11 1 15 4" />
      <path d="M20 16c-2-6-7-10-13-10 5 2 8 6 10 10" />
      <path d="M20 16c3-6 9-9 15-8-5 1-9 4-12 8" />
      <path d="M20 16c6-2 13 0 17 5-6-2-11-2-15 0" />
    </Line>
  )
}

export function SearchIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <circle cx="11" cy="11" r="6.5" />
      <path d="M16 16l4.5 4.5" />
    </Line>
  )
}

export function ChevronIcon({ className }: IconProps) {
  return (
    <Line className={className}>
      <path d="M6 9l6 6 6-6" />
    </Line>
  )
}
