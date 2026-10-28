import React, { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  CAlert,
  CBadge,
  CButton,
  CButtonGroup,
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CFormInput,
  CFormLabel,
  CNav,
  CNavItem,
  CNavLink,
  CRow,
  CSpinner,
  CTabContent,
  CTabPane,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilBasket,
  cilCalendar,
  cilCloudDownload,
  cilDescription,
  cilSearch,
  cilSpreadsheet,
  cilTruck,
} from '@coreui/icons'
import OrdenesDespachoService from '../../services/OrdenesDespachoService'
import FacturasCanastillasService from '../../services/FacturasCanastillasService'
import { getIdentificacionTercero, getNombreUsuario } from '../../services/AuthService'
import Toast from '../toast/Toast'
import FiltrosOrdenes from '../Reportes/ordenes/FiltrosOrdenes'
import ResumenOrdenes from '../Reportes/ordenes/ResumenOrdenes'
import TablaOrdenes from '../Reportes/ordenes/TablaOrdenes'
import { descargarOrdenesExcel, descargarOrdenesPDF } from '../Reportes/ordenes/exportarOrdenes'
import {
  FILTROS_VACIOS as FILTROS_ORDENES,
  calcularEstadisticas,
  filtrarOrdenes,
  opcionesFiltroOrdenes,
} from '../Reportes/ordenes/ordenesUtils'
import FiltrosCanastilla from '../Canastilla/facturas/FiltrosCanastilla'
import ResumenCanastilla from '../Canastilla/facturas/ResumenCanastilla'
import TablaFacturasCanastilla from '../Canastilla/facturas/TablaFacturasCanastilla'
import { descargarCanastillaPDF } from '../Canastilla/facturas/exportarCanastilla'
import {
  FILTROS_VACIOS as FILTROS_CANASTILLA,
  calcularTotales,
  filtrarFacturas,
  hayFiltrosActivos,
  idFactura,
  opcionesFiltroFacturas,
} from '../Canastilla/facturas/canastillaUtils'

const PESTANA_ORDENES = 'ordenes'
const PESTANA_CANASTILLA = 'canastilla'

// Consulta por defecto: últimos 30 días
const DIAS_POR_DEFECTO = 30
// Mismo límite que valida el API (NombresEstaciones.DiasMaximosConsulta)
const DIAS_MAXIMOS = 366
const MS_POR_DIA = 24 * 60 * 60 * 1000

const fechaISO = (fecha) => fecha.toISOString().split('T')[0]

const rangoPorDefecto = () => {
  const hoy = new Date()
  const inicio = new Date(hoy)
  inicio.setDate(hoy.getDate() - DIAS_POR_DEFECTO)
  return { inicial: fechaISO(inicio), final: fechaISO(hoy) }
}

const Cargando = ({ texto }) => (
  <div className="text-center p-3">
    <CSpinner color="primary" />
    <div className="mt-2">{texto}</div>
  </div>
)

/**
 * Facturas del cliente en todas las estaciones: órdenes de despacho y facturas de canastilla,
 * con la estación de cada una. Reemplaza las vistas por estación para los usuarios cliente.
 */
const MisFacturas = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const ordenesService = useMemo(() => new OrdenesDespachoService(), [])
  const canastillaService = useMemo(() => new FacturasCanastillasService(), [])

  const [fechaInicial, setFechaInicial] = useState(() => rangoPorDefecto().inicial)
  const [fechaFinal, setFechaFinal] = useState(() => rangoPorDefecto().final)
  const [pestana, setPestana] = useState(PESTANA_ORDENES)
  const [cargando, setCargando] = useState(false)
  const [consultado, setConsultado] = useState(false)

  const [ordenes, setOrdenes] = useState([])
  const [errorOrdenes, setErrorOrdenes] = useState(null)
  const [filtrosOrdenes, setFiltrosOrdenes] = useState(FILTROS_ORDENES)

  const [reporteCanastilla, setReporteCanastilla] = useState(null)
  const [errorCanastilla, setErrorCanastilla] = useState(null)
  const [filtrosCanastilla, setFiltrosCanastilla] = useState(FILTROS_CANASTILLA)

  const buscar = async (inicial = fechaInicial, final = fechaFinal) => {
    if (!inicial || !final) {
      toastRef.current?.addMessage('Debe seleccionar las fechas inicial y final', 'error')
      return
    }
    if (inicial > final) {
      toastRef.current?.addMessage(
        'La fecha inicial no puede ser mayor que la fecha final',
        'error',
      )
      return
    }
    if ((new Date(final) - new Date(inicial)) / MS_POR_DIA > DIAS_MAXIMOS) {
      toastRef.current?.addMessage(
        `El rango de fechas no puede superar ${DIAS_MAXIMOS} días`,
        'error',
      )
      return
    }

    setCargando(true)
    // Las dos consultas son independientes: si una falla la otra se muestra igual
    const [resOrdenes, resCanastilla] = await Promise.allSettled([
      ordenesService.getOrdenesTodasLasEstaciones(inicial, final),
      canastillaService.getFacturasReporteTodasLasEstaciones(inicial, final),
    ])

    if (resOrdenes.status === 'fulfilled' && Array.isArray(resOrdenes.value)) {
      setOrdenes(resOrdenes.value)
      setErrorOrdenes(null)
    } else {
      console.error('Error consultando órdenes:', resOrdenes.reason || resOrdenes.value)
      setOrdenes([])
      setErrorOrdenes('No se pudieron consultar las órdenes de despacho')
    }

    if (resCanastilla.status === 'fulfilled' && resCanastilla.value?.facturas) {
      setReporteCanastilla(resCanastilla.value)
      setErrorCanastilla(null)
    } else {
      console.error('Error consultando canastilla:', resCanastilla.reason || resCanastilla.value)
      setReporteCanastilla(null)
      setErrorCanastilla('No se pudieron consultar las facturas de canastilla')
    }

    setFiltrosOrdenes(FILTROS_ORDENES)
    setFiltrosCanastilla(FILTROS_CANASTILLA)
    setConsultado(true)
    setCargando(false)
  }

  useEffect(() => {
    buscar()
  }, [])

  const facturasCanastilla = useMemo(() => reporteCanastilla?.facturas || [], [reporteCanastilla])

  // Memorizados: la paginación vuelve a la página 1 solo cuando cambia el resultado filtrado
  const opcionesOrdenes = useMemo(() => opcionesFiltroOrdenes(ordenes), [ordenes])
  const ordenesFiltradas = useMemo(
    () => filtrarOrdenes(ordenes, filtrosOrdenes),
    [ordenes, filtrosOrdenes],
  )
  const estadisticas = useMemo(() => calcularEstadisticas(ordenesFiltradas), [ordenesFiltradas])

  const opcionesCanastilla = useMemo(
    () => opcionesFiltroFacturas(facturasCanastilla),
    [facturasCanastilla],
  )
  const canastillaFiltrada = useMemo(
    () => filtrarFacturas(facturasCanastilla, filtrosCanastilla),
    [facturasCanastilla, filtrosCanastilla],
  )
  const totalesCanastilla = useMemo(() => calcularTotales(canastillaFiltrada), [canastillaFiltrada])

  // En los reportes va el nombre del cliente (tercero); si no hay datos, el del usuario
  const encabezado =
    ordenes[0]?.nombreTercero?.trim() ||
    facturasCanastilla[0]?.terceroId?.nombre ||
    getNombreUsuario() ||
    'Cliente'
  const nit = getIdentificacionTercero() || ''

  const exportar = (accion, mensaje) => {
    try {
      accion()
      toastRef.current?.addMessage(mensaje, 'success')
    } catch (error) {
      console.error('Error exportando:', error)
      toastRef.current?.addMessage('Error al descargar el reporte', 'error')
    }
  }

  const opcionesExportacionOrdenes = () => ({
    fechaInicial,
    fechaFinal,
    estadisticas,
    encabezado,
    nit,
    mostrarEstacion: true,
  })

  const verOrden = (orden) =>
    navigate(`/OrdenDespachoDetalle/${orden.guid || orden.idVentaLocal}`, { state: { orden } })

  const verFacturaCanastilla = (factura) => {
    const id = idFactura(factura)
    if (id) {
      navigate(`/FacturaCanastillaDetalle/${id}`)
    } else {
      toastRef.current?.addMessage('Error: ID de factura no encontrado', 'error')
    }
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilDescription} className="me-2" />
            Mis facturas
          </h4>
          <div className="small text-medium-emphasis">Todas las estaciones</div>
        </CCardHeader>
        <CCardBody>
          <CRow className="g-2 align-items-end">
            <CCol xs={6} md={4} lg={3}>
              <CFormLabel htmlFor="misFacturasFechaInicial">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Inicial
              </CFormLabel>
              <CFormInput
                type="date"
                id="misFacturasFechaInicial"
                value={fechaInicial}
                max={fechaFinal || undefined}
                onChange={(e) => setFechaInicial(e.target.value)}
              />
            </CCol>
            <CCol xs={6} md={4} lg={3}>
              <CFormLabel htmlFor="misFacturasFechaFinal">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Final
              </CFormLabel>
              <CFormInput
                type="date"
                id="misFacturasFechaFinal"
                value={fechaFinal}
                min={fechaInicial || undefined}
                onChange={(e) => setFechaFinal(e.target.value)}
              />
            </CCol>
            <CCol xs={12} md={4} lg={2} className="d-grid">
              <CButton color="primary" onClick={() => buscar()} disabled={cargando}>
                {cargando ? (
                  <>
                    <CSpinner size="sm" className="me-2" />
                    Buscando...
                  </>
                ) : (
                  <>
                    <CIcon icon={cilSearch} className="me-2" />
                    Buscar
                  </>
                )}
              </CButton>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      <CCard>
        <CCardHeader>
          <CNav
            variant="tabs"
            className="card-header-tabs flex-nowrap overflow-auto"
            role="tablist"
          >
            <CNavItem>
              <CNavLink
                href="#"
                active={pestana === PESTANA_ORDENES}
                onClick={(e) => {
                  e.preventDefault()
                  setPestana(PESTANA_ORDENES)
                }}
                className="text-nowrap"
              >
                <CIcon icon={cilTruck} className="me-1" />
                Órdenes de despacho
                {consultado && (
                  <CBadge color="primary" className="ms-2">
                    {ordenesFiltradas.length}
                  </CBadge>
                )}
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                href="#"
                active={pestana === PESTANA_CANASTILLA}
                onClick={(e) => {
                  e.preventDefault()
                  setPestana(PESTANA_CANASTILLA)
                }}
                className="text-nowrap"
              >
                <CIcon icon={cilBasket} className="me-1" />
                Canastilla
                {consultado && (
                  <CBadge color="primary" className="ms-2">
                    {canastillaFiltrada.length}
                  </CBadge>
                )}
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody>
          <CTabContent>
            <CTabPane visible={pestana === PESTANA_ORDENES}>
              {cargando ? (
                <Cargando texto="Cargando órdenes..." />
              ) : errorOrdenes ? (
                <CAlert color="danger">{errorOrdenes}</CAlert>
              ) : (
                consultado && (
                  <>
                    <ResumenOrdenes estadisticas={estadisticas} />
                    <div className="d-flex justify-content-end mb-2">
                      <CButtonGroup size="sm">
                        <CButton
                          color="danger"
                          variant="outline"
                          disabled={!ordenesFiltradas.length}
                          onClick={() =>
                            exportar(
                              () =>
                                descargarOrdenesPDF(ordenesFiltradas, opcionesExportacionOrdenes()),
                              'Reporte descargado exitosamente',
                            )
                          }
                        >
                          <CIcon icon={cilCloudDownload} className="me-2" />
                          PDF
                        </CButton>
                        <CButton
                          color="success"
                          variant="outline"
                          disabled={!ordenesFiltradas.length}
                          onClick={() =>
                            exportar(
                              () =>
                                descargarOrdenesExcel(
                                  ordenesFiltradas,
                                  opcionesExportacionOrdenes(),
                                ),
                              'Reporte Excel descargado exitosamente',
                            )
                          }
                        >
                          <CIcon icon={cilSpreadsheet} className="me-2" />
                          Excel
                        </CButton>
                      </CButtonGroup>
                    </div>
                    <FiltrosOrdenes
                      filtros={filtrosOrdenes}
                      opciones={opcionesOrdenes}
                      onChange={setFiltrosOrdenes}
                      mostrarEstacion
                    />
                    <TablaOrdenes ordenes={ordenesFiltradas} onVer={verOrden} mostrarEstacion />
                  </>
                )
              )}
            </CTabPane>

            <CTabPane visible={pestana === PESTANA_CANASTILLA}>
              {cargando ? (
                <Cargando texto="Cargando facturas de canastilla..." />
              ) : errorCanastilla ? (
                <CAlert color="danger">{errorCanastilla}</CAlert>
              ) : (
                consultado && (
                  <>
                    <ResumenCanastilla totales={totalesCanastilla} />
                    <div className="d-flex justify-content-end mb-2">
                      <CButton
                        size="sm"
                        color="success"
                        variant="outline"
                        disabled={!canastillaFiltrada.length}
                        onClick={() =>
                          exportar(
                            () =>
                              descargarCanastillaPDF(canastillaFiltrada, {
                                // Los desgloses del API son de todo el período
                                reporte: hayFiltrosActivos(filtrosCanastilla)
                                  ? null
                                  : reporteCanastilla,
                                totales: totalesCanastilla,
                                fechaInicial,
                                fechaFinal,
                                encabezado,
                                nit,
                                mostrarEstacion: true,
                              }),
                            'Reporte PDF generado exitosamente',
                          )
                        }
                      >
                        <CIcon icon={cilCloudDownload} className="me-2" />
                        Exportar PDF
                      </CButton>
                    </div>
                    <FiltrosCanastilla
                      filtros={filtrosCanastilla}
                      opciones={opcionesCanastilla}
                      onChange={setFiltrosCanastilla}
                      mostrarEstacion
                    />
                    <TablaFacturasCanastilla
                      facturas={canastillaFiltrada}
                      onDetalle={verFacturaCanastilla}
                      mostrarEstacion
                    />
                  </>
                )
              )}
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>

      <Toast ref={toastRef} />
    </>
  )
}

export default MisFacturas
