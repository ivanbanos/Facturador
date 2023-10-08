import configData from "./config.json";
import React, { useState, useEffect } from 'react';
import { Client } from '@stomp/stompjs';
import { Modal, Button } from "react-bootstrap";
import "./styles/home.css";
import "./styles/modal.css";

const characters ='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

const generateString = (length) => {
  let result = ' ';
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

const SampleComponent = (props) => {

  const [show, setShow] = useState(false);
  const [vehiculo, setVehiculo] = useState({placa:""});
  const [estado, setEstado] = useState("Autorizado");

   const handleCloseModal = () => setShow(false);
   // The compat mode syntax is totally different, converting to v5 syntax
    // Client is imported from '@stomp/stompjs'
    let client = new Client();
    let randomNumer = 'Isla'+generateString(10);
    client.configure({
      brokerURL: configData.RabbitWebSocket,
      onConnect: () => {
        client.subscribe(randomNumer, message => {
          console.log(message.body);
            const now = new Date();
            setVehiculo(JSON.parse(message.body));
            let vehiculoJson = JSON.parse(message.body);
            console.log(vehiculoJson.estado);
            console.log(vehiculoJson.estado != 0);
            setEstado("Autorizado")
            if (compareDates(now,vehiculoJson.fechaFin)>=0) {
              setEstado("No Autorizado, motivo vencido")
            } 
             if(vehiculoJson.estado != 0){
              setEstado("No Autorizado, motivo "+vehiculoJson.motivoTexto)
            } 
            setShow(true);
        });
        client.publish({ destination: 'islas', body: randomNumer });
      },
    
    });
    
    client.activate();
  
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
          <Modal.Title>Vehiculo</Modal.Title>
        </Modal.Header>
        <Modal.Body>
              <label className="col-sm-12 col-form-label">
                {estado}
              </label>
            <div className="row mb-3">
              <label className="col-sm-6 col-form-label">
                Placa
              </label>
              <label className="col-sm-6 col-form-label">
                {vehiculo.placa}
              </label>
              <label className="col-sm-6 col-form-label">
                IButton
              </label>
              <label className="col-sm-6 col-form-label">
                {vehiculo.idrom}
              </label>
              <label className="col-sm-6 col-form-label">
                Fecha de vencimiento
              </label>
              <label className="col-sm-6 col-form-label">
                {vehiculo.fechaFin}
              </label>
            </div>

        </Modal.Body>
      </Modal>
    </>
  );
}

export default SampleComponent;