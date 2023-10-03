import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";

import "./styles/modal.css";

const ModalFacturaElectronica = (props) => {
  const handleCloseFacturaElectronica = props.handleCloseFacturaElectronica;

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
          <Modal.Title>Convertir a Factura Electrónica</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Desea convertir la orden de compra a una factura electrónica?
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseFacturaElectronica();
            }}
          >
            Convertir a Factura Electrónica
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              handleCloseFacturaElectronica();
            }}
          >
            No convertir a Factura Electrónica
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalFacturaElectronica;
