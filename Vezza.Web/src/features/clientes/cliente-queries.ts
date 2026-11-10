import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createCliente, deleteCliente, getCliente, listClientes, updateCliente } from './cliente-service'

export const clienteKeys = {
  all: ['clientes'] as const,
  detail: (id: number) => ['clientes', id] as const,
}

export function useClientes() {
  return useQuery({ queryKey: clienteKeys.all, queryFn: listClientes })
}

export function useCliente(clienteId: number) {
  return useQuery({ queryKey: clienteKeys.detail(clienteId), queryFn: () => getCliente(clienteId), enabled: clienteId > 0 })
}

export function useCreateCliente() {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn: createCliente, onSuccess: () => queryClient.invalidateQueries({ queryKey: clienteKeys.all }) })
}

export function useUpdateCliente() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateCliente,
    onSuccess: (_, input) => {
      queryClient.invalidateQueries({ queryKey: clienteKeys.all })
      queryClient.invalidateQueries({ queryKey: clienteKeys.detail(input.clienteId) })
    },
  })
}

export function useDeleteCliente() {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn: deleteCliente, onSuccess: () => queryClient.invalidateQueries({ queryKey: clienteKeys.all }) })
}
