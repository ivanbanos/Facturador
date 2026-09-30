using System;
using System.Collections.Generic;
using System.Linq;

namespace EstacionesServicio.Negocio.Extention
{
    public class ValidadorGuidAFacturaElectronica : IValidadorGuidAFacturaElectronica
    {
        public readonly List<string> guidsSiendoProcesados;
        public readonly Dictionary<Guid,Queue<string>> facturasImprimirCanastilla;

        public ValidadorGuidAFacturaElectronica()
        {
            guidsSiendoProcesados = new List<string>();
            facturasImprimirCanastilla = new Dictionary <Guid,Queue<string>>();
        }

        

        public bool FacturaSiendoProceada(string ordenGuid)
        {
            lock (guidsSiendoProcesados)
            {
                if (guidsSiendoProcesados.Contains(ordenGuid))
                {
                    return true;
                }
                else
                {
                    guidsSiendoProcesados.Add(ordenGuid);
                    return false;
                }
            }
        }

        public bool FacturasSiendoProceada(IEnumerable<string> facturasGuids)
        {
            lock (guidsSiendoProcesados)
            {
                if (guidsSiendoProcesados.Any(x => facturasGuids.Contains(x)))
                {
                    return true;
                }
                else
                {
                    guidsSiendoProcesados.AddRange(facturasGuids);
                    return false;
                }
            }
        }

        public void SacarFactura(string ordenGuid)
        {
            lock (guidsSiendoProcesados)
            {
                guidsSiendoProcesados.Remove(ordenGuid);
            }
        }

        public void SacarFacturas(IEnumerable<string> facturasGuids)
        {
            lock (guidsSiendoProcesados)
            {
                guidsSiendoProcesados.RemoveAll(x => facturasGuids.Contains(x));
            }
        }


        public void AgregarAColaImpresionCanastilla(string guid, Guid idEstacion)
        {
            if (!facturasImprimirCanastilla.ContainsKey(idEstacion))
            {
                facturasImprimirCanastilla.Add(idEstacion, new Queue<string>());
            }
            facturasImprimirCanastilla[idEstacion].Enqueue(guid);
        }

        public string? ObtenerColaImpresionCanastilla(Guid idEstacion)
        {
            if (facturasImprimirCanastilla.ContainsKey(idEstacion) && facturasImprimirCanastilla[idEstacion].Count > 0)
            {
                return facturasImprimirCanastilla[idEstacion].Dequeue();
            }
            return null;
        }
    }
}