
const AgregarBolsa = async (isla, codigo, cantidad, moneda, numero) => {
    try {
      const response = await fetch(
        window.SERVER_URL + "/api/Turnos/AgregarBolsa/" + isla + "/" + codigo + "/" + cantidad+"/"+moneda+"/"+numero,
        {
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
        let respuesta = await response.text();

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
  export default AgregarBolsa;
  