const ReimprimirFacturaCanastilla = async (consecutivo) => {
  try {
    const response = await fetch(
      window.SERVER_URL +
        "/api/Canastilla/ReimprimirFacturaCanastilla/" +
        consecutivo,
      {
        method: "POST",
        mode: "cors",
        headers: {
          Accept: "text/plain",
          "Access-Control-Allow-Origin": "*",
          "Content-Type": "application/json",
        },
      }
    );

    if (response.status === 200) {
      return "ok";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default ReimprimirFacturaCanastilla;
