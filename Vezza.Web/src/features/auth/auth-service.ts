import { apiRequest } from '../../services/api-client'
import type { LoginRequest, LoginResponse } from '../../types/auth'

export async function authenticate(credentials: LoginRequest) {
  const result = await apiRequest<LoginResponse>('/v1/usuarios/autenticar', {
    method: 'POST',
    body: JSON.stringify(credentials),
  })

  if (!result.data?.token) throw new Error('A API não retornou um token de acesso válido.')
  return result.data
}
