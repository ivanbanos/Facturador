import React, { useEffect, useRef } from 'react'
import QRCode from 'qrcode'

const QRCodeDisplay = ({ value, size = 200, className = '' }) => {
  const canvasRef = useRef(null)

  useEffect(() => {
    if (value && canvasRef.current) {
      QRCode.toCanvas(canvasRef.current, value, {
        width: size,
        height: size,
        color: {
          dark: '#000000',
          light: '#FFFFFF',
        },
      })
    }
  }, [value, size])

  if (!value) {
    return null
  }

  return (
    <div className={`text-center ${className}`}>
      <canvas ref={canvasRef} style={{ maxWidth: '100%', height: 'auto' }} />
    </div>
  )
}

export default QRCodeDisplay
