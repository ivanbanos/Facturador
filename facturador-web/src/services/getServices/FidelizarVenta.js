
const FidelizarVenta = async (identificacion, ventaId) => {
  try {
    const response = await fetch(
      window.SERVER_URL +
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
          Authorization: "Bearer ",
          "sec-fetch-mode": "cors",
          "Access-Control-Allow-Headers": "Content-Type",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
        },
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
