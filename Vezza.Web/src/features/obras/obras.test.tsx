import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { jsonResponse } from '../../test/test-utils'
import { ClienteObrasSection } from './ClienteObrasSection'
import { ObraDetailPage } from './ObraDetailPage'
import { ObraFormPage } from './ObraFormPage'
import { ObrasListPage } from './ObrasListPage'

const clientes = [{ clienteId: 10, razaoSocial: 'Alfa Engenharia', cnpjCpf: '11.111.111/0001-11' }]
const obras = [{ obraId: 1, clienteId: 10, descricao: 'Edifício Central', cidade: 'Curitiba', estado: 'PR', dataCadastro: '2026-01-10T00:00:00' }]

function mockApi(options?: { obras?: unknown[]; deleteError?: string }) {
  return vi.fn().mockImplementation((url: string, init?: RequestInit) => {
    if (init?.method === 'DELETE') return options?.deleteError ? jsonResponse({ statusCode: 400, errors: [options.deleteError] }, 400) : jsonResponse({ statusCode: 200, data: obras[0] })
    if (init?.method === 'POST') return jsonResponse({ statusCode: 201, data: obras[0] }, 201)
    if (init?.method === 'PUT') return jsonResponse({ statusCode: 200, data: { ...obras[0], descricao: 'Edifício Renovado' } })
    if (url.match(/\/v1\/obras\/1$/)) return jsonResponse({ statusCode: 200, data: obras[0] })
    if (url.match(/\/v1\/clientes\/10$/)) return jsonResponse({ statusCode: 200, data: clientes[0] })
    if (url.endsWith('/v1/clientes')) return jsonResponse({ statusCode: 200, data: clientes })
    if (url.endsWith('/v1/obras')) return jsonResponse({ statusCode: 200, data: options?.obras ?? obras })
    if (url.endsWith('/v1/orcamentos')) return jsonResponse({ statusCode: 200, data: [] })
    throw new Error(`URL não prevista: ${url}`)
  })
}

function renderRoutes(entry = '/app/obras', element?: React.ReactNode) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter initialEntries={[entry]}>{element ?? <Routes>
    <Route path="/app/obras" element={<ObrasListPage />} />
    <Route path="/app/obras/nova" element={<ObraFormPage />} />
    <Route path="/app/obras/:obraId/editar" element={<ObraFormPage />} />
    <Route path="/app/obras/:obraId" element={<ObraDetailPage />} />
    <Route path="/app/clientes/:clienteId" element={<div>Cliente aberto</div>} />
  </Routes>}</MemoryRouter></QueryClientProvider>)
}

describe('módulo de obras', () => {
  it('lista obras associadas ao nome do cliente', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes()
    expect(await screen.findByText('Edifício Central')).toBeInTheDocument()
    expect(screen.getByText('Alfa Engenharia')).toBeInTheDocument()
    expect(screen.getByText('Curitiba / PR')).toBeInTheDocument()
  })

  it('pesquisa por cliente e localização', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes()
    await screen.findByText('Edifício Central')
    fireEvent.change(screen.getByLabelText('Pesquisar obras'), { target: { value: 'alfa' } })
    expect(screen.getByText('Edifício Central')).toBeInTheDocument()
    fireEvent.change(screen.getByLabelText('Pesquisar obras'), { target: { value: 'inexistente' } })
    expect(screen.getByText('Nenhuma obra encontrada')).toBeInTheDocument()
  })

  it('apresenta estado vazio', async () => {
    vi.stubGlobal('fetch', mockApi({ obras: [] }))
    renderRoutes()
    expect(await screen.findByText('Nenhuma obra cadastrada')).toBeInTheDocument()
  })

  it('cadastra obra com cliente selecionado', async () => {
    const fetchMock = mockApi()
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes('/app/obras/nova')
    const user = userEvent.setup()
    await user.selectOptions(await screen.findByLabelText(/cliente vinculado/i), '10')
    await user.type(screen.getByLabelText(/descrição da obra/i), 'Nova unidade')
    await user.click(screen.getByRole('button', { name: 'Cadastrar obra' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'POST')).toBe(true))
  })

  it('valida cliente e descrição obrigatórios', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes('/app/obras/nova')
    fireEvent.click(await screen.findByRole('button', { name: 'Cadastrar obra' }))
    expect(screen.getByText('Selecione um cliente válido.')).toBeInTheDocument()
    expect(screen.getByText('Descrição é obrigatória.')).toBeInTheDocument()
  })

  it('carrega e edita a obra', async () => {
    const fetchMock = mockApi()
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes('/app/obras/1/editar')
    const description = await screen.findByLabelText(/descrição da obra/i)
    fireEvent.change(description, { target: { value: 'Edifício Renovado' } })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'PUT')).toBe(true))
  })

  it('exclui após confirmação', async () => {
    const fetchMock = mockApi()
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes()
    await screen.findByText('Edifício Central')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Edifício Central' }))
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Confirmar exclusão' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(true))
  })

  it('exibe bloqueio de exclusão por orçamento', async () => {
    vi.stubGlobal('fetch', mockApi({ deleteError: 'Obra vinculada a orçamentos não pode ser excluída' }))
    renderRoutes()
    await screen.findByText('Edifício Central')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Edifício Central' }))
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Confirmar exclusão' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Obra vinculada a orçamentos não pode ser excluída')
  })

  it('mostra obras relacionadas na tela do cliente', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes('/app/clientes/10', <ClienteObrasSection clienteId={10} />)
    expect(await screen.findByText('Edifício Central')).toBeInTheDocument()
    expect(screen.getByText('Curitiba / PR')).toBeInTheDocument()
  })

  it('pré-seleciona cliente ao criar obra pelo detalhe do cliente', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes('/app/obras/nova?clienteId=10')
    expect(await screen.findByLabelText(/cliente vinculado/i)).toHaveValue('10')
  })

  it('navega do detalhe da obra para o cliente', async () => {
    vi.stubGlobal('fetch', mockApi())
    renderRoutes('/app/obras/1')
    const link = await screen.findByRole('link', { name: /alfa engenharia/i })
    expect(link).toHaveAttribute('href', '/app/clientes/10')
    fireEvent.click(link)
    expect(screen.getByText('Cliente aberto')).toBeInTheDocument()
  })
})
