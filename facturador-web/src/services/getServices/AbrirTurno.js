import configData from "../../components/config.json";

const AbrirTurno = async (isla, codigo) => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Turnos/AbrirTurno/" + isla + "/" + codigo,
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
export default AbrirTurno;
