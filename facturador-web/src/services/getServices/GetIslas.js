import configData from "../../components/config.json";

const GetIslas = async () => {
  try {
    const response = await fetch(configData.SERVER_URL + "/api/Estacion/Islas");

    if (response.status === 200) {
      return await response.json();
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
