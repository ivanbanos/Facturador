import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import ImprimirPorConsecutivo from "../Services/getServices/ImprimirPorConsecutivo";
import "./styles/home.css";
import "./styles/modal.css";
import { Alert } from "react-bootstrap";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

const ModalImprimirPorConsecutivo = (props) => {
  const [showModalImprimirPorConsecutivo, setShowModalImprimirPorConsecutivo] =
    useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const handleCloseModalImprimirPorConsecutivo = () =>
    setShowModalImprimirPorConsecutivo(false);
  const handleShowModalImprimirPorConsecutivo = () =>
    setShowModalImprimirPorConsecutivo(true);
  const [consecutivo, setConsecutivo] = useState("");
  const [showAlertImpresionExitosa, setShowAlertImpresionExitosa] =
    useState(false);

  return (
    <>
      <Button
        className="botton-light-blue right-botton m-1"
        disabled={isProcessing}
        onClick={() => {
          handleShowModalImprimirPorConsecutivo();
        }}
      >
        Imprimir Por Consecutivo
      </Button>

      <Modal
        show={showModalImprimirPorConsecutivo}
        onHide={handleCloseModalImprimirPorConsecutivo}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Imprimir Factura Por Consecutivo</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Consecutivo de Factura{" "}
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={consecutivo}
                  disabled={isProcessing}
                  onChange={(event) => setConsecutivo(event.target.value)}
                ></input>
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            disabled={isProcessing}
            onClick={() => {
              handleCloseModalImprimirPorConsecutivo();
              setConsecutivo("");
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            disabled={isProcessing}
            onClick={async () => {
              if (isProcessing) {
                return;
              }

              setIsProcessing(true);
              handleCloseModalImprimirPorConsecutivo();
              try {
                const respuestaImprimir = await ImprimirPorConsecutivo(
                  consecutivo
                );
                if (respuestaImprimir === "fail") {
                  props.handleSetShowAlertError(true);
                } else {
                  if (window.imprimirNativo) {
                    await ImprimirNativo(respuestaImprimir);
                  }
                  setConsecutivo("");
                  setShowAlertImpresionExitosa(true);
                }
              } finally {
                setIsProcessing(false);
              }
            }}
          >
            Imprimir
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
          <Alert.Heading>Factura impresa de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalImprimirPorConsecutivo;
