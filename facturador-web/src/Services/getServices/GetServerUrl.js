const GetServerUrl = () => {
  const serverUrl =
    typeof window !== "undefined" ? window.SERVER_URL : undefined;

  if (!serverUrl) {
    console.error(
      "[Config] SERVER_URL no está definido. Revisa que config.js cargue sin errores y exponga SERVER_URL."
    );
    return null;
  }

  return String(serverUrl).replace(/\/+$/, "");
};

export default GetServerUrl;