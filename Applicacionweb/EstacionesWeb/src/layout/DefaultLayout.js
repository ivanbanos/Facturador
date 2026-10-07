import React, { useEffect } from 'react'
import { Navigate } from 'react-router-dom'
import { AppContent, AppSidebar, AppFooter, AppHeader } from '../components/index'
import { getMsHastaExpiracion, isAuthenticated, redirectToLogin } from '../services/AuthService'

const DefaultLayout = () => {
  const autenticado = isAuthenticated()

  // Al expirar el token (1 hora) se cierra la sesion y se vuelve al inicio de sesion
  useEffect(() => {
    if (!autenticado) {
      return undefined
    }
    const timer = setTimeout(redirectToLogin, getMsHastaExpiracion())
    return () => clearTimeout(timer)
  }, [autenticado])

  if (!autenticado) {
    return <Navigate to="/login" replace />
  }

  return (
    <div>
      <AppSidebar />
      <div className="wrapper d-flex flex-column min-vh-100 bg-light">
        <AppHeader />
        <div className="body flex-grow-1 px-3">
          <AppContent />
        </div>
        <AppFooter />
      </div>
    </div>
  )
}

export default DefaultLayout
