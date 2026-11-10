export interface OrcamentoItem {
  orcamentoItemId: number
  orcamentoId: number
  descricao: string
  valor?: number
}

export interface OrcamentoItemInput { descricao: string; valor: string }
export interface OrcamentoItemPayload { orcamentoId: number; descricao: string; valor: number }
export interface UpdateOrcamentoItemPayload extends OrcamentoItemPayload { orcamentoItemId: number }
export const emptyOrcamentoItemInput: OrcamentoItemInput = { descricao: '', valor: '' }
