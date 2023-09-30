import configData from "../../components/config.json";

const GetTiposDeIdentificacion = async () => {
  try {
    const response = await fetch(
      configData.SERVER_URL + "/api/Terceros/TiposIdentificacion"
    );
    console.log(response.status);
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

export default GetTiposDeIdentificacion;
