import React, { useEffect, useState } from 'react'
import { CFormSelect } from '@coreui/react'
import EstacionService from '../../services/EstacionService'

const guardarEstacion = (estacion) => {
  localStorage.setItem('estacion', estacion.guid)
  localStorage.setItem('estacionGuid', estacion.guid)
  localStorage.setItem('estacionNombre', estacion.nombre || '')
  localStorage.setItem('estacionNit', estacion.nit || '')
}

// Selector de estacion para usuarios cliente (los administradores la eligen en la pagina Estaciones)
const EstacionSelector = () => {
  const [estaciones, setEstaciones] = useState([])
  const [seleccionada, setSeleccionada] = useState(localStorage.getItem('estacionGuid') || '')

  useEffect(() => {
    const cargar = async () => {
      try {
        const response = await new EstacionService().getEstaciones()
        const lista = Array.isArray(response) ? response : []
        setEstaciones(lista)
        const actual = lista.find((e) => e.guid === localStorage.getItem('estacionGuid'))
        if (!actual && lista.length > 0) {
          guardarEstacion(lista[0])
          setSeleccionada(lista[0].guid)
        }
      } catch (error) {
        console.error('Error cargando estaciones:', error)
      }
    }
    cargar()
  }, [])

  const handleChange = (event) => {
    const estacion = estaciones.find((e) => e.guid === event.target.value)
    if (estacion) {
      guardarEstacion(estacion)
      setSeleccionada(estacion.guid)
      // Las paginas leen la estacion al consultar; se recarga para refrescar los datos
      window.location.reload()
    }
  }

  if (estaciones.length <= 1) {
    return null
  }

  return (
    <CFormSelect
      size="sm"
      aria-label="Estación"
      value={seleccionada}
      onChange={handleChange}
      style={{ minWidth: 180 }}
    >
      {estaciones.map((estacion) => (
        <option key={estacion.guid} value={estacion.guid}>
          {estacion.nombre}
        </option>
      ))}
    </CFormSelect>
  )
}

export default EstacionSelector
