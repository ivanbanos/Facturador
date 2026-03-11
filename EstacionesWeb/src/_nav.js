import React from 'react'
import CIcon from '@coreui/icons-react'
import {
  cilCalculator,
  cilSpeedometer,
  cilUser,
  cilPlus,
  cilCog,
  cilContact,
  cilPeople,
  cilSpreadsheet,
} from '@coreui/icons'
import { CNavGroup, CNavItem, CNavTitle } from '@coreui/react'

const _nav = [
  {
    component: CNavItem,
    name: 'Estaciones',
    to: '/dashboard',
    icon: <CIcon icon={cilSpeedometer} customClassName="nav-icon" />,
    level: 2,
  },
  {
    component: CNavItem,
    name: 'Terceros',
    to: '/terceros',
    icon: <CIcon icon={cilPeople} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Órdenes de Despacho',
    to: '/OrdenesDespacho',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Reporte Fiscal',
    to: '/ReporteFiscal',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Items de Canastilla',
    to: '/CanastillaItem',
    icon: <CIcon icon={cilPlus} customClassName="nav-icon" />,
    level: 2,
  },
  {
    component: CNavItem,
    name: 'Facturas de Canastilla',
    to: '/FacturasCanastilla',
    icon: <CIcon icon={cilContact} customClassName="nav-icon" />,
    level: 2,
  },
  {
    component: CNavItem,
    name: 'Ventas por clientes',
    to: '/VentasClientes',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Turnos',
    to: '/Turnos',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Cupo por cliente',
    to: '/PorCliente',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Cupo por automotores',
    to: '/PorAutomotores',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Consolidado de Órdenes',
    to: '/consolidado-ordenes',
    icon: <CIcon icon={cilSpreadsheet} customClassName="nav-icon" />,
    level: 1,
  },
]

export default _nav
