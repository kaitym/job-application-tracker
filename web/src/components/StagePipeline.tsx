import type { JobStatus } from '../api/types'
import { isEnded, pipeline, pipelineIndex, stageLabels } from '../lib/stages'

// Beans picked → Brewing → Taste test → Served, with the current step dark
export function StagePipeline({ status }: { status: JobStatus }) {
  const current = pipelineIndex(status)

  return (
    <ol className="grid grid-cols-2 gap-1.5 sm:grid-cols-4" aria-label={`Stage: ${stageLabels[status]}`}>
      {pipeline.map((stage, index) => {
        const isCurrent = index === current
        const isDone = current > index

        const style = isCurrent
          ? 'bg-espresso text-whitewash'
          : isDone
            ? 'bg-sand text-muted'
            : 'border border-dashed border-line text-muted'

        return (
          <li
            key={stage}
            aria-current={isCurrent ? 'step' : undefined}
            className={`rounded-lg py-2 text-center text-xs font-bold ${style}`}
          >
            {stageLabels[stage]}
          </li>
        )
      })}
      {isEnded(status) && (
        <li className="col-span-2 rounded-lg bg-sand py-2 text-center text-xs font-bold text-muted sm:col-span-4">
          {stageLabels[status]}
        </li>
      )}
    </ol>
  )
}
