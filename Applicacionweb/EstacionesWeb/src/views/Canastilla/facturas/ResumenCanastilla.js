import React from 'react'
import { CAlert, CCol, CRow } from '@coreui/react'
import { formatCurrency } from './canastillaUtils'

/**
 * Totales de las facturas filtradas.
 * @param {{ totales: ReturnType<import('./canastillaUtils').calcularTotales> }} props
 */
const ResumenCanastilla = ({ totales }) => (
  <CAlert color="info" className="mb-3">
    <CRow className="g-1">
      <CCol xs={6} md={2}>
        <strong>Cantidad:</strong> {totales.cantidad}
      </CCol>
      <CCol xs={6} md={2}>
        <strong>Subtotal:</strong> {formatCurrency(totales.subtotal)}
      </CCol>
      <CCol xs={6} md={2}>
        <strong>Descuento:</strong> {formatCurrency(totales.descuento)}
      </CCol>
      <CCol xs={6} md={2}>
        <strong>IVA:</strong> {formatCurrency(totales.iva)}
      </CCol>
      <CCol xs={12} md={4}>
        <strong>Total:</strong> {formatCurrency(totales.total)}
      </CCol>
    </CRow>
  </CAlert>
)

export default ResumenCanastilla
