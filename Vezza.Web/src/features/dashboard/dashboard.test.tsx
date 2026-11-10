import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { jsonResponse } from '../../test/test-utils'
import { DashboardPage } from './DashboardPage'

const clientes = [{ clienteId: 10, razaoSocial: 'Alfa Engenharia', cnpjCpf: '11' }]
const obras = [{ obraId: 20, clienteId: 10, descricao: 'Edifício Central' }]
const orcamentos = [
  { orcamentoId: 30, obraId: 20, data: '2026-01-05', objetoProposta: 'Proposta antiga', valor: 100.1, situacao: 'Aberto' },
  { orcamentoId: 31, obraId: 20, data: '2026-03-10', objetoProposta: 'Proposta recente', valor: 200.2, situacao: 'Aprovado' },
  { orcamentoId: 32, obraId: 20, data: 'inválida', objetoProposta: 'Sem data válida', valor: 0, situacao: 'Aberto' },
]

type Resource = 'clientes' | 'obras' | 'orcamentos'

function mockApi(options?: { clientes?: unknown[]; obras?: unknown[]; orcamentos?: unknown[]; errors?: Resource[]; pending?: boolean }) {
  return vi.fn().mockImplementation((url: string) => {
    if (options?.pending) return new Promise(() => undefined)
    const resource = (['clientes', 'obras', 'orcamentos'] as Resource[]).find((name) => url.endsWith(`/v1/${name}`))
    if (!resource) throw new Error(`URL não prevista: ${url}`)
    if (options?.errors?.includes(resource)) return jsonResponse({ statusCode: 500, errors: [`Falha em ${resource}`] }, 500)
    return jsonResponse({ statusCode: 200, data: options?.[resource] ?? { clientes, obras, orcamentos }[resource] })
  })
}

function renderDashboard() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter initialEntries={['/app/dashboard']}><Routes><Route path="/app/dashboard" element={<DashboardPage />} /><Route path="/app/orcamentos/:orcamentoId" element={<div>Detalhe aberto</div>} /></Routes></MemoryRouter></QueryClientProvider>)
}

describe('dashboard', () => {
  it('renderiza os quatro indicadores com dados retornados pela API', async () => {
    vi.stubGlobal('fetch', mockApi()); renderDashboard()
    const metrics = within(screen.getByLabelText('Indicadores gerenciais'))
    expect(await metrics.findByText(/300,30/)).toBeInTheDocument()
    expect(metrics.getAllByText('1')).toHaveLength(2)
    expect(metrics.getByText('3')).toBeInTheDocument()
    expect(metrics.getByText(/300,30/)).toBeInTheDocument()
  })

  it('soma somente os valores comerciais com precisão em centavos', async () => {
    vi.stubGlobal('fetch', mockApi()); renderDashboard()
    expect(await within(screen.getByLabelText('Indicadores gerenciais')).findByText(/300,30/)).toBeInTheDocument()
  })

  it('agrupa orçamentos por situação', async () => {
    vi.stubGlobal('fetch', mockApi()); renderDashboard()
    const panel = within(screen.getByRole('region', { name: 'Orçamentos por situação' }))
    expect(await panel.findByText('Aberto')).toBeInTheDocument()
    expect(panel.getByText('2 orçamentos')).toBeInTheDocument()
    expect(panel.getByText('Aprovado')).toBeInTheDocument()
    expect(panel.getByText('1 orçamento')).toBeInTheDocument()
  })

  it('ordena orçamentos recentes por data válida e deixa datas inválidas ao final', async () => {
    vi.stubGlobal('fetch', mockApi()); renderDashboard()
    await screen.findByText('Proposta recente')
    const panel = screen.getByRole('region', { name: 'Orçamentos recentes' })
    const proposals = within(panel).getAllByRole('link').filter((link) => link.getAttribute('href')?.match(/\/orcamentos\/\d+$/))
    expect(proposals.map((link) => link.textContent)).toEqual([
      expect.stringContaining('Proposta recente'), expect.stringContaining('Proposta antiga'), expect.stringContaining('Sem data válida'),
    ])
  })

  it('mantém relacionamentos ausentes visíveis sem quebrar a lista', async () => {
    vi.stubGlobal('fetch', mockApi({ obras: [] })); renderDashboard()
    expect(await screen.findAllByText('Obra #20 não encontrada')).not.toHaveLength(0)
    expect(screen.getByText('Proposta recente')).toBeInTheDocument()
  })

  it('apresenta estados vazios sem inventar métricas', async () => {
    vi.stubGlobal('fetch', mockApi({ clientes: [], obras: [], orcamentos: [] })); renderDashboard()
    expect(await screen.findByText('Nenhum orçamento para distribuir.')).toBeInTheDocument()
    expect(screen.getByText('Nenhum orçamento cadastrado.')).toBeInTheDocument()
    expect(within(screen.getByLabelText('Indicadores gerenciais')).getAllByText('0')).toHaveLength(3)
  })

  it('apresenta carregamento enquanto as consultas estão pendentes', () => {
    vi.stubGlobal('fetch', mockApi({ pending: true })); renderDashboard()
    expect(screen.getByLabelText('Carregando Clientes')).toBeInTheDocument()
    expect(screen.getByText('Carregando distribuição…')).toBeInTheDocument()
    expect(screen.getByText('Carregando orçamentos recentes…')).toBeInTheDocument()
  })

  it('não apresenta zero quando as consultas falham', async () => {
    vi.stubGlobal('fetch', mockApi({ errors: ['clientes', 'obras', 'orcamentos'] })); renderDashboard()
    expect(await screen.findAllByText('Indisponível')).toHaveLength(4)
    expect(screen.getByText('Distribuição indisponível.')).toBeInTheDocument()
    expect(screen.getByText('Orçamentos recentes indisponíveis.')).toBeInTheDocument()
  })

  it('preserva dados disponíveis quando uma relação falha parcialmente', async () => {
    vi.stubGlobal('fetch', mockApi({ errors: ['clientes'] })); renderDashboard()
    expect(await screen.findByText('Alguns dados não puderam ser carregados.')).toBeInTheDocument()
    expect(screen.getByText('Proposta recente')).toBeInTheDocument()
    expect(screen.getAllByText('Edifício Central')).not.toHaveLength(0)
    expect(screen.getAllByText('Relacionamento indisponível')).not.toHaveLength(0)
  })

  it('navega para os detalhes do orçamento', async () => {
    vi.stubGlobal('fetch', mockApi()); renderDashboard()
    fireEvent.click(await screen.findByRole('link', { name: /Proposta recente/ }))
    expect(screen.getByText('Detalhe aberto')).toBeInTheDocument()
  })
})
