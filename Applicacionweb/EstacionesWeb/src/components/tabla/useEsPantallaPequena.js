import { useEffect, useState } from 'react'

// Por debajo de este ancho (breakpoint md de CoreUI) las tablas se muestran como tarjetas
const CONSULTA = '(max-width: 767.98px)'

const coincide = () =>
  typeof window !== 'undefined' && window.matchMedia ? window.matchMedia(CONSULTA).matches : false

/**
 * Indica si la pantalla es de celular, y se actualiza al rotar o cambiar el tamaño de la ventana.
 * @returns {boolean}
 */
const useEsPantallaPequena = () => {
  const [esPequena, setEsPequena] = useState(coincide)

  useEffect(() => {
    if (!window.matchMedia) {
      return undefined
    }
    const media = window.matchMedia(CONSULTA)
    const actualizar = () => setEsPequena(media.matches)
    actualizar()
    media.addEventListener('change', actualizar)
    return () => media.removeEventListener('change', actualizar)
  }, [])

  return esPequena
}

export default useEsPantallaPequena
