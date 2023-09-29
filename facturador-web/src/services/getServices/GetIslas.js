import configData from "../../components/config.json";

const GetIslas = async () => {
  try {
    console.log(configData.SERVER_URL);
    const response = await fetch(
      configData.SERVER_URL + "/api/Estacion/Islas",
      {
        method: "GET",
        mode: "cors",
        headers: {
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
      let islas = await response.json();
      return islas;
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetIslas;
