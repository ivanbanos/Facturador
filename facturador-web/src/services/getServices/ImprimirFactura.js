const ImprimirFactura = async (ultimaFactura, inpresiones) => {
  try {
    const kilometraje = ultimaFactura?.kilometraje ? ultimaFactura.kilometraje : "NP";
    const placa = ultimaFactura?.placa ? ultimaFactura.placa : "NP";
    const numeroTransaccion = ultimaFactura?.numeroTransaccion
      ? ultimaFactura.numeroTransaccion
      : "NA";
    const formaPago2 =
      ultimaFactura?.codigoFormaPago2 !== null &&
      ultimaFactura?.codigoFormaPago2 !== undefined
        ? Number(ultimaFactura.codigoFormaPago2)
        : "";
    const total1 =
      ultimaFactura?.total1 !== null && ultimaFactura?.total1 !== undefined
        ? Number(ultimaFactura.total1)
        : "";
    const total2 =
      ultimaFactura?.total2 !== null && ultimaFactura?.total2 !== undefined
        ? Number(ultimaFactura.total2)
        : "";
    const impresiones = Number(inpresiones) > 0 ? Number(inpresiones) : 1;
    const response = await fetch(
      window.SERVER_URL +
        "/api/Facturas/Imprimir/" +
        ultimaFactura.facturaPOSId +
        "/" +
        ultimaFactura.tercero.terceroId +
        "/" +
        ultimaFactura.codigoFormaPago +
        "/" +
        ultimaFactura.ventaId +
        "?Kilometraje=" +
        encodeURIComponent(kilometraje) +
        "&Placa=" +
        encodeURIComponent(placa) +
        "&NumeroTransaccion=" +
        encodeURIComponent(numeroTransaccion) +
        "&FormaPago2=" +
        encodeURIComponent(formaPago2) +
        "&total1=" +
        encodeURIComponent(total1) +
        "&total2=" +
        encodeURIComponent(total2) +
        "&impresiones=" +
        impresiones,
      {
        method: "POST",
        mode: "cors",
        headers: {
          Accept: "text/plain",
          "Access-Control-Allow-Origin": "*",
          "Content-Type": "application/json",
          // Authorization: "Bearer ",
          // "sec-fetch-mode": "cors",
          // "Access-Control-Allow-Headers": "Content-Type",
          // "Access-Control-Allow-Origin": "*",
          // "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
        },
      }
    );

    if (response.status === 200) {
      let respuesta = await response.text();

      return respuesta;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default ImprimirFactura;
