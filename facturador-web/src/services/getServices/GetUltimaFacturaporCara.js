import configData from "../../components/config.json";

const GetUltimaFacturaPorCara = async (id_cara) => {
  try {
    console.log(
      configData.SERVER_URL + "/api/Facturas/UltimaFacturaPorCara/" + id_cara
    );
    const response = await fetch(
      configData.SERVER_URL + "/api/Facturas/UltimaFacturaPorCara/" + id_cara
    );
    if (response.status === 200) {
      return await response.json();
    }
    if (response.status === 403) {
      return null;
    }
    return null;
  } catch (error) {
    return null;
  }
};

export default GetUltimaFacturaPorCara;
