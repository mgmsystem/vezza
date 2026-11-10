import { describe, expect, it } from 'vitest'
import { parseApiDate } from './date'

describe('parseApiDate', () => {
  it('interpreta datas sem horário no calendário local', () => {
    const date = parseApiDate('2026-03-10')
    expect(date).not.toBeNull()
    expect(date?.getFullYear()).toBe(2026)
    expect(date?.getMonth()).toBe(2)
    expect(date?.getDate()).toBe(10)
  })

  it('rejeita datas inválidas', () => {
    expect(parseApiDate('data-inválida')).toBeNull()
  })
})
