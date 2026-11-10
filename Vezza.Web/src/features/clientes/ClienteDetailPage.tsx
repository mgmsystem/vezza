import { Link, useNavigate, useParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { useCliente } from './cliente-queries'
import { ClienteObrasSection } from '../obras/ClienteObrasSection'
import { parseApiDate } from '../../utils/date'

function formatDate(value?: string) {
  if (!value) return 'Não informada'
  const date = parseApiDate(value)
  return date ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' }).format(date) : 'Não informada'
}

export function ClienteDetailPage() {
  const id = Number(useParams().clienteId ?? 0)
  const query = useCliente(id)
  const navigate = useNavigate()

  if (query.isLoading) return <section className="content-page"><LoadingState label="Carregando cliente…" /></section>
  if (query.isError || !query.data) return <section className="content-page"><ErrorState message={query.error?.message ?? 'Cliente não encontrado.'} onRetry={() => query.refetch()} /></section>

  const cliente = query.data
  const address = [cliente.logradouro, cliente.numero, cliente.complemento, cliente.bairro].filter(Boolean).join(', ')
  const location = [cliente.cidade, cliente.estado].filter(Boolean).join(' / ')

  return (
    <section className="content-page">
      <Link className="back-link" to="/app/clientes"><Icon name="back" />Voltar para clientes</Link>
      <div className="page-title-row detail-title-row"><div className="page-heading"><span className="eyebrow">Cliente #{cliente.clienteId}</span><h1>{cliente.razaoSocial}</h1><p>Cadastrado em {formatDate(cliente.dataCadastro)}</p></div><button className="solid-button" onClick={() => navigate(`/app/clientes/${id}/editar`)}><Icon name="edit" />Editar cliente</button></div>
      <div className="detail-grid">
        <article className="detail-card"><h2>Identificação</h2><dl><div><dt>Razão social</dt><dd>{cliente.razaoSocial}</dd></div><div><dt>CPF/CNPJ</dt><dd>{cliente.cnpjCpf}</dd></div></dl></article>
        <article className="detail-card"><h2>Contato</h2><dl><div><dt>Telefone</dt><dd>{cliente.telefone || 'Não informado'}</dd></div></dl></article>
        <article className="detail-card detail-card-wide"><h2>Endereço</h2><dl className="detail-columns"><div><dt>Logradouro</dt><dd>{address || 'Não informado'}</dd></div><div><dt>CEP</dt><dd>{cliente.cep || 'Não informado'}</dd></div><div><dt>Cidade / Estado</dt><dd>{location || 'Não informado'}</dd></div></dl></article>
      </div>
      <ClienteObrasSection clienteId={cliente.clienteId} />
    </section>
  )
}
