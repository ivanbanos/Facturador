// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;

public class CurrentOutput
{
    public int id { get; set; }
    public int outputInventoryTypeId { get; set; }
    public int paymentFormInventoryId { get; set; }
    public int paymentMethodInventoryId { get; set; }
    public int paymentMeanInventoryId { get; set; }
    public int inventoryOutputStateId { get; set; }
    public int inventoryTypeBillingId { get; set; }
    public int idPersona { get; set; }
    public string observacion { get; set; }
    public string dispenserNumber { get; set; }
    public object kilometrosVeh { get; set; }
    public string surtidor { get; set; }
    public string isla { get; set; }
    public string manguera { get; set; }
    public string nroCruce { get; set; }
    public object prefijoCruce { get; set; }
    public object externalVehicleId { get; set; }
    public object idVehiculo { get; set; }
    public DateTime fechaCobro { get; set; }
    public DateTime fecMov { get; set; }
    public object idTarjetaBanco { get; set; }
    public object nroTransaccion { get; set; }
    public string valorTotal { get; set; }
    public string valorNeto { get; set; }
    public string valorImpuesto { get; set; }
    public string valorDescuento { get; set; }
    public string valorAbono { get; set; }
    public string saldoActual { get; set; }
    public string valorNetoSinImp { get; set; }
    public string porcentajeDescuento { get; set; }
    public string valorDescuentoSinImp { get; set; }
    public string valorDescuentoConImp { get; set; }
    public string totalImpuesto { get; set; }
    public string valorCredito { get; set; }
    public int idSucursal { get; set; }
    public int idUsuarioCre { get; set; }
    public bool softwareExterno { get; set; }
    public int idEmpresaOperadora { get; set; }
    public DateTime createdAt { get; set; }
    public string prefijo { get; set; }
    public string numero { get; set; }
    public string prefijoResolucion { get; set; }
    public string numeroResolucion { get; set; }
    public object resolucion { get; set; }
    public object idUsuarioMod { get; set; }
    public object updatedAt { get; set; }
    public object razonModificacion { get; set; }
}

public class Message
{
    public CurrentOutput currentOutput { get; set; }
}

public class RespuestaSilog
{
    public string error { get; set; }
    public Message message { get; set; }
    public string cufe { get; set; }
    public object error_cufe { get; set; }
}

