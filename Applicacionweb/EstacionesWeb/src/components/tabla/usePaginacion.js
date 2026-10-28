import { useEffect, useMemo, useState } from 'react'

/**
 * Estado de paginación sobre una lista ya filtrada. Vuelve a la primera página cuando la lista cambia
 * (nueva búsqueda o filtros) y ajusta la página si queda fuera de rango.
 * @param {Array} items
 * @param {number} tamanoInicial
 */
const usePaginacion = (items, tamanoInicial = 10) => {
  const [paginaActual, setPaginaActual] = useState(1)
  const [itemsPorPagina, setItemsPorPagina] = useState(tamanoInicial)

  useEffect(() => {
    setPaginaActual(1)
  }, [items])

  const itemsPagina = useMemo(() => {
    const inicio = (paginaActual - 1) * itemsPorPagina
    return items.slice(inicio, inicio + itemsPorPagina)
  }, [items, paginaActual, itemsPorPagina])

  const cambiarTamano = (tamano) => {
    setItemsPorPagina(tamano)
    setPaginaActual(1)
  }

  return {
    itemsPagina,
    paginacion: {
      paginaActual,
      totalItems: items.length,
      itemsPorPagina,
      onCambiarPagina: setPaginaActual,
      onCambiarTamano: cambiarTamano,
    },
  }
}

export default usePaginacion
