import { React, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  CCol,
  CDropdown,
  CDropdownMenu,
  CDropdownItem,
  CDropdownToggle,
  CWidgetStatsA,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilOptions, cilCalculator } from '@coreui/icons'

const MachinesDropdown = (props) => {
  const [isSelected, setIsSelected] = useState(false)

  const handleChangeSelected = (event) => {
    setIsSelected(event.target.checked)
    if (event.target.checked) {
      props.setEstacionSeleccionada(props.estacion.guid)
      localStorage.setItem('estacion', props.estacion.guid)
      localStorage.setItem('estacionNombre', props.estacion.nombre)
      localStorage.setItem('estacionNit', props.estacion.nit)
    }
  }

  const fetchSelected = async () => {
    const guid = localStorage.getItem('estacion')
    setIsSelected(props.estacion.guid == guid)
  }

  useEffect(() => {
    fetchSelected()
  }, [props.estacionSeleccionada])

  return (
    <CCol sm={6} lg={3}>
      <CWidgetStatsA
        className="mb-4"
        value={
          <>
            <input type="checkbox" checked={isSelected} onChange={handleChangeSelected} />
            {props.estacion.nombre}
          </>
        }
        title={props.estacion.guid}
      />
    </CCol>
  )
}

export default MachinesDropdown
