import { Link } from 'react-router-dom'
import { Icon } from '../../components/Icon'
import { ErrorState, LoadingState } from '../../components/feedback/PageState'
import { useObras } from './obra-queries'

export function ClienteObrasSection({ clienteId }: { clienteId: number }) {
  const query = useObras()
  if (query.isLoading) return <section className="related-list-card"><LoadingState label="Carregando obras relacionadas…" /></section>
  if (query.isError) return <section className="related-list-card"><ErrorState message={query.error.message} onRetry={() => query.refetch()} /></section>
  const obras = (query.data ?? []).filter((obra) => obra.clienteId === clienteId)
  return <section className="related-list-card"><div className="related-list-heading"><div><span className="eyebrow">Relacionamento</span><h2>Obras relacionadas</h2><p>Projetos cadastrados para este cliente.</p></div><Link className="solid-button" to={`/app/obras/nova?clienteId=${clienteId}`}><Icon name="plus" />Nova obra</Link></div>
    {obras.length === 0 ? <div className="related-empty"><div className="empty-icon"><Icon name="works" size={24} /></div><div><h3>Nenhuma obra vinculada</h3><p>Cadastre uma obra para iniciar o histórico de projetos deste cliente.</p></div></div> : <div className="related-items">{obras.map((obra) => <Link key={obra.obraId} to={`/app/obras/${obra.obraId}`} className="related-item"><div className="related-item-icon"><Icon name="works" /></div><div><strong>{obra.descricao}</strong><span>{[obra.cidade, obra.estado].filter(Boolean).join(' / ') || 'Localização não informada'}</span></div><Icon name="arrow" /></Link>)}</div>}
  </section>
}
