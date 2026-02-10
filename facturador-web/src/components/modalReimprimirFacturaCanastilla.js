import React, { useState } from "react";
import { Modal, Button, Alert } from "react-bootstrap";
import ReimprimirFacturaCanastilla from "../Services/getServices/ReimprimirFacturaCanastilla";
import "./styles/home.css";
import "./styles/modal.css";

const ModalReimprimirFacturaCanastilla = (props) => {
  const [showModal, setShowModal] = useState(false);
  const handleCloseModal = () => setShowModal(false);
  const handleShowModal = () => setShowModal(true);
  const [consecutivo, setConsecutivo] = useState("");
  const [showAlertExitosa, setShowAlertExitosa] = useState(false);

  return (
    <>
      <button
        className="botton-medium-blue m-3 right-botton right-botton-xs"
        onClick={handleShowModal}
      >
        <span>Reimprimir</span> <span>Canastilla</span>
      </button>

      <Modal
        show={showModal}
        onHide={handleCloseModal}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Reimprimir Factura Canastilla</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Consecutivo de Factura{" "}
              </label>
              <div className="col-sm-7">
                <input
                  type="number"
                  className="form-control modal-tercero-input"
                  name="consecutivo"
                  value={consecutivo}
                  onChange={(event) => setConsecutivo(event.target.value)}
                ></input>
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseModal();
              setConsecutivo("");
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseModal();
              const respuesta = await ReimprimirFacturaCanastilla(consecutivo);
              if (respuesta === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                setConsecutivo("");
                setShowAlertExitosa(true);
              }
            }}
          >
            Reimprimir
          </Button>
        </Modal.Footer>
      </Modal>
      <div
        className={`alert-container ${showAlertExitosa ? "active" : ""}`}
      >
        <Alert
          variant="info"
          show={showAlertExitosa}
          onClose={() => setShowAlertExitosa(false)}
          dismissible
        >
          <Alert.Heading>
            Factura canastilla enviada a reimprimir exitosamente
          </Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalReimprimirFacturaCanastilla;
