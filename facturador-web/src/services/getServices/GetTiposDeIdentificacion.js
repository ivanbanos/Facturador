import configData from "../../components/config.json";

const GetTiposDeIdentificacion = async () => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Terceros/TiposIdentificacion",
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
      let tiposIdentificacion = await response.json();
      return tiposIdentificacion;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetTiposDeIdentificacion;
