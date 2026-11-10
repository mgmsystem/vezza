import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ConfirmDialog } from '../../components/feedback/ConfirmDialog'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { ApiError } from '../../services/api-client'
import { useClientes } from '../clientes/cliente-queries'
import { useDeleteObra, useObras } from './obra-queries'
import type { Obra } from './obra-types'
import { parseApiDate } from '../../utils/date'

function shortDate(value?: string) {
  if (!value) return '—'
  const date = parseApiDate(value)
  return date ? new Intl.DateTimeFormat('pt-BR').format(date) : '—'
}

export function ObrasListPage() {
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<Obra | null>(null)
  const [actionError, setActionError] = useState('')
  const obrasQuery = useObras()
  const clientesQuery = useClientes()
  const deletion = useDeleteObra()
  const navigate = useNavigate()
  const names = useMemo(() => new Map((clientesQuery.data ?? []).map((item) => [item.clienteId, item.razaoSocial])), [clientesQuery.data])
  const obras = useMemo(() => {
    const term = search.trim().toLocaleLowerCase('pt-BR')
    if (!term) return obrasQuery.data ?? []
    return (obrasQuery.data ?? []).filter((obra) => [obra.descricao, names.get(obra.clienteId), obra.cidade, obra.estado].filter(Boolean).some((value) => value!.toLocaleLowerCase('pt-BR').includes(term)))
  }, [names, obrasQuery.data, search])

  async function confirmDelete() {
    if (!selected) return
    setActionError('')
    try { await deletion.mutateAsync(selected.obraId); setSelected(null) }
    catch (cause) { setSelected(null); setActionError(cause instanceof ApiError ? cause.messages.join(' ') : 'Não foi possível excluir a obra.') }
  }

  const loading = obrasQuery.isLoading || clientesQuery.isLoading
  const error = obrasQuery.error ?? clientesQuery.error
  return <section className="content-page">
    <div className="page-title-row"><div className="page-heading"><span className="eyebrow">Projetos</span><h1>Obras</h1><p>Acompanhe locais, responsáveis e projetos vinculados aos clientes.</p></div><Link className="solid-button" to="/app/obras/nova"><Icon name="plus" />Nova obra</Link></div>
    {actionError && <div className="inline-alert" role="alert">{actionError}</div>}
    <div className="data-card"><div className="data-toolbar"><div className="search-field"><Icon name="search" /><input aria-label="Pesquisar obras" placeholder="Buscar por descrição, cliente ou localização" value={search} onChange={(event) => setSearch(event.target.value)} /></div>{!loading && <span className="result-count">{obras.length} {obras.length === 1 ? 'obra' : 'obras'}</span>}</div>
      {loading ? <LoadingState label="Carregando obras…" /> : error ? <ErrorState message={error.message} onRetry={() => { obrasQuery.refetch(); clientesQuery.refetch() }} /> : obras.length === 0 ? <div className="empty-table"><div className="empty-icon"><Icon name="works" size={28} /></div><h2>{search ? 'Nenhuma obra encontrada' : 'Nenhuma obra cadastrada'}</h2><p>{search ? 'Revise o termo ou limpe a pesquisa.' : 'Cadastre a primeira obra para relacioná-la a um cliente.'}</p>{!search && <Link className="secondary-button" to="/app/obras/nova">Cadastrar primeira obra</Link>}</div> :
      <div className="table-scroll"><table><thead><tr><th>Obra</th><th>Cliente</th><th>Localização</th><th>Cadastro</th><th><span className="sr-only">Ações</span></th></tr></thead><tbody>{obras.map((obra) => <tr key={obra.obraId}><td data-label="Obra"><strong>{obra.descricao}</strong></td><td data-label="Cliente">{names.get(obra.clienteId) ?? `Cliente #${obra.clienteId}`}</td><td data-label="Localização">{[obra.cidade, obra.estado].filter(Boolean).join(' / ') || '—'}</td><td data-label="Cadastro">{shortDate(obra.dataCadastro)}</td><td className="row-actions"><button aria-label={`Visualizar ${obra.descricao}`} onClick={() => navigate(`/app/obras/${obra.obraId}`)}><Icon name="eye" /></button><button aria-label={`Editar ${obra.descricao}`} onClick={() => navigate(`/app/obras/${obra.obraId}/editar`)}><Icon name="edit" /></button><button className="danger-icon-button" aria-label={`Excluir ${obra.descricao}`} onClick={() => setSelected(obra)}><Icon name="trash" /></button></td></tr>)}</tbody></table></div>}
    </div>
    <ConfirmDialog open={Boolean(selected)} title="Excluir obra?" description={`Você está prestes a excluir ${selected?.descricao ?? 'esta obra'}. A exclusão será bloqueada caso existam orçamentos vinculados.`} busy={deletion.isPending} onClose={() => setSelected(null)} onConfirm={confirmDelete} />
  </section>
}
