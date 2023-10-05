import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import ImprimirFactura from "../services/getServices/ImprimirFactura";
import EnviarFacturaElectronica from "../services/getServices/EnviarFacturaElectronica";

import "./styles/modal.css";

const ModalFacturaElectronica = (props) => {
  const handleCloseFacturaElectronica = props.handleCloseFacturaElectronica;
  const ultimaFactura = props.ultimaFactura;

  return (
    <>
      <Modal
        show={props.showFacturaElectronica}
        onHide={handleCloseFacturaElectronica}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Enviar Factura Electrónica</Modal.Title>
        </Modal.Header>
        <Modal.Body>Desea enviar la factura electrónica?</Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseFacturaElectronica();
              ImprimirFactura(ultimaFactura.ventaId, ultimaFactura);
              EnviarFacturaElectronica(ultimaFactura.ventaId);
            }}
          >
            Enviar e Imprimir
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              handleCloseFacturaElectronica();
              ImprimirFactura(ultimaFactura.ventaId, ultimaFactura);
            }}
          >
            No Enviar e Imprimir
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalFacturaElectronica;
