import React from 'react'
import {
  CAlert,
  CBadge,
  CButton,
  CButtonGroup,
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
import { cilContact, cilSearch } from '@coreui/icons'
import Paginacion from '../../../components/tabla/Paginacion'
import usePaginacion from '../../../components/tabla/usePaginacion'
import useEsPantallaPequena from '../../../components/tabla/useEsPantallaPequena'
import {
  formatCurrency,
  formatDate,
  getEstadoDisplay,
  getFormaPago,
  idFactura,
} from './canastillaUtils'

const claveFactura = (factura) => `${factura.idEstacion || ''}-${idFactura(factura)}`

const Estado = ({ factura }) => (
  <CBadge color={factura.estado === 'Anulada' ? 'danger' : 'success'}>
    {getEstadoDisplay(factura.estado)}
  </CBadge>
)

const Acciones = ({ factura, onResumen, onDetalle }) => (
  <CButtonGroup size="sm">
    {onResumen && (
      <CButton
        color="info"
        variant="outline"
        onClick={() => onResumen(factura)}
        title="Ver resumen"
      >
        <CIcon icon={cilSearch} />
      </CButton>
    )}
    <CButton
      color="primary"
      variant="outline"
      onClick={() => onDetalle(factura)}
      title="Ver detalle completo"
    >
      <CIcon icon={cilContact} />
    </CButton>
  </CButtonGroup>
)

const TablaEscritorio = ({ facturas, mostrarEstacion, onResumen, onDetalle }) => (
  <CTable hover responsive striped small className="align-middle">
    <CTableHead>
      <CTableRow>
        <CTableHeaderCell>Consecutivo</CTableHeaderCell>
        {mostrarEstacion && <CTableHeaderCell>Estación</CTableHeaderCell>}
        <CTableHeaderCell>Fecha</CTableHeaderCell>
        <CTableHeaderCell>Tercero</CTableHeaderCell>
        <CTableHeaderCell className="d-none d-lg-table-cell">Forma de pago</CTableHeaderCell>
        <CTableHeaderCell className="text-end d-none d-xl-table-cell">Subtotal</CTableHeaderCell>
        <CTableHeaderCell className="text-end d-none d-xl-table-cell">Descuento</CTableHeaderCell>
        <CTableHeaderCell className="text-end d-none d-lg-table-cell">IVA</CTableHeaderCell>
        <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
        <CTableHeaderCell>Estado</CTableHeaderCell>
        <CTableHeaderCell>Acciones</CTableHeaderCell>
      </CTableRow>
    </CTableHead>
    <CTableBody>
      {facturas.map((factura) => (
        <CTableRow key={claveFactura(factura)}>
          <CTableDataCell>
            <strong>{factura.consecutivo || '-'}</strong>
          </CTableDataCell>
          {mostrarEstacion && <CTableDataCell>{factura.estacionNombre || '-'}</CTableDataCell>}
          <CTableDataCell className="text-nowrap">{formatDate(factura.fecha)}</CTableDataCell>
          <CTableDataCell>
            {factura.terceroId?.nombre || '-'}
            {factura.terceroId?.identificacion && (
              <div className="small text-muted">{factura.terceroId.identificacion}</div>
            )}
          </CTableDataCell>
          <CTableDataCell className="d-none d-lg-table-cell">
            {getFormaPago(factura) || '-'}
          </CTableDataCell>
          <CTableDataCell className="text-end text-nowrap d-none d-xl-table-cell">
            {formatCurrency(factura.subtotal)}
          </CTableDataCell>
          <CTableDataCell className="text-end text-nowrap d-none d-xl-table-cell">
            {formatCurrency(factura.descuento)}
          </CTableDataCell>
          <CTableDataCell className="text-end text-nowrap d-none d-lg-table-cell">
            {formatCurrency(factura.iva)}
          </CTableDataCell>
          <CTableDataCell className="text-end text-nowrap">
            <strong>{formatCurrency(factura.total)}</strong>
          </CTableDataCell>
          <CTableDataCell>
            <Estado factura={factura} />
          </CTableDataCell>
          <CTableDataCell>
            <Acciones factura={factura} onResumen={onResumen} onDetalle={onDetalle} />
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

const TarjetasMovil = ({ facturas, mostrarEstacion, onResumen, onDetalle }) => (
  <div className="d-flex flex-column gap-2">
    {facturas.map((factura) => (
      <CCard key={claveFactura(factura)} className="shadow-sm">
        <CCardBody className="p-3">
          <div className="d-flex justify-content-between align-items-start mb-2">
            <div>
              <strong>#{factura.consecutivo || '-'}</strong>
              <div className="small text-muted">{formatDate(factura.fecha)}</div>
            </div>
            <div className="text-end">
              <div className="fw-bold">{formatCurrency(factura.total)}</div>
              <Estado factura={factura} />
            </div>
          </div>
          {mostrarEstacion && <Dato etiqueta="Estación">{factura.estacionNombre || '-'}</Dato>}
          <Dato etiqueta="Tercero">{factura.terceroId?.nombre || '-'}</Dato>
          <Dato etiqueta="Forma de pago">{getFormaPago(factura) || '-'}</Dato>
          <Dato etiqueta="IVA">{formatCurrency(factura.iva)}</Dato>
          <div className="text-end mt-2">
            <Acciones factura={factura} onResumen={onResumen} onDetalle={onDetalle} />
          </div>
        </CCardBody>
      </CCard>
    ))}
  </div>
)

/**
 * Tabla paginada de facturas de canastilla: tabla en pantallas medianas y grandes, tarjetas en celulares.
 * @param {{ facturas: Array, onDetalle: Function, onResumen?: Function, mostrarEstacion?: boolean,
 *   mensajeVacio?: string }} props
 */
const TablaFacturasCanastilla = ({
  facturas,
  onDetalle,
  onResumen,
  mostrarEstacion = false,
  mensajeVacio = 'No se encontraron facturas con el criterio de búsqueda',
}) => {
  const esPantallaPequena = useEsPantallaPequena()
  const { itemsPagina, paginacion } = usePaginacion(facturas)

  if (!facturas.length) {
    return (
      <CAlert color="warning" className="mb-0">
        {mensajeVacio}
      </CAlert>
    )
  }

  const Vista = esPantallaPequena ? TarjetasMovil : TablaEscritorio
  return (
    <>
      <Vista
        facturas={itemsPagina}
        mostrarEstacion={mostrarEstacion}
        onResumen={onResumen}
        onDetalle={onDetalle}
      />
      <Paginacion {...paginacion} />
    </>
  )
}

export default TablaFacturasCanastilla
