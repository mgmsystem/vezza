import type { ApiErrorDetails, GenericResult } from '../types/api'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly messages: string[]

  constructor({ status, messages }: ApiErrorDetails) {
    super(messages[0] ?? 'Não foi possível concluir a solicitação.')
    this.name = 'ApiError'
    this.status = status
    this.messages = messages
  }
}

let accessToken: string | null = null
let unauthorizedHandler: (() => void) | null = null

export function setApiAccessToken(token: string | null) {
  accessToken = token
}

export function setUnauthorizedHandler(handler: (() => void) | null) {
  unauthorizedHandler = handler
}

function friendlyMessage(status: number) {
  if (status === 401) return 'Sua sessão expirou ou as credenciais são inválidas.'
  if (status === 429) return 'Muitas tentativas. Aguarde um momento e tente novamente.'
  if (status >= 500) return 'O serviço está temporariamente indisponível.'
  return 'Não foi possível concluir a solicitação.'
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<GenericResult<T>> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')

  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  } catch {
    throw new ApiError({
      status: 0,
      messages: ['Não foi possível conectar à API. Verifique se o serviço está em execução.'],
    })
  }

  const contentType = response.headers.get('content-type') ?? ''
  const body = contentType.includes('application/json')
    ? ((await response.json()) as GenericResult<T>)
    : undefined

  if (!response.ok) {
    if (response.status === 401 && accessToken) unauthorizedHandler?.()
    throw new ApiError({
      status: response.status,
      messages: body?.errors?.length ? body.errors : [friendlyMessage(response.status)],
    })
  }

  if (!body) {
    throw new ApiError({ status: response.status, messages: ['A API retornou uma resposta inesperada.'] })
  }

  return body
}
