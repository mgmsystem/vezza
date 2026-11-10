import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ConfirmDialog } from '../../components/feedback/ConfirmDialog'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { useClientes, useDeleteCliente } from './cliente-queries'
import type { Cliente } from './cliente-types'

export function ClientesListPage() {
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<Cliente | null>(null)
  const [actionError, setActionError] = useState('')
  const query = useClientes()
  const deletion = useDeleteCliente()
  const navigate = useNavigate()

  const clientes = useMemo(() => {
    const term = search.trim().toLocaleLowerCase('pt-BR')
    if (!term) return query.data ?? []
    return (query.data ?? []).filter((cliente) => cliente.razaoSocial.toLocaleLowerCase('pt-BR').includes(term) || cliente.cnpjCpf.toLocaleLowerCase('pt-BR').includes(term))
  }, [query.data, search])

  async function confirmDelete() {
    if (!selected) return
    setActionError('')
    try {
      await deletion.mutateAsync(selected.clienteId)
      setSelected(null)
    } catch (cause) {
      setSelected(null)
      setActionError(cause instanceof ApiError ? cause.messages.join(' ') : 'Não foi possível excluir o cliente.')
    }
  }

  return (
    <section className="content-page">
      <div className="page-title-row">
        <div className="page-heading"><span className="eyebrow">Relacionamentos</span><h1>Clientes</h1><p>Gerencie informações cadastrais e mantenha sua base organizada.</p></div>
        <Link className="solid-button" to="/app/clientes/novo"><Icon name="plus" />Cadastrar cliente</Link>
      </div>

      {actionError && <div className="inline-alert" role="alert">{actionError}</div>}

      <div className="data-card">
        <div className="data-toolbar">
          <div className="search-field"><Icon name="search" /><input aria-label="Pesquisar clientes" placeholder="Buscar por razão social ou CPF/CNPJ" value={search} onChange={(event) => setSearch(event.target.value)} /></div>
          {!query.isLoading && <span className="result-count">{clientes.length} {clientes.length === 1 ? 'cliente' : 'clientes'}</span>}
        </div>

        {query.isLoading ? <LoadingState label="Carregando clientes…" /> : query.isError ? <ErrorState message={query.error.message} onRetry={() => query.refetch()} /> : clientes.length === 0 ? (
          <div className="empty-table"><div className="empty-icon"><Icon name="clients" size={28} /></div><h2>{search ? 'Nenhum resultado encontrado' : 'Sua base de clientes está vazia'}</h2><p>{search ? 'Revise o termo pesquisado ou limpe a busca.' : 'Cadastre o primeiro cliente para iniciar a gestão.'}</p>{!search && <Link className="secondary-button" to="/app/clientes/novo">Cadastrar primeiro cliente</Link>}</div>
        ) : (
          <div className="table-scroll"><table><thead><tr><th>Razão social</th><th>CPF/CNPJ</th><th>Cidade</th><th>UF</th><th><span className="sr-only">Ações</span></th></tr></thead><tbody>{clientes.map((cliente) => (
            <tr key={cliente.clienteId}><td data-label="Razão social"><strong>{cliente.razaoSocial}</strong></td><td data-label="CPF/CNPJ">{cliente.cnpjCpf}</td><td data-label="Cidade">{cliente.cidade || '—'}</td><td data-label="UF">{cliente.estado || '—'}</td><td className="row-actions"><button aria-label={`Visualizar ${cliente.razaoSocial}`} onClick={() => navigate(`/app/clientes/${cliente.clienteId}`)}><Icon name="eye" /></button><button aria-label={`Editar ${cliente.razaoSocial}`} onClick={() => navigate(`/app/clientes/${cliente.clienteId}/editar`)}><Icon name="edit" /></button><button className="danger-icon-button" aria-label={`Excluir ${cliente.razaoSocial}`} onClick={() => setSelected(cliente)}><Icon name="trash" /></button></td></tr>
          ))}</tbody></table></div>
        )}
      </div>
      <ConfirmDialog open={Boolean(selected)} title="Excluir cliente?" description={`Você está prestes a excluir ${selected?.razaoSocial ?? 'este cliente'}. Esta ação não pode ser desfeita e será bloqueada caso existam obras vinculadas.`} busy={deletion.isPending} onClose={() => setSelected(null)} onConfirm={confirmDelete} />
    </section>
  )
}
