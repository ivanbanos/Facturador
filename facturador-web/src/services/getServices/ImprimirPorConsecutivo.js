import configData from "../../components/config.json";

const ImprimirPorConsecutivo = async (consecutivo) => {
  try {
    const response = await fetch(
      configData.SERVER_URL +
        "/api/Facturas/ImprimirPorConsecutivo/" +
        consecutivo,
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
    console.log(response.status);
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
