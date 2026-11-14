// Line-drawn icons (stand-ins until we draw our own)

type IconProps = { className?: string }

export function BellIcon({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M6 16V11a6 6 0 0 1 12 0v5l2 2H4z" />
      <path d="M10 21h4" />
    </svg>
  )
}

export function ShackIcon({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 64 64" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M8 44h40" />
      <path d="M14 44V24h28v20" />
      <path d="M10 24l18-12 18 12" />
      <path d="M24 44V32h8v12" />
      <path d="M50 44c2-8 2-18 0-28" />
      <path d="M50 16c3 2 5 6 5 10" />
    </svg>
  )
}

export function CupIcon({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 64 64" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M16 26h28l-4 26H20z" />
      <path d="M44 31c8 0 8 12 0 12" />
      <path className="steam" d="M24 18c-2-4 2-5 0-9" />
      <path className="steam" style={{ animationDelay: '0.4s' }} d="M31 18c-2-4 2-5 0-9" />
      <path className="steam" style={{ animationDelay: '0.8s' }} d="M38 18c-2-4 2-5 0-9" />
    </svg>
  )
}

export function WaveIcon({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 120 16" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" aria-hidden="true">
      <path d="M2 10c8-6 12 6 20 0s12 6 20 0 12 6 20 0 12 6 20 0 12 6 20 0 12 6 16 2" />
    </svg>
  )
}
