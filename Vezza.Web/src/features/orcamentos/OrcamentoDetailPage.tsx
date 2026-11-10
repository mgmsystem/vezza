import { Link, useNavigate, useParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { useCliente } from '../clientes/cliente-queries'
import { useObra } from '../obras/obra-queries'
import { OrcamentoItemsSection } from '../orcamento-items/OrcamentoItemsSection'
import { formatCurrency, formatDate } from './formatters'
import { useOrcamento } from './orcamento-queries'

export function OrcamentoDetailPage() {
  const id = Number(useParams().orcamentoId ?? 0)
  const query = useOrcamento(id)
  const obraQuery = useObra(query.data?.obraId ?? 0)
  const clienteQuery = useCliente(obraQuery.data?.clienteId ?? 0)
  const navigate = useNavigate()
  const loading = query.isLoading || (query.data && obraQuery.isLoading) || (obraQuery.data && clienteQuery.isLoading)
  if (loading) return <section className="content-page"><LoadingState label="Carregando orçamento…" /></section>
  const error = query.error
  if (error || !query.data) return <section className="content-page"><ErrorState message={error?.message ?? 'Orçamento não encontrado.'} onRetry={() => query.refetch()} /></section>
  const item = query.data
  return <section className="content-page"><Link className="back-link" to="/app/orcamentos"><Icon name="back" />Voltar para orçamentos</Link>
    <div className="page-title-row detail-title-row"><div className="page-heading"><span className="eyebrow">Orçamento #{item.orcamentoId}</span><h1>{item.objetoProposta || 'Proposta sem descrição'}</h1><p>{formatDate(item.data, true)}</p></div><button className="solid-button" onClick={() => navigate(`/app/orcamentos/${id}/editar`)}><Icon name="edit" />Editar orçamento</button></div>
    <div className="budget-summary"><div><span>Valor do orçamento</span><strong>{formatCurrency(item.valor)}</strong></div><div><span>Situação</span><strong className="status-badge">{item.situacao || 'Não informada'}</strong></div></div>
    <div className="detail-grid"><article className="detail-card"><h2>Relacionamentos</h2><dl><div><dt>Obra vinculada</dt><dd>{obraQuery.data ? <Link className="detail-link" to={`/app/obras/${obraQuery.data.obraId}`}>{obraQuery.data.descricao}<Icon name="arrow" size={14} /></Link> : `Obra #${item.obraId} não encontrada`}</dd></div><div><dt>Cliente</dt><dd>{clienteQuery.data ? <Link className="detail-link" to={`/app/clientes/${clienteQuery.data.clienteId}`}>{clienteQuery.data.razaoSocial}<Icon name="arrow" size={14} /></Link> : 'Cliente não encontrado'}</dd></div></dl></article><article className="detail-card"><h2>Condições</h2><dl><div><dt>Pagamento</dt><dd>{item.condPagamento || 'Não informado'}</dd></div><div><dt>Prazos</dt><dd>{item.prazos || 'Não informado'}</dd></div></dl></article><article className="detail-card detail-card-wide"><h2>Período da obra</h2><dl className="detail-columns"><div><dt>Início</dt><dd>{formatDate(item.inicioObra)}</dd></div><div><dt>Término</dt><dd>{formatDate(item.terminoObra)}</dd></div><div><dt>Data da proposta</dt><dd>{formatDate(item.data)}</dd></div></dl></article><article className="detail-card"><h2>Comentários</h2><p className="detail-copy">{item.comentarios || 'Nenhum comentário informado.'}</p></article><article className="detail-card"><h2>Notas gerais</h2><p className="detail-copy">{item.notasGerais || 'Nenhuma nota informada.'}</p></article></div>
    <OrcamentoItemsSection orcamentoId={item.orcamentoId} valorOrcamento={item.valor} />
  </section>
}
