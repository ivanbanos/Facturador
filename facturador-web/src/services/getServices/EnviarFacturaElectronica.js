import configData from "../../components/config.json";

const EnviarFacturaElectronica = async (id_venta) => {
  try {
    const response = await fetch(
      configData.SERVER_URL +
        "/api/Facturas/EnviarFacturaElectronica/" +
        id_venta,
      {
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
    console.log(response.status);
    if (response.status === 200) {
      let respuesta = await response.json();
      console.log(respuesta);
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
