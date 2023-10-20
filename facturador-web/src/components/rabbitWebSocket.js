import configData from "./config.json";
import React, { useState, useEffect } from 'react';
import { Client } from '@stomp/stompjs';
import { Modal, Button } from "react-bootstrap";
import "./styles/home.css";
import "./styles/modal.css";

const characters ='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

const generateString = (length) => {
  let result = '';
  const charactersLength = characters.length;
  for ( let i = 0; i < length; i++ ) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
  }

  return result;
}

const compareDates = (d1, d2) => {
  let date1 = d1.getTime();
  let date2 = new Date(d2).getTime();
  console.log(d1);
  console.log(d2);
  console.log(date1);
  console.log(date2);
  if (date1 < date2) {
    return -1;
  } else if (date1 > date2) {
    return 1;
  } else {
    return 0;
  }
};

const VehiculosSICOMModal = (props) => {

  const [show, setShow] = useState(false);
  const [vehiculo, setVehiculo] = useState({placa:""});
  const [estado, setEstado] = useState("Autorizado");

   const handleCloseModal = () => setShow(false);
   // The compat mode syntax is totally different, converting to v5 syntax
    // Client is imported from '@stomp/stompjs'
    const client = new Client({
      brokerURL: configData.RabbitWebSocket,
      reconnectDelay: 5000,
      heartbeatIncoming: 4000,
      heartbeatOutgoing: 4000,
      connectHeaders: {
        login: 'siges',
        passcode: 'siges',
      },
      onConnect: () => {
        
        client.subscribe('VehiculosSICOM', message => {
            const now = new Date();
            setVehiculo(JSON.parse(message.body));
            let vehiculoJson = JSON.parse(message.body);
            if((vehiculoJson.isla == JSON.parse(localStorage.getItem("islaSelectName")) || null)){
              setEstado("Autorizado")
              if (compareDates(now,vehiculoJson.fechaFin)>=0) {
                setEstado("No Autorizado, motivo vencido")
              } 
               if(vehiculoJson.estado != 0){
                setEstado("No Autorizado, motivo "+vehiculoJson.motivoTexto)
              } 
              setShow(true);
            }
        });
      },
    
    });
    useEffect(() => {
      client.activate();
    }, []); 
    useEffect(() => {
      const interval = setInterval(() => {
        setShow(false);
      }, 15000);
      return () => clearInterval(interval);
    }, []);
  return (
    <>
    <Modal
        show={show}
        onHide={handleCloseModal}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title className="SICOM">Vehiculo</Modal.Title>
        </Modal.Header>
        <Modal.Body>
              <label className="col-sm-12 col-form-label SICOM">
                {estado}
              </label>
            <div className="row mb-3">
              <label className="col-sm-6 col-form-label SICOM">
                Placa
              </label>
              <label className="col-sm-6 col-form-label SICOM">
                {vehiculo.placa}
              </label>
              <label className="col-sm-6 col-form-label SICOM">
                IButton
              </label>
              <label className="col-sm-6 col-form-label SICOM">
                {vehiculo.idrom}
              </label>
              <label className="col-sm-6 col-form-label SICOM">
                Fecha de vencimiento
              </label>
              <label className="col-sm-6 col-form-label SICOM">
                {vehiculo.fechaFin}
              </label>
            </div>

        </Modal.Body>
      </Modal>
    </>
  );
}

export default VehiculosSICOMModal;