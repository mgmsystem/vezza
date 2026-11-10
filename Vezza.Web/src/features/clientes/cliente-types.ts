export interface Cliente {
  clienteId: number
  razaoSocial: string
  cnpjCpf: string
  cep?: string
  logradouro?: string
  numero?: string
  complemento?: string
  bairro?: string
  cidade?: string
  estado?: string
  telefone?: string
  dataCadastro?: string
}

export interface ClienteInput {
  razaoSocial: string
  cnpjCpf: string
  cep: string
  logradouro: string
  numero: string
  complemento: string
  bairro: string
  cidade: string
  estado: string
  telefone: string
}

export interface UpdateClienteInput extends ClienteInput {
  clienteId: number
}

export const emptyClienteInput: ClienteInput = {
  razaoSocial: '', cnpjCpf: '', telefone: '', cep: '', logradouro: '', numero: '',
  complemento: '', bairro: '', cidade: '', estado: '',
}
