import configData from "../../components/config.json";

const GetUltimaFacturaPorCaraTexto = async (id_cara) => {
  try {
    console.log(
      configData.SERVER_URL +
        "/api/Facturas/UltimaFacturaPorCara/" +
        id_cara +
        "/Texto"
    );
    const response = await fetch(
      configData.SERVER_URL +
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

    console.log(response.status);
    if (response.status === 200) {
      return await response.text();
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    console.log(error);
    return "fail";
  }
};

export default GetUltimaFacturaPorCaraTexto;
