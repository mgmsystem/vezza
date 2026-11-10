import { apiRequest } from '../../services/api-client'
import type { Cliente, ClienteInput, UpdateClienteInput } from './cliente-types'

export async function listClientes() {
  const result = await apiRequest<Cliente[]>('/v1/clientes')
  return result.data ?? []
}

export async function getCliente(clienteId: number) {
  const result = await apiRequest<Cliente>(`/v1/clientes/${clienteId}`)
  if (!result.data) throw new Error('A API não retornou os dados do cliente.')
  return result.data
}

export async function createCliente(input: ClienteInput) {
  const result = await apiRequest<Cliente>('/v1/clientes', { method: 'POST', body: JSON.stringify(input) })
  return result.data
}

export async function updateCliente(input: UpdateClienteInput) {
  const result = await apiRequest<Cliente>('/v1/clientes', { method: 'PUT', body: JSON.stringify(input) })
  return result.data
}

export async function deleteCliente(clienteId: number) {
  return apiRequest<Cliente>(`/v1/clientes/${clienteId}`, { method: 'DELETE' })
}
