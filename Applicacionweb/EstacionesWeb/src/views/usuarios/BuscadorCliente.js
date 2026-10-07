import React, { useState } from 'react'
import PropTypes from 'prop-types'
import {
  CAlert,
  CButton,
  CFormInput,
  CFormLabel,
  CInputGroup,
  CListGroup,
  CListGroupItem,
  CSpinner,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilSearch, cilX } from '@coreui/icons'
import TercerosService from '../../services/TercerosService'

const tercerosService = new TercerosService()

const nombreTercero = (tercero) =>
  [tercero.nombre, tercero.segundo, tercero.apellidos]
    .filter((parte) => parte && parte !== 'No')
    .join(' ')

// Busca un cliente (tercero) por identificacion y permite seleccionarlo
const BuscadorCliente = ({ seleccionado, onSeleccionar }) => {
  const [identificacion, setIdentificacion] = useState('')
  const [resultados, setResultados] = useState([])
  const [buscando, setBuscando] = useState(false)
  const [mensaje, setMensaje] = useState(null)

  const buscar = async () => {
    if (!identificacion.trim()) {
      setMensaje('Ingrese una identificación')
      return
    }
    setBuscando(true)
    setMensaje(null)
    try {
      const response = await tercerosService.buscarPorIdentificacionServidor(identificacion)
      const lista = Array.isArray(response) ? response : []
      setResultados(lista)
      if (lista.length === 0) {
        setMensaje('No se encontró ningún cliente con esa identificación')
      } else if (lista.length === 1) {
        onSeleccionar(lista[0])
      }
    } catch (error) {
      console.error('Error buscando cliente:', error)
      setMensaje('Error al buscar el cliente')
    } finally {
      setBuscando(false)
    }
  }

  const handleKeyDown = (event) => {
    if (event.key === 'Enter') {
      event.preventDefault()
      buscar()
    }
  }

  if (seleccionado) {
    return (
      <CAlert color="success" className="d-flex justify-content-between align-items-center py-2">
        <span>
          <strong>{seleccionado.identificacion}</strong> — {nombreTercero(seleccionado)}
        </span>
        <CButton
          color="link"
          size="sm"
          title="Quitar cliente"
          onClick={() => {
            onSeleccionar(null)
            setResultados([])
          }}
        >
          <CIcon icon={cilX} />
        </CButton>
      </CAlert>
    )
  }

  return (
    <div>
      <CFormLabel htmlFor="buscarIdentificacion">Buscar cliente por identificación</CFormLabel>
      <CInputGroup>
        <CFormInput
          id="buscarIdentificacion"
          placeholder="Número de identificación"
          value={identificacion}
          onChange={(e) => setIdentificacion(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <CButton type="button" color="secondary" onClick={buscar} disabled={buscando}>
          {buscando ? <CSpinner size="sm" /> : <CIcon icon={cilSearch} />} Buscar
        </CButton>
      </CInputGroup>
      {mensaje && <div className="text-danger small mt-1">{mensaje}</div>}
      {resultados.length > 1 && (
        <CListGroup className="mt-2">
          {resultados.map((tercero) => (
            <CListGroupItem
              key={tercero.guid || tercero.identificacion}
              component="button"
              type="button"
              onClick={() => onSeleccionar(tercero)}
            >
              <strong>{tercero.identificacion}</strong> — {nombreTercero(tercero)}
            </CListGroupItem>
          ))}
        </CListGroup>
      )}
    </div>
  )
}

BuscadorCliente.propTypes = {
  seleccionado: PropTypes.object,
  onSeleccionar: PropTypes.func.isRequired,
}

export default BuscadorCliente
