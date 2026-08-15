import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api, tokenStore } from '@/api/client'
import { CompanyContext } from '@/app/contexts'
import type { CompanyContextValue } from '@/app/contexts'

const SELECTED_COMPANY_KEY = 'govai.selectedCompany'

export default function CompanyProvider({ children }: { children: ReactNode }) {
  const [selectedCompanyId, setSelectedCompanyId] = useState<string | null>(() =>
    localStorage.getItem(SELECTED_COMPANY_KEY),
  )

  const {
    data: companies = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['companies'],
    queryFn: api.listCompanies,
    enabled: Boolean(tokenStore.get()),
  })

  // İlk yüklemede veya seçili firma artık listede yoksa ilk firmaya düş.
  useEffect(() => {
    if (companies.length === 0) return

    const stillExists = companies.some((company) => company.id === selectedCompanyId)
    if (!stillExists) {
      setSelectedCompanyId(companies[0].id)
      localStorage.setItem(SELECTED_COMPANY_KEY, companies[0].id)
    }
  }, [companies, selectedCompanyId])

  const selectCompany = useCallback((id: string) => {
    setSelectedCompanyId(id)
    localStorage.setItem(SELECTED_COMPANY_KEY, id)
  }, [])

  const value = useMemo<CompanyContextValue>(
    () => ({ companies, selectedCompanyId, selectCompany, isLoading, error }),
    [companies, selectedCompanyId, selectCompany, isLoading, error],
  )

  return <CompanyContext.Provider value={value}>{children}</CompanyContext.Provider>
}
