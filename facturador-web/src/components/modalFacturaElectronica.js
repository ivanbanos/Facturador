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
  const [enviando, setEnviando] = useState(false);
  const [cantidad, setCantidad] = useState(2);

  const [
    showAlertImpresionExitosaSinFacturacion,
    setShowAlertImpresionExitosaSinFacturacion,
  ] = useState(false);

  const handleChangeCantidad = async (event) => {
    const value = event.target.value;
    if (value === "") {
      setCantidad("");
      return;
    }
    const numericValue = Number(value);
    setCantidad(Number.isNaN(numericValue) ? 1 : Math.max(1, numericValue));
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
            min={1}
            step={1}
            onChange={handleChangeCantidad}
          ></input>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            
            disabled={enviando}
            onClick={async () => {
              setEnviando(true);
              const cantidadEnviar = Number(cantidad) > 0 ? Number(cantidad) : 1;
              const respuestaEnviar = await EnviarFacturaElectronica(
                ultimaFactura,
                cantidadEnviar
              );
              if (respuestaEnviar === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                setShowAlertImpresionExitosa(true);

                handleCloseFacturaElectronica();
                props.getFacturaInformacion();
              }
              setEnviando(false);
            }}
          >
            Enviar e Imprimir
          </Button>
          <Button
            className="botton-medium-blue-modal"
            disabled={enviando}
            onClick={async () => {
              setEnviando(true);
              const cantidadImprimir = Number(cantidad) > 0 ? Number(cantidad) : 1;
              const respuestaImprimir = await ImprimirFactura(
                ultimaFactura,
                cantidadImprimir
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
              
              setEnviando(false);
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
