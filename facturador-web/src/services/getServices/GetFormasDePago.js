import configData from "../../components/config.json";

const GetFormasDePago = async () => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Factura/FormasDePago",
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
      let formasPago = await response.json();
      return formasPago;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetFormasDePago;
