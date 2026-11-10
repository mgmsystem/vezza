import { useMemo, useState } from 'react'
import { Icon } from '../../components/Icon'
import { ConfirmDialog } from '../../components/feedback/ConfirmDialog'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { formatCurrency } from '../orcamentos/formatters'
import { useCreateOrcamentoItem, useDeleteOrcamentoItem, useOrcamentoItems, useUpdateOrcamentoItem } from './orcamento-item-queries'
import type { OrcamentoItem, OrcamentoItemInput } from './orcamento-item-types'
import { emptyOrcamentoItemInput } from './orcamento-item-types'
import { sumItemCents, toCents } from './money'
import { OrcamentoItemForm } from './OrcamentoItemForm'

export function OrcamentoItemsSection({ orcamentoId, valorOrcamento }: { orcamentoId: number; valorOrcamento?: number }) {
  const query = useOrcamentoItems()
  const createMutation = useCreateOrcamentoItem()
  const updateMutation = useUpdateOrcamentoItem()
  const deleteMutation = useDeleteOrcamentoItem()
  const [editing, setEditing] = useState<OrcamentoItem | 'new' | null>(null)
  const [deleting, setDeleting] = useState<OrcamentoItem | null>(null)
  const items = useMemo(() => (query.data ?? []).filter((item) => item.orcamentoId === orcamentoId), [query.data, orcamentoId])
  const itemCents = sumItemCents(items.map((item) => item.valor))
  const differenceCents = toCents(valorOrcamento) - itemCents
  const activeMutation = editing === 'new' ? createMutation : updateMutation
  function closeForm() { createMutation.reset(); updateMutation.reset(); setEditing(null) }
  function save(values: OrcamentoItemInput) {
    const payload = { orcamentoId, descricao: values.descricao.trim(), valor: Number(values.valor) }
    if (editing === 'new') createMutation.mutate(payload, { onSuccess: closeForm })
    else if (editing) updateMutation.mutate({ ...payload, orcamentoItemId: editing.orcamentoItemId }, { onSuccess: closeForm })
  }
  const initialValues = editing && editing !== 'new' ? { descricao: editing.descricao, valor: String(editing.valor ?? '') } : emptyOrcamentoItemInput
  return <section className="related-list-card budget-items-section" aria-labelledby="budget-items-title">
    <div className="related-list-heading"><div><span className="eyebrow">Composição</span><h2 id="budget-items-title">Itens do orçamento</h2><p>Valores descritivos vinculados a este orçamento.</p></div><button className="solid-button" onClick={() => { createMutation.reset(); setEditing('new') }} disabled={editing !== null}><Icon name="plus" size={17} />Adicionar item</button></div>
    <div className="item-totals" aria-label="Resumo dos valores"><div><span>Valor do orçamento</span><strong>{formatCurrency(valorOrcamento)}</strong></div><div><span>Soma dos itens</span><strong>{formatCurrency(itemCents / 100)}</strong></div><div><span>Diferença informativa</span><strong className={differenceCents < 0 ? 'negative-value' : ''}>{formatCurrency(differenceCents / 100)}</strong></div></div>
    <p className="item-total-note">A soma dos itens é informativa e não altera o valor comercial do orçamento.</p>
    {editing && <div className="item-form-shell"><h3>{editing === 'new' ? 'Novo item' : 'Editar item'}</h3><OrcamentoItemForm key={editing === 'new' ? 'new' : editing.orcamentoItemId} initialValues={initialValues} submitLabel={editing === 'new' ? 'Adicionar item' : 'Salvar alterações'} busy={activeMutation.isPending} serverError={activeMutation.error?.message} onSubmit={save} onCancel={closeForm} /></div>}
    {query.isLoading ? <LoadingState label="Carregando itens…" /> : query.error ? <ErrorState message={query.error.message} onRetry={() => query.refetch()} /> : items.length === 0 ? <div className="related-empty"><span className="related-item-icon"><Icon name="budgets" /></span><div><h3>Nenhum item cadastrado</h3><p>Adicione o primeiro item para compor este orçamento.</p></div></div> : <div className="budget-item-list">{items.map((item) => <article className="budget-item-row" key={item.orcamentoItemId}><div><strong>{item.descricao}</strong><span>Item #{item.orcamentoItemId}</span></div><b>{formatCurrency(item.valor)}</b><div className="row-actions"><button aria-label={`Editar ${item.descricao}`} onClick={() => { updateMutation.reset(); setEditing(item) }} disabled={editing !== null}><Icon name="edit" size={17} /></button><button className="danger-icon-button" aria-label={`Excluir ${item.descricao}`} onClick={() => { deleteMutation.reset(); setDeleting(item) }}><Icon name="trash" size={17} /></button></div></article>)}</div>}
    {deleteMutation.error && <div className="inline-alert" role="alert">{deleteMutation.error.message}</div>}
    <ConfirmDialog open={Boolean(deleting)} title="Excluir item?" description={`O item “${deleting?.descricao ?? ''}” será removido definitivamente deste orçamento.`} busy={deleteMutation.isPending} onClose={() => { if (!deleteMutation.isPending) setDeleting(null) }} onConfirm={() => deleting && deleteMutation.mutate(deleting.orcamentoItemId, { onSuccess: () => setDeleting(null) })} />
  </section>
}
