import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { LoginPage } from '../../pages/LoginPage'
import { jsonResponse, makeToken } from '../../test/test-utils'
import { AuthProvider } from './AuthProvider'
import { ProtectedRoute } from './ProtectedRoute'
import { useAuth } from './auth-context'

function SessionProbe() {
  const { isAuthenticated, logout } = useAuth()
  return <><span>{isAuthenticated ? 'autenticado' : 'anônimo'}</span><button onClick={logout}>Sair agora</button></>
}

function AuthTestProvider({ children, client = new QueryClient() }: PropsWithChildren<{ client?: QueryClient }>) {
  return <QueryClientProvider client={client}><AuthProvider>{children}</AuthProvider></QueryClientProvider>
}

function renderLogin() {
  return render(<MemoryRouter initialEntries={['/login']}><AuthTestProvider><Routes><Route path="/login" element={<LoginPage />} /><Route path="/app/dashboard" element={<div>Área protegida</div>} /></Routes></AuthTestProvider></MemoryRouter>)
}

describe('autenticação', () => {
  it('preenche as credenciais públicas ao solicitar a demonstração', async () => {
    renderLogin()
    const user = userEvent.setup()

    await user.click(screen.getByRole('button', { name: /experimentar demonstração/i }))

    expect(screen.getByLabelText('E-mail')).toHaveValue('demo@vezza.app')
    expect(screen.getByLabelText('Senha')).toHaveValue('VezzaDemo2026!')
  })

  it('realiza login bem-sucedido e persiste a sessão', async () => {
    const token = makeToken()
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 200, data: { token, type: 'Bearer' } })))
    renderLogin()
    const user = userEvent.setup()
    await user.type(screen.getByLabelText('E-mail'), 'teste@vezza.dev')
    await user.type(screen.getByLabelText('Senha'), 'segredo')
    await user.click(screen.getByRole('button', { name: /entrar no vezza/i }))
    expect(await screen.findByText('Área protegida')).toBeInTheDocument()
    expect(sessionStorage.getItem('vezza.session.token')).toBe(token)
  })

  it('exibe mensagem para credenciais inválidas', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 401, errors: ['Email ou senha inválidos!'] }, 401)))
    renderLogin()
    fireEvent.change(screen.getByLabelText('E-mail'), { target: { value: 'teste@vezza.dev' } })
    fireEvent.change(screen.getByLabelText('Senha'), { target: { value: 'incorreta' } })
    fireEvent.click(screen.getByRole('button', { name: /entrar no vezza/i }))
    expect(await screen.findByRole('alert')).toHaveTextContent('E-mail ou senha inválidos')
  })

  it('exibe mensagem amigável para HTTP 429', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 429, errors: ['Muitas tentativas. Tente novamente mais tarde.'] }, 429)))
    renderLogin()
    fireEvent.change(screen.getByLabelText('E-mail'), { target: { value: 'teste@vezza.dev' } })
    fireEvent.change(screen.getByLabelText('Senha'), { target: { value: 'segredo' } })
    fireEvent.click(screen.getByRole('button', { name: /entrar no vezza/i }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Muitas tentativas')
  })

  it('restaura sessão válida e permite logout', async () => {
    sessionStorage.setItem('vezza.session.token', makeToken())
    render(<AuthTestProvider><SessionProbe /></AuthTestProvider>)
    expect(screen.getByText('autenticado')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Sair agora' }))
    await waitFor(() => expect(screen.getByText('anônimo')).toBeInTheDocument())
    expect(sessionStorage.getItem('vezza.session.token')).toBeNull()
  })

  it('descarta sessão expirada', () => {
    sessionStorage.setItem('vezza.session.token', makeToken(Date.now() - 60_000))
    render(<AuthTestProvider><SessionProbe /></AuthTestProvider>)
    expect(screen.getByText('anônimo')).toBeInTheDocument()
    expect(sessionStorage.getItem('vezza.session.token')).toBeNull()
  })

  it('redireciona rota protegida para login', () => {
    render(<MemoryRouter initialEntries={['/app/clientes']}><AuthTestProvider><Routes><Route element={<ProtectedRoute />}><Route path="/app/clientes" element={<div>Clientes privados</div>} /></Route><Route path="/login" element={<div>Página de login</div>} /></Routes></AuthTestProvider></MemoryRouter>)
    expect(screen.getByText('Página de login')).toBeInTheDocument()
    expect(screen.queryByText('Clientes privados')).not.toBeInTheDocument()
  })

  it('remove dados em cache ao encerrar a sessão', async () => {
    const client = new QueryClient()
    client.setQueryData(['clientes'], [{ clienteId: 1, razaoSocial: 'Privado' }])
    sessionStorage.setItem('vezza.session.token', makeToken())
    render(<AuthTestProvider client={client}><SessionProbe /></AuthTestProvider>)

    fireEvent.click(screen.getByRole('button', { name: 'Sair agora' }))

    await waitFor(() => expect(client.getQueryData(['clientes'])).toBeUndefined())
  })
})
