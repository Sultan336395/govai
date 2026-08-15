import { Navigate, Route, Routes } from 'react-router-dom'
import AppLayout from '@/app/AppLayout'
import CompanyProvider from '@/app/CompanyProvider'
import { useAuth } from '@/app/contexts'
import CompanyPage from '@/pages/CompanyPage'
import DashboardPage from '@/pages/DashboardPage'
import EligibilityDetailPage from '@/pages/EligibilityDetailPage'
import LoginPage from '@/pages/LoginPage'
import MatchesPage from '@/pages/MatchesPage'
import NotificationsPage from '@/pages/NotificationsPage'
import OpportunitiesPage from '@/pages/OpportunitiesPage'
import SimulationPage from '@/pages/SimulationPage'
import SourcesPage from '@/pages/SourcesPage'

function ProtectedRoutes() {
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return (
    <CompanyProvider>
      <AppLayout />
    </CompanyProvider>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoutes />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/matches" element={<MatchesPage />} />
        <Route path="/matches/:assessmentId" element={<EligibilityDetailPage />} />
        <Route path="/opportunities" element={<OpportunitiesPage />} />
        <Route path="/company" element={<CompanyPage />} />
        <Route path="/simulation" element={<SimulationPage />} />
        <Route path="/notifications" element={<NotificationsPage />} />
        <Route path="/sources" element={<SourcesPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
