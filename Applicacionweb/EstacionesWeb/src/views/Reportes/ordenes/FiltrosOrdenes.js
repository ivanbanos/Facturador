import React from 'react'
import {
  CButton,
  CCol,
  CFormInput,
  CFormLabel,
  CFormSelect,
  CInputGroup,
  CInputGroupText,
  CRow,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilFilterX, cilSearch } from '@coreui/icons'
import { FILTROS_VACIOS, OPCIONES_FACTURA, hayFiltrosActivos } from './ordenesUtils'

const SelectFiltro = ({ id, etiqueta, valor, opciones, onChange }) => (
  <>
    <CFormLabel htmlFor={id} className="small mb-1">
      {etiqueta}
    </CFormLabel>
    <CFormSelect id={id} size="sm" value={valor} onChange={(e) => onChange(e.target.value)}>
      <option value="">Todos</option>
      {opciones.map((opcion) => (
        <option key={opcion} value={opcion}>
          {opcion}
        </option>
      ))}
    </CFormSelect>
  </>
)

/**
 * Filtros sobre las órdenes ya consultadas (no vuelven a llamar al API).
 * @param {{ filtros: object, opciones: object, onChange: Function, mostrarEstacion?: boolean }} props
 */
const FiltrosOrdenes = ({ filtros, opciones, onChange, mostrarEstacion = false }) => {
  const cambiar = (campo) => (valor) => onChange({ ...filtros, [campo]: valor })

  return (
    <CRow className="g-2 align-items-end mb-3">
      <CCol xs={12} lg={3}>
        <CFormLabel htmlFor="filtroTexto" className="small mb-1">
          Buscar
        </CFormLabel>
        <CInputGroup size="sm">
          <CInputGroupText>
            <CIcon icon={cilSearch} />
          </CInputGroupText>
          <CFormInput
            id="filtroTexto"
            placeholder="ID, cliente, placa, combustible..."
            value={filtros.texto}
            onChange={(e) => cambiar('texto')(e.target.value)}
          />
        </CInputGroup>
      </CCol>
      {mostrarEstacion && (
        <CCol xs={6} md={4} lg={2}>
          <SelectFiltro
            id="filtroEstacion"
            etiqueta="Estación"
            valor={filtros.estacion}
            opciones={opciones.estaciones}
            onChange={cambiar('estacion')}
          />
        </CCol>
      )}
      <CCol xs={6} md={4} lg={2}>
        <SelectFiltro
          id="filtroEstado"
          etiqueta="Estado"
          valor={filtros.estado}
          opciones={opciones.estados}
          onChange={cambiar('estado')}
        />
      </CCol>
      <CCol xs={6} md={4} lg={mostrarEstacion ? 1 : 2}>
        <SelectFiltro
          id="filtroCombustible"
          etiqueta="Combustible"
          valor={filtros.combustible}
          opciones={opciones.combustibles}
          onChange={cambiar('combustible')}
        />
      </CCol>
      <CCol xs={6} md={4} lg={2}>
        <SelectFiltro
          id="filtroFormaPago"
          etiqueta="Forma de pago"
          valor={filtros.formaPago}
          opciones={opciones.formasPago}
          onChange={cambiar('formaPago')}
        />
      </CCol>
      <CCol xs={6} md={4} lg={mostrarEstacion ? 1 : 2}>
        <CFormLabel htmlFor="filtroFactura" className="small mb-1">
          Factura
        </CFormLabel>
        <CFormSelect
          id="filtroFactura"
          size="sm"
          value={filtros.factura}
          onChange={(e) => cambiar('factura')(e.target.value)}
        >
          {OPCIONES_FACTURA.map((opcion) => (
            <option key={opcion.valor} value={opcion.valor}>
              {opcion.etiqueta}
            </option>
          ))}
        </CFormSelect>
      </CCol>
      <CCol xs={12} md={4} lg={1} className="d-grid">
        <CButton
          size="sm"
          color="secondary"
          variant="outline"
          disabled={!hayFiltrosActivos(filtros)}
          onClick={() => onChange(FILTROS_VACIOS)}
          title="Limpiar filtros"
        >
          <CIcon icon={cilFilterX} className="me-1" />
          Limpiar
        </CButton>
      </CCol>
    </CRow>
  )
}

export default FiltrosOrdenes
