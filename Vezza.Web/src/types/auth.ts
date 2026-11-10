export interface LoginRequest {
  email: string
  senha: string
}

export interface LoginResponse {
  token: string
  type: string
}

export interface SessionIdentity {
  name?: string
  email?: string
}

export interface JwtPayload {
  exp?: number
  unique_name?: string
  email?: string
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string
}
