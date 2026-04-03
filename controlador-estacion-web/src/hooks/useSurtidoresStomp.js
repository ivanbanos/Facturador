import { useEffect, useRef } from "react";
import { Client } from "@stomp/stompjs";
import { runtimeConfig } from "../config/runtimeConfig";

const readField = (obj, key) => {
  if (!obj || typeof obj !== "object") {
    return undefined;
  }

  if (Object.prototype.hasOwnProperty.call(obj, key)) {
    return obj[key];
  }

  const lower = key.toLowerCase();
  const foundKey = Object.keys(obj).find((k) => k.toLowerCase() === lower);
  return foundKey ? obj[foundKey] : undefined;
};

export const useSurtidoresStomp = (onMessage) => {
  const clientRef = useRef(null);
  const subscriptionRef = useRef(null);

  useEffect(() => {
    const { brokerUrl, login, passcode, destination } = runtimeConfig.rabbit;

    if (!brokerUrl || !destination) {
      return undefined;
    }

    if (!login || !passcode) {
      console.warn("[STOMP] Credenciales no configuradas en public/config.js");
      return undefined;
    }

    const client = new Client({
      brokerURL: brokerUrl,
      reconnectDelay: 5000,
      heartbeatIncoming: 4000,
      heartbeatOutgoing: 4000,
      connectHeaders: {
        login,
        passcode,
      },
      onConnect: () => {
        subscriptionRef.current = client.subscribe(destination, (message) => {
          try {
            const parsed = JSON.parse(message.body);
            const payload = {
              surtidorId: readField(parsed, "SurtidorId"),
              turno: readField(parsed, "Turno") || "",
              empleado: readField(parsed, "Empleado") || "",
              ubicacion: readField(parsed, "Ubicacion") || "",
              estado: readField(parsed, "Estado") || "",
            };

            if (payload.surtidorId !== undefined) {
              onMessage(payload);
            }
          } catch (error) {
            console.error("[STOMP] Mensaje invalido", error);
          }
        });
      },
      onStompError: (frame) => {
        console.error("[STOMP] Error", frame);
      },
      onWebSocketError: (event) => {
        console.error("[STOMP] WebSocket error", event);
      },
    });

    clientRef.current = client;
    client.activate();

    return () => {
      if (subscriptionRef.current) {
        subscriptionRef.current.unsubscribe();
        subscriptionRef.current = null;
      }

      if (clientRef.current?.active) {
        clientRef.current.deactivate();
      }

      clientRef.current = null;
    };
  }, [onMessage]);
};
