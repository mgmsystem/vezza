import { apiRequest } from '../../services/api-client'
import type { OrcamentoItem, OrcamentoItemPayload, UpdateOrcamentoItemPayload } from './orcamento-item-types'

export async function listOrcamentoItems() {
  const result = await apiRequest<OrcamentoItem[]>('/v1/orcamento-items')
  return result.data ?? []
}
export async function createOrcamentoItem(input: OrcamentoItemPayload) {
  return (await apiRequest<OrcamentoItem>('/v1/orcamento-items', { method: 'POST', body: JSON.stringify(input) })).data
}
export async function updateOrcamentoItem(input: UpdateOrcamentoItemPayload) {
  return (await apiRequest<OrcamentoItem>('/v1/orcamento-items', { method: 'PUT', body: JSON.stringify(input) })).data
}
export async function deleteOrcamentoItem(id: number) {
  return apiRequest<OrcamentoItem>(`/v1/orcamento-items/${id}`, { method: 'DELETE' })
}
