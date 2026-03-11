import React, { useState, useEffect, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CRow,
  CCol,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CTableDataCell,
  CSpinner,
  CBadge,
  CButtonGroup,
  CAlert,
  CCollapse,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilArrowLeft,
  cilPrint,
  cilContact,
  cilUser,
  cilCalendar,
  cilDollar,
  cilTask,
} from '@coreui/icons'
import FacturasCanastillasService from '../../services/FacturasCanastillasService'
import FacturaElectronicaService from '../../services/FacturaElectronicaService'
import QRCodeDisplay from '../../components/QRCodeDisplay'
import Toast from '../toast/Toast'

// Formato de moneda colombiana
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
  }).format(amount || 0)
}

// Formato de fecha
const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('es-CO', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// Función para transformar el estado mostrado al usuario
const getEstadoDisplay = (estado) => {
  if (!estado) return 'Activa'
  const estadoLower = estado.toLowerCase()
  if (estadoLower === 'anulada' || estadoLower === 'anulado') {
    return 'Facturada'
  }
  return estado
}

const FacturaCanastillaDetalle = () => {
  const navigate = useNavigate()
  const { idFactura } = useParams()
  const toastRef = useRef()
  const facturasService = new FacturasCanastillasService()
  const printRef = useRef()

  // State management
  const [factura, setFactura] = useState(null)
  const [loading, setLoading] = useState(true)
  const [facturaElectronica, setFacturaElectronica] = useState(null)
  const [loadingFacturaElectronica, setLoadingFacturaElectronica] = useState(false)
  const [mostrarFacturaElectronica, setMostrarFacturaElectronica] = useState(false)
  const [estacionInfo, setEstacionInfo] = useState({
    nombre: localStorage.getItem('estacionNombre') || 'Estación',
    nit: localStorage.getItem('estacionNit') || '',
    direccion: localStorage.getItem('estacionDireccion') || '',
    telefono: localStorage.getItem('estacionTelefono') || '',
  })

  // Función para parsear información de factura electrónica desde el campo idFacturaElectronica
  const parseFacturaElectronicaFromId = (idFacturaElectronica) => {
    if (!idFacturaElectronica) return null

    try {
      // Formato: DIAN_ACEPTADO:FEES165606:b3077c2c8530a18e95d42192b3d800babda53c2ab90dd2bcd1e1a0d38e699d6b5bdbf6124b62424c3ad0d908e38595a7
      const parts = idFacturaElectronica.split(':')

      if (parts.length >= 3) {
        const estado = parts[0] // DIAN_ACEPTADO (lo podemos obviar)
        const consecutivo = parts[1] // FEES165606
        const cufe = parts[2] // b3077c2c...

        return {
          consecutivo: consecutivo,
          cufe: cufe,
          fecha: factura?.fecha || 'N/A', // Usar la fecha de la factura
          qrUrl: `https://catalogo-vpfe.dian.gov.co/document/searchqr?documentkey=${cufe}`,
        }
      }

      return null
    } catch (error) {
      console.error('Error parsing factura electrónica:', error)
      return null
    }
  }

  useEffect(() => {
    if (idFactura) {
      cargarFactura()
    } else {
      navigate('/FacturasCanastilla')
    }
  }, [idFactura])

  const cargarFactura = async () => {
    setLoading(true)
    try {
      // Cargar factura principal
      const facturaResponse = await facturasService.getFacturaCanastilla(idFactura)

      if (facturaResponse === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setFactura(facturaResponse)

      // Si la factura tiene idFacturaElectronica, parsear la información
      if (facturaResponse?.idFacturaElectronica) {
        const facturaElectronicaData = parseFacturaElectronicaFromId(
          facturaResponse.idFacturaElectronica,
        )
        if (facturaElectronicaData) {
          setFacturaElectronica(facturaElectronicaData)
        }
      }

      toastRef.current?.addMessage('Factura cargada exitosamente', 'success')
    } catch (error) {
      console.error('Error loading factura:', error)
      toastRef.current?.addMessage('Error al cargar la factura', 'error')
      navigate('/FacturasCanastilla')
    } finally {
      setLoading(false)
    }
  }

  const handlePrint = () => {
    // Crear una nueva ventana para impresión
    const printWindow = window.open('', '_blank')

    if (!printWindow) {
      toastRef.current?.addMessage(
        'Error: No se pudo abrir la ventana de impresión. Verifique que no esté bloqueada por el navegador.',
        'error',
      )
      return
    }

    const printContent = generatePrintContent()

    printWindow.document.open()
    printWindow.document.write(printContent)
    printWindow.document.close()

    // Esperar a que se cargue el contenido y luego imprimir
    printWindow.onload = () => {
      printWindow.print()
      printWindow.close()
    }

    toastRef.current?.addMessage('Enviando a impresión...', 'info')
  }

  const generatePrintContent = () => {
    const today = new Date().toLocaleDateString('es-CO')

    return `
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8">
      <title>Factura de Canastilla - ${factura.consecutivo}</title>
      <style>
        @media print {
          @page {
            margin: 0.5in;
            size: letter;
          }
        }
        
        body {
          font-family: Arial, sans-serif;
          font-size: 12px;
          line-height: 1.4;
          color: #333;
          margin: 0;
          padding: 20px;
        }
        
        .header {
          text-align: center;
          border-bottom: 2px solid #333;
          padding-bottom: 20px;
          margin-bottom: 20px;
        }
        
        .company-name {
          font-size: 18px;
          font-weight: bold;
          margin-bottom: 5px;
        }
        
        .company-info {
          font-size: 11px;
          color: #666;
        }
        
        .invoice-title {
          font-size: 16px;
          font-weight: bold;
          margin: 20px 0;
          text-align: center;
          background-color: #f5f5f5;
          padding: 10px;
          border: 1px solid #ddd;
        }
        
        .info-section {
          margin-bottom: 20px;
        }
        
        .info-row {
          display: flex;
          margin-bottom: 8px;
        }
        
        .info-label {
          font-weight: bold;
          min-width: 120px;
        }
        
        .info-value {
          flex: 1;
        }
        
        .two-column {
          display: flex;
          gap: 40px;
          margin-bottom: 20px;
        }
        
        .column {
          flex: 1;
        }
        
        .table {
          width: 100%;
          border-collapse: collapse;
          margin-bottom: 20px;
        }
        
        .table th,
        .table td {
          border: 1px solid #ddd;
          padding: 8px;
          text-align: left;
        }
        
        .table th {
          background-color: #f5f5f5;
          font-weight: bold;
        }
        
        .table .text-right {
          text-align: right;
        }
        
        .table .text-center {
          text-align: center;
        }
        
        .totals-section {
          margin-top: 20px;
          border-top: 2px solid #333;
          padding-top: 15px;
        }
        
        .total-row {
          display: flex;
          justify-content: space-between;
          margin-bottom: 5px;
          padding: 3px 0;
        }
        
        .total-label {
          font-weight: bold;
        }
        
        .total-value {
          font-weight: bold;
          min-width: 120px;
          text-align: right;
        }
        
        .grand-total {
          font-size: 14px;
          border-top: 1px solid #333;
          padding-top: 8px;
          margin-top: 8px;
        }
        
        .footer {
          margin-top: 40px;
          text-align: center;
          font-size: 10px;
          color: #666;
          border-top: 1px solid #ddd;
          padding-top: 10px;
        }
        
        .status-badge {
          display: inline-block;
          padding: 4px 8px;
          border-radius: 4px;
          font-size: 10px;
          font-weight: bold;
          text-transform: uppercase;
        }
        
        .status-active {
          background-color: #d4edda;
          color: #155724;
          border: 1px solid #c3e6cb;
        }
        
        .status-cancelled {
          background-color: #f8d7da;
          color: #721c24;
          border: 1px solid #f5c6cb;
        }
        
        @media print {
          body {
            padding: 0;
          }
          
          .no-print {
            display: none !important;
          }
        }
      </style>
    </head>
    <body>
      <div class="header">
        <div class="company-name">${estacionInfo.nombre}</div>
        <div class="company-info">
          NIT: ${estacionInfo.nit}<br>
          ${estacionInfo.direccion ? `${estacionInfo.direccion}<br>` : ''}
          ${estacionInfo.telefono ? `Tel: ${estacionInfo.telefono}` : ''}
        </div>
      </div>
      
      <div class="invoice-title">
        FACTURA DE CANASTILLA
      </div>
      
      <div class="two-column">
        <div class="column">
          <div class="info-section">
            <h4>Información de la Factura</h4>
            <div class="info-row">
              <span class="info-label">Consecutivo:</span>
              <span class="info-value">${factura.consecutivo || '-'}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Fecha:</span>
              <span class="info-value">${formatDate(factura.fecha)}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Estado:</span>
              <span class="info-value">
                <span class="status-badge ${
                  factura.estado === 'Anulada' ? 'status-cancelled' : 'status-active'
                }">
                  ${getEstadoDisplay(factura.estado)}
                </span>
              </span>
            </div>
            ${
              factura.resolucion
                ? `
            <div class="info-row">
              <span class="info-label">Resolución:</span>
              <span class="info-value">${factura.resolucion.descripcion || ''} - ${
                    factura.resolucion.autorizacion || ''
                  }</span>
            </div>
            `
                : ''
            }
          </div>
        </div>
        
        <div class="column">
          <div class="info-section">
            <h4>Información del Cliente</h4>
            <div class="info-row">
              <span class="info-label">Nombre:</span>
              <span class="info-value">${factura.terceroId?.nombre || '-'}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Identificación:</span>
              <span class="info-value">${factura.terceroId?.identificacion || '-'}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Dirección:</span>
              <span class="info-value">${factura.terceroId?.direccion || '-'}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Teléfono:</span>
              <span class="info-value">${
                factura.terceroId?.telefono || factura.terceroId?.celular || '-'
              }</span>
            </div>
          </div>
        </div>
      </div>
      
      ${
        factura.canastillas && factura.canastillas.length > 0
          ? `
      <div class="info-section">
        <h4>Detalle de Items</h4>
        <table class="table">
          <thead>
            <tr>
              <th>Descripción</th>
              <th class="text-center">Unidad</th>
              <th class="text-center">Cantidad</th>
              <th class="text-right">Precio Unit.</th>
              <th class="text-right">Subtotal</th>
              <th class="text-right">IVA</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            ${factura.canastillas
              .map(
                (item) => `
            <tr>
              <td>${item.canastilla?.descripcion || '-'}</td>
              <td class="text-center">${item.canastilla?.unidad || '-'}</td>
              <td class="text-center">${item.cantidad || 0}</td>
              <td class="text-right">${formatCurrency(item.precio)}</td>
              <td class="text-right">${formatCurrency(item.subtotal)}</td>
              <td class="text-right">${formatCurrency(item.iva)}</td>
              <td class="text-right">${formatCurrency(item.total)}</td>
            </tr>
            `,
              )
              .join('')}
          </tbody>
        </table>
      </div>
      `
          : ''
      }
      
      <div class="totals-section">
        <div class="total-row">
          <span class="total-label">Subtotal:</span>
          <span class="total-value">${formatCurrency(factura.subtotal)}</span>
        </div>
        <div class="total-row">
          <span class="total-label">Descuento:</span>
          <span class="total-value">${formatCurrency(factura.descuento)}</span>
        </div>
        <div class="total-row">
          <span class="total-label">IVA:</span>
          <span class="total-value">${formatCurrency(factura.iva)}</span>
        </div>
        <div class="total-row grand-total">
          <span class="total-label">TOTAL:</span>
          <span class="total-value">${formatCurrency(factura.total)}</span>
        </div>
      </div>
      
      <div class="footer">
        <p>Factura generada el ${today} - SIGES Soluciones</p>
        <p>Este documento es una representación impresa de la factura electrónica</p>
      </div>
    </body>
    </html>
    `
  }

  const volver = () => {
    navigate('/FacturasCanastilla')
  }

  if (loading) {
    return (
      <CCard>
        <CCardBody>
          <div className="text-center p-5">
            <CSpinner color="primary" size="lg" />
            <div className="mt-3">Cargando factura...</div>
          </div>
        </CCardBody>
      </CCard>
    )
  }

  if (!factura) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="danger">
            <h4>Factura no encontrada</h4>
            <p>No se pudo cargar la factura solicitada.</p>
            <CButton color="primary" onClick={volver}>
              <CIcon icon={cilArrowLeft} className="me-2" />
              Volver al listado
            </CButton>
          </CAlert>
        </CCardBody>
      </CCard>
    )
  }

  return (
    <>
      <CCard>
        <CCardHeader>
          <CRow>
            <CCol>
              <h4>
                <CIcon icon={cilContact} className="me-2" />
                Factura de Canastilla - {factura.consecutivo}
              </h4>
            </CCol>
            <CCol xs="auto">
              <CButtonGroup>
                <CButton color="success" onClick={handlePrint}>
                  <CIcon icon={cilPrint} className="me-2" />
                  Imprimir
                </CButton>
                <CButton color="secondary" variant="outline" onClick={volver}>
                  <CIcon icon={cilArrowLeft} className="me-2" />
                  Volver
                </CButton>
              </CButtonGroup>
            </CCol>
          </CRow>
        </CCardHeader>

        <CCardBody>
          {/* Información Principal */}
          <CRow className="mb-4">
            <CCol md={6}>
              <CCard className="h-100">
                <CCardHeader>
                  <CIcon icon={cilContact} className="me-2" />
                  Información de la Factura
                </CCardHeader>
                <CCardBody>
                  <div className="mb-3">
                    <strong>Consecutivo:</strong> {factura.consecutivo || '-'}
                  </div>
                  <div className="mb-3">
                    <strong>Fecha:</strong> {formatDate(factura.fecha)}
                  </div>
                  <div className="mb-3">
                    <strong>Estado:</strong>{' '}
                    <CBadge color={factura.estado === 'Anulada' ? 'danger' : 'success'}>
                      {getEstadoDisplay(factura.estado)}
                    </CBadge>
                  </div>
                  {factura.resolucion && (
                    <div className="mb-3">
                      <strong>Resolución:</strong> {factura.resolucion.descripcion || ''} -{' '}
                      {factura.resolucion.autorizacion || ''}
                    </div>
                  )}
                  {factura.codigoFormaPago && (
                    <div className="mb-3">
                      <strong>Forma de Pago:</strong> {factura.codigoFormaPago.descripcion || '-'}
                    </div>
                  )}
                  {factura.idFacturaElectronica && (
                    <div className="mb-3">
                      <strong>Factura Electrónica:</strong>{' '}
                      <CBadge color="success">Disponible</CBadge>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>

            <CCol md={6}>
              <CCard className="h-100">
                <CCardHeader>
                  <CIcon icon={cilUser} className="me-2" />
                  Información del Cliente
                </CCardHeader>
                <CCardBody>
                  <div className="mb-3">
                    <strong>Nombre:</strong> {factura.terceroId?.nombre || '-'}
                  </div>
                  <div className="mb-3">
                    <strong>Identificación:</strong> {factura.terceroId?.identificacion || '-'}
                  </div>
                  <div className="mb-3">
                    <strong>Dirección:</strong> {factura.terceroId?.direccion || '-'}
                  </div>
                  <div className="mb-3">
                    <strong>Teléfono:</strong>{' '}
                    {factura.terceroId?.telefono || factura.terceroId?.celular || '-'}
                  </div>
                  <div className="mb-3">
                    <strong>Email:</strong> {factura.terceroId?.correo || '-'}
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          {/* Información de Factura Electrónica */}
          {factura?.idFacturaElectronica && (
            <CRow className="mb-4">
              <CCol>
                <CCard className="border-primary">
                  <CCardHeader className="bg-primary text-white">
                    <CButton
                      color="primary"
                      variant="ghost"
                      className="text-white p-0 border-0 d-flex align-items-center"
                      onClick={() => setMostrarFacturaElectronica(!mostrarFacturaElectronica)}
                    >
                      <span className="me-2">{mostrarFacturaElectronica ? '▲' : '▼'}</span>
                      <CIcon icon={cilTask} className="me-2" />
                      Información de Factura Electrónica
                    </CButton>
                  </CCardHeader>
                  <CCollapse visible={mostrarFacturaElectronica}>
                    <CCardBody>
                      {facturaElectronica ? (
                        <CRow>
                          <CCol md={6}>
                            <table className="table table-sm">
                              <tbody>
                                <tr>
                                  <td>
                                    <strong>Consecutivo:</strong>
                                  </td>
                                  <td>{facturaElectronica.consecutivo}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>Fecha:</strong>
                                  </td>
                                  <td>{formatDate(facturaElectronica.fecha)}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>CUFE:</strong>
                                  </td>
                                  <td className="text-break small">{facturaElectronica.cufe}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>Consultar en DIAN:</strong>
                                  </td>
                                  <td>
                                    <a
                                      href={facturaElectronica.qrUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      className="btn btn-sm btn-outline-primary"
                                    >
                                      Ver en DIAN
                                    </a>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </CCol>
                          <CCol md={6}>
                            <div className="text-center">
                              <p className="small text-muted mb-2">
                                Código QR para consulta en DIAN
                              </p>
                              <QRCodeDisplay value={facturaElectronica.qrUrl} size={150} />
                            </div>
                          </CCol>
                        </CRow>
                      ) : (
                        <CAlert color="warning" className="mb-0">
                          No se pudo procesar la información de la factura electrónica.
                        </CAlert>
                      )}
                    </CCardBody>
                  </CCollapse>
                </CCard>
              </CCol>
            </CRow>
          )}

          {/* Items de la Factura */}
          {factura.canastillas && factura.canastillas.length > 0 && (
            <CCard className="mb-4">
              <CCardHeader>
                <h5>Detalle de Items</h5>
              </CCardHeader>
              <CCardBody>
                <CTable hover responsive>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>Descripción</CTableHeaderCell>
                      <CTableHeaderCell>Unidad</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">Precio Unit.</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">Subtotal</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">IVA</CTableHeaderCell>
                      <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {factura.canastillas.map((item, index) => (
                      <CTableRow key={index}>
                        <CTableDataCell>{item.canastilla?.descripcion || '-'}</CTableDataCell>
                        <CTableDataCell>{item.canastilla?.unidad || '-'}</CTableDataCell>
                        <CTableDataCell className="text-end">{item.cantidad || 0}</CTableDataCell>
                        <CTableDataCell className="text-end">
                          {formatCurrency(item.precio)}
                        </CTableDataCell>
                        <CTableDataCell className="text-end">
                          {formatCurrency(item.subtotal)}
                        </CTableDataCell>
                        <CTableDataCell className="text-end">
                          {formatCurrency(item.iva)}
                        </CTableDataCell>
                        <CTableDataCell className="text-end">
                          <strong>{formatCurrency(item.total)}</strong>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>
              </CCardBody>
            </CCard>
          )}

          {/* Totales */}
          <CCard>
            <CCardHeader>
              <CIcon icon={cilDollar} className="me-2" />
              Resumen de Totales
            </CCardHeader>
            <CCardBody>
              <CRow>
                <CCol md={6}>
                  <div className="mb-2">
                    <strong>Subtotal:</strong>
                    <span className="float-end">{formatCurrency(factura.subtotal)}</span>
                  </div>
                  <div className="mb-2">
                    <strong>Descuento:</strong>
                    <span className="float-end">{formatCurrency(factura.descuento)}</span>
                  </div>
                </CCol>
                <CCol md={6}>
                  <div className="mb-2">
                    <strong>IVA:</strong>
                    <span className="float-end">{formatCurrency(factura.iva)}</span>
                  </div>
                  <div className="mb-2 border-top pt-2">
                    <strong style={{ fontSize: '1.2em' }}>TOTAL:</strong>
                    <span className="float-end" style={{ fontSize: '1.2em', fontWeight: 'bold' }}>
                      {formatCurrency(factura.total)}
                    </span>
                  </div>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCardBody>
      </CCard>

      <Toast ref={toastRef} />
    </>
  )
}

export default FacturaCanastillaDetalle
