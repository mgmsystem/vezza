export interface Orcamento {
  orcamentoId: number
  obraId: number
  data?: string
  objetoProposta?: string
  valor?: number
  condPagamento?: string
  prazos?: string
  comentarios?: string
  notasGerais?: string
  inicioObra?: string
  terminoObra?: string
  situacao?: string
}

export interface OrcamentoInput {
  obraId: number
  data: string
  objetoProposta: string
  valor: string
  condPagamento: string
  prazos: string
  comentarios: string
  notasGerais: string
  inicioObra: string
  terminoObra: string
  situacao: string
}

export interface OrcamentoPayload {
  obraId: number
  data: string
  objetoProposta: string
  valor: number
  condPagamento: string
  prazos: string
  comentarios: string
  notasGerais: string
  inicioObra?: string
  terminoObra?: string
  situacao: string
}

export interface UpdateOrcamentoPayload extends OrcamentoPayload { orcamentoId: number }

export const emptyOrcamentoInput: OrcamentoInput = {
  obraId: 0, data: '', objetoProposta: '', valor: '', condPagamento: '', prazos: '', comentarios: '', notasGerais: '', inicioObra: '', terminoObra: '', situacao: '',
}
