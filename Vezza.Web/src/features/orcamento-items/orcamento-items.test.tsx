import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { jsonResponse } from '../../test/test-utils'
import { OrcamentoItemsSection } from './OrcamentoItemsSection'

const initialItems = [
  { orcamentoItemId: 1, orcamentoId: 30, descricao: 'Demolição', valor: 100.1 },
  { orcamentoItemId: 2, orcamentoId: 30, descricao: 'Alvenaria', valor: 200.2 },
  { orcamentoItemId: 3, orcamentoId: 99, descricao: 'Outro orçamento', valor: 999 },
]

function apiMock(options?: { items?: typeof initialItems; listError?: string; mutationError?: string }) {
  let items = [...(options?.items ?? initialItems)]
  return vi.fn().mockImplementation((url: string, init?: RequestInit) => {
    if (url.endsWith('/v1/orcamento-items') && !init?.method) {
      if (options?.listError) return jsonResponse({ statusCode: 500, errors: [options.listError] }, 500)
      return jsonResponse({ statusCode: 200, data: items })
    }
    if (options?.mutationError) return jsonResponse({ statusCode: 400, errors: [options.mutationError] }, 400)
    if (init?.method === 'POST') {
      const body = JSON.parse(init.body as string)
      const created = { ...body, orcamentoItemId: 4 }; items = [...items, created]
      return jsonResponse({ statusCode: 201, data: created }, 201)
    }
    if (init?.method === 'PUT') {
      const body = JSON.parse(init.body as string)
      items = items.map((item) => item.orcamentoItemId === body.orcamentoItemId ? body : item)
      return jsonResponse({ statusCode: 200, data: body })
    }
    if (init?.method === 'DELETE') {
      const id = Number(url.split('/').at(-1)); const deleted = items.find((item) => item.orcamentoItemId === id)
      items = items.filter((item) => item.orcamentoItemId !== id)
      return jsonResponse({ statusCode: 200, data: deleted })
    }
    throw new Error(`URL não prevista: ${url}`)
  })
}

function renderSection(props = { orcamentoId: 30, valorOrcamento: 500 }) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return render(<QueryClientProvider client={client}><OrcamentoItemsSection {...props} /></QueryClientProvider>)
}

async function openAndFill(description = 'Pintura', value = '125.35') {
  const user = userEvent.setup()
  await user.click(await screen.findByRole('button', { name: 'Adicionar item' }))
  await user.type(screen.getByLabelText('Descrição *'), description)
  fireEvent.change(screen.getByLabelText('Valor *'), { target: { value } })
  return user
}

describe('itens do orçamento', () => {
  it('lista descrição e valor dos itens', async () => {
    vi.stubGlobal('fetch', apiMock()); renderSection()
    expect(await screen.findByText('Demolição')).toBeInTheDocument()
    expect(screen.getByText(/100,10/)).toBeInTheDocument()
  })

  it('filtra os itens pelo orçamento atual', async () => {
    vi.stubGlobal('fetch', apiMock()); renderSection()
    await screen.findByText('Demolição')
    expect(screen.queryByText('Outro orçamento')).not.toBeInTheDocument()
  })

  it('apresenta estado vazio', async () => {
    vi.stubGlobal('fetch', apiMock({ items: [] })); renderSection()
    expect(await screen.findByText('Nenhum item cadastrado')).toBeInTheDocument()
  })

  it('cadastra e atualiza a lista após a mutação', async () => {
    const fetchMock = apiMock({ items: [] }); vi.stubGlobal('fetch', fetchMock); renderSection()
    const user = await openAndFill(); await user.click(screen.getAllByRole('button', { name: 'Adicionar item' }).at(-1)!)
    expect(await screen.findByText('Pintura')).toBeInTheDocument()
    const post = fetchMock.mock.calls.find(([, init]) => (init as RequestInit)?.method === 'POST')
    expect(JSON.parse((post![1] as RequestInit).body as string)).toEqual({ orcamentoId: 30, descricao: 'Pintura', valor: 125.35 })
  })

  it('edita um item e mantém o orçamento contextual', async () => {
    const fetchMock = apiMock(); vi.stubGlobal('fetch', fetchMock); renderSection(); const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Editar Demolição' }))
    const description = screen.getByLabelText('Descrição *'); await user.clear(description); await user.type(description, 'Demolição revisada')
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }))
    expect(await screen.findByText('Demolição revisada')).toBeInTheDocument()
    const put = fetchMock.mock.calls.find(([, init]) => (init as RequestInit)?.method === 'PUT')
    expect(JSON.parse((put![1] as RequestInit).body as string).orcamentoId).toBe(30)
  })

  it('exclui somente após confirmação e atualiza a lista', async () => {
    const fetchMock = apiMock(); vi.stubGlobal('fetch', fetchMock); renderSection(); const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Excluir Demolição' }))
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Confirmar exclusão' }))
    await waitFor(() => expect(screen.queryByText('Demolição')).not.toBeInTheDocument())
    expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(true)
  })

  it('cancela a exclusão sem chamar a API', async () => {
    const fetchMock = apiMock(); vi.stubGlobal('fetch', fetchMock); renderSection(); const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Excluir Demolição' }))
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Cancelar' }))
    expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'DELETE')).toBe(false)
  })

  it('exibe erro ao carregar itens', async () => {
    vi.stubGlobal('fetch', apiMock({ listError: 'Falha ao consultar itens' })); renderSection()
    expect(await screen.findByRole('alert')).toHaveTextContent('Falha ao consultar itens')
  })

  it('exibe erros de mutação retornados pela API', async () => {
    vi.stubGlobal('fetch', apiMock({ items: [], mutationError: 'Item não foi salvo' })); renderSection()
    const user = await openAndFill(); await user.click(screen.getAllByRole('button', { name: 'Adicionar item' }).at(-1)!)
    expect(await screen.findByRole('alert')).toHaveTextContent('Item não foi salvo')
  })

  it('soma os itens em centavos sem artefatos de ponto flutuante', async () => {
    vi.stubGlobal('fetch', apiMock()); renderSection()
    await screen.findByText('Demolição')
    expect(within(screen.getByLabelText('Resumo dos valores')).getByText(/300,30/)).toBeInTheDocument()
  })

  it('mantém soma dos itens independente do valor do orçamento', async () => {
    vi.stubGlobal('fetch', apiMock()); renderSection({ orcamentoId: 30, valorOrcamento: 500 })
    await screen.findByText('Demolição')
    const summary = within(screen.getByLabelText('Resumo dos valores'))
    expect(summary.getByText(/500,00/)).toBeInTheDocument()
    expect(summary.getByText(/300,30/)).toBeInTheDocument()
    expect(summary.getByText(/199,70/)).toBeInTheDocument()
    expect(screen.getByText(/não altera o valor comercial/i)).toBeInTheDocument()
  })

  it('não envia campos que o backend não suporta', async () => {
    const fetchMock = apiMock({ items: [] }); vi.stubGlobal('fetch', fetchMock); renderSection()
    const user = await openAndFill(); await user.click(screen.getAllByRole('button', { name: 'Adicionar item' }).at(-1)!)
    await waitFor(() => expect(fetchMock.mock.calls.some(([, init]) => (init as RequestInit)?.method === 'POST')).toBe(true))
    const post = fetchMock.mock.calls.find(([, init]) => (init as RequestInit)?.method === 'POST')
    expect(Object.keys(JSON.parse((post![1] as RequestInit).body as string)).sort()).toEqual(['descricao', 'orcamentoId', 'valor'])
  })
})
