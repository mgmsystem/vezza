import { apiRequest } from '../../services/api-client'
import type { Obra, ObraInput, UpdateObraInput } from './obra-types'

export async function listObras() {
  const result = await apiRequest<Obra[]>('/v1/obras')
  return result.data ?? []
}

export async function getObra(obraId: number) {
  const result = await apiRequest<Obra>(`/v1/obras/${obraId}`)
  if (!result.data) throw new Error('A API não retornou os dados da obra.')
  return result.data
}

export async function createObra(input: ObraInput) {
  return (await apiRequest<Obra>('/v1/obras', { method: 'POST', body: JSON.stringify(input) })).data
}

export async function updateObra(input: UpdateObraInput) {
  return (await apiRequest<Obra>('/v1/obras', { method: 'PUT', body: JSON.stringify(input) })).data
}

export async function deleteObra(obraId: number) {
  return apiRequest<Obra>(`/v1/obras/${obraId}`, { method: 'DELETE' })
}
