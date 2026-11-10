import { Link, useNavigate, useParams } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { ClienteForm } from './ClienteForm'
import { useCliente, useCreateCliente, useUpdateCliente } from './cliente-queries'
import { emptyClienteInput, type Cliente, type ClienteInput } from './cliente-types'

function toInput(cliente: Cliente): ClienteInput {
  return {
    razaoSocial: cliente.razaoSocial ?? '', cnpjCpf: cliente.cnpjCpf ?? '', telefone: cliente.telefone ?? '',
    cep: cliente.cep ?? '', logradouro: cliente.logradouro ?? '', numero: cliente.numero ?? '',
    complemento: cliente.complemento ?? '', bairro: cliente.bairro ?? '', cidade: cliente.cidade ?? '', estado: cliente.estado ?? '',
  }
}

export function ClienteFormPage() {
  const { clienteId } = useParams()
  const id = Number(clienteId ?? 0)
  const isEditing = id > 0
  const navigate = useNavigate()
  const query = useCliente(id)
  const creation = useCreateCliente()
  const update = useUpdateCliente()
  const mutation = isEditing ? update : creation

  async function submit(values: ClienteInput) {
    try {
      if (isEditing) await update.mutateAsync({ ...values, clienteId: id })
      else await creation.mutateAsync(values)
      navigate('/app/clientes', { replace: true })
    } catch { /* erro exibido pela mutação */ }
  }

  const mutationError = mutation.error instanceof ApiError ? mutation.error.messages.join(' ') : mutation.error?.message

  return (
    <section className="content-page narrow-page">
      <Link className="back-link" to={isEditing ? `/app/clientes/${id}` : '/app/clientes'}><Icon name="back" />Voltar</Link>
      <div className="page-heading"><span className="eyebrow">Clientes</span><h1>{isEditing ? 'Editar cliente' : 'Novo cliente'}</h1><p>{isEditing ? 'Atualize as informações cadastrais necessárias.' : 'Preencha os dados para incluir um cliente na sua base.'}</p></div>
      {isEditing && query.isLoading ? <LoadingState label="Carregando cliente…" /> : isEditing && query.isError ? <ErrorState message={query.error.message} onRetry={() => query.refetch()} /> : (
        <ClienteForm key={query.data?.clienteId ?? 'new'} initialValues={query.data ? toInput(query.data) : emptyClienteInput} submitLabel={isEditing ? 'Salvar alterações' : 'Cadastrar cliente'} busy={mutation.isPending} serverError={mutationError} onCancel={() => navigate(isEditing ? `/app/clientes/${id}` : '/app/clientes')} onSubmit={submit} />
      )}
    </section>
  )
}
