import configData from "../../components/config.json";

const GetUltimaFacturaPorCara = async (id_cara) => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Facturas/UltimaFacturaPorCara/" + id_cara,
      {
        method: "GET",
        mode: "cors",
        headers: {
          Authorization: "Bearer ",
          "sec-fetch-mode": "cors",
          "Access-Control-Allow-Headers": "Content-Type",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
        },
      }
    );
    if (response.status === 200) {
      let ultimaFactura = await response.json();
      return ultimaFactura;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetUltimaFacturaPorCara;
