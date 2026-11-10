import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ConfirmDialog } from '../../components/feedback/ConfirmDialog'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { useClientes } from '../clientes/cliente-queries'
import { useObras } from '../obras/obra-queries'
import { formatCurrency, formatDate } from './formatters'
import { useDeleteOrcamento, useOrcamentos } from './orcamento-queries'
import type { Orcamento } from './orcamento-types'

export function OrcamentosListPage() {
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<Orcamento | null>(null)
  const [actionError, setActionError] = useState('')
  const query = useOrcamentos()
  const obrasQuery = useObras()
  const clientesQuery = useClientes()
  const deletion = useDeleteOrcamento()
  const navigate = useNavigate()
  const obras = useMemo(() => new Map((obrasQuery.data ?? []).map((item) => [item.obraId, item])), [obrasQuery.data])
  const clientes = useMemo(() => new Map((clientesQuery.data ?? []).map((item) => [item.clienteId, item])), [clientesQuery.data])
  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase('pt-BR')
    if (!term) return query.data ?? []
    return (query.data ?? []).filter((item) => {
      const obra = obras.get(item.obraId)
      const cliente = obra ? clientes.get(obra.clienteId) : undefined
      return [item.objetoProposta, item.situacao, obra?.descricao, cliente?.razaoSocial].filter(Boolean).some((value) => value!.toLocaleLowerCase('pt-BR').includes(term))
    })
  }, [clientes, obras, query.data, search])
  const loading = query.isLoading || obrasQuery.isLoading || clientesQuery.isLoading
  const error = query.error ?? obrasQuery.error ?? clientesQuery.error

  async function confirmDelete() {
    if (!selected) return
    setActionError('')
    try { await deletion.mutateAsync(selected.orcamentoId); setSelected(null) }
    catch (cause) { setSelected(null); setActionError(cause instanceof ApiError ? cause.messages.join(' ') : 'Não foi possível excluir o orçamento.') }
  }

  return <section className="content-page">
    <div className="page-title-row"><div className="page-heading"><span className="eyebrow">Propostas</span><h1>Orçamentos</h1><p>Organize propostas, valores e prazos vinculados às obras.</p></div><Link className="solid-button" to="/app/orcamentos/novo"><Icon name="plus" />Novo orçamento</Link></div>
    {actionError && <div className="inline-alert" role="alert">{actionError}</div>}
    <div className="data-card"><div className="data-toolbar"><div className="search-field"><Icon name="search" /><input aria-label="Pesquisar orçamentos" placeholder="Buscar por proposta, obra, cliente ou situação" value={search} onChange={(event) => setSearch(event.target.value)} /></div>{!loading && <span className="result-count">{filtered.length} {filtered.length === 1 ? 'orçamento' : 'orçamentos'}</span>}</div>
      {loading ? <LoadingState label="Carregando orçamentos…" /> : error ? <ErrorState message={error.message} onRetry={() => { query.refetch(); obrasQuery.refetch(); clientesQuery.refetch() }} /> : filtered.length === 0 ? <div className="empty-table"><div className="empty-icon"><Icon name="budgets" size={28} /></div><h2>{search ? 'Nenhum orçamento encontrado' : 'Nenhum orçamento cadastrado'}</h2><p>{search ? 'Revise o termo ou limpe a pesquisa.' : 'Crie a primeira proposta vinculada a uma obra.'}</p>{!search && <Link className="secondary-button" to="/app/orcamentos/novo">Criar primeiro orçamento</Link>}</div> :
      <div className="table-scroll"><table><thead><tr><th>Proposta</th><th>Obra / Cliente</th><th>Data</th><th>Situação</th><th>Valor</th><th><span className="sr-only">Ações</span></th></tr></thead><tbody>{filtered.map((item) => {
        const obra = obras.get(item.obraId); const cliente = obra ? clientes.get(obra.clienteId) : undefined; const missing = !obra || !cliente
        return <tr key={item.orcamentoId}><td data-label="Proposta"><strong>{item.objetoProposta || `Orçamento #${item.orcamentoId}`}</strong></td><td data-label="Obra / Cliente"><span className={missing ? 'missing-relation' : ''}>{obra?.descricao ?? `Obra #${item.obraId} não encontrada`}</span><small className="cell-secondary">{obra ? (cliente?.razaoSocial ?? `Cliente #${obra.clienteId} não encontrado`) : 'Relacionamento indisponível'}</small></td><td data-label="Data">{formatDate(item.data)}</td><td data-label="Situação"><span className="status-badge">{item.situacao || 'Não informada'}</span></td><td data-label="Valor">{formatCurrency(item.valor)}</td><td className="row-actions"><button aria-label={`Visualizar ${item.objetoProposta || item.orcamentoId}`} onClick={() => navigate(`/app/orcamentos/${item.orcamentoId}`)}><Icon name="eye" /></button><button aria-label={`Editar ${item.objetoProposta || item.orcamentoId}`} onClick={() => navigate(`/app/orcamentos/${item.orcamentoId}/editar`)}><Icon name="edit" /></button><button className="danger-icon-button" aria-label={`Excluir ${item.objetoProposta || item.orcamentoId}`} onClick={() => setSelected(item)}><Icon name="trash" /></button></td></tr>
      })}</tbody></table></div>}
    </div>
    <ConfirmDialog open={Boolean(selected)} title="Excluir orçamento?" description="A exclusão do orçamento também removerá todos os itens e lançamentos financeiros vinculados a ele. Essa operação não poderá ser desfeita pela interface." busy={deletion.isPending} onClose={() => setSelected(null)} onConfirm={confirmDelete} />
  </section>
}
