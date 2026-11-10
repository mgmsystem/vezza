import { Icon } from '../components/Icon'

const pageContent = {
  dashboard: { title: 'Dashboard', description: 'Uma visão organizada da sua operação.', icon: 'dashboard' as const },
  clientes: { title: 'Clientes', description: 'Relacionamentos e dados cadastrais em um só lugar.', icon: 'clients' as const },
  obras: { title: 'Obras', description: 'Acompanhamento centralizado dos seus projetos.', icon: 'works' as const },
  orcamentos: { title: 'Orçamentos', description: 'Propostas e composição de valores com clareza.', icon: 'budgets' as const },
}

export function ComingSoonPage({ page }: { page: keyof typeof pageContent }) {
  const content = pageContent[page]
  return (
    <section className="content-page">
      <div className="page-heading"><span className="eyebrow">Visão geral</span><h1>{content.title}</h1><p>{content.description}</p></div>
      <div className="empty-state-card">
        <div className="empty-icon"><Icon name={content.icon} size={28} /></div>
        <span className="development-tag">Em desenvolvimento</span>
        <h2>Esta área está sendo preparada</h2>
        <p>Os recursos de {content.title.toLowerCase()} serão conectados à API real na próxima etapa. Nenhum dado demonstrativo é exibido aqui.</p>
      </div>
    </section>
  )
}
