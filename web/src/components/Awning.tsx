// Striped café awning across the top of the page
export function Awning() {
  const stripes = 24

  return (
    <div aria-hidden="true" className="w-full overflow-hidden">
      <svg className="block h-12 w-full sm:h-16" viewBox={`0 0 ${stripes * 20} 40`} preserveAspectRatio="none">
        {Array.from({ length: stripes }, (_, i) => (
          <g key={i}>
            <rect x={i * 20} y="0" width="20" height="28" fill={i % 2 === 0 ? '#a8391f' : '#fffdf8'} />
            {/* Scalloped edge */}
            <path d={`M${i * 20} 28 a10 10 0 0 0 20 0z`} fill={i % 2 === 0 ? '#a8391f' : '#fffdf8'} />
          </g>
        ))}
        <rect x="0" y="0" width={stripes * 20} height="4" fill="#2b211b" />
      </svg>
    </div>
  )
}
