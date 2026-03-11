import React, { useState } from 'react'
import ObtenerOrdenesSinFacturaCreditoDirecto from '../../services/ObtenerOrdenesSinFacturaCreditoDirecto'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CButton,
  CSpinner,
  CAlert,
  CFormInput,
  CFormLabel,
  CRow,
  CCol,
} from '@coreui/react'

const ConsolidadoOrdenes = () => {
  const [ordenes, setOrdenes] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')

  const handleBuscar = async () => {
    setLoading(true)
    setError('')
    try {
      const filtro = {
        fechaInicial,
        fechaFinal,
      }
      const result = await ObtenerOrdenesSinFacturaCreditoDirecto(filtro)
      if (result === 'fail') {
        setError('Error al obtener las órdenes')
        setOrdenes([])
      } else {
        setOrdenes(result)
      }
    } catch (e) {
      setError('Error inesperado')
      setOrdenes([])
    } finally {
      setLoading(false)
    }
  }

  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h4>Consolidado de Órdenes</h4>
      </CCardHeader>
      <CCardBody>
        <CRow className="mb-3">
          <CCol md={4}>
            <CFormLabel>Fecha Inicial</CFormLabel>
            <CFormInput
              type="date"
              value={fechaInicial}
              onChange={(e) => setFechaInicial(e.target.value)}
            />
          </CCol>
          <CCol md={4}>
            <CFormLabel>Fecha Final</CFormLabel>
            <CFormInput
              type="date"
              value={fechaFinal}
              onChange={(e) => setFechaFinal(e.target.value)}
            />
          </CCol>
          <CCol md={4} className="d-flex align-items-end">
            <CButton color="primary" onClick={handleBuscar} disabled={loading}>
              {loading ? <CSpinner size="sm" className="me-2" /> : null}
              Buscar
            </CButton>
          </CCol>
        </CRow>
        {error && <CAlert color="danger">{error}</CAlert>}
        <CTable striped hover responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>ID Venta Local</CTableHeaderCell>
              <CTableHeaderCell>Cliente</CTableHeaderCell>
              <CTableHeaderCell>Identificación</CTableHeaderCell>
              <CTableHeaderCell>Combustible</CTableHeaderCell>
              <CTableHeaderCell>Placa</CTableHeaderCell>
              <CTableHeaderCell>Cantidad</CTableHeaderCell>
              <CTableHeaderCell>Total</CTableHeaderCell>
              <CTableHeaderCell>Fecha</CTableHeaderCell>
              <CTableHeaderCell>Estado</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {ordenes.length === 0 && !loading ? (
              <CTableRow>
                <CTableDataCell colSpan={9} className="text-center">
                  No hay órdenes para mostrar.
                </CTableDataCell>
              </CTableRow>
            ) : (
              ordenes.map((orden, idx) => (
                <CTableRow key={orden.idVentaLocal || idx}>
                  <CTableDataCell>{orden.idVentaLocal || 'N/A'}</CTableDataCell>
                  <CTableDataCell>{orden.nombreTercero || 'N/A'}</CTableDataCell>
                  <CTableDataCell>{orden.identificacion || 'N/A'}</CTableDataCell>
                  <CTableDataCell>{orden.combustible || 'N/A'}</CTableDataCell>
                  <CTableDataCell>{orden.placa || 'N/A'}</CTableDataCell>
                  <CTableDataCell>{orden.cantidad || 0}</CTableDataCell>
                  <CTableDataCell>{orden.total || 0}</CTableDataCell>
                  <CTableDataCell>
                    {orden.fecha ? new Date(orden.fecha).toLocaleDateString('es-ES') : 'N/A'}
                  </CTableDataCell>
                  <CTableDataCell>{orden.estado || 'N/A'}</CTableDataCell>
                </CTableRow>
              ))
            )}
          </CTableBody>
        </CTable>
      </CCardBody>
    </CCard>
  )
}

export default ConsolidadoOrdenes
