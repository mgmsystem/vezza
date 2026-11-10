import { describe, expect, it, vi } from 'vitest'
import { apiRequest, setApiAccessToken, setUnauthorizedHandler } from './api-client'
import { jsonResponse } from '../test/test-utils'

describe('apiRequest', () => {
  it('inclui automaticamente o token Bearer', async () => {
    const fetchMock = vi.fn().mockImplementation(() => jsonResponse({ statusCode: 200, data: [] }))
    vi.stubGlobal('fetch', fetchMock)
    setApiAccessToken('token-seguro')

    await apiRequest('/v1/clientes')

    const headers = (fetchMock.mock.calls[0][1] as RequestInit).headers as Headers
    expect(headers.get('Authorization')).toBe('Bearer token-seguro')
    setApiAccessToken(null)
  })

  it('encerra a sessão ao receber 401 autenticado', async () => {
    const unauthorized = vi.fn()
    vi.stubGlobal('fetch', vi.fn().mockImplementation(() => jsonResponse({ statusCode: 401, errors: ['Não autorizado'] }, 401)))
    setApiAccessToken('token-expirado')
    setUnauthorizedHandler(unauthorized)

    await expect(apiRequest('/v1/clientes')).rejects.toMatchObject({ status: 401 })
    expect(unauthorized).toHaveBeenCalledOnce()
    setApiAccessToken(null)
    setUnauthorizedHandler(null)
  })
})
