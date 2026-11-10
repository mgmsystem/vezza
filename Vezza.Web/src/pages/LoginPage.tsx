import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { Icon } from '../components/Icon'
import { useAuth } from '../features/auth/auth-context'
import { ApiError } from '../services/api-client'

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const { login, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  if (isAuthenticated) return <Navigate to="/app/dashboard" replace />

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!email.trim() || !password) {
      setError('Informe seu e-mail e sua senha para continuar.')
      return
    }
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      setError('Informe um endereço de e-mail válido.')
      return
    }

    setIsLoading(true)
    try {
      await login({ email: email.trim(), senha: password })
      const destination = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname
      navigate(destination || '/app/dashboard', { replace: true })
    } catch (cause) {
      if (cause instanceof ApiError) {
        setError(cause.status === 401 ? 'E-mail ou senha inválidos. Verifique os dados e tente novamente.' : cause.message)
      } else {
        setError(cause instanceof Error ? cause.message : 'Não foi possível entrar agora.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="login-page">
      <section className="login-story" aria-label="Apresentação do Vezza">
        <div className="story-grid" />
        <div className="story-content">
          <div className="brand brand-light"><span className="brand-mark">V</span><span><strong>Vezza</strong><small>Gestão inteligente</small></span></div>
          <div className="story-message">
            <span className="eyebrow">Gestão que transforma</span>
            <h1>Controle claro.<br /><em>Decisões melhores.</em></h1>
            <p>Clientes, obras e orçamentos reunidos em uma experiência simples e confiável.</p>
          </div>
          <div className="story-footer"><span className="status-dot" /> Plataforma de gestão integrada</div>
        </div>
      </section>

      <main className="login-panel">
        <div className="login-card">
          <div className="mobile-brand"><span className="brand-mark">V</span><strong>Vezza</strong></div>
          <span className="login-kicker">Bem-vindo de volta</span>
          <h2>Acesse sua conta</h2>
          <p className="login-intro">Use suas credenciais para entrar no ambiente de gestão.</p>

          <form onSubmit={handleSubmit} noValidate>
            <label htmlFor="email">E-mail</label>
            <input id="email" type="email" autoComplete="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="seu@email.com" disabled={isLoading} />
            <label htmlFor="password">Senha</label>
            <input id="password" type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Digite sua senha" disabled={isLoading} />

            {error && <div className="form-error" role="alert">{error}</div>}

            <button className="primary-button" type="submit" disabled={isLoading}>
              <span>{isLoading ? 'Conectando…' : 'Entrar no Vezza'}</span>
              {!isLoading && <Icon name="arrow" />}
            </button>
          </form>
          <div className="security-note"><Icon name="lock" size={16} /> Sua sessão é protegida e expira automaticamente.</div>
        </div>
      </main>
    </div>
  )
}
