import React from 'react'
import {
  CAlert,
  CBadge,
  CButton,
  CCard,
  CCardBody,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilCalculator } from '@coreui/icons'
import Paginacion from '../../../components/tabla/Paginacion'
import usePaginacion from '../../../components/tabla/usePaginacion'
import useEsPantallaPequena from '../../../components/tabla/useEsPantallaPequena'
import {
  formatCurrency,
  formatDate,
  formatIdFacturaElectronica,
  getEstadoBadgeColor,
  getEstadoDisplay,
  getFormasPagoDisplay,
} from './ordenesUtils'

const claveOrden = (orden) => `${orden.estacionGuid || ''}-${orden.guid || orden.idVentaLocal}`

const FacturaElectronica = ({ orden }) => {
  const facturaInfo = formatIdFacturaElectronica(orden.idFacturaElectronica)
  if (typeof facturaInfo !== 'object') {
    return <CBadge color="secondary">Sin Factura</CBadge>
  }

  return (
    <div>
      <CBadge color={facturaInfo.estado === 'Ok' ? 'success' : 'warning'} className="mb-1">
        {facturaInfo.consecutivo}
      </CBadge>
      {facturaInfo.cufe && (
        <div className="small text-muted" title={facturaInfo.cufe}>
          CUFE: {facturaInfo.cufe.substring(0, 8)}...
        </div>
      )}
    </div>
  )
}

const Descuento = ({ orden }) =>
  orden.descuento ? `-${formatCurrency(orden.descuento)}` : formatCurrency(0)

const TablaEscritorio = ({ ordenes, mostrarEstacion, onVer }) => (
  <CTable striped hover responsive small className="align-middle">
    <CTableHead>
      <CTableRow>
        <CTableHeaderCell>ID Transacción</CTableHeaderCell>
        {mostrarEstacion && <CTableHeaderCell>Estación</CTableHeaderCell>}
        <CTableHeaderCell>Fecha</CTableHeaderCell>
        <CTableHeaderCell>Cliente</CTableHeaderCell>
        <CTableHeaderCell className="d-none d-xl-table-cell">Formas de Pago</CTableHeaderCell>
        <CTableHeaderCell>Combustible</CTableHeaderCell>
        <CTableHeaderCell className="d-none d-lg-table-cell">Placa</CTableHeaderCell>
        <CTableHeaderCell className="text-center d-none d-lg-table-cell">Cantidad</CTableHeaderCell>
        <CTableHeaderCell>ID Factura</CTableHeaderCell>
        <CTableHeaderCell>Estado</CTableHeaderCell>
        <CTableHeaderCell className="text-end d-none d-xl-table-cell">Descuento</CTableHeaderCell>
        <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
        <CTableHeaderCell>Acciones</CTableHeaderCell>
      </CTableRow>
    </CTableHead>
    <CTableBody>
      {ordenes.map((orden) => (
        <CTableRow key={claveOrden(orden)}>
          <CTableDataCell>
            <strong>{orden.numeroTransaccion || orden.idVentaLocal || 'N/A'}</strong>
          </CTableDataCell>
          {mostrarEstacion && <CTableDataCell>{orden.estacionNombre || 'N/A'}</CTableDataCell>}
          <CTableDataCell className="text-nowrap">
            <small>{formatDate(orden.fecha)}</small>
          </CTableDataCell>
          <CTableDataCell>
            <strong>{orden.nombreTercero || 'N/A'}</strong>
            {orden.identificacion && <div className="small text-muted">{orden.identificacion}</div>}
          </CTableDataCell>
          <CTableDataCell className="d-none d-xl-table-cell">
            <small>{getFormasPagoDisplay(orden)}</small>
          </CTableDataCell>
          <CTableDataCell>
            <CBadge color="info">{orden.combustible || 'N/A'}</CBadge>
          </CTableDataCell>
          <CTableDataCell className="d-none d-lg-table-cell">
            <CBadge color="secondary">{orden.placa || 'N/A'}</CBadge>
          </CTableDataCell>
          <CTableDataCell className="text-center d-none d-lg-table-cell">
            <CBadge color="primary">{orden.cantidad || '0'}</CBadge>
          </CTableDataCell>
          <CTableDataCell>
            <FacturaElectronica orden={orden} />
          </CTableDataCell>
          <CTableDataCell>
            <CBadge color={getEstadoBadgeColor(orden.estado)}>
              {getEstadoDisplay(orden.estado)}
            </CBadge>
          </CTableDataCell>
          <CTableDataCell className="text-end text-danger text-nowrap d-none d-xl-table-cell">
            <Descuento orden={orden} />
          </CTableDataCell>
          <CTableDataCell className="text-end fw-bold text-success text-nowrap">
            {formatCurrency(orden.total)}
          </CTableDataCell>
          <CTableDataCell>
            <CButton color="info" size="sm" onClick={() => onVer(orden)}>
              <CIcon icon={cilCalculator} size="sm" className="me-1" />
              Ver
            </CButton>
          </CTableDataCell>
        </CTableRow>
      ))}
    </CTableBody>
  </CTable>
)

const Dato = ({ etiqueta, children }) => (
  <div className="d-flex justify-content-between gap-2 small">
    <span className="text-medium-emphasis">{etiqueta}</span>
    <span className="text-end">{children}</span>
  </div>
)

const TarjetasMovil = ({ ordenes, mostrarEstacion, onVer }) => (
  <div className="d-flex flex-column gap-2">
    {ordenes.map((orden) => (
      <CCard key={claveOrden(orden)} className="shadow-sm">
        <CCardBody className="p-3">
          <div className="d-flex justify-content-between align-items-start mb-2">
            <div>
              <strong>#{orden.numeroTransaccion || orden.idVentaLocal || 'N/A'}</strong>
              <div className="small text-muted">{formatDate(orden.fecha)}</div>
            </div>
            <div className="text-end">
              <div className="fw-bold text-success">{formatCurrency(orden.total)}</div>
              <CBadge color={getEstadoBadgeColor(orden.estado)}>
                {getEstadoDisplay(orden.estado)}
              </CBadge>
            </div>
          </div>
          {mostrarEstacion && <Dato etiqueta="Estación">{orden.estacionNombre || 'N/A'}</Dato>}
          <Dato etiqueta="Cliente">{orden.nombreTercero || 'N/A'}</Dato>
          <Dato etiqueta="Combustible">
            {orden.combustible || 'N/A'} · {orden.cantidad || '0'}
          </Dato>
          <Dato etiqueta="Placa">{orden.placa || 'N/A'}</Dato>
          <Dato etiqueta="Pago">{getFormasPagoDisplay(orden)}</Dato>
          <div className="d-flex justify-content-between align-items-center mt-2">
            <FacturaElectronica orden={orden} />
            <CButton color="info" size="sm" onClick={() => onVer(orden)}>
              <CIcon icon={cilCalculator} size="sm" className="me-1" />
              Ver
            </CButton>
          </div>
        </CCardBody>
      </CCard>
    ))}
  </div>
)

/**
 * Tabla paginada de órdenes: tabla en pantallas medianas y grandes, tarjetas en celulares.
 * @param {{ ordenes: Array, onVer: Function, mostrarEstacion?: boolean }} props
 */
const TablaOrdenes = ({ ordenes, onVer, mostrarEstacion = false }) => {
  const esPantallaPequena = useEsPantallaPequena()
  const { itemsPagina, paginacion } = usePaginacion(ordenes)

  if (!ordenes.length) {
    return (
      <CAlert color="warning" className="mb-0">
        No se encontraron órdenes para los criterios especificados.
      </CAlert>
    )
  }

  const Vista = esPantallaPequena ? TarjetasMovil : TablaEscritorio
  return (
    <>
      <Vista ordenes={itemsPagina} mostrarEstacion={mostrarEstacion} onVer={onVer} />
      <Paginacion {...paginacion} />
    </>
  )
}

export default TablaOrdenes
