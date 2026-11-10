import { Link } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { formatCurrency, formatDate } from './formatters'
import { useOrcamentos } from './orcamento-queries'

export function ObraOrcamentosSection({ obraId }: { obraId: number }) {
  const query = useOrcamentos()
  if (query.isLoading) return <section className="related-list-card"><LoadingState label="Carregando orçamentos relacionados…" /></section>
  if (query.isError) return <section className="related-list-card"><ErrorState message={query.error.message} onRetry={() => query.refetch()} /></section>
  const items = (query.data ?? []).filter((item) => item.obraId === obraId)
  return <section className="related-list-card"><div className="related-list-heading"><div><span className="eyebrow">Relacionamento</span><h2>Orçamentos relacionados</h2><p>Propostas cadastradas para esta obra.</p></div><Link className="solid-button" to={`/app/orcamentos/novo?obraId=${obraId}`}><Icon name="plus" />Novo orçamento</Link></div>
    {items.length === 0 ? <div className="related-empty"><div className="empty-icon"><Icon name="budgets" size={24} /></div><div><h3>Nenhum orçamento vinculado</h3><p>Crie uma proposta para iniciar o histórico comercial desta obra.</p></div></div> : <div className="related-items">{items.map((item) => <Link key={item.orcamentoId} to={`/app/orcamentos/${item.orcamentoId}`} className="related-item"><div className="related-item-icon"><Icon name="budgets" /></div><div><strong>{item.objetoProposta || `Orçamento #${item.orcamentoId}`}</strong><span>{formatDate(item.data)} · {item.situacao || 'Situação não informada'} · {formatCurrency(item.valor)}</span></div><Icon name="arrow" /></Link>)}</div>}
  </section>
}
