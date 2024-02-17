
const GetCarasPorIsla = async (id_isla) => {
  try {
    const response = await fetch(
      window.SERVER_URL + "/api/Estacion/CarasPorIsla?idIsla=" + id_isla,
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
          //body: JSON.stringify(id_isla),
        },
      }
    );

    if (response.status === 200) {
      let caras = await response.json();
      return caras;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetCarasPorIsla;
