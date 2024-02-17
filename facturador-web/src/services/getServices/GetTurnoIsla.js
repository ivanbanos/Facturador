
const GetTurnoIsla = async (id_isla) => {
  try {
    const response = await fetch(
      window.SERVER_URL + "/api/Estacion/TurnoPorIsla?idIsla=" + id_isla,
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
      let turno = await response.json();
      return turno;
    }
    if (response.status === 204) {
      let turno = await response.json();
      return "";
    }
    if (response.status === 403) {
      return "";
    }
    return "";
  } catch (error) {
    return "";
  }
};

export default GetTurnoIsla;
