import React from 'react'
import { NavLink } from 'react-router-dom'
import { useSelector, useDispatch } from 'react-redux'
import {
  CContainer,
  CHeader,
  CHeaderBrand,
  CHeaderDivider,
  CHeaderNav,
  CHeaderToggler,
  CNavLink,
  CNavItem,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilAccountLogout, cilLockLocked, cilMenu } from '@coreui/icons'

import { AppBreadcrumb } from './index'
import { AppHeaderDropdown } from './header/index'
import { logo } from 'src/assets/brand/logo'
import EstacionSelector from './header/EstacionSelector'
import {
  getNombreUsuario,
  getRutaInicio,
  isCliente,
  usuariosClientesActivo,
} from '../services/AuthService'

const AppHeader = () => {
  const dispatch = useDispatch()
  const sidebarShow = useSelector((state) => state.sidebarShow)
  const cliente = isCliente()

  return (
    <CHeader position="sticky" className="mb-4">
      <CContainer fluid>
        <CHeaderToggler
          className="ps-1"
          onClick={() => dispatch({ type: 'set', sidebarShow: !sidebarShow })}
        >
          <CIcon icon={cilMenu} size="lg" />
        </CHeaderToggler>
        <CHeaderBrand className="mx-auto d-md-none" to="/">
          <CIcon icon={logo} height={48} alt="Logo" />
        </CHeaderBrand>
        <CHeaderNav className="d-none d-md-flex me-auto">
          <CNavItem>
            <CNavLink to={getRutaInicio()} component={NavLink}>
              {cliente ? 'Inicio' : 'Dashboard'}
            </CNavLink>
          </CNavItem>
        </CHeaderNav>
        <CHeaderNav className="align-items-center">
          {cliente && (
            <CNavItem className="me-2">
              <EstacionSelector />
            </CNavItem>
          )}
          {getNombreUsuario() && (
            <CNavItem className="d-none d-md-block text-medium-emphasis me-2">
              {getNombreUsuario()}
            </CNavItem>
          )}
          {usuariosClientesActivo() && (
            <CNavItem>
              <CNavLink to="/CambiarContrasena" component={NavLink} title="Cambiar contraseña">
                <CIcon icon={cilLockLocked} size="lg" />
              </CNavLink>
            </CNavItem>
          )}
          <CNavItem>
            <CNavLink href="#/Login">
              <CIcon icon={cilAccountLogout} size="lg" />
              Logout
            </CNavLink>
          </CNavItem>
        </CHeaderNav>
      </CContainer>
      <CHeaderDivider />
      <CContainer fluid>
        <AppBreadcrumb />
      </CContainer>
    </CHeader>
  )
}

export default AppHeader
