import configData from "../../components/config.json";

const PostCanastilla = async (canastilla) => {
  try {
    const response = await fetch(configData.SERVER_URL + "/api/Canastilla", {
      method: "POST",
      mode: "cors",
      headers: {
        "Access-Control-Allow-Origin": "*",
        "Content-Type": "application/json",
        // Authorization: "Bearer ",
        // "sec-fetch-mode": "cors",
        // "Access-Control-Allow-Headers": "Content-Type",
        // "Access-Control-Allow-Origin": "*",
        // "Access-Control-Allow-Methods": "OPTIONS,POST,GET",
      },
      body: JSON.stringify(canastilla),
    });
    console.log(response.status);
    if (response.status === 200) {
      let respuesta = await response.json();
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

export default PostCanastilla;