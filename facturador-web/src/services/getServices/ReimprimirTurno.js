
const ReimprimirTurno = async (fecha, isla, posicion) => {
  try {
    const response = await fetch(
      window.SERVER_URL +
        "/api/Turnos/reimprimirTurno/" +
        fecha +
        "/" +
        isla +
        "/" +
        posicion,
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

    if (response.status === 200) {
      return "ok";
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default ReimprimirTurno;
