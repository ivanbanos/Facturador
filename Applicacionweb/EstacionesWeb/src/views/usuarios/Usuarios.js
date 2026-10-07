import React, { useEffect, useRef, useState } from 'react'
import {
  CBadge,
  CButton,
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CForm,
  CFormCheck,
  CFormInput,
  CFormLabel,
  CRow,
  CSpinner,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
} from '@coreui/react'
import UserService from '../../services/UserService'
import Toast from '../toast/Toast'
import BuscadorCliente from './BuscadorCliente'

const userService = new UserService()

const FORMULARIO_VACIO = {
  nombre: '',
  usuario: '',
  contrasena: '',
  confirmacion: '',
}

const validar = (form, esCliente, cliente) => {
  if (!form.nombre.trim()) return 'El nombre es obligatorio'
  if (form.usuario.trim().length < 3) return 'El usuario debe tener al menos 3 caracteres'
  if (/[\s/\\?#%]/.test(form.usuario)) return 'El usuario no puede tener espacios ni / \\ ? # %'
  if (form.contrasena.length < 6) return 'La contraseña debe tener al menos 6 caracteres'
  if (form.contrasena !== form.confirmacion) return 'Las contraseñas no coinciden'
  if (esCliente && !cliente) return 'Seleccione el cliente al que pertenece el usuario'
  return null
}

const Usuarios = () => {
  const toastRef = useRef()
  const [usuarios, setUsuarios] = useState([])
  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [form, setForm] = useState(FORMULARIO_VACIO)
  const [esCliente, setEsCliente] = useState(true)
  const [cliente, setCliente] = useState(null)
  const [error, setError] = useState(null)

  const cargarUsuarios = async () => {
    setCargando(true)
    try {
      const response = await userService.listar()
      setUsuarios(Array.isArray(response) ? response : [])
    } catch (e) {
      console.error('Error cargando usuarios:', e)
      toastRef.current?.addMessage('Error al cargar los usuarios', 'error')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    cargarUsuarios()
  }, [])

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const mensajeError = validar(form, esCliente, cliente)
    setError(mensajeError)
    if (mensajeError) {
      return
    }

    setGuardando(true)
    try {
      const resultado = await userService.crear({
        nombre: form.nombre.trim(),
        usuario: form.usuario.trim(),
        contrasena: form.contrasena,
        identificacionTercero: esCliente ? cliente.identificacion : null,
      })
      if (!resultado.ok) {
        setError(resultado.message)
        return
      }
      toastRef.current?.addMessage(`Usuario "${form.usuario.trim()}" creado`, 'success')
      setForm(FORMULARIO_VACIO)
      setCliente(null)
      cargarUsuarios()
    } catch (e) {
      console.error('Error creando usuario:', e)
      setError('Error al crear el usuario')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <>
      <Toast ref={toastRef} />
      <CRow>
        <CCol lg={5}>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Nuevo usuario</strong>
            </CCardHeader>
            <CCardBody>
              <CForm onSubmit={handleSubmit} autoComplete="off">
                <div className="mb-3">
                  <CFormLabel htmlFor="nombre">Nombre</CFormLabel>
                  <CFormInput
                    id="nombre"
                    name="nombre"
                    maxLength={50}
                    value={form.nombre}
                    onChange={handleChange}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="usuario">Usuario</CFormLabel>
                  <CFormInput
                    id="usuario"
                    name="usuario"
                    maxLength={50}
                    value={form.usuario}
                    onChange={handleChange}
                  />
                </div>
                <CRow className="mb-3">
                  <CCol sm={6}>
                    <CFormLabel htmlFor="contrasena">Contraseña</CFormLabel>
                    <CFormInput
                      id="contrasena"
                      name="contrasena"
                      type="password"
                      autoComplete="new-password"
                      value={form.contrasena}
                      onChange={handleChange}
                    />
                  </CCol>
                  <CCol sm={6}>
                    <CFormLabel htmlFor="confirmacion">Confirmar contraseña</CFormLabel>
                    <CFormInput
                      id="confirmacion"
                      name="confirmacion"
                      type="password"
                      autoComplete="new-password"
                      value={form.confirmacion}
                      onChange={handleChange}
                    />
                  </CCol>
                </CRow>
                <div className="mb-3">
                  <CFormLabel className="d-block">Tipo de usuario</CFormLabel>
                  <CFormCheck
                    inline
                    type="radio"
                    name="tipoUsuario"
                    id="tipoCliente"
                    label="Cliente"
                    checked={esCliente}
                    onChange={() => setEsCliente(true)}
                  />
                  <CFormCheck
                    inline
                    type="radio"
                    name="tipoUsuario"
                    id="tipoAdministrador"
                    label="Administrador"
                    checked={!esCliente}
                    onChange={() => setEsCliente(false)}
                  />
                </div>
                {esCliente && (
                  <div className="mb-3">
                    <BuscadorCliente seleccionado={cliente} onSeleccionar={setCliente} />
                  </div>
                )}
                {error && <div className="text-danger mb-3">{error}</div>}
                <CButton type="submit" color="primary" disabled={guardando}>
                  {guardando && <CSpinner size="sm" className="me-2" />}
                  Crear usuario
                </CButton>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol lg={7}>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Usuarios</strong>
            </CCardHeader>
            <CCardBody>
              {cargando ? (
                <div className="text-center">
                  <CSpinner color="primary" />
                </div>
              ) : (
                <CTable hover responsive small>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>Nombre</CTableHeaderCell>
                      <CTableHeaderCell>Usuario</CTableHeaderCell>
                      <CTableHeaderCell>Rol</CTableHeaderCell>
                      <CTableHeaderCell>Cliente</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {usuarios.map((u) => (
                      <CTableRow key={u.guid}>
                        <CTableDataCell>{u.nombre}</CTableDataCell>
                        <CTableDataCell>{u.usuario}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={u.rol === 'Cliente' ? 'info' : 'secondary'}>
                            {u.rol}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{u.identificacionTercero || '—'}</CTableDataCell>
                      </CTableRow>
                    ))}
                    {usuarios.length === 0 && (
                      <CTableRow>
                        <CTableDataCell colSpan={4} className="text-center text-medium-emphasis">
                          No hay usuarios
                        </CTableDataCell>
                      </CTableRow>
                    )}
                  </CTableBody>
                </CTable>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  )
}

export default Usuarios
