import type { Orcamento } from '../orcamentos/orcamento-types'
import { parseApiDate } from '../../utils/date'

export interface SituationGroup { label: string; count: number }

export function commercialValueCents(orcamentos: Orcamento[]) {
  return orcamentos.reduce((total, item) => total + (Number.isFinite(item.valor) ? Math.round((item.valor ?? 0) * 100) : 0), 0)
}

export function groupBySituation(orcamentos: Orcamento[]): SituationGroup[] {
  const groups = new Map<string, number>()
  for (const item of orcamentos) {
    const label = item.situacao?.trim() || 'Não informada'
    groups.set(label, (groups.get(label) ?? 0) + 1)
  }
  return [...groups.entries()].map(([label, count]) => ({ label, count })).sort((a, b) => b.count - a.count || a.label.localeCompare(b.label, 'pt-BR'))
}

function validTimestamp(value?: string) {
  if (!value) return null
  return parseApiDate(value)?.getTime() ?? null
}

export function recentOrcamentos(orcamentos: Orcamento[], limit = 5) {
  return orcamentos.map((item, index) => ({ item, index, timestamp: validTimestamp(item.data) }))
    .sort((a, b) => {
      if (a.timestamp == null && b.timestamp == null) return a.index - b.index
      if (a.timestamp == null) return 1
      if (b.timestamp == null) return -1
      return b.timestamp - a.timestamp || a.index - b.index
    })
    .slice(0, limit)
    .map(({ item }) => item)
}
