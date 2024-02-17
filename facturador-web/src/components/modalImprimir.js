import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import ConvertirAFactura from "../Services/getServices/ConvertiraFactura";
import ConvertirAOrden from "../Services/getServices/ConvertirAOrden";
import { Alert } from "react-bootstrap";
import ImprimirFactura from "../Services/getServices/ImprimirFactura";

import "./styles/modal.css";

const ModalImprimir = (props) => {
  const onClickImprimir = () => {
    let tempUltimaFactura = {
      ...props.ultimaFactura,
      manguera: "",
      iButton: "",
      codigoInterno: "",
    };

    props.handleSetUltimaFactura(tempUltimaFactura);
  };
  const ultimaFactura = props.ultimaFactura;
  const [show, setShow] = useState(false);
  const [showConvertirAFactura, setShowConvertirAFactura] = useState(false);

  const [showAlertImpresionExitosa, setShowAlertImpresionExitosa] =
    useState(false);
  const handleClose = () => setShow(false);
  const handleShow = () => {if(!window.ConvertirAOrden){props.handleShowFacturaElectrónica();} else{setShow(true);}}
  const handleCloseConvertirAFactura = () => setShowConvertirAFactura(false);
  const handleShowConvertirAFactura = () => {if(!window.ConvertirAFactura){props.handleShowFacturaElectrónica();} else{setShowConvertirAFactura(true);}}

  async function onClickConvertirAOrden() {
    handleClose();
    const respuesta = await ConvertirAOrden(ultimaFactura.ventaId);

    if (respuesta === "fail") {
      props.handleSetShowAlertError(true);
    } else {
      if(!window.GenerarFacturaelectronica){
        const respuestaImprimir = await ImprimirFactura(ultimaFactura);
        if (respuestaImprimir === "fail") {
          props.handleSetShowAlertError(true);
        } else {
          props.getFacturaInformacion();
          setShowAlertImpresionExitosa(true);
        }
      }else{
          props.handleShowFacturaElectrónica();}
    }
  }
  async function onClickConvertirAFactura() {
    handleCloseConvertirAFactura();
    const respuesta = await ConvertirAFactura(ultimaFactura.ventaId);
    if (respuesta === "fail") {
      props.handleSetShowAlertError(true);
    } else {
      props.handleShowFacturaElectrónica();
    }
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
        onClick={() => {
          
          ultimaFactura.consecutivo === 0
            ? handleShowConvertirAFactura()
            : handleShow();
          onClickImprimir();
        }}
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
          <Modal.Title>Convertir Orden a Factura</Modal.Title>
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
            Convertir a Factura
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
          <Modal.Title>Convertir Factura a Orden de Compra</Modal.Title>
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
      <Alert
          variant="info"
          show={showAlertImpresionExitosa}
          onClose={() => setShowAlertImpresionExitosa(false)}
          dismissible
        >
          <Alert.Heading>Fatura impresa de forma exitosa</Alert.Heading>
        </Alert>
    </>
  );
};

export default ModalImprimir;
