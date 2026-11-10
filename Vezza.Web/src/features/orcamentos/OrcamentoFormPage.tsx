import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { useClientes } from '../clientes/cliente-queries'
import { useObras } from '../obras/obra-queries'
import { dateInputValue } from './formatters'
import { OrcamentoForm } from './OrcamentoForm'
import { useCreateOrcamento, useOrcamento, useUpdateOrcamento } from './orcamento-queries'
import { emptyOrcamentoInput, type Orcamento, type OrcamentoInput, type OrcamentoPayload } from './orcamento-types'

function toInput(item: Orcamento): OrcamentoInput {
  return { obraId: item.obraId, data: dateInputValue(item.data), objetoProposta: item.objetoProposta ?? '', valor: item.valor == null ? '' : String(item.valor), condPagamento: item.condPagamento ?? '', prazos: item.prazos ?? '', comentarios: item.comentarios ?? '', notasGerais: item.notasGerais ?? '', inicioObra: dateInputValue(item.inicioObra), terminoObra: dateInputValue(item.terminoObra), situacao: item.situacao ?? '' }
}

export function OrcamentoFormPage() {
  const id = Number(useParams().orcamentoId ?? 0)
  const editing = id > 0
  const [params] = useSearchParams()
  const preselectedObraId = Number(params.get('obraId') ?? 0)
  const navigate = useNavigate()
  const itemQuery = useOrcamento(id)
  const obrasQuery = useObras()
  const clientesQuery = useClientes()
  const creation = useCreateOrcamento()
  const update = useUpdateOrcamento()
  const mutation = editing ? update : creation
  const validPreselection = (obrasQuery.data ?? []).some((obra) => obra.obraId === preselectedObraId) ? preselectedObraId : 0
  const initial = itemQuery.data ? toInput(itemQuery.data) : { ...emptyOrcamentoInput, obraId: validPreselection }
  const loading = obrasQuery.isLoading || clientesQuery.isLoading || (editing && itemQuery.isLoading)
  const error = obrasQuery.error ?? clientesQuery.error ?? (editing ? itemQuery.error : null)
  const serverError = mutation.error instanceof ApiError ? mutation.error.messages.join(' ') : mutation.error?.message

  async function submit(values: OrcamentoPayload) {
    try { if (editing) await update.mutateAsync({ ...values, orcamentoId: id }); else await creation.mutateAsync(values); navigate('/app/orcamentos', { replace: true }) }
    catch { /* exibido pela mutação */ }
  }

  return <section className="content-page narrow-page"><Link className="back-link" to={editing ? `/app/orcamentos/${id}` : '/app/orcamentos'}><Icon name="back" />Voltar</Link><div className="page-heading"><span className="eyebrow">Orçamentos</span><h1>{editing ? 'Editar orçamento' : 'Novo orçamento'}</h1><p>{editing ? 'Atualize os dados comerciais e o vínculo da proposta.' : 'Crie uma proposta vinculada a uma obra existente.'}</p></div>
    {loading ? <LoadingState label="Preparando formulário…" /> : error ? <ErrorState message={error.message} onRetry={() => { obrasQuery.refetch(); clientesQuery.refetch(); if (editing) itemQuery.refetch() }} /> : obrasQuery.data?.length === 0 ? <div className="empty-table standalone-empty"><h2>Cadastre uma obra primeiro</h2><p>Um orçamento precisa estar vinculado a uma obra existente.</p><Link className="solid-button" to="/app/obras/nova">Cadastrar obra</Link></div> : <OrcamentoForm key={`${itemQuery.data?.orcamentoId ?? 'new'}-${validPreselection}`} initialValues={initial} obras={obrasQuery.data ?? []} clientes={clientesQuery.data ?? []} submitLabel={editing ? 'Salvar alterações' : 'Cadastrar orçamento'} busy={mutation.isPending} serverError={serverError} onCancel={() => navigate(editing ? `/app/orcamentos/${id}` : '/app/orcamentos')} onSubmit={submit} />}
  </section>
}
