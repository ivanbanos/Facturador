const AgregarAnticipo = async (anticipo) => {
  try {
    const response = await fetch(window.SERVER_URL + "/api/Turnos/CrearAnticipo", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Access-Control-Allow-Origin": "*",
        Authorization: "Bearer ",
        "sec-fetch-mode": "cors",
      },
      body: JSON.stringify(anticipo),
    });
    if (response.status === 200) {
      return "ok";
    }
    if (response.status === 400) {
      const text = await response.text();
      return text || "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default AgregarAnticipo;
