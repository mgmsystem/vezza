import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createOrcamento, deleteOrcamento, getOrcamento, listOrcamentos, updateOrcamento } from './orcamento-service'

export const orcamentoKeys = { all: ['orcamentos'] as const, detail: (id: number) => ['orcamentos', id] as const }
export function useOrcamentos() { return useQuery({ queryKey: orcamentoKeys.all, queryFn: listOrcamentos }) }
export function useOrcamento(id: number) { return useQuery({ queryKey: orcamentoKeys.detail(id), queryFn: () => getOrcamento(id), enabled: id > 0 }) }
export function useCreateOrcamento() { const client = useQueryClient(); return useMutation({ mutationFn: createOrcamento, onSuccess: () => client.invalidateQueries({ queryKey: orcamentoKeys.all }) }) }
export function useUpdateOrcamento() { const client = useQueryClient(); return useMutation({ mutationFn: updateOrcamento, onSuccess: (_, input) => { client.invalidateQueries({ queryKey: orcamentoKeys.all }); client.invalidateQueries({ queryKey: orcamentoKeys.detail(input.orcamentoId) }) } }) }
export function useDeleteOrcamento() { const client = useQueryClient(); return useMutation({ mutationFn: deleteOrcamento, onSuccess: () => client.invalidateQueries({ queryKey: orcamentoKeys.all }) }) }
