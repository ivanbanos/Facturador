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
      console.log("success");
      return await response.json();
    }
    if (response.status === 403) {
      return "fail 1";
    }
    return "fail 2";
  } catch (error) {
    console.log(error);
    return "fail 3";
  }
};

export default GetUltimaFacturaPorCaraTexto;
