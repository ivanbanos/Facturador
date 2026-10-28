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
import { FILTROS_VACIOS, hayFiltrosActivos } from './canastillaUtils'

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
 * Filtros sobre las facturas de canastilla ya consultadas (no vuelven a llamar al API).
 * @param {{ filtros: object, opciones: object, onChange: Function, mostrarEstacion?: boolean }} props
 */
const FiltrosCanastilla = ({ filtros, opciones, onChange, mostrarEstacion = false }) => {
  const cambiar = (campo) => (valor) => onChange({ ...filtros, [campo]: valor })

  return (
    <CRow className="g-2 align-items-end mb-3">
      <CCol xs={12} lg={mostrarEstacion ? 4 : 5}>
        <CFormLabel htmlFor="filtroTextoCanastilla" className="small mb-1">
          Buscar en resultados
        </CFormLabel>
        <CInputGroup size="sm">
          <CInputGroupText>
            <CIcon icon={cilSearch} />
          </CInputGroupText>
          <CFormInput
            id="filtroTextoCanastilla"
            placeholder="Consecutivo, tercero, identificación..."
            value={filtros.texto}
            onChange={(e) => cambiar('texto')(e.target.value)}
          />
        </CInputGroup>
      </CCol>
      {mostrarEstacion && (
        <CCol xs={6} md={4} lg={2}>
          <SelectFiltro
            id="filtroEstacionCanastilla"
            etiqueta="Estación"
            valor={filtros.estacion}
            opciones={opciones.estaciones}
            onChange={cambiar('estacion')}
          />
        </CCol>
      )}
      <CCol xs={6} md={4} lg={mostrarEstacion ? 2 : 3}>
        <SelectFiltro
          id="filtroEstadoCanastilla"
          etiqueta="Estado"
          valor={filtros.estado}
          opciones={opciones.estados}
          onChange={cambiar('estado')}
        />
      </CCol>
      <CCol xs={6} md={4} lg={mostrarEstacion ? 2 : 3}>
        <SelectFiltro
          id="filtroFormaPagoCanastilla"
          etiqueta="Forma de pago"
          valor={filtros.formaPago}
          opciones={opciones.formasPago}
          onChange={cambiar('formaPago')}
        />
      </CCol>
      <CCol xs={6} md={12} lg={mostrarEstacion ? 2 : 1} className="d-grid">
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

export default FiltrosCanastilla
