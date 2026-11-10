import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { jsonResponse } from '../../test/test-utils'
import { ClienteFormPage } from './ClienteFormPage'
import { ClientesListPage } from './ClientesListPage'

const clientes = [
  { clienteId: 1, razaoSocial: 'Alfa Engenharia', cnpjCpf: '11.111.111/0001-11', cidade: 'Curitiba', estado: 'PR' },
  { clienteId: 2, razaoSocial: 'Beta Construções', cnpjCpf: '222.222.222-22', cidade: 'São Paulo', estado: 'SP' },
]

function renderRoutes(initialEntry = '/app/clientes') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[initialEntry]}><Routes>
    <Route path="/app/clientes" element={<ClientesListPage />} />
    <Route path="/app/clientes/novo" element={<ClienteFormPage />} />
    <Route path="/app/clientes/:clienteId/editar" element={<ClienteFormPage />} />
    <Route path="/app/clientes/:clienteId" element={<div>Detalhe aberto</div>} />
  </Routes></MemoryRouter></QueryClientProvider>)
}

describe('módulo de clientes', () => {
  it('lista clientes retornados pela API', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 200, data: clientes })))
    renderRoutes()
    expect(await screen.findByText('Alfa Engenharia')).toBeInTheDocument()
    expect(screen.getByText('Beta Construções')).toBeInTheDocument()
    expect(screen.getByText('2 clientes')).toBeInTheDocument()
  })

  it('apresenta estado vazio', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 200, data: [] })))
    renderRoutes()
    expect(await screen.findByText('Sua base de clientes está vazia')).toBeInTheDocument()
  })

  it('pesquisa por razão social e documento', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 200, data: clientes })))
    renderRoutes()
    await screen.findByText('Alfa Engenharia')
    const search = screen.getByLabelText('Pesquisar clientes')
    fireEvent.change(search, { target: { value: '222.222' } })
    expect(screen.getByText('Beta Construções')).toBeInTheDocument()
    expect(screen.queryByText('Alfa Engenharia')).not.toBeInTheDocument()
  })

  it('cadastra cliente válido', async () => {
    const fetchMock = vi.fn().mockImplementation((_url: string, init?: RequestInit) => {
      if (init?.method === 'POST') return jsonResponse({ statusCode: 201, data: { clienteId: 3, razaoSocial: 'Nova Empresa', cnpjCpf: '123' } }, 201)
      return jsonResponse({ statusCode: 200, data: [] })
    })
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes('/app/clientes/novo')
    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/razão social/i), 'Nova Empresa')
    await user.type(screen.getByLabelText(/cpf\/cnpj/i), '123')
    await user.click(screen.getByRole('button', { name: 'Cadastrar cliente' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'POST')).toBe(true))
    expect(await screen.findByText('Sua base de clientes está vazia')).toBeInTheDocument()
  })

  it('valida os campos obrigatórios sem chamar a API', async () => {
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes('/app/clientes/novo')
    fireEvent.click(screen.getByRole('button', { name: 'Cadastrar cliente' }))
    expect(await screen.findByText('Razão social é obrigatória.')).toBeInTheDocument()
    expect(screen.getByText('CPF/CNPJ é obrigatório.')).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('carrega e edita um cliente', async () => {
    const fetchMock = vi.fn().mockImplementation((_url: string, init?: RequestInit) => {
      if (init?.method === 'PUT') return jsonResponse({ statusCode: 200, data: { ...clientes[0], razaoSocial: 'Alfa Atualizada' } })
      return jsonResponse({ statusCode: 200, data: clientes[0] })
    })
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes('/app/clientes/1/editar')
    const name = await screen.findByLabelText(/razão social/i)
    fireEvent.change(name, { target: { value: 'Alfa Atualizada' } })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'PUT')).toBe(true))
  })

  it('exclui após confirmação explícita', async () => {
    const fetchMock = vi.fn().mockImplementation((_url: string, init?: RequestInit) => {
      if (init?.method === 'DELETE') return jsonResponse({ statusCode: 200, data: clientes[0] })
      return jsonResponse({ statusCode: 200, data: clientes })
    })
    vi.stubGlobal('fetch', fetchMock)
    renderRoutes()
    await screen.findByText('Alfa Engenharia')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Alfa Engenharia' }))
    const dialog = screen.getByRole('dialog')
    expect(within(dialog).getByText('Excluir cliente?')).toBeInTheDocument()
    fireEvent.click(within(dialog).getByRole('button', { name: 'Confirmar exclusão' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(true))
  })

  it('exibe regra de exclusão bloqueada por obras vinculadas', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation((_url: string, init?: RequestInit) => {
      if (init?.method === 'DELETE') return jsonResponse({ statusCode: 400, errors: ['Cliente vinculado a obras não pode ser excluído'] }, 400)
      return jsonResponse({ statusCode: 200, data: clientes })
    }))
    renderRoutes()
    await screen.findByText('Alfa Engenharia')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Alfa Engenharia' }))
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Confirmar exclusão' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Cliente vinculado a obras não pode ser excluído')
  })

  it('exibe erros retornados pela API no cadastro', async () => {
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 400, errors: ['Documento já cadastrado'] }, 400)))
    renderRoutes('/app/clientes/novo')
    fireEvent.change(screen.getByLabelText(/razão social/i), { target: { value: 'Empresa Válida' } })
    fireEvent.change(screen.getByLabelText(/cpf\/cnpj/i), { target: { value: '123' } })
    fireEvent.click(screen.getByRole('button', { name: 'Cadastrar cliente' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Documento já cadastrado')
  })
})
