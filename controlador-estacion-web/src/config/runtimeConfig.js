const readString = (value, fallback = "") => {
  if (typeof value === "string" && value.trim().length > 0) {
    return value.trim();
  }

  return fallback;
};

export const runtimeConfig = {
  apiBaseUrl: readString(window.ControladorApiUrl, "https://localhost:7106"),
  rabbit: {
    brokerUrl: readString(window.RabbitWebSocket),
    login: readString(window.RabbitStompUser),
    passcode: readString(window.RabbitStompPassword),
    destination: readString(window.RabbitStompDestination, "controlador"),
  },
};
