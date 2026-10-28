import React from 'react'
import { CCard, CCardBody, CCol, CRow } from '@coreui/react'
import { formatCurrency } from './ordenesUtils'

const Indicador = ({ valor, etiqueta, color }) => (
  <CCard className="text-center h-100">
    <CCardBody className="p-2 p-md-3">
      <div className={`fs-5 fw-semibold text-${color} text-break`}>{valor}</div>
      <div className="text-medium-emphasis text-uppercase fw-semibold small">{etiqueta}</div>
    </CCardBody>
  </CCard>
)

/**
 * Tarjetas de resumen de las órdenes filtradas (2 por fila en celular, 5 en escritorio).
 * @param {{ estadisticas: ReturnType<import('./ordenesUtils').calcularEstadisticas> }} props
 */
const ResumenOrdenes = ({ estadisticas }) => (
  <CRow className="g-2 mb-4">
    <CCol xs={6} md={4} lg={2}>
      <Indicador valor={estadisticas.total} etiqueta="Total Órdenes" color="primary" />
    </CCol>
    <CCol xs={6} md={4} lg={2}>
      <Indicador valor={estadisticas.pendientes} etiqueta="Pendientes" color="warning" />
    </CCol>
    <CCol xs={6} md={4} lg={2}>
      <Indicador valor={estadisticas.anuladas} etiqueta="Facturadas" color="danger" />
    </CCol>
    <CCol xs={6} md={4} lg={2}>
      <Indicador valor={estadisticas.completadas} etiqueta="Completadas" color="success" />
    </CCol>
    <CCol xs={12} md={8} lg={4}>
      <Indicador
        valor={formatCurrency(estadisticas.totalMonto)}
        etiqueta="Total Monto"
        color="success"
      />
    </CCol>
  </CRow>
)

export default ResumenOrdenes
