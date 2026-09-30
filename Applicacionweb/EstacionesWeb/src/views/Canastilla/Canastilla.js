import { React, useState, useEffect, useRef } from 'react'
import GetFacturasCanastilla from '../../services/GetFacturasCanastilla'
import { Link, useNavigate } from 'react-router-dom'
import {
  CButton,
  CRow,
  CCol,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CFormInput,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormSelect,
} from '@coreui/react'
import Toast from '../toast/Toast'
var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})
const Canastilla = () => {
  let navigate = useNavigate()
  const [fechaInicial, setFechaInicial] = useState([])
  const [fechaFinal, setFechaFinal] = useState([])
  const [facturas, setFacturas] = useState([])
  const [totales, setTotales] = useState([])

  const toastRef = useRef()
  const filtrarInfoCanastilla = async () => {
    let response = await GetFacturasCanastilla(fechaInicial, fechaFinal)
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
      setFacturas(response)
      let totales = {
        consecutivoInicial: response.reduce(
          (minValue, val) => (val.consecutivo < minValue ? val.consecutivo : minValue),
          response[0].consecutivo,
        ),
        consecutivoFinal: response.reduce(
          (maxValue, val) => (val.consecutivo > maxValue ? val.consecutivo : maxValue),
          response[0].consecutivo,
        ),
        totalfacturas: response.length,
        total: response.reduce((accumulator, value) => {
          return accumulator + value.total
        }, 0),
      }
      console.log(totales)
      setTotales(totales)
    }
  }

  const handlefechaInicialSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaInicial(selectedDate)
    }
  }
  const handlefechaFinalSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaFinal(selectedDate)
    }
  }

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/
    return pattern.test(date)
  }

  const descargarReporte = async (event) => {
    var today = new Date()
    var dd = String(today.getDate()).padStart(2, '0')
    var mm = String(today.getMonth() + 1).padStart(2, '0') //January is 0!
    var yyyy = today.getFullYear()

    today = dd + '/' + mm + '/' + yyyy
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')

    let facturasTabla = [
      ['Consecutivo', 'Fecha', 'Tercero', 'Subtotal', 'Descuento', 'Iva', 'Total'],
    ]
    let facturasTablaElements = facturas.map((factura) => [
      factura.consecutivo,
      factura.fecha,
      factura.nombre,
      { text: cop.format(factura.subtotal), style: 'tableRight' },
      { text: cop.format(factura.descuento), style: 'tableRight' },
      { text: cop.format(factura.iva), style: 'tableRight' },
      { text: cop.format(factura.total), style: 'tableRight' },
    ])

    facturasTabla = facturasTabla.concat(facturasTablaElements)
    var dd = {
      watermark: {
        text: 'SIGES Soluciones',
        color: 'blue',
        opacity: 0.1,
        bold: true,
        italics: false,
      },
      footer: function (currentPage, pageCount) {
        return (
          'SIGES Soluciones Reportes - ' +
          estacionNombre +
          ' - ' +
          currentPage.toString() +
          ' of ' +
          pageCount
        )
      },

      content: [
        {
          stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
          style: 'header',
        },
        {
          text: 'Facturas de canastilla',
          style: 'header',
        },
        {
          text: 'Fecha Inicial ' + fechaInicial + ' Fecha Final ' + fechaFinal,
        },
        {
          text: 'Totales',
          style: 'header',
        },
        {
          text: 'Consecutivo inicial ' + totales.consecutivoInicial,
        },
        {
          text: 'Consecutivo final ' + totales.consecutivoFinal,
        },
        {
          text: 'Cantidad ' + totales.totalfacturas,
        },
        {
          text: 'Total ' + cop.format(totales.total),
        },

        {
          text: 'Facturas',
          style: 'header',
        },
        {
          style: 'tableExample',
          table: {
            headerRows: 1,
            body: facturasTabla,
          },
        },
      ],

      styles: {
        header: {
          fontSize: 18,
          bold: true,
          alignment: 'center',
        },
        subheader: {
          fontSize: 14,
        },
        superMargin: {
          fontSize: 15,
        },

        tableRight: {
          alignment: 'right',
        },
        tableHeader: {
          bold: true,
          fontSize: 13,
          color: 'black',
        },
      },
    }

    pdfMake
      .createPdf(dd)
      .download('FacturasdeCanastilla - ' + estacionNombre + ' - ' + today + '.pdf')
  }

  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Facturas de canastilla</h1>

      <CRow>
        <CCol xs={2}>
          <label>Fecha Inicial</label>
        </CCol>
        <CCol xs={4}>
          <input
            type="date"
            className="form-control modal-tercero-input"
            name="fechaTurno"
            value={fechaInicial}
            onChange={handlefechaInicialSelectedChange}
          ></input>
        </CCol>
        <CCol xs={2}>
          <label>Fecha Final</label>
        </CCol>
        <CCol xs={4}>
          <input
            type="date"
            className="form-control modal-tercero-input"
            name="fechaTurno"
            value={fechaFinal}
            onChange={handlefechaFinalSelectedChange}
          ></input>
        </CCol>
        <CButton style={{ margin: '2pt' }} onClick={filtrarInfoCanastilla}>
          Filtrar
        </CButton>
      </CRow>
      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
      <CRow>
        <h2>Resumen general</h2>
        <label>
          <b>Consecutivo Inicial:</b> {totales.consecutivoInicial}
        </label>
        <label>
          {' '}
          <b>Consecutivo Final:</b> {totales.consecutivoFinal}
        </label>
        <label>
          <b>Cantidad:</b> {totales.totalfacturas}
        </label>
        <label>
          <b>Total:</b> {cop.format(totales.total)}
        </label>

        <h2>Facturas</h2>
        <CTable>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell scope="col">Consecutivo</CTableHeaderCell>
              <CTableHeaderCell scope="col">Fecha</CTableHeaderCell>
              <CTableHeaderCell scope="col">Tercero</CTableHeaderCell>
              <CTableHeaderCell scope="col">Sub total</CTableHeaderCell>
              <CTableHeaderCell scope="col">Descuento</CTableHeaderCell>
              <CTableHeaderCell scope="col">Iva</CTableHeaderCell>
              <CTableHeaderCell scope="col">Total</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {facturas.map((factura) => (
              <CTableRow key={factura.consecutivo}>
                <CTableHeaderCell>{factura.consecutivo}</CTableHeaderCell>
                <CTableHeaderCell>{factura.fecha}</CTableHeaderCell>
                <CTableHeaderCell>{factura.nombre}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(factura.subtotal)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(factura.descuento)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(factura.iva)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(factura.total)}</CTableHeaderCell>
              </CTableRow>
            ))}
          </CTableBody>
        </CTable>
      </CRow>
      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
    </>
  )
}

export default Canastilla
