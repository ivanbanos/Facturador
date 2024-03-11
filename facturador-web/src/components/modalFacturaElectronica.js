import React, { useEffect, useState } from "react";
import { Modal, Button } from "react-bootstrap";
import ImprimirFactura from "../Services/getServices/ImprimirFactura";
import EnviarFacturaElectronica from "../Services/getServices/EnviarFacturaElectronica";
import { Alert } from "react-bootstrap";
import "./styles/modal.css";
import GetUltimaFacturaPorCaraTexto from "../Services/getServices/GetUltimaFacturaPorCaraTexto";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

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
              
              const respuestaEnviar = await EnviarFacturaElectronica(
                ultimaFactura
              );
              if (respuestaEnviar === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                setShowAlertImpresionExitosa(true);

                props.getFacturaInformacion();
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
              
          const text = await GetUltimaFacturaPorCaraTexto(ultimaFactura.cara);
          await ImprimirNativo(text);
              if (respuestaImprimir === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                props.getFacturaInformacion();
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
      >
        <Alert
          variant="info"
          show={showAlertImpresionExitosa}
          onClose={() => setShowAlertImpresionExitosa(false)}
          dismissible
        >
          <Alert.Heading>Fatura impresa de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalFacturaElectronica;
