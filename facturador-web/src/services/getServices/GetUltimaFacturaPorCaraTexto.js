
const GetUltimaFacturaPorCaraTexto = async (id_cara) => {
  try {
    const response = await fetch(
      window.SERVER_URL +
        "/api/Facturas/UltimaFacturaPorCara/" +
        id_cara +
        "/Texto",
      {
        method: "GET",
        headers: {
          Accept: "text/plain", // Especifica el tipo de respuesta que esperas
        },
      }
    );

    if (response.status === 200) {
      return await response.text();
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetUltimaFacturaPorCaraTexto;
