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
    const [enviando, setEnviando] =
      useState(false);
  const [cantidad, setCantidad] = useState(2);

  const [
    showAlertImpresionExitosaSinFacturacion,
    setShowAlertImpresionExitosaSinFacturacion,
  ] = useState(false);

  const handleChangeCantidad = async (event) => {
    setCantidad(event.target.value);
  };
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
        <Modal.Body>
          Desea enviar la factura electrónica?
          <br />
          Cantidad a imprimir{" "}
          <input
            type="number"
            className="form-control dark-blue-input w-100 input-identificacion "
            placeholder="Cantidad"
            name="cantidad"
            value={cantidad || ""}
            onkeydown="return /[a-zA-Z0-9]/i.test(event.key)"
            onChange={handleChangeCantidad}
          ></input>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            
            disabled={enviando}
            onClick={async () => {
              setEnviando(true)
              const respuestaEnviar = await EnviarFacturaElectronica(
                ultimaFactura,
                cantidad
              );
              if (respuestaEnviar === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                setShowAlertImpresionExitosa(true);

                handleCloseFacturaElectronica();
                props.getFacturaInformacion();
              }
              setEnviando(false)
            }}
          >
            Enviar e Imprimir
          </Button>
          <Button
            className="botton-medium-blue-modal"
            disabled={enviando}
            onClick={async () => {
              
              setEnviando(true)
              const respuestaImprimir = await ImprimirFactura(
                ultimaFactura,
                cantidad
              );
              console.log(ultimaFactura);
              const text = await GetUltimaFacturaPorCaraTexto(
                ultimaFactura.idCara
              );if(window.imprimirNativo){

          await ImprimirNativo(text);
        }
              if (respuestaImprimir === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                props.getFacturaInformacion();
                setShowAlertImpresionExitosa(true);
                handleCloseFacturaElectronica();
              }
              
              setEnviando(false)
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
          onClose={() => {
            setShowAlertImpresionExitosa(false);
            handleCloseFacturaElectronica();
          }}
          dismissible
        >
          <Alert.Heading>Fatura impresa de forma exitosa</Alert.Heading>
        </Alert>
        <Alert
          variant="info"
          show={showAlertImpresionExitosaSinFacturacion}
          onClose={() => {
            setShowAlertImpresionExitosaSinFacturacion(false);
            handleCloseFacturaElectronica();
          }}
          dismissible
        >
          <Alert.Heading>
            Fatura impresa de forma exitosa. Sin envio a DIAN. Tercero no
            actualizado
          </Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalFacturaElectronica;
