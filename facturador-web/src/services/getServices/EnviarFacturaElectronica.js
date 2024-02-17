
const EnviarFacturaElectronica = async (ultimaFactura) => {
  try {
    if(ultimaFactura.kilometraje==""){
    ultimaFactura.kilometraje="NP";
  }
  if(ultimaFactura.placa==""){
    ultimaFactura.placa="NP";
  }
    const response = await fetch(
      window.SERVER_URL +
        "/api/Facturas/EnviarFacturaElectronica/" +
        ultimaFactura.ventaId +
        "/" +
        ultimaFactura.tercero.terceroId +
        "/" +
        ultimaFactura.codigoFormaPago +
        "/" +
        ultimaFactura.ventaId +
        
        "?Kilometraje="
        +ultimaFactura.kilometraje+"&Placa="+
        ultimaFactura.placa,
      {
        method: "POST",
        mode: "cors",
        headers: {
          Accept: "text/plain",
          "Access-Control-Allow-Origin": "*",
          Authorization: "Bearer ",
          "sec-fetch-mode": "cors",
          "Access-Control-Allow-Headers": "Content-Type",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
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

export default EnviarFacturaElectronica;
