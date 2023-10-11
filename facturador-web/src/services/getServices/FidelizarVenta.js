import configData from "../../components/config.json";

const FidelizarVenta = async (ultimaFactura) => {
  try {
    const response = await fetch(
      configData.SERVER_URL +
        "/api/Fidelizacion/FidelizarVenta/" +
        ultimaFactura.tercero.identificacion +
        "/" +
        ultimaFactura.ventaId,
      {
        method: "POST",
        mode: "cors",
        headers: {
          "Access-Control-Allow-Origin": "*",
          "Content-Type": "application/json",
        },
      }
    );
    console.log(response.status);
    if (response.status === 200) {
      let respuesta = await response.json();

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

export default FidelizarVenta;
