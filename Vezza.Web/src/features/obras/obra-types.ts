export interface Obra {
  obraId: number
  clienteId: number
  descricao: string
  cep?: string
  logradouro?: string
  numero?: string
  complemento?: string
  bairro?: string
  cidade?: string
  estado?: string
  dataCadastro?: string
}

export interface ObraInput {
  clienteId: number
  descricao: string
  cep: string
  logradouro: string
  numero: string
  complemento: string
  bairro: string
  cidade: string
  estado: string
}

export interface UpdateObraInput extends ObraInput { obraId: number }

export const emptyObraInput: ObraInput = {
  clienteId: 0, descricao: '', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', estado: '',
}
