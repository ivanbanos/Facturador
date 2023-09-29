import configData from "../../components/config.json";

const GetTurnoIsla = async (id_isla) => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Estacion/TurnoPorIsla",
      {
        method: "GET",
        mode: "cors",
        headers: {
          Authorization: "Bearer ",
          "sec-fetch-mode": "cors",
          "Access-Control-Allow-Headers": "Content-Type",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
          body: id_isla,
        },
      }
    );
    if (response.status === 200) {
      let turno = await response.json();
      return turno;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetTurnoIsla;
