
const ImprimirPorConsecutivo = async (consecutivo) => {
  try {
    const response = await fetch(
      window.SERVER_URL +
        "/api/Facturas/ImprimirPorConsecutivo/" +
        consecutivo,
      {
        method: "POST",
        mode: "cors",
        headers: {
          Accept: "text/plain",
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

export default ImprimirPorConsecutivo;
