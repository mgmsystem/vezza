import { Link } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { useClientes } from '../clientes/cliente-queries'
import { useObras } from '../obras/obra-queries'
import { formatCurrency, formatDate } from '../orcamentos/formatters'
import { useOrcamentos } from '../orcamentos/orcamento-queries'
import { commercialValueCents, groupBySituation, recentOrcamentos } from './dashboard-utils'

function MetricCard({ label, value, loading, error, icon }: { label: string; value: string; loading: boolean; error: boolean; icon: 'clients' | 'works' | 'budgets' }) {
  return <article className={`dashboard-metric ${error ? 'metric-unavailable' : ''}`}><span className="metric-icon"><Icon name={icon} size={20} /></span><div><span>{label}</span>{loading ? <span className="metric-loading" aria-label={`Carregando ${label}`} /> : <strong>{error ? 'Indisponível' : value}</strong>}</div></article>
}

export function DashboardPage() {
  const clientesQuery = useClientes()
  const obrasQuery = useObras()
  const orcamentosQuery = useOrcamentos()
  const clientes = clientesQuery.data ?? []
  const obras = obrasQuery.data ?? []
  const orcamentos = orcamentosQuery.data ?? []
  const obraById = new Map(obras.map((obra) => [obra.obraId, obra]))
  const clienteById = new Map(clientes.map((cliente) => [cliente.clienteId, cliente]))
  const situations = groupBySituation(orcamentos)
  const recent = recentOrcamentos(orcamentos)
  const partialErrors = [clientesQuery, obrasQuery, orcamentosQuery].filter((query) => query.isError).length

  return <section className="content-page dashboard-page">
    <div className="page-title-row"><div className="page-heading"><span className="eyebrow">Visão geral</span><h1>Dashboard</h1><p>Indicadores consolidados a partir dos registros atuais da operação.</p></div><nav className="dashboard-shortcuts" aria-label="Atalhos principais"><Link to="/app/clientes">Clientes</Link><Link to="/app/obras">Obras</Link><Link to="/app/orcamentos">Orçamentos</Link></nav></div>

    {partialErrors > 0 && <div className="dashboard-warning" role="alert"><strong>Alguns dados não puderam ser carregados.</strong><span>Os demais indicadores continuam disponíveis. Tente atualizar a página para consultar novamente.</span></div>}

    <div className="dashboard-metrics" aria-label="Indicadores gerenciais">
      <MetricCard label="Clientes" value={String(clientes.length)} loading={clientesQuery.isLoading} error={clientesQuery.isError} icon="clients" />
      <MetricCard label="Obras" value={String(obras.length)} loading={obrasQuery.isLoading} error={obrasQuery.isError} icon="works" />
      <MetricCard label="Orçamentos" value={String(orcamentos.length)} loading={orcamentosQuery.isLoading} error={orcamentosQuery.isError} icon="budgets" />
      <MetricCard label="Valor comercial" value={formatCurrency(commercialValueCents(orcamentos) / 100)} loading={orcamentosQuery.isLoading} error={orcamentosQuery.isError} icon="budgets" />
    </div>

    <div className="dashboard-grid">
      <section className="dashboard-panel" aria-labelledby="situation-title"><div className="dashboard-panel-heading"><div><span className="eyebrow">Distribuição</span><h2 id="situation-title">Orçamentos por situação</h2></div><Link to="/app/orcamentos">Ver todos<Icon name="arrow" size={14} /></Link></div>
        {orcamentosQuery.isLoading ? <div className="dashboard-panel-state" role="status">Carregando distribuição…</div> : orcamentosQuery.isError ? <div className="dashboard-panel-state dashboard-panel-error">Distribuição indisponível.</div> : situations.length === 0 ? <div className="dashboard-panel-state">Nenhum orçamento para distribuir.</div> : <div className="situation-list">{situations.map((group) => <div className="situation-row" key={group.label}><div><strong>{group.label}</strong><span>{group.count} {group.count === 1 ? 'orçamento' : 'orçamentos'}</span></div><div className="situation-track" aria-hidden="true"><span style={{ width: `${(group.count / orcamentos.length) * 100}%` }} /></div><b>{Math.round((group.count / orcamentos.length) * 100)}%</b></div>)}</div>}
      </section>

      <section className="dashboard-panel recent-panel" aria-labelledby="recent-title"><div className="dashboard-panel-heading"><div><span className="eyebrow">Atividade</span><h2 id="recent-title">Orçamentos recentes</h2></div><Link to="/app/orcamentos/novo"><Icon name="plus" size={14} />Novo orçamento</Link></div>
        {orcamentosQuery.isLoading ? <div className="dashboard-panel-state" role="status">Carregando orçamentos recentes…</div> : orcamentosQuery.isError ? <div className="dashboard-panel-state dashboard-panel-error">Orçamentos recentes indisponíveis.</div> : recent.length === 0 ? <div className="dashboard-panel-state">Nenhum orçamento cadastrado.</div> : <div className="recent-budget-list">{recent.map((orcamento) => {
          const obra = obraById.get(orcamento.obraId)
          const cliente = obra ? clienteById.get(obra.clienteId) : undefined
          const obraLabel = obra?.descricao ?? (obrasQuery.isError ? 'Relacionamento indisponível' : `Obra #${orcamento.obraId} não encontrada`)
          const clienteLabel = cliente?.razaoSocial ?? (obra ? (clientesQuery.isError ? 'Relacionamento indisponível' : `Cliente #${obra.clienteId} não encontrado`) : '')
          return <Link className="recent-budget" to={`/app/orcamentos/${orcamento.orcamentoId}`} key={orcamento.orcamentoId}><div className="recent-budget-main"><strong>{orcamento.objetoProposta || 'Proposta sem descrição'}</strong><span>{formatDate(orcamento.data)}</span></div><div className="recent-budget-relation"><strong>{obraLabel}</strong><span>{clienteLabel}</span></div><span className="status-badge">{orcamento.situacao?.trim() || 'Não informada'}</span><b>{formatCurrency(orcamento.valor)}</b><Icon name="arrow" size={16} /></Link>
        })}</div>}
      </section>
    </div>
  </section>
}
