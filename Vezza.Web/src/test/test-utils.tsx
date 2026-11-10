import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'

export function makeToken(expiresAt = Date.now() + 3_600_000) {
  const payload = btoa(JSON.stringify({ exp: Math.floor(expiresAt / 1000), unique_name: 'Pessoa Teste', email: 'teste@vezza.dev' }))
  return `header.${payload}.signature`
}

export function jsonResponse(body: unknown, status = 200) {
  return Promise.resolve(new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))
}

export function renderWithProviders(element: ReactElement, route = '/') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return {
    queryClient,
    ...render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[route]}>{element}</MemoryRouter></QueryClientProvider>),
  }
}
