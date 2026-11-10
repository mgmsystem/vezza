import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createObra, deleteObra, getObra, listObras, updateObra } from './obra-service'

export const obraKeys = { all: ['obras'] as const, detail: (id: number) => ['obras', id] as const }

export function useObras() { return useQuery({ queryKey: obraKeys.all, queryFn: listObras }) }
export function useObra(id: number) { return useQuery({ queryKey: obraKeys.detail(id), queryFn: () => getObra(id), enabled: id > 0 }) }

export function useCreateObra() {
  const client = useQueryClient()
  return useMutation({ mutationFn: createObra, onSuccess: () => client.invalidateQueries({ queryKey: obraKeys.all }) })
}

export function useUpdateObra() {
  const client = useQueryClient()
  return useMutation({ mutationFn: updateObra, onSuccess: (_, input) => { client.invalidateQueries({ queryKey: obraKeys.all }); client.invalidateQueries({ queryKey: obraKeys.detail(input.obraId) }) } })
}

export function useDeleteObra() {
  const client = useQueryClient()
  return useMutation({ mutationFn: deleteObra, onSuccess: () => client.invalidateQueries({ queryKey: obraKeys.all }) })
}
