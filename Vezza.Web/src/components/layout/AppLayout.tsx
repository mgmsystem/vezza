import { useEffect, useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../../features/auth/auth-context'
import { Icon } from '../Icon'

const navigation = [
  { to: '/app/dashboard', label: 'Dashboard', icon: 'dashboard' as const },
  { to: '/app/clientes', label: 'Clientes', icon: 'clients' as const },
  { to: '/app/obras', label: 'Obras', icon: 'works' as const },
  { to: '/app/orcamentos', label: 'Orçamentos', icon: 'budgets' as const },
]

export function AppLayout() {
  const [menuOpen, setMenuOpen] = useState(false)
  const { identity, logout } = useAuth()
  const displayName = identity.name || identity.email || 'Sessão ativa'

  useEffect(() => {
    if (!menuOpen) return
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === 'Escape') setMenuOpen(false) }
    window.addEventListener('keydown', closeOnEscape)
    return () => window.removeEventListener('keydown', closeOnEscape)
  }, [menuOpen])

  return (
    <div className="app-shell">
      <button className={`sidebar-backdrop ${menuOpen ? 'is-visible' : ''}`} aria-label="Fechar menu" onClick={() => setMenuOpen(false)} />
      <aside id="main-navigation" className={`sidebar ${menuOpen ? 'is-open' : ''}`}>
        <div className="brand">
          <span className="brand-mark">V</span>
          <span><strong>Vezza</strong><small>Gestão inteligente</small></span>
        </div>
        <div className="nav-label">Navegação</div>
        <nav className="sidebar-nav" aria-label="Navegação principal">
          {navigation.map((item) => (
            <NavLink key={item.to} to={item.to} onClick={() => setMenuOpen(false)}>
              <Icon name={item.icon} /><span>{item.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-foot">
          <span className="status-dot" />
          <span><strong>Ambiente seguro</strong><small>Conexão protegida</small></span>
        </div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <button className="icon-button menu-button" aria-label="Abrir menu" aria-controls="main-navigation" aria-expanded={menuOpen} onClick={() => setMenuOpen(true)}><Icon name="menu" /></button>
          <div className="topbar-spacer" />
          <div className="session-copy"><small>Conectado como</small><strong>{displayName}</strong></div>
          <div className="avatar" aria-hidden="true">{displayName.charAt(0).toUpperCase()}</div>
          <button className="logout-button" onClick={logout}><Icon name="logout" /><span>Sair</span></button>
        </header>
        <main className="page-area"><Outlet /></main>
      </div>
    </div>
  )
}
