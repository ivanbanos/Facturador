import React from 'react'
import {
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilChart } from '@coreui/icons'
import { formatCurrency } from './canastillaUtils'

/**
 * Detalle del reporte por forma de pago y por artículo (lo calcula el API sobre todo el período).
 * @param {{ reporte: { detalleFormaPago?: Array, detalleArticulo?: Array } }} props
 */
const DetalleReporteCanastilla = ({ reporte }) => {
  const formasPago = reporte?.detalleFormaPago || []
  const articulos = reporte?.detalleArticulo || []
  if (!formasPago.length && !articulos.length) {
    return null
  }

  return (
    <CAccordion className="mb-3">
      {formasPago.length > 0 && (
        <CAccordionItem itemKey="formasPago">
          <CAccordionHeader>
            <CIcon icon={cilChart} className="me-2" />
            Detalle por Forma de Pago ({formasPago.length})
          </CAccordionHeader>
          <CAccordionBody>
            <CTable size="sm" responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Forma de Pago</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {formasPago.map((forma, index) => (
                  <CTableRow key={index}>
                    <CTableDataCell>{forma.formaDePago || '-'}</CTableDataCell>
                    <CTableDataCell className="text-end">{forma.cantidad || 0}</CTableDataCell>
                    <CTableDataCell className="text-end text-nowrap">
                      <strong>{formatCurrency(forma.precio)}</strong>
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>
          </CAccordionBody>
        </CAccordionItem>
      )}

      {articulos.length > 0 && (
        <CAccordionItem itemKey="articulos">
          <CAccordionHeader>
            <CIcon icon={cilChart} className="me-2" />
            Detalle por Artículo ({articulos.length})
          </CAccordionHeader>
          <CAccordionBody>
            <CTable size="sm" responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Artículo</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Subtotal</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">IVA</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {articulos.map((articulo, index) => (
                  <CTableRow key={index}>
                    <CTableDataCell>{articulo.descripcion || '-'}</CTableDataCell>
                    <CTableDataCell className="text-end">{articulo.cantidad || 0}</CTableDataCell>
                    <CTableDataCell className="text-end text-nowrap">
                      {formatCurrency(articulo.subtotal)}
                    </CTableDataCell>
                    <CTableDataCell className="text-end text-nowrap">
                      {formatCurrency(articulo.iva)}
                    </CTableDataCell>
                    <CTableDataCell className="text-end text-nowrap">
                      <strong>{formatCurrency(articulo.total)}</strong>
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>
          </CAccordionBody>
        </CAccordionItem>
      )}
    </CAccordion>
  )
}

export default DetalleReporteCanastilla
