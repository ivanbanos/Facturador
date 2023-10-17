import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import ImprimirFactura from "../Services/getServices/ImprimirFactura";
import EnviarFacturaElectronica from "../Services/getServices/EnviarFacturaElectronica";
import { Alert } from "react-bootstrap";
import "./styles/modal.css";

const ModalFacturaElectronica = (props) => {
  const handleCloseFacturaElectronica = props.handleCloseFacturaElectronica;
  const ultimaFactura = props.ultimaFactura;
  const [showAlertImpresionExitosa, setShowAlertImpresionExitosa] =
    useState(false);
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
            onClick={async () => {
              handleCloseFacturaElectronica();
              const respuestaImprimir = await ImprimirFactura(ultimaFactura);
              const respuestaEnviar = await EnviarFacturaElectronica(
                ultimaFactura.ventaId
              );
              if (respuestaImprimir === "fail" || respuestaEnviar === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                setShowAlertImpresionExitosa(true);

                props.resetEstadoInicial();
              }
            }}
          >
            Enviar e Imprimir
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseFacturaElectronica();

              const respuestaImprimir = await ImprimirFactura(ultimaFactura);
              if (respuestaImprimir === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                props.resetEstadoInicial();
                setShowAlertImpresionExitosa(true);
              }
            }}
          >
            No Enviar e Imprimir
          </Button>
        </Modal.Footer>
      </Modal>
      <div
        className={`alert-container ${
          showAlertImpresionExitosa ? "active" : ""
        }`}
        onClick={() => {
          setShowAlertImpresionExitosa(false);
        }}
      >
        <Alert variant={"info"}>Factura Impresa Exitosamente</Alert>
      </div>
    </>
  );
};

export default ModalFacturaElectronica;
