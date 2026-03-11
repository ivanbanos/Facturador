//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
//{
//    public class ArrDet
//    {
//        public string cod { get; set; }
//        public string producto { get; set; }
//        public string cantidad { get; set; }
//        public string valor_unitario { get; set; }
//        public string porcentaje_descuento { get; set; }
//        public string porcentaje_iva { get; set; }
//        public string iva { get; set; }
//        public string valor_neto { get; set; }
//    }

//    public class ArrFactura
//    {
//        public string id { get; set; }
//        public string nro { get; set; }
//        public string identificacion_cliente { get; set; }
//        public string dir { get; set; }
//        public string telefono { get; set; }
//        public string cliente { get; set; }
//        public string vehiculo { get; set; }
//        public string valor_factura { get; set; }
//        public string fecha { get; set; }
//        public string forma_pago { get; set; }
//        public string estado { get; set; }
//        public string usuario_genera { get; set; }
//        public string observacion { get; set; }
//        public string nro_transaccion { get; set; }
//        public string nro_dispensador { get; set; }
//        public string cant_det { get; set; }
//        public List<ArrDet> arr_det { get; set; }
//    }

//    public class DatosFactura
//    {
//        public string cant_facturas { get; set; }
//        public List<ArrFactura> arr_facturas { get; set; }
//    }

//    public class REspuestaSiigo
//    {
//        public string msj { get; set; }
//        public List<DatosFactura> datos_factura { get; set; }
//        public string uuid_cufe { get; set; }
//    }

//}
