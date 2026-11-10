import { useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { setApiAccessToken, setUnauthorizedHandler } from '../../services/api-client'
import type { LoginRequest } from '../../types/auth'
import { authenticate } from './auth-service'
import { AuthContext } from './auth-context'
import { getSessionIdentity, isTokenExpired } from './token'

const SESSION_KEY = 'vezza.session.token'

function readStoredToken() {
  const stored = sessionStorage.getItem(SESSION_KEY)
  if (!stored || isTokenExpired(stored)) {
    sessionStorage.removeItem(SESSION_KEY)
    return null
  }
  return stored
}

export function AuthProvider({ children }: PropsWithChildren) {
  const queryClient = useQueryClient()
  const [token, setToken] = useState<string | null>(readStoredToken)

  const logout = useCallback(() => {
    sessionStorage.removeItem(SESSION_KEY)
    setToken(null)
    setApiAccessToken(null)
    queryClient.clear()
  }, [queryClient])

  useEffect(() => {
    setApiAccessToken(token)
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout, token])

  useEffect(() => {
    if (!token) return
    const interval = window.setInterval(() => {
      if (isTokenExpired(token)) logout()
    }, 30_000)
    return () => window.clearInterval(interval)
  }, [logout, token])

  const login = useCallback(async (credentials: LoginRequest) => {
    const response = await authenticate(credentials)
    if (isTokenExpired(response.token)) throw new Error('A API retornou uma sessão expirada.')
    sessionStorage.setItem(SESSION_KEY, response.token)
    setApiAccessToken(response.token)
    setToken(response.token)
  }, [])

  const value = useMemo(
    () => ({
      isAuthenticated: Boolean(token),
      identity: token ? getSessionIdentity(token) : {},
      login,
      logout,
    }),
    [login, logout, token],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
