import { createContext, useContext } from 'react'
import type { CompanySummary, LoginResponse } from '@/api/types'

/**
 * Context nesneleri ve hook'ları burada, provider bileşenlerinden ayrı tutulur.
 * Bileşen dosyalarının yalnızca bileşen dışa aktarması, Vite'ın hızlı yenileme (fast refresh)
 * davranışının doğru çalışması için gereklidir.
 */

export type SessionUser = LoginResponse['user']

export interface AuthContextValue {
  user: SessionUser | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth, AuthProvider içinde kullanılmalıdır.')
  }
  return context
}

export interface CompanyContextValue {
  companies: CompanySummary[]
  selectedCompanyId: string | null
  selectCompany: (id: string) => void
  isLoading: boolean
  error: unknown
}

export const CompanyContext = createContext<CompanyContextValue | null>(null)

export function useCompanies(): CompanyContextValue {
  const context = useContext(CompanyContext)
  if (!context) {
    throw new Error('useCompanies, CompanyProvider içinde kullanılmalıdır.')
  }
  return context
}
