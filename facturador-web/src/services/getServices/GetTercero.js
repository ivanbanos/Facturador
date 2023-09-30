import configData from "../../components/config.json";

const GetTercero = async (identificacion) => {
  try {
    const response = await fetch(
      configData.SERVER_URL +
        "/api/Terceros/TiposIdentificacion" +
        identificacion,
      {
        method: "GET",
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
      let tercero = await response.json();
      return tercero;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetTercero;
