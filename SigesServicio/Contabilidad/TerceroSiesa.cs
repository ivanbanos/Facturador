// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System.Collections.Generic;

public class ClienteSiesa
{
    public string F_CIA { get; set; }
    public string F201_ID_TERCERO { get; set; }
    public string F201_ID_SUCURSAL { get; set; }
    public string F201_DESCRIPCION_SUCURSAL { get; set; }
    public string F201_ID_VENDEDOR { get; set; }
    public string F201_ID_COND_PAGO { get; set; }
    public string F201_CUPO_CREDITO { get; set; }
    public string F201_ID_TIPO_CLI { get; set; }
    public string F201_ID_LISTA_PRECIO { get; set; }
    public string F201_IND_BLOQUEADO { get; set; }
    public string F201_IND_BLOQUEO_CUPO { get; set; }
    public string F201_IND_BLOQUEO_MORA { get; set; }
    public string F201_ID_CO_FACTURA { get; set; }
    public string F015_CONTACTO { get; set; }
    public string F015_DIRECCION1 { get; set; }
    public string F015_DIRECCION2 { get; set; }
    public string F015_DIRECCION3 { get; set; }
    public string F015_ID_PAIS { get; set; }
    public string F015_ID_DEPTO { get; set; }
    public string F015_ID_CIUDAD { get; set; }
    public string F015_TELEFONO { get; set; }
    public string F015_EMAIL { get; set; }
    public string F201_FECHA_INGRESO { get; set; }
    public string F201_ID_CO_MOVTO_FACTURA { get; set; }
    public string F201_ID_UN_MOVTO_FACTURA { get; set; }
    public string f015_celular { get; set; }
}

public class Compania
{
    public string F_CIA { get; set; }
}

public class ImptosReten
{
    public string F_TIPO_REG { get; set; }
    public string F_CIA { get; set; }
    public string F_ID_TERCERO { get; set; }
    public string F_ID_SUCURSAL { get; set; }
    public string F_ID_CLASE { get; set; }
    public string F_ID_VALOR_TERCERO { get; set; }
}

public class Root
{
    public List<Compania> Inicial { get; set; }
    public List<TerceroSiesa> Terceros { get; set; }
    public List<ClienteSiesa> Clientes { get; set; }
    public List<ImptosReten> Imptos_Reten { get; set; }
    public List<Compania> Final { get; set; }
}

public class TerceroSiesa
{
    public string F_CIA { get; set; }
    public string F200_ID { get; set; }
    public string F200_NIT { get; set; }
    public string F200_ID_TIPO_IDENT { get; set; }
    public string F200_IND_TIPO_TERCERO { get; set; }
    public string F200_RAZON_SOCIAL { get; set; }
    public string F200_APELLIDO1 { get; set; }
    public string F200_APELLIDO2 { get; set; }
    public string F200_NOMBRES { get; set; }
    public string F200_NOMBRE_EST { get; set; }
    public string F015_CONTACTO { get; set; }
    public string F015_DIRECCION1 { get; set; }
    public string F015_DIRECCION2 { get; set; }
    public string F015_DIRECCION3 { get; set; }
    public string F015_ID_PAIS { get; set; }
    public string F015_ID_DEPTO { get; set; }
    public string F015_ID_CIUDAD { get; set; }
    public string F015_TELEFONO { get; set; }
    public string F015_EMAIL { get; set; }
    public string F200_FECHA_NACIMIENTO { get; set; }
    public string F200_ID_CIIU { get; set; }
    public string F015_CELULAR { get; set; }
}

