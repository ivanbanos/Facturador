using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Contabilidad
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Caja
    {
        public string F_CIA { get; set; }
        public string F350_ID_CO { get; set; }
        public string F350_ID_TIPO_DOCTO { get; set; }
        public string F350_CONSEC_DOCTO { get; set; }
        public string F351_ID_AUXILIAR { get; set; }
        public string F351_ID_CO_MOV { get; set; }
        public string F351_ID_UN { get; set; }
        public string F351_ID_FE { get; set; }
        public string F351_ID_CCOSTO { get; set; }
        public string F351_VALOR_DB { get; set; }
        public string F351_VALOR_CR { get; set; }
        public string F351_NOTAS { get; set; }
        public string F358_ID_CAJA { get; set; }
        public string F358_ID_MEDIOS_PAGO { get; set; }
        public string F358_NRO_CUENTA { get; set; }
        public string F358_COD_SEGURIDAD { get; set; }
        public string F358_NRO_AUTORIZACION { get; set; }
        public string F358_FECHA_VCTO { get; set; }
        public string F358_REFERENCIA_OTROS { get; set; }
        public string F358_NOTAS { get; set; }
    }
    public class Documentocontable
    {
        public string F_CIA { get; set; }
        public string F_CONSEC_AUTO_REG { get; set; }
        public string F350_ID_CO { get; set; }
        public string F350_ID_TIPO_DOCTO { get; set; }
        public string F350_CONSEC_DOCTO { get; set; }
        public string F350_FECHA { get; set; }
        public string F350_ID_TERCERO { get; set; }
        public string F350_IND_ESTADO { get; set; }
        public string F350_NOTAS { get; set; }
    }


    public class Movimientocontable
    {
        public string F_CIA { get; set; }
        public string F350_ID_CO { get; set; }
        public string F350_ID_TIPO_DOCTO { get; set; }
        public string F350_CONSEC_DOCTO { get; set; }
        public string F351_ID_AUXILIAR { get; set; }
        public string F351_ID_TERCERO { get; set; }
        public string F351_ID_CO_MOV { get; set; }
        public string F351_ID_UN { get; set; }
        public string F351_ID_CCOSTO { get; set; }
        public string F351_ID_FE { get; set; }
        public string F351_VALOR_DB { get; set; }
        public string F351_VALOR_CR { get; set; }
        public string F351_BASE_GRAVABLE { get; set; }
        public string F351_DOCTO_BANCO { get; set; }
        public string F351_NRO_DOCTO_BANCO { get; set; }
        public string F351_NOTAS { get; set; }
    }

    public class MovimientoCxC
    {
        public string F_CIA { get; set; }
        public string F350_ID_CO { get; set; }
        public string F350_ID_TIPO_DOCTO { get; set; }
        public string F350_CONSEC_DOCTO { get; set; }
        public string F351_ID_AUXILIAR { get; set; }
        public string F351_ID_TERCERO { get; set; }
        public string F351_ID_CO_MOV { get; set; }
        public string F351_ID_UN { get; set; }
        public string F351_ID_CCOSTO { get; set; }
        public string F351_VALOR_DB { get; set; }
        public string F351_VALOR_CR { get; set; }
        public string F351_NOTAS { get; set; }
        public string F353_ID_SUCURSAL { get; set; }
        public string F353_ID_TIPO_DOCTO_CRUCE { get; set; }
        public string F353_CONSEC_DOCTO_CRUCE { get; set; }
        public string F353_NRO_CUOTA_CRUCE { get; set; }
        public string F353_FECHA_VCTO { get; set; }
        public string F353_FECHA_DSCTO_PP { get; set; }
        public string F354_TERCERO_VEND { get; set; }
        public string F354_NOTAS { get; set; }
    }

    public class Movimientos
    {
        public List<Compania> Inicial { get; set; }
        public List<Documentocontable> Documentocontable { get; set; }
        public List<Movimientocontable> Movimientocontable { get; set; }
        public List<MovimientoCxC> MovimientoCxC { get; set; }
        public List<Compania> Final { get; set; }
    }

    public class MovimientosCaja
    {
        public List<Compania> Inicial { get; set; }
        public List<Documentocontable> Documentocontable { get; set; }
        public List<Movimientocontable> Movimientocontable { get; set; }
        public List<Caja> Caja { get; set; }
        public List<Compania> Final { get; set; }
    }
}
