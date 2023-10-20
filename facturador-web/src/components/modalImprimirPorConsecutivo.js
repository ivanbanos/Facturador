import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import "./styles/home.css";
import "./styles/modal.css";

const ModalImprimirPorConsecutivo = (props) => {
  const [showModalImprimirPorConsecutivo, setShowModalImprimirPorConsecutivo] =
    useState(false);
  const handleCloseModalImprimirPorConsecutivo = () =>
    setShowModalImprimirPorConsecutivo(false);
  const handleShowModalImprimirPorConsecutivo = () =>
    setShowModalImprimirPorConsecutivo(true);
  const [consecutivo, setConsecutivo] = useState("");

  return (
    <>
      <Button
        className="botton-light-blue right-botton m-1"
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
              handleCloseModalImprimirPorConsecutivo();
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              handleCloseModalImprimirPorConsecutivo();
            }}
          >
            Imprimir
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalImprimirPorConsecutivo;
