
import GetServerUrl from "./GetServerUrl";

const GetFormasDePago = async () => {
  try {
    const serverUrl = GetServerUrl();
    if (!serverUrl) {
      return "fail";
    }

    const response = await fetch(
      serverUrl + "/api/Facturas/FormasDePago",
      {
        method: "GET",
        mode: "cors",
        headers: {
          "Access-Control-Allow-Origin": "*",
          accept: "text/plain",
          Authorization: "Bearer ",
          "sec-fetch-mode": "cors",
          "Access-Control-Allow-Headers": "Content-Type",
          "Access-Control-Allow-Origin": "*",
          "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
        },
      }
    );
    if (response.status === 200) {
      let formasDePago = await response.json();
      return formasDePago;
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
