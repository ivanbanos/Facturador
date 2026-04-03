import React, { useState, useEffect, useRef } from "react";
import { Client } from "@stomp/stompjs";
import { Modal } from "react-bootstrap";
import "./styles/home.css";
import "./styles/modal.css";

const compareDates = (d1, d2) => {
  const date1 = d1.getTime();
  const date2 = new Date(d2).getTime();
  if (Number.isNaN(date2)) {
    return 1;
  }

  if (date1 < date2) {
    return -1;
  }

  if (date1 > date2) {
    return 1;
  }

  return 0;
};

const getStompConfig = () => {
  const rabbitWebSocketUrl = window.RabbitWebSocket;
  const destination = window.RabbitWebSocketDestination || "VehiculosSICOM";
  const login = window.RabbitWebSocketUser;
  const passcode = window.RabbitWebSocketPassword;

  const brokerURL =
    window.location.protocol === "https:" &&
    rabbitWebSocketUrl?.startsWith("ws://")
      ? rabbitWebSocketUrl.replace("ws://", "wss://")
      : rabbitWebSocketUrl;

  return {
    brokerURL,
    destination,
    login,
    passcode,
  };
};

const VehiculosSICOMModal = () => {
  const [show, setShow] = useState(false);
  const [vehiculo, setVehiculo] = useState({ placa: "" });
  const [estado, setEstado] = useState("Autorizado");
  const clientRef = useRef(null);
  const subscriptionRef = useRef(null);

  const handleCloseModal = () => setShow(false);

  const shouldDisplayVehiculo = (vehiculoJson) => {
    const islaRaw = localStorage.getItem("islaSelectName");
    if (!islaRaw) {
      return false;
    }

    const islaSeleccionada = JSON.parse(islaRaw);
    return vehiculoJson.isla === islaSeleccionada;
  };

  const computeEstado = (vehiculoJson) => {
    if (compareDates(new Date(), vehiculoJson.fechaFin) >= 0) {
      return "No Autorizado, motivo vencido";
    }

    if (vehiculoJson.estado !== 0) {
      return "No Autorizado, motivo " + vehiculoJson.motivoTexto;
    }

    return "Autorizado";
  };

  useEffect(() => {
    if (!window.RabbitWebSocket) {
      console.error(
        "[RabbitWebSocket] No está definida la variable RabbitWebSocket en config.js"
      );
      return;
    }

    const { brokerURL, destination, login, passcode } = getStompConfig();

    if (!brokerURL) {
      console.error("[RabbitWebSocket] brokerURL inválido");
      return;
    }

    if (!login || !passcode) {
      console.error(
        "[RabbitWebSocket] Credenciales no configuradas (RabbitWebSocketUser/RabbitWebSocketPassword)"
      );
      return;
    }

    const stompClient = new Client({
      brokerURL,
      reconnectDelay: 5000,
      heartbeatIncoming: 4000,
      heartbeatOutgoing: 4000,
      connectHeaders: {
        login,
        passcode,
      },
      onConnect: () => {
        subscriptionRef.current = stompClient.subscribe(destination, (message) => {
          try {
            const vehiculoJson = JSON.parse(message.body);

            if (shouldDisplayVehiculo(vehiculoJson)) {
              setVehiculo(vehiculoJson);
              setEstado(computeEstado(vehiculoJson));
              setShow(true);
            }
          } catch (error) {
            console.error("[RabbitWebSocket] Mensaje inválido", error);
          }
        });
      },
      onStompError: (frame) => {
        console.error("[RabbitWebSocket] STOMP error", frame);
      },
      onWebSocketError: (event) => {
        console.error("[RabbitWebSocket] WebSocket error", event);
      },
      onWebSocketClose: (event) => {
        console.warn("[RabbitWebSocket] WebSocket closed", event);
      },
    });

    clientRef.current = stompClient;
    stompClient.activate();

    return () => {
      if (subscriptionRef.current) {
        subscriptionRef.current.unsubscribe();
        subscriptionRef.current = null;
      }

      if (clientRef.current && clientRef.current.active) {
        clientRef.current.deactivate();
      }
      clientRef.current = null;
    };
  }, []);

  useEffect(() => {
    if (show) {
      const timeoutId = setTimeout(() => {
        setShow(false);
      }, 30000);

      return () => clearTimeout(timeoutId);
    }
  }, [show]);

  return (
    <>
      <Modal
        show={show}
        onHide={handleCloseModal}
        backdrop="static"
        keyboard={false}
        className="SICOM-modal"
        dialogClassName="custom-modal SICOM-modal"
        aria-labelledby="contained-modal-title-vcenter"
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title className="SICOM">Vehiculo</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <label className="col-sm-12 col-form-label SICOM">{estado}</label>
          <div className="row mb-3">
            <label className="col-sm-6 col-form-label SICOM">Placa</label>
            <label className="col-sm-6 col-form-label SICOM">
              {vehiculo.placa}
            </label>
            <label className="col-sm-12 col-form-label SICOM">IButton</label>
            <label className="col-sm-12 col-form-label SICOM">
              {vehiculo.idrom}
            </label>
            <label className="col-sm-12 col-form-label SICOM">
              Fecha de vencimiento
            </label>
            <label className="col-sm-12 col-form-label SICOM">
              {vehiculo.fechaFin}
            </label>
          </div>
        </Modal.Body>
      </Modal>
    </>
  );
};

export default VehiculosSICOMModal;
