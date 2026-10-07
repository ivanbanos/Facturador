import React, { useRef, useState } from 'react'
import {
  CButton,
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CForm,
  CFormInput,
  CFormLabel,
  CRow,
  CSpinner,
} from '@coreui/react'
import UserService from '../../services/UserService'
import { getNombreUsuario } from '../../services/AuthService'
import Toast from '../toast/Toast'

const userService = new UserService()

const FORMULARIO_VACIO = { actual: '', nueva: '', confirmacion: '' }

const validar = ({ actual, nueva, confirmacion }) => {
  if (!actual) return 'Ingrese la contraseña actual'
  if (nueva.length < 6) return 'La contraseña nueva debe tener al menos 6 caracteres'
  if (nueva === actual) return 'La contraseña nueva debe ser diferente a la actual'
  if (nueva !== confirmacion) return 'Las contraseñas nuevas no coinciden'
  return null
}

const CambiarContrasena = () => {
  const toastRef = useRef()
  const [form, setForm] = useState(FORMULARIO_VACIO)
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const mensajeError = validar(form)
    setError(mensajeError)
    if (mensajeError) {
      return
    }

    setGuardando(true)
    try {
      const resultado = await userService.cambiarContrasena(form.actual, form.nueva)
      if (!resultado.ok) {
        setError(resultado.message)
        return
      }
      setForm(FORMULARIO_VACIO)
      toastRef.current?.addMessage('Contraseña actualizada correctamente', 'success')
    } catch (e) {
      console.error('Error cambiando contraseña:', e)
      setError('Error al cambiar la contraseña')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <>
      <Toast ref={toastRef} />
      <CRow className="justify-content-center">
        <CCol md={8} lg={6}>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Cambiar contraseña</strong>
              {getNombreUsuario() && (
                <span className="text-medium-emphasis ms-2">({getNombreUsuario()})</span>
              )}
            </CCardHeader>
            <CCardBody>
              <CForm onSubmit={handleSubmit}>
                <div className="mb-3">
                  <CFormLabel htmlFor="actual">Contraseña actual</CFormLabel>
                  <CFormInput
                    id="actual"
                    name="actual"
                    type="password"
                    autoComplete="current-password"
                    value={form.actual}
                    onChange={handleChange}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="nueva">Contraseña nueva</CFormLabel>
                  <CFormInput
                    id="nueva"
                    name="nueva"
                    type="password"
                    autoComplete="new-password"
                    value={form.nueva}
                    onChange={handleChange}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="confirmacion">Confirmar contraseña nueva</CFormLabel>
                  <CFormInput
                    id="confirmacion"
                    name="confirmacion"
                    type="password"
                    autoComplete="new-password"
                    value={form.confirmacion}
                    onChange={handleChange}
                  />
                </div>
                {error && <div className="text-danger mb-3">{error}</div>}
                <CButton type="submit" color="primary" disabled={guardando}>
                  {guardando && <CSpinner size="sm" className="me-2" />}
                  Guardar
                </CButton>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  )
}

export default CambiarContrasena
