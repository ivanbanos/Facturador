import configData from "../../components/config.json";

const ConvertirAOrden = async (id_venta) => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Facturas/ConvertirAOrden/" + id_venta,
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

export default ConvertirAOrden;
