import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createOrcamentoItem, deleteOrcamentoItem, listOrcamentoItems, updateOrcamentoItem } from './orcamento-item-service'

export const orcamentoItemKeys = { all: ['orcamento-items'] as const }
export function useOrcamentoItems() { return useQuery({ queryKey: orcamentoItemKeys.all, queryFn: listOrcamentoItems }) }
export function useCreateOrcamentoItem() { const client = useQueryClient(); return useMutation({ mutationFn: createOrcamentoItem, onSuccess: () => client.invalidateQueries({ queryKey: orcamentoItemKeys.all }) }) }
export function useUpdateOrcamentoItem() { const client = useQueryClient(); return useMutation({ mutationFn: updateOrcamentoItem, onSuccess: () => client.invalidateQueries({ queryKey: orcamentoItemKeys.all }) }) }
export function useDeleteOrcamentoItem() { const client = useQueryClient(); return useMutation({ mutationFn: deleteOrcamentoItem, onSuccess: () => client.invalidateQueries({ queryKey: orcamentoItemKeys.all }) }) }
