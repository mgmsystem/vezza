import type { JwtPayload, SessionIdentity } from '../../types/auth'

function decodePayload(token: string): JwtPayload | null {
  try {
    const encoded = token.split('.')[1]
    if (!encoded) return null
    const normalized = encoded.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(normalized)
        .split('')
        .map((character) => `%${character.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    )
    return JSON.parse(json) as JwtPayload
  } catch {
    return null
  }
}

export function isTokenExpired(token: string) {
  const payload = decodePayload(token)
  return !payload?.exp || payload.exp * 1000 <= Date.now()
}

export function getSessionIdentity(token: string): SessionIdentity {
  const payload = decodePayload(token)
  return {
    name:
      payload?.unique_name ??
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
    email:
      payload?.email ??
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
  }
}
