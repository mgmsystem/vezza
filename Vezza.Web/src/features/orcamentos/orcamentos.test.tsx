import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { jsonResponse } from '../../test/test-utils'
import { ObraOrcamentosSection } from './ObraOrcamentosSection'
import { OrcamentoDetailPage } from './OrcamentoDetailPage'
import { OrcamentoFormPage } from './OrcamentoFormPage'
import { OrcamentosListPage } from './OrcamentosListPage'

const clientes = [{ clienteId: 10, razaoSocial: 'Alfa Engenharia', cnpjCpf: '11.111.111/0001-11' }]
const obras = [{ obraId: 20, clienteId: 10, descricao: 'Edifício Central', cidade: 'Curitiba', estado: 'PR' }]
const orcamentos = [{ orcamentoId: 30, obraId: 20, data: '2026-11-10T00:00:00', objetoProposta: 'Reforma completa', valor: 15000, condPagamento: 'À vista', prazos: '60 dias', inicioObra: '2026-12-01', terminoObra: '2027-01-30', situacao: 'Aberto' }]

function mockApi(options?: { orcamentos?: unknown[]; orphan?: boolean; deleteError?: string; createError?: string }) {
  return vi.fn().mockImplementation((url: string, init?: RequestInit) => {
    if (init?.method === 'DELETE') return options?.deleteError ? jsonResponse({ statusCode: 500, errors: [options.deleteError] }, 500) : jsonResponse({ statusCode: 200, data: orcamentos[0] })
    if (init?.method === 'POST') return options?.createError ? jsonResponse({ statusCode: 400, errors: [options.createError] }, 400) : jsonResponse({ statusCode: 201, data: orcamentos[0] }, 201)
    if (init?.method === 'PUT') return jsonResponse({ statusCode: 200, data: { ...orcamentos[0], objetoProposta: 'Reforma revisada' } })
    if (url.match(/\/v1\/orcamentos\/30$/)) return jsonResponse({ statusCode: 200, data: orcamentos[0] })
    if (url.match(/\/v1\/obras\/20$/)) return jsonResponse({ statusCode: 200, data: obras[0] })
    if (url.match(/\/v1\/clientes\/10$/)) return jsonResponse({ statusCode: 200, data: clientes[0] })
    if (url.endsWith('/v1/orcamentos')) return jsonResponse({ statusCode: 200, data: options?.orphan ? [{ ...orcamentos[0], obraId: 999 }] : (options?.orcamentos ?? orcamentos) })
    if (url.endsWith('/v1/obras')) return jsonResponse({ statusCode: 200, data: obras })
    if (url.endsWith('/v1/clientes')) return jsonResponse({ statusCode: 200, data: clientes })
    throw new Error(`URL não prevista: ${url}`)
  })
}

function renderRoutes(entry = '/app/orcamentos', element?: React.ReactNode) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter initialEntries={[entry]}>{element ?? <Routes>
    <Route path="/app/orcamentos" element={<OrcamentosListPage />} />
    <Route path="/app/orcamentos/novo" element={<OrcamentoFormPage />} />
    <Route path="/app/orcamentos/:orcamentoId/editar" element={<OrcamentoFormPage />} />
    <Route path="/app/orcamentos/:orcamentoId" element={<OrcamentoDetailPage />} />
    <Route path="/app/obras/:obraId" element={<div>Obra aberta</div>} />
    <Route path="/app/clientes/:clienteId" element={<div>Cliente aberto</div>} />
  </Routes>}</MemoryRouter></QueryClientProvider>)
}

async function fillRequiredForm() {
  const user = userEvent.setup()
  await user.selectOptions(await screen.findByLabelText(/obra vinculada/i), '20')
  fireEvent.change(screen.getByLabelText(/^data/i), { target: { value: '2026-11-10' } })
  await user.type(screen.getByLabelText(/objeto da proposta/i), 'Reforma completa')
  fireEvent.change(screen.getByLabelText(/valor do orçamento/i), { target: { value: '15000.25' } })
  return user
}

describe('módulo de orçamentos', () => {
  it('lista orçamentos associados à obra e ao cliente', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes()
    expect(await screen.findByText('Reforma completa')).toBeInTheDocument()
    expect(screen.getByText('Edifício Central')).toBeInTheDocument()
    expect(screen.getByText('Alfa Engenharia')).toBeInTheDocument()
    expect(screen.getByText(/15\.000,00/)).toBeInTheDocument()
  })

  it('pesquisa por proposta, obra, cliente e situação', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes(); await screen.findByText('Reforma completa')
    const search = screen.getByLabelText('Pesquisar orçamentos')
    for (const term of ['Reforma', 'Edifício', 'Alfa', 'Aberto']) { fireEvent.change(search, { target: { value: term } }); expect(screen.getByText('Reforma completa')).toBeInTheDocument() }
    fireEvent.change(search, { target: { value: 'inexistente' } }); expect(screen.getByText('Nenhum orçamento encontrado')).toBeInTheDocument()
  })

  it('apresenta estado vazio', async () => {
    vi.stubGlobal('fetch', mockApi({ orcamentos: [] })); renderRoutes()
    expect(await screen.findByText('Nenhum orçamento cadastrado')).toBeInTheDocument()
  })

  it('cadastra orçamento preservando valor numérico', async () => {
    const fetchMock = mockApi(); vi.stubGlobal('fetch', fetchMock); renderRoutes('/app/orcamentos/novo')
    const user = await fillRequiredForm(); await user.click(screen.getByRole('button', { name: 'Cadastrar orçamento' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'POST')).toBe(true))
    const post = fetchMock.mock.calls.find(([, init]) => (init as RequestInit)?.method === 'POST')
    expect(JSON.parse((post![1] as RequestInit).body as string).valor).toBe(15000.25)
  })

  it('valida os campos obrigatórios', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes('/app/orcamentos/novo')
    fireEvent.click(await screen.findByRole('button', { name: 'Cadastrar orçamento' }))
    expect(screen.getByText('Selecione uma obra válida.')).toBeInTheDocument()
    expect(screen.getByText('Data é obrigatória.')).toBeInTheDocument()
    expect(screen.getByText('Objeto da proposta é obrigatório.')).toBeInTheDocument()
    expect(screen.getByText('Valor do orçamento é obrigatório.')).toBeInTheDocument()
  })

  it('carrega e edita um orçamento', async () => {
    const fetchMock = mockApi(); vi.stubGlobal('fetch', fetchMock); renderRoutes('/app/orcamentos/30/editar')
    const proposal = await screen.findByLabelText(/objeto da proposta/i)
    fireEvent.change(proposal, { target: { value: 'Reforma revisada' } }); fireEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'PUT')).toBe(true))
  })

  it('pré-seleciona uma obra existente', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes('/app/orcamentos/novo?obraId=20')
    expect(await screen.findByLabelText(/obra vinculada/i)).toHaveValue('20')
  })

  it('não pré-seleciona obra inexistente', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes('/app/orcamentos/novo?obraId=999')
    expect(await screen.findByLabelText(/obra vinculada/i)).toHaveValue('')
  })

  it('navega do orçamento para obra e cliente', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes('/app/orcamentos/30')
    expect(await screen.findByRole('link', { name: /edifício central/i })).toHaveAttribute('href', '/app/obras/20')
    expect(screen.getByRole('link', { name: /alfa engenharia/i })).toHaveAttribute('href', '/app/clientes/10')
  })

  it('mostra orçamentos relacionados na obra', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes('/app/obras/20', <ObraOrcamentosSection obraId={20} />)
    expect(await screen.findByText('Reforma completa')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /novo orçamento/i })).toHaveAttribute('href', '/app/orcamentos/novo?obraId=20')
  })

  it('exclui somente após confirmação', async () => {
    const fetchMock = mockApi(); vi.stubGlobal('fetch', fetchMock); renderRoutes(); await screen.findByText('Reforma completa')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Reforma completa' }))
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Confirmar exclusão' }))
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(true))
  })

  it('cancela a exclusão sem chamar a API', async () => {
    const fetchMock = mockApi(); vi.stubGlobal('fetch', fetchMock); renderRoutes(); await screen.findByText('Reforma completa')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Reforma completa' }))
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Cancelar' }))
    expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(false)
  })

  it('avisa sobre remoção de itens e fluxos', async () => {
    vi.stubGlobal('fetch', mockApi()); renderRoutes(); await screen.findByText('Reforma completa')
    fireEvent.click(screen.getByRole('button', { name: 'Excluir Reforma completa' }))
    expect(screen.getByRole('dialog')).toHaveTextContent('também removerá todos os itens e lançamentos financeiros')
  })

  it('exibe erros retornados pela API', async () => {
    vi.stubGlobal('fetch', mockApi({ createError: 'Falha ao salvar orçamento' })); renderRoutes('/app/orcamentos/novo')
    const user = await fillRequiredForm(); await user.click(screen.getByRole('button', { name: 'Cadastrar orçamento' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Falha ao salvar orçamento')
  })

  it('mantém relacionamentos inexistentes visíveis', async () => {
    vi.stubGlobal('fetch', mockApi({ orphan: true })); renderRoutes()
    expect(await screen.findByText('Reforma completa')).toBeInTheDocument()
    expect(screen.getByText('Obra #999 não encontrada')).toBeInTheDocument()
    expect(screen.getByText('Relacionamento indisponível')).toBeInTheDocument()
  })
})
