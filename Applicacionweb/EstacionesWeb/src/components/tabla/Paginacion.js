import React from 'react'
import { CFormSelect, CPagination, CPaginationItem } from '@coreui/react'

export const TAMANOS_PAGINA = [10, 25, 50, 100]

// Páginas visibles alrededor de la actual; el resto se resume con "…"
const VECINAS = 1

/**
 * Números de página a mostrar: primera, última y las vecinas de la actual.
 * @param {number} actual
 * @param {number} total
 * @returns {Array<number|string>} números de página y '…' donde se omiten páginas
 */
export const paginasVisibles = (actual, total) => {
  const paginas = []
  for (let pagina = 1; pagina <= total; pagina++) {
    const esExtremo = pagina === 1 || pagina === total
    const esVecina = Math.abs(pagina - actual) <= VECINAS
    if (esExtremo || esVecina) {
      paginas.push(pagina)
    } else if (paginas[paginas.length - 1] !== '…') {
      paginas.push('…')
    }
  }
  return paginas
}

/**
 * Paginación compacta (no crece con la cantidad de páginas) con selector de filas por página.
 */
const Paginacion = ({
  paginaActual,
  totalItems,
  itemsPorPagina,
  onCambiarPagina,
  onCambiarTamano,
}) => {
  const totalPaginas = Math.max(1, Math.ceil(totalItems / itemsPorPagina))
  const desde = totalItems === 0 ? 0 : (paginaActual - 1) * itemsPorPagina + 1
  const hasta = Math.min(paginaActual * itemsPorPagina, totalItems)

  return (
    <div className="d-flex flex-column flex-md-row align-items-center justify-content-between gap-2 mt-3">
      <div className="d-flex align-items-center gap-2 small text-medium-emphasis">
        <span className="text-nowrap">
          {desde}-{hasta} de {totalItems}
        </span>
        {onCambiarTamano && (
          <CFormSelect
            size="sm"
            aria-label="Filas por página"
            value={itemsPorPagina}
            onChange={(e) => onCambiarTamano(Number(e.target.value))}
            style={{ width: 'auto' }}
          >
            {TAMANOS_PAGINA.map((tamano) => (
              <option key={tamano} value={tamano}>
                {tamano} por página
              </option>
            ))}
          </CFormSelect>
        )}
      </div>
      {totalPaginas > 1 && (
        <CPagination size="sm" className="mb-0 flex-wrap" aria-label="Paginación">
          <CPaginationItem
            aria-label="Anterior"
            disabled={paginaActual === 1}
            onClick={() => onCambiarPagina(paginaActual - 1)}
          >
            &laquo;
          </CPaginationItem>
          {paginasVisibles(paginaActual, totalPaginas).map((pagina, index) =>
            pagina === '…' ? (
              <CPaginationItem key={`salto-${index}`} disabled>
                …
              </CPaginationItem>
            ) : (
              <CPaginationItem
                key={pagina}
                active={pagina === paginaActual}
                onClick={() => onCambiarPagina(pagina)}
              >
                {pagina}
              </CPaginationItem>
            ),
          )}
          <CPaginationItem
            aria-label="Siguiente"
            disabled={paginaActual === totalPaginas}
            onClick={() => onCambiarPagina(paginaActual + 1)}
          >
            &raquo;
          </CPaginationItem>
        </CPagination>
      )}
    </div>
  )
}

export default Paginacion
