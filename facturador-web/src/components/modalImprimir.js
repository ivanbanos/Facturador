import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import ConvertirAFactura from "../services/getServices/ConvertiraFactura";
import ConvertirAOrden from "../services/getServices/ConvertirAOrden";

import "./styles/modal.css";

const ModalImprimir = (props) => {
  const ultimaFactura = props.ultimaFactura;
  const [show, setShow] = useState(false);
  const [showConvertirAFactura, setShowConvertirAFactura] = useState(false);

  const handleClose = () => setShow(false);
  const handleShow = () => setShow(true);
  const handleCloseConvertirAFactura = () => setShowConvertirAFactura(false);
  const handleShowConvertirAFactura = () => setShowConvertirAFactura(true);

  function onClickConvertirAOrden() {
    handleClose();
    ConvertirAOrden(ultimaFactura.ventaId);
    props.handleShowFacturaElectrónica();
  }
  function onClickConvertirAFactura() {
    handleCloseConvertirAFactura();
    ConvertirAFactura(ultimaFactura.ventaId);
    props.handleShowFacturaElectrónica();
  }
  function onClickNoConvertirAOrden() {
    handleClose();
    props.handleShowFacturaElectrónica();
  }
  function onClickNoConvertirAFactura() {
    handleCloseConvertirAFactura();
    props.handleShowFacturaElectrónica();
  }

  return (
    <>
      <Button
        className="print-button-modal botton-light-blue-modal"
        onClick={
          ultimaFactura.consecutivo === 0
            ? handleShowConvertirAFactura
            : handleShow
        }
      >
        Imprimir Factura
      </Button>

      <Modal
        show={showConvertirAFactura}
        onHide={handleCloseConvertirAFactura}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>
            Convertir a Factura {ultimaFactura.consecutivo} es ORDEN
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Desea convertir la orden de compra a una factura?
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              onClickConvertirAFactura();
            }}
          >
            Convertir a Fac
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              onClickNoConvertirAFactura();
            }}
          >
            No convertir
          </Button>
        </Modal.Footer>
      </Modal>

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
          <Modal.Title>
            Convertir a Orden de Compra {ultimaFactura.consecutivo} es FACTURA
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Desea convertir la factura a una orden de compra?
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              onClickConvertirAOrden();
            }}
          >
            Convertir a Orden de Compra
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              onClickNoConvertirAOrden();
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
