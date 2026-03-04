
import GetServerUrl from "./GetServerUrl";

const GetIslas = async () => {
  try {
    const serverUrl = GetServerUrl();
    if (!serverUrl) {
      return "fail";
    }

    const response = await fetch(serverUrl + "/api/Estacion/Islas");

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
