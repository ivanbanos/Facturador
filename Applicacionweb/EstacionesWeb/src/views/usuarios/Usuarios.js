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
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CRow,
  CSpinner,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilBan, cilCheckCircle, cilPencil, cilTrash } from '@coreui/icons'
import UserService from '../../services/UserService'
import Toast from '../toast/Toast'
import BuscadorCliente from './BuscadorCliente'

const userService = new UserService()

const TIPO_CLIENTE = 'Cliente'
const TIPO_ADMINISTRADOR = 'Administrador'
const TIPO_OPERADOR = 'Operador'

const TIPOS_USUARIO = [
  { valor: TIPO_CLIENTE, etiqueta: 'Cliente' },
  { valor: TIPO_ADMINISTRADOR, etiqueta: 'Administrador' },
  { valor: TIPO_OPERADOR, etiqueta: 'Operador (sin gestión de usuarios)' },
]

const COLOR_ROL = { Cliente: 'info', Operador: 'primary' }

const FORMULARIO_VACIO = {
  nombre: '',
  usuario: '',
  contrasena: '',
  confirmacion: '',
}

// Al editar, la contraseña es opcional: vacía = se conserva la actual
const validar = (form, esCliente, cliente, editando) => {
  if (!form.nombre.trim()) return 'El nombre es obligatorio'
  if (form.usuario.trim().length < 3) return 'El usuario debe tener al menos 3 caracteres'
  if (/[\s/\\?#%]/.test(form.usuario)) return 'El usuario no puede tener espacios ni / \\ ? # %'
  const cambiaContrasena = !editando || form.contrasena.length > 0 || form.confirmacion.length > 0
  if (cambiaContrasena && form.contrasena.length < 6)
    return 'La contraseña debe tener al menos 6 caracteres'
  if (cambiaContrasena && form.contrasena !== form.confirmacion)
    return 'Las contraseñas no coinciden'
  if (esCliente && !cliente) return 'Seleccione el cliente al que pertenece el usuario'
  return null
}

const Usuarios = () => {
  const toastRef = useRef()
  const [usuarios, setUsuarios] = useState([])
  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [form, setForm] = useState(FORMULARIO_VACIO)
  const [tipo, setTipo] = useState(TIPO_CLIENTE)
  const [cliente, setCliente] = useState(null)
  const [error, setError] = useState(null)
  // Usuario que se está editando (null = creando uno nuevo)
  const [editando, setEditando] = useState(null)
  // Acción pendiente de confirmar: { tipo: 'eliminar' | 'inhabilitar', usuario }
  const [confirmacion, setConfirmacion] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const usuarioActual = userService.getCurrentUser()
  const esCliente = tipo === TIPO_CLIENTE

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

  const limpiarFormulario = () => {
    setForm(FORMULARIO_VACIO)
    setCliente(null)
    setTipo(TIPO_CLIENTE)
    setEditando(null)
    setError(null)
  }

  const iniciarEdicion = (u) => {
    setEditando(u)
    setForm({ nombre: u.nombre || '', usuario: u.usuario || '', contrasena: '', confirmacion: '' })
    setTipo(
      u.identificacionTercero
        ? TIPO_CLIENTE
        : u.rol === TIPO_OPERADOR
        ? TIPO_OPERADOR
        : TIPO_ADMINISTRADOR,
    )
    setCliente(u.identificacionTercero ? { identificacion: u.identificacionTercero } : null)
    setError(null)
  }

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const mensajeError = validar(form, esCliente, cliente, Boolean(editando))
    setError(mensajeError)
    if (mensajeError) {
      return
    }

    const datos = {
      nombre: form.nombre.trim(),
      usuario: form.usuario.trim(),
      contrasena: form.contrasena,
      identificacionTercero: esCliente ? cliente.identificacion : null,
      rol: esCliente ? null : tipo,
    }

    setGuardando(true)
    try {
      const resultado = editando
        ? await userService.actualizar(editando.guid, datos)
        : await userService.crear(datos)
      if (!resultado.ok) {
        setError(resultado.message)
        return
      }
      toastRef.current?.addMessage(
        `Usuario "${datos.usuario}" ${editando ? 'actualizado' : 'creado'}`,
        'success',
      )
      limpiarFormulario()
      cargarUsuarios()
    } catch (e) {
      console.error('Error guardando usuario:', e)
      setError(`Error al ${editando ? 'actualizar' : 'crear'} el usuario`)
    } finally {
      setGuardando(false)
    }
  }

  const ejecutarAccion = async (tipo, u) => {
    setProcesando(true)
    try {
      const resultado =
        tipo === 'eliminar'
          ? await userService.eliminar(u.guid)
          : await userService.cambiarEstado(u.guid, tipo === 'habilitar')
      if (!resultado.ok) {
        toastRef.current?.addMessage(resultado.message, 'error')
        return
      }
      const textos = { eliminar: 'eliminado', inhabilitar: 'inhabilitado', habilitar: 'habilitado' }
      toastRef.current?.addMessage(`Usuario "${u.usuario}" ${textos[tipo]}`, 'success')
      if (editando?.guid === u.guid) {
        limpiarFormulario()
      }
      cargarUsuarios()
    } catch (e) {
      console.error(`Error (${tipo}) usuario:`, e)
      toastRef.current?.addMessage('No se pudo completar la acción', 'error')
    } finally {
      setProcesando(false)
      setConfirmacion(null)
    }
  }

  const esPropio = (u) => Boolean(usuarioActual) && u.usuario === usuarioActual

  return (
    <>
      <Toast ref={toastRef} />
      <CRow>
        <CCol lg={5}>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>{editando ? `Editar usuario "${editando.usuario}"` : 'Nuevo usuario'}</strong>
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
                    <CFormLabel htmlFor="contrasena">
                      {editando ? 'Nueva contraseña' : 'Contraseña'}
                    </CFormLabel>
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
                  {editando && (
                    <div className="form-text">
                      Déjela vacía para conservar la contraseña actual.
                    </div>
                  )}
                </CRow>
                <div className="mb-3">
                  <CFormLabel className="d-block">Tipo de usuario</CFormLabel>
                  {TIPOS_USUARIO.map((opcion) => (
                    <CFormCheck
                      key={opcion.valor}
                      inline
                      type="radio"
                      name="tipoUsuario"
                      id={`tipo${opcion.valor}`}
                      label={opcion.etiqueta}
                      checked={tipo === opcion.valor}
                      onChange={() => setTipo(opcion.valor)}
                    />
                  ))}
                </div>
                {esCliente && (
                  <div className="mb-3">
                    <BuscadorCliente seleccionado={cliente} onSeleccionar={setCliente} />
                  </div>
                )}
                {error && <div className="text-danger mb-3">{error}</div>}
                <CButton type="submit" color="primary" disabled={guardando}>
                  {guardando && <CSpinner size="sm" className="me-2" />}
                  {editando ? 'Guardar cambios' : 'Crear usuario'}
                </CButton>
                {editando && (
                  <CButton
                    type="button"
                    color="secondary"
                    variant="outline"
                    className="ms-2"
                    disabled={guardando}
                    onClick={limpiarFormulario}
                  >
                    Cancelar
                  </CButton>
                )}
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
                      <CTableHeaderCell>Estado</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">Acciones</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {usuarios.map((u) => (
                      <CTableRow key={u.guid} active={editando?.guid === u.guid}>
                        <CTableDataCell>{u.nombre}</CTableDataCell>
                        <CTableDataCell>{u.usuario}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={COLOR_ROL[u.rol] || 'secondary'}>{u.rol}</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{u.identificacionTercero || '—'}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={u.activo ? 'success' : 'warning'}>
                            {u.activo ? 'Activo' : 'Inhabilitado'}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell className="text-end text-nowrap">
                          <CButton
                            color="info"
                            variant="ghost"
                            size="sm"
                            title="Editar"
                            onClick={() => iniciarEdicion(u)}
                          >
                            <CIcon icon={cilPencil} />
                          </CButton>
                          {u.activo ? (
                            <CButton
                              color="warning"
                              variant="ghost"
                              size="sm"
                              title={
                                esPropio(u)
                                  ? 'No puede inhabilitar su propio usuario'
                                  : 'Inhabilitar'
                              }
                              disabled={esPropio(u) || procesando}
                              onClick={() => setConfirmacion({ tipo: 'inhabilitar', usuario: u })}
                            >
                              <CIcon icon={cilBan} />
                            </CButton>
                          ) : (
                            <CButton
                              color="success"
                              variant="ghost"
                              size="sm"
                              title="Habilitar"
                              disabled={procesando}
                              onClick={() => ejecutarAccion('habilitar', u)}
                            >
                              <CIcon icon={cilCheckCircle} />
                            </CButton>
                          )}
                          <CButton
                            color="danger"
                            variant="ghost"
                            size="sm"
                            title={esPropio(u) ? 'No puede eliminar su propio usuario' : 'Eliminar'}
                            disabled={esPropio(u) || procesando}
                            onClick={() => setConfirmacion({ tipo: 'eliminar', usuario: u })}
                          >
                            <CIcon icon={cilTrash} />
                          </CButton>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                    {usuarios.length === 0 && (
                      <CTableRow>
                        <CTableDataCell colSpan={6} className="text-center text-medium-emphasis">
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

      <CModal visible={Boolean(confirmacion)} onClose={() => !procesando && setConfirmacion(null)}>
        <CModalHeader>
          <CModalTitle>
            {confirmacion?.tipo === 'eliminar' ? 'Eliminar usuario' : 'Inhabilitar usuario'}
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          {confirmacion?.tipo === 'eliminar' ? (
            <>
              ¿Eliminar el usuario <strong>{confirmacion?.usuario.usuario}</strong>? Dejará de
              aparecer en la lista y no podrá iniciar sesión.
            </>
          ) : (
            <>
              ¿Inhabilitar el usuario <strong>{confirmacion?.usuario.usuario}</strong>? No podrá
              iniciar sesión hasta que se habilite de nuevo.
            </>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            variant="outline"
            disabled={procesando}
            onClick={() => setConfirmacion(null)}
          >
            Cancelar
          </CButton>
          <CButton
            color={confirmacion?.tipo === 'eliminar' ? 'danger' : 'warning'}
            disabled={procesando}
            onClick={() => ejecutarAccion(confirmacion.tipo, confirmacion.usuario)}
          >
            {procesando && <CSpinner size="sm" className="me-2" />}
            {confirmacion?.tipo === 'eliminar' ? 'Eliminar' : 'Inhabilitar'}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  )
}

export default Usuarios
