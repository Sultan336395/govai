import type { ReactNode } from 'react'
import type { EligibilityVerdict } from '@/api/types'
import { verdictClass, verdictLabels } from '@/lib/format'

export function Loading({ label = 'Yükleniyor…' }: { label?: string }) {
  return <div className="state">{label}</div>
}

export function ErrorBox({ error }: { error: unknown }) {
  const message = error instanceof Error ? error.message : 'Beklenmeyen bir hata oluştu.'
  return <div className="error-box">{message}</div>
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <div className="state">{children}</div>
}

export function VerdictBadge({ verdict }: { verdict: EligibilityVerdict }) {
  return <span className={`badge ${verdictClass[verdict]}`}>{verdictLabels[verdict]}</span>
}

export function Kpi({
  label,
  value,
  hint,
}: {
  label: string
  value: ReactNode
  hint?: ReactNode
}) {
  return (
    <div className="card">
      <div className="kpi-label">{label}</div>
      <div className="kpi-value">{value}</div>
      {hint ? <div className="kpi-hint">{hint}</div> : null}
    </div>
  )
}

/** Skoru hem sayı hem görsel çubuk olarak gösterir; tarama sırasında hızlı karşılaştırma sağlar. */
export function ScoreCell({ score }: { score: number }) {
  return (
    <div>
      <div className="score">{score.toFixed(1)}</div>
      <div className="meter">
        <span style={{ width: `${Math.min(100, Math.max(0, score))}%` }} />
      </div>
    </div>
  )
}
