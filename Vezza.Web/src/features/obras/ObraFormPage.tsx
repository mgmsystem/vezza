import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { useClientes } from '../clientes/cliente-queries'
import { ObraForm } from './ObraForm'
import { useCreateObra, useObra, useUpdateObra } from './obra-queries'
import { emptyObraInput, type Obra, type ObraInput } from './obra-types'

function toInput(obra: Obra): ObraInput { return { clienteId: obra.clienteId, descricao: obra.descricao ?? '', cep: obra.cep ?? '', logradouro: obra.logradouro ?? '', numero: obra.numero ?? '', complemento: obra.complemento ?? '', bairro: obra.bairro ?? '', cidade: obra.cidade ?? '', estado: obra.estado ?? '' } }

export function ObraFormPage() {
  const id = Number(useParams().obraId ?? 0)
  const editing = id > 0
  const [params] = useSearchParams()
  const preselectedClienteId = Number(params.get('clienteId') ?? 0)
  const navigate = useNavigate()
  const obraQuery = useObra(id)
  const clientesQuery = useClientes()
  const creation = useCreateObra()
  const update = useUpdateObra()
  const mutation = editing ? update : creation
  const initial = obraQuery.data ? toInput(obraQuery.data) : { ...emptyObraInput, clienteId: preselectedClienteId }

  async function submit(values: ObraInput) {
    try { if (editing) await update.mutateAsync({ ...values, obraId: id }); else await creation.mutateAsync(values); navigate('/app/obras', { replace: true }) }
    catch { /* exibido pela mutação */ }
  }
  const serverError = mutation.error instanceof ApiError ? mutation.error.messages.join(' ') : mutation.error?.message
  const loading = clientesQuery.isLoading || (editing && obraQuery.isLoading)
  const error = clientesQuery.error ?? (editing ? obraQuery.error : null)

  return <section className="content-page narrow-page"><Link className="back-link" to={editing ? `/app/obras/${id}` : '/app/obras'}><Icon name="back" />Voltar</Link><div className="page-heading"><span className="eyebrow">Obras</span><h1>{editing ? 'Editar obra' : 'Nova obra'}</h1><p>{editing ? 'Atualize o vínculo, a identificação ou o endereço.' : 'Vincule a obra a um cliente e informe sua localização.'}</p></div>
    {loading ? <LoadingState label="Preparando formulário…" /> : error ? <ErrorState message={error.message} onRetry={() => { clientesQuery.refetch(); if (editing) obraQuery.refetch() }} /> : clientesQuery.data?.length === 0 ? <div className="empty-table standalone-empty"><h2>Cadastre um cliente primeiro</h2><p>Uma obra precisa estar vinculada a um cliente existente.</p><Link className="solid-button" to="/app/clientes/novo">Cadastrar cliente</Link></div> : <ObraForm key={`${obraQuery.data?.obraId ?? 'new'}-${preselectedClienteId}`} initialValues={initial} clientes={clientesQuery.data ?? []} submitLabel={editing ? 'Salvar alterações' : 'Cadastrar obra'} busy={mutation.isPending} serverError={serverError} onCancel={() => navigate(editing ? `/app/obras/${id}` : '/app/obras')} onSubmit={submit} />}
  </section>
}
