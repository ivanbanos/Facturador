import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";

import "./styles/modal.css";

const ModalImprimir = (props) => {
  const [show, setShow] = useState(false);

  const handleClose = () => setShow(false);
  const handleShow = () => setShow(true);

  function onClickConvertir() {
    handleClose();
    props.handleShowFacturaElectrónica();
  }
  function onClickNoConvertir() {
    handleClose();
    props.handleShowFacturaElectrónica();
  }

  return (
    <>
      <Button
        className="print-button-modal botton-light-blue-modal"
        onClick={handleShow}
      >
        Imprimir Factura
      </Button>

      <Modal
        show={show}
        onHide={handleClose}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Convertir a Factura</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Desea convertir la orden de compra a una factura?
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              onClickConvertir();
            }}
          >
            Convertir a Factura
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              onClickNoConvertir();
            }}
          >
            No convertir
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalImprimir;
