import { apiRequest } from '../../services/api-client'
import type { Orcamento, OrcamentoPayload, UpdateOrcamentoPayload } from './orcamento-types'

export async function listOrcamentos() {
  const result = await apiRequest<Orcamento[]>('/v1/orcamentos')
  return result.data ?? []
}

export async function getOrcamento(id: number) {
  const result = await apiRequest<Orcamento>(`/v1/orcamentos/${id}`)
  if (!result.data) throw new Error('A API não retornou os dados do orçamento.')
  return result.data
}

export async function createOrcamento(input: OrcamentoPayload) {
  return (await apiRequest<Orcamento>('/v1/orcamentos', { method: 'POST', body: JSON.stringify(input) })).data
}

export async function updateOrcamento(input: UpdateOrcamentoPayload) {
  return (await apiRequest<Orcamento>('/v1/orcamentos', { method: 'PUT', body: JSON.stringify(input) })).data
}

export async function deleteOrcamento(id: number) {
  return apiRequest<Orcamento>(`/v1/orcamentos/${id}`, { method: 'DELETE' })
}
