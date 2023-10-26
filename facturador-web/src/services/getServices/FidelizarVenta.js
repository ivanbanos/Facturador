import configData from "../../components/config.json";

const FidelizarVenta = async (identificacion, ventaId) => {
  try {
    const response = await fetch(
      configData.SERVER_URL +
        "/api/Fidelizacion/FidelizarVenta/" +
        identificacion +
        "/" +
        ventaId,
      {
        Accept: "text/plain",
        method: "POST",
        mode: "cors",
        headers: {
          "Access-Control-Allow-Origin": "*",
          "Content-Type": "application/json",
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

export default FidelizarVenta;
