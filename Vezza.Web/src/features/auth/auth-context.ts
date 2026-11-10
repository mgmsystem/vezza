import { createContext, useContext } from 'react'
import type { LoginRequest, SessionIdentity } from '../../types/auth'

export interface AuthContextValue {
  isAuthenticated: boolean
  identity: SessionIdentity
  login: (credentials: LoginRequest) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth deve ser usado dentro de AuthProvider.')
  return context
}
