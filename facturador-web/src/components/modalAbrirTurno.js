import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import "./styles/home.css";

import "./styles/modal.css";

const ModalAbrirTurno = (props) => {
  const [showModalAbrirTurno, setShowModalAbrirTurno] = useState(false);
  const handleCloseModalAbrirTurno = () => setShowModalAbrirTurno(false);
  const handleShowModalAbrirTurno = () => setShowModalAbrirTurno(true);

  return (
    <>
      <Button
        className="botton-green m-3 right-botton"
        onClick={() => {
          handleShowModalAbrirTurno();
        }}
      >
        Abrir Turno
      </Button>

      <Modal
        show={showModalAbrirTurno}
        onHide={handleCloseModalAbrirTurno}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Abrir Turno</Modal.Title>
        </Modal.Header>
        <Modal.Body>Abrir Turno</Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseModalAbrirTurno();
            }}
          >
            Convertir a Orden de Compra
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              handleCloseModalAbrirTurno();
            }}
          >
            No convertir
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalAbrirTurno;
