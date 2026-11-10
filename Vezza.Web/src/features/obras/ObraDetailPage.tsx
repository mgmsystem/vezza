import { Link, useNavigate, useParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { useCliente } from '../clientes/cliente-queries'
import { useObra } from './obra-queries'
import { ObraOrcamentosSection } from '../orcamentos/ObraOrcamentosSection'
import { parseApiDate } from '../../utils/date'

function dateLabel(value?: string) { const date = parseApiDate(value); return date ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' }).format(date) : 'Não informada' }

export function ObraDetailPage() {
  const id = Number(useParams().obraId ?? 0)
  const obraQuery = useObra(id)
  const clienteQuery = useCliente(obraQuery.data?.clienteId ?? 0)
  const navigate = useNavigate()
  if (obraQuery.isLoading || (obraQuery.data && clienteQuery.isLoading)) return <section className="content-page"><LoadingState label="Carregando obra…" /></section>
  const error = obraQuery.error ?? clienteQuery.error
  if (error || !obraQuery.data) return <section className="content-page"><ErrorState message={error?.message ?? 'Obra não encontrada.'} onRetry={() => { obraQuery.refetch(); clienteQuery.refetch() }} /></section>
  const obra = obraQuery.data
  const address = [obra.logradouro, obra.numero, obra.complemento, obra.bairro].filter(Boolean).join(', ')
  return <section className="content-page">
    <Link className="back-link" to="/app/obras"><Icon name="back" />Voltar para obras</Link>
    <div className="page-title-row detail-title-row"><div className="page-heading"><span className="eyebrow">Obra #{obra.obraId}</span><h1>{obra.descricao}</h1><p>Cadastrada em {dateLabel(obra.dataCadastro)}</p></div><button className="solid-button" onClick={() => navigate(`/app/obras/${id}/editar`)}><Icon name="edit" />Editar obra</button></div>
    <div className="detail-grid"><article className="detail-card"><h2>Identificação</h2><dl><div><dt>Descrição</dt><dd>{obra.descricao}</dd></div><div><dt>Cliente vinculado</dt><dd>{clienteQuery.data ? <Link className="detail-link" to={`/app/clientes/${clienteQuery.data.clienteId}`}>{clienteQuery.data.razaoSocial}<Icon name="arrow" size={14} /></Link> : `Cliente #${obra.clienteId}`}</dd></div></dl></article><article className="detail-card"><h2>Cadastro</h2><dl><div><dt>Data de cadastro</dt><dd>{dateLabel(obra.dataCadastro)}</dd></div></dl></article><article className="detail-card detail-card-wide"><h2>Endereço</h2><dl className="detail-columns"><div><dt>Logradouro</dt><dd>{address || 'Não informado'}</dd></div><div><dt>CEP</dt><dd>{obra.cep || 'Não informado'}</dd></div><div><dt>Cidade / Estado</dt><dd>{[obra.cidade, obra.estado].filter(Boolean).join(' / ') || 'Não informado'}</dd></div></dl></article></div>
    <ObraOrcamentosSection obraId={obra.obraId} />
  </section>
}
