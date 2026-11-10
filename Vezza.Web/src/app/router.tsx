import { Navigate, createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { ProtectedRoute } from '../features/auth/ProtectedRoute'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { LoginPage } from '../pages/LoginPage'
import { ClientesListPage } from '../features/clientes/ClientesListPage'
import { ClienteFormPage } from '../features/clientes/ClienteFormPage'
import { ClienteDetailPage } from '../features/clientes/ClienteDetailPage'
import { ObrasListPage } from '../features/obras/ObrasListPage'
import { ObraFormPage } from '../features/obras/ObraFormPage'
import { ObraDetailPage } from '../features/obras/ObraDetailPage'
import { OrcamentosListPage } from '../features/orcamentos/OrcamentosListPage'
import { OrcamentoFormPage } from '../features/orcamentos/OrcamentoFormPage'
import { OrcamentoDetailPage } from '../features/orcamentos/OrcamentoDetailPage'

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    element: <ProtectedRoute />,
    children: [{
      path: '/app',
      element: <AppLayout />,
      children: [
        { index: true, element: <Navigate to="dashboard" replace /> },
        { path: 'dashboard', element: <DashboardPage /> },
        { path: 'clientes', element: <ClientesListPage /> },
        { path: 'clientes/novo', element: <ClienteFormPage /> },
        { path: 'clientes/:clienteId', element: <ClienteDetailPage /> },
        { path: 'clientes/:clienteId/editar', element: <ClienteFormPage /> },
        { path: 'obras', element: <ObrasListPage /> },
        { path: 'obras/nova', element: <ObraFormPage /> },
        { path: 'obras/:obraId', element: <ObraDetailPage /> },
        { path: 'obras/:obraId/editar', element: <ObraFormPage /> },
        { path: 'orcamentos', element: <OrcamentosListPage /> },
        { path: 'orcamentos/novo', element: <OrcamentoFormPage /> },
        { path: 'orcamentos/:orcamentoId', element: <OrcamentoDetailPage /> },
        { path: 'orcamentos/:orcamentoId/editar', element: <OrcamentoFormPage /> },
      ],
    }],
  },
  { path: '*', element: <Navigate to="/app/dashboard" replace /> },
])
