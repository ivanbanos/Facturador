
const GetUltimaFacturaPorCara = async (id_cara) => {
  try {
    const response = await fetch(
      window.SERVER_URL + "/api/Facturas/UltimaFacturaPorCara/" + id_cara
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
