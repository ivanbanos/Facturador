using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Venta
    {
        public Venta()
        {
        }

        public int idVenta;
        internal string EMPLEADO;

        public int CONSECUTIVO { get; set; }
        public string COD_CLI { get; set; }
        public string PLACA { get; set; }
        public decimal CANTIDAD { get; set; }
        public decimal PRECIO_UNI { get; set; }
        public int IVA { get; set; }
        public decimal SUBTOTAL { get; set; }
        public decimal TOTAL { get; set; }
        public decimal TOTALCalculado { get { return (CANTIDAD * PRECIO_UNI) - (CANTIDAD * PRECIO_UNI * IVA / 100); } }

        public decimal VALORNETO { get; set; }
        public string NOMBRE { get; set; }
        public string TIPO_NIT { get; set; }
        public string NIT { get; set; }
        public string DIR_OFICINA { get; set; }
        public string TEL_OFICINA { get; set; }
        public string IMP_NOM { get; set; }
        public string COD_INT { get; set; }
        public int COD_FOR_PAG { get; set; }
        public decimal? KILOMETRAJE { get; set; }
        public DateTime? FECH_ULT_ACTU { get; set; }
        public DateTime? FECH_PRMA { get; set; }
        public short COD_SUR { get; set; }
        public short COD_CAR { get; set; }
        public string Combustible { get; set; }
        public decimal Descuento { get; set; }
        public string CEDULA { get; set; }
        public string COD_EMP { get; set; }
        public DateTime? FECHA_REAL { get; set; }
    }
}
