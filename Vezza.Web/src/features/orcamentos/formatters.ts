import { parseApiDate } from '../../utils/date'

export function formatCurrency(value?: number) {
  return value == null ? 'Não informado' : new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

export function formatDate(value?: string, long = false) {
  if (!value) return 'Não informada'
  const date = parseApiDate(value)
  return date ? new Intl.DateTimeFormat('pt-BR', long ? { dateStyle: 'long' } : undefined).format(date) : 'Não informada'
}

export function dateInputValue(value?: string) {
  if (!value) return ''
  return value.slice(0, 10)
}
