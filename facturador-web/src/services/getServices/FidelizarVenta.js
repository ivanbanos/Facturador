
const FidelizarVenta = async (identificacion, ventaId, ultimaFactura) => {
  try {
    const body = {
      facturaPOSId: ultimaFactura?.facturaPOSId ?? 0,
      terceroId: ultimaFactura?.tercero?.terceroId ?? 0,
      codigoFormaPago: ultimaFactura?.codigoFormaPago ?? 0,
      codigoFormaPago2:
        ultimaFactura?.codigoFormaPago2 !== null &&
        ultimaFactura?.codigoFormaPago2 !== undefined
          ? Number(ultimaFactura.codigoFormaPago2)
          : null,
      placa: ultimaFactura?.placa || "NP",
      kilometraje: ultimaFactura?.kilometraje || "NP",
      numeroTransaccion: ultimaFactura?.numeroTransaccion || "NP",
      total1:
        ultimaFactura?.total1 !== null && ultimaFactura?.total1 !== undefined
          ? Number(ultimaFactura.total1)
          : null,
      total2:
        ultimaFactura?.total2 !== null && ultimaFactura?.total2 !== undefined
          ? Number(ultimaFactura.total2)
          : null,
    };

    const response = await fetch(
      window.SERVER_URL +
        "/api/Fidelizacion/FidelizarVenta/" +
        identificacion +
        "/" +
        ventaId,
      {
        method: "POST",
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
          Accept: "text/plain",
          "Access-Control-Allow-Origin": "*",
        },
        body: JSON.stringify(body),
      }
    );

    if (response.status === 200) {
      return "ok";
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default FidelizarVenta;
