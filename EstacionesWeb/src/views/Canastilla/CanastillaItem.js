import React, { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  CButton,
  CRow,
  CCol,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CTableDataCell,
  CFormInput,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormSelect,
  CCard,
  CCardBody,
  CCardHeader,
  CSpinner,
  CAlert,
  CBadge,
  CButtonGroup,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilPencil, cilTrash, cilPlus, cilSave, cilX, cilReload, cilSearch } from '@coreui/icons'
import CanastillasService from '../../services/CanastillasService'
import GuidService from '../../services/GuidService'
import Toast from '../toast/Toast'

const CanastillaItem = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const canastillasService = new CanastillasService()
  const guidService = new GuidService()

  // State management
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(false)
  const [showModal, setShowModal] = useState(false)
  const [editingItem, setEditingItem] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [showDeleted, setShowDeleted] = useState(false)

  // Form state - adjusted to match canastilla item structure
  const [formData, setFormData] = useState({
    canastillaId: '',
    descripcion: '',
    unidad: '',
    precio: 0,
    iva: 0,
    campoextra: '',
    deleted: false,
    guid: '',
  })

  // Form validation errors
  const [errors, setErrors] = useState({})

  // Load items on component mount
  useEffect(() => {
    loadCanastillaItems()
  }, [])

  const loadCanastillaItems = async () => {
    setLoading(true)
    try {
      // Obtener el GUID de la estación desde localStorage
      const estacionGuid = localStorage.getItem('estacionGuid')
      if (!estacionGuid) {
        toastRef.current?.addMessage('No se encontró información de la estación', 'error')
        navigate('/Login', { replace: true })
        return
      }

      const response = await canastillasService.getCanastillas(estacionGuid)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
      } else {
        setItems(response || [])
      }
    } catch (error) {
      toastRef.current?.addMessage('Error al cargar los items de canastilla', 'error')
    } finally {
      setLoading(false)
    }
  }

  const validateForm = () => {
    const newErrors = {}

    if (!formData.canastillaId) {
      newErrors.canastillaId = 'ID de canastilla es requerido'
    }

    if (!formData.descripcion.trim()) {
      newErrors.descripcion = 'Descripción es requerida'
    }

    if (!formData.unidad.trim()) {
      newErrors.unidad = 'Unidad es requerida'
    }

    if (formData.precio <= 0) {
      newErrors.precio = 'El precio debe ser mayor a 0'
    }

    if (formData.iva < 0 || formData.iva > 100) {
      newErrors.iva = 'El IVA debe estar entre 0 y 100'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }))

    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({
        ...prev,
        [field]: '',
      }))
    }
  }

  const openAddModal = () => {
    setEditingItem(null)
    const estacionGuid = localStorage.getItem('estacionGuid')
    setFormData({
      canastillaId: '',
      descripcion: '',
      unidad: '',
      precio: 0,
      iva: 0,
      campoextra: '',
      deleted: false,
      guid: guidService.generateGuid(),
      estacion: estacionGuid,
    })
    setErrors({})
    setShowModal(true)
  }

  const openEditModal = (item) => {
    setEditingItem(item)
    const estacionGuid = localStorage.getItem('estacionGuid')
    setFormData({
      canastillaId: item.canastillaId || item.id || '',
      descripcion: item.descripcion || '',
      unidad: item.unidad || '',
      precio: item.precio || 0,
      iva: item.iva || 0,
      campoextra: item.campoextra || '',
      deleted: item.deleted || false,
      guid: item.guid || item.id || '',
      estacion: item.estacion || estacionGuid,
    })
    setErrors({})
    setShowModal(true)
  }

  const handleSave = async () => {
    if (!validateForm()) {
      return
    }

    setLoading(true)
    try {
      // Preparar el item usando el método del servicio
      const preparedItem = canastillasService.prepareCanastillaForAPI(formData)

      // Usar el nuevo método createOrUpdateCanastillas que espera un array
      await canastillasService.createOrUpdateCanastillas([preparedItem])

      const message = editingItem ? 'Item actualizado exitosamente' : 'Item creado exitosamente'
      toastRef.current?.addMessage(message, 'success')

      setShowModal(false)
      loadCanastillaItems()
    } catch (error) {
      console.error('Error saving item:', error)
      toastRef.current?.addMessage('Error al guardar el item', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (item) => {
    if (window.confirm(`¿Está seguro de eliminar el item "${item.descripcion}"?`)) {
      setLoading(true)
      try {
        // Soft delete by updating the item with deleted: true
        const updatedItem = canastillasService.prepareCanastillaForAPI({ ...item, deleted: true })
        await canastillasService.createOrUpdateCanastillas([updatedItem])
        toastRef.current?.addMessage('Item eliminado exitosamente', 'success')
        loadCanastillaItems()
      } catch (error) {
        console.error('Error deleting item:', error)
        toastRef.current?.addMessage('Error al eliminar el item', 'error')
      } finally {
        setLoading(false)
      }
    }
  }

  const handleRestore = async (item) => {
    setLoading(true)
    try {
      // For restore functionality, we'll update the item with deleted: false
      const updatedItem = canastillasService.prepareCanastillaForAPI({ ...item, deleted: false })
      await canastillasService.createOrUpdateCanastillas([updatedItem])
      toastRef.current?.addMessage('Item restaurado exitosamente', 'success')
      loadCanastillaItems()
    } catch (error) {
      console.error('Error restoring item:', error)
      toastRef.current?.addMessage('Error al restaurar el item', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Filter items based on search term and deleted status
  const filteredItems = items.filter((item) => {
    // Handle cases where properties might be undefined
    const descripcion = item.descripcion || ''
    const unidad = item.unidad || ''
    const canastillaId = item.canastillaId || item.id || ''
    const campoextra = item.campoextra || ''

    const matchesSearch =
      descripcion.toLowerCase().includes(searchTerm.toLowerCase()) ||
      unidad.toLowerCase().includes(searchTerm.toLowerCase()) ||
      campoextra.toLowerCase().includes(searchTerm.toLowerCase()) ||
      canastillaId.toString().includes(searchTerm)

    const matchesDeletedFilter = showDeleted ? item.deleted : !item.deleted

    return matchesSearch && matchesDeletedFilter
  })

  const formatPrice = (price) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
    }).format(price)
  }

  const calculateTotal = (precio, iva) => {
    const subtotal = precio
    const ivaAmount = subtotal * (iva / 100)
    return subtotal + ivaAmount
  }

  return (
    <>
      <CCard>
        <CCardHeader>
          <CRow>
            <CCol>
              <h4>Gestión de Items de Canastilla</h4>
            </CCol>
            <CCol xs="auto">
              <CButton color="primary" onClick={openAddModal}>
                <CIcon icon={cilPlus} className="me-2" />
                Agregar Item
              </CButton>
            </CCol>
          </CRow>
        </CCardHeader>

        <CCardBody>
          {/* Search and filters */}
          <CRow className="mb-3">
            <CCol md={6}>
              <CFormInput
                placeholder="Buscar por descripción, unidad o ID de canastilla..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </CCol>
            <CCol md={3}>
              <CFormSelect
                value={showDeleted}
                onChange={(e) => setShowDeleted(e.target.value === 'true')}
              >
                <option value="false">Items Activos</option>
                <option value="true">Items Eliminados</option>
              </CFormSelect>
            </CCol>
            <CCol md={3}>
              <CButton
                color="info"
                variant="outline"
                onClick={loadCanastillaItems}
                disabled={loading}
              >
                <CIcon icon={cilReload} className="me-2" />
                Actualizar
              </CButton>
            </CCol>
          </CRow>

          {/* Loading spinner */}
          {loading && (
            <div className="text-center p-3">
              <CSpinner color="primary" />
            </div>
          )}

          {/* Items table */}
          {!loading && (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>ID Canastilla</CTableHeaderCell>
                  <CTableHeaderCell>Descripción</CTableHeaderCell>
                  <CTableHeaderCell>Unidad</CTableHeaderCell>
                  <CTableHeaderCell>Precio</CTableHeaderCell>
                  <CTableHeaderCell>IVA (%)</CTableHeaderCell>
                  <CTableHeaderCell>Total con IVA</CTableHeaderCell>
                  <CTableHeaderCell>Campo Extra</CTableHeaderCell>
                  <CTableHeaderCell>Estado</CTableHeaderCell>
                  <CTableHeaderCell>Acciones</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredItems.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={9} className="text-center">
                      No se encontraron items
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredItems.map((item) => (
                    <CTableRow key={item.guid || item.id}>
                      <CTableDataCell>{item.canastillaId || item.id || '-'}</CTableDataCell>
                      <CTableDataCell>{item.descripcion || '-'}</CTableDataCell>
                      <CTableDataCell>{item.unidad || '-'}</CTableDataCell>
                      <CTableDataCell>{formatPrice(item.precio || 0)}</CTableDataCell>
                      <CTableDataCell>{item.iva || 0}%</CTableDataCell>
                      <CTableDataCell>
                        {formatPrice(calculateTotal(item.precio || 0, item.iva || 0))}
                      </CTableDataCell>
                      <CTableDataCell>{item.campoextra || '-'}</CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={item.deleted ? 'danger' : 'success'}>
                          {item.deleted ? 'Eliminado' : 'Activo'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CButtonGroup>
                          <CButton
                            color="info"
                            variant="outline"
                            size="sm"
                            onClick={() => openEditModal(item)}
                            disabled={loading}
                          >
                            <CIcon icon={cilPencil} />
                          </CButton>
                          {item.deleted ? (
                            <CButton
                              color="success"
                              variant="outline"
                              size="sm"
                              onClick={() => handleRestore(item)}
                              disabled={loading}
                            >
                              <CIcon icon={cilReload} />
                            </CButton>
                          ) : (
                            <CButton
                              color="danger"
                              variant="outline"
                              size="sm"
                              onClick={() => handleDelete(item)}
                              disabled={loading}
                            >
                              <CIcon icon={cilTrash} />
                            </CButton>
                          )}
                        </CButtonGroup>
                      </CTableDataCell>
                    </CTableRow>
                  ))
                )}
              </CTableBody>
            </CTable>
          )}
        </CCardBody>
      </CCard>

      {/* Add/Edit Modal */}
      <CModal visible={showModal} onClose={() => setShowModal(false)} size="lg">
        <CModalHeader>
          <CModalTitle>
            {editingItem ? 'Editar Item de Canastilla' : 'Agregar Item de Canastilla'}
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CRow>
            <CCol md={6}>
              <div className="mb-3">
                <label className="form-label">ID Canastilla *</label>
                <CFormInput
                  type="number"
                  value={formData.canastillaId}
                  onChange={(e) =>
                    handleInputChange('canastillaId', parseInt(e.target.value) || '')
                  }
                  invalid={!!errors.canastillaId}
                />
                {errors.canastillaId && (
                  <div className="invalid-feedback">{errors.canastillaId}</div>
                )}
              </div>
            </CCol>
            <CCol md={6}>
              <div className="mb-3">
                <label className="form-label">Unidad *</label>
                <CFormInput
                  value={formData.unidad}
                  onChange={(e) => handleInputChange('unidad', e.target.value)}
                  invalid={!!errors.unidad}
                  placeholder="Ej: Kg, Unidad, Litro"
                />
                {errors.unidad && <div className="invalid-feedback">{errors.unidad}</div>}
              </div>
            </CCol>
          </CRow>

          <div className="mb-3">
            <label className="form-label">Descripción *</label>
            <CFormInput
              value={formData.descripcion}
              onChange={(e) => handleInputChange('descripcion', e.target.value)}
              invalid={!!errors.descripcion}
              placeholder="Descripción del item"
            />
            {errors.descripcion && <div className="invalid-feedback">{errors.descripcion}</div>}
          </div>

          <div className="mb-3">
            <label className="form-label">Campo Extra</label>
            <CFormInput
              value={formData.campoextra}
              onChange={(e) => handleInputChange('campoextra', e.target.value)}
              placeholder="Información adicional (opcional)"
            />
          </div>

          <CRow>
            <CCol md={6}>
              <div className="mb-3">
                <label className="form-label">Precio *</label>
                <CFormInput
                  type="number"
                  step="0.01"
                  value={formData.precio}
                  onChange={(e) => handleInputChange('precio', parseFloat(e.target.value) || 0)}
                  invalid={!!errors.precio}
                />
                {errors.precio && <div className="invalid-feedback">{errors.precio}</div>}
              </div>
            </CCol>
            <CCol md={6}>
              <div className="mb-3">
                <label className="form-label">IVA (%) *</label>
                <CFormInput
                  type="number"
                  min="0"
                  max="100"
                  value={formData.iva}
                  onChange={(e) => handleInputChange('iva', parseInt(e.target.value) || 0)}
                  invalid={!!errors.iva}
                />
                {errors.iva && <div className="invalid-feedback">{errors.iva}</div>}
              </div>
            </CCol>
          </CRow>

          {formData.precio > 0 && (
            <CAlert color="info">
              <strong>Total con IVA: </strong>
              {formatPrice(calculateTotal(formData.precio, formData.iva))}
            </CAlert>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowModal(false)}>
            <CIcon icon={cilX} className="me-2" />
            Cancelar
          </CButton>
          <CButton color="primary" onClick={handleSave} disabled={loading}>
            <CIcon icon={cilSave} className="me-2" />
            {loading ? <CSpinner size="sm" className="me-2" /> : null}
            Guardar
          </CButton>
        </CModalFooter>
      </CModal>

      <Toast ref={toastRef} />
    </>
  )
}

export default CanastillaItem
