import { React, useState, useEffect, useRef, useImperativeHandle, forwardRef } from 'react'
import {
  CButton,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardText,
  CCardHeader,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CFormInput,
  CFormSelect,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CToast,
  CToastBody,
  CToastClose,
  CToastHeader,
  CToaster,
} from '@coreui/react'

const Toast = forwardRef((props, ref) => {
  const [toast, addToast] = useState(0)
  const toaster = useRef()

  useImperativeHandle(ref, () => ({
    addMessage(message, type = 'primary') {
      const colorMap = {
        success: 'success',
        error: 'danger',
        warning: 'warning',
        info: 'info',
        primary: 'primary',
      }

      addToast(
        <CToast
          autohide={true}
          delay={4000}
          color={colorMap[type] || 'primary'}
          className="text-white align-items-center"
          visible={true}
        >
          <div className="d-flex">
            <CToastBody>{message}</CToastBody>
            <CToastClose className="me-2 m-auto" white />
          </div>
        </CToast>,
      )
    },
    showToast(message) {
      this.addMessage(message, 'primary')
    },
  }))

  return (
    <>
      <CToaster ref={toaster} push={toast} placement="top-end" />
    </>
  )
})

Toast.displayName = 'Toast'

export default Toast
