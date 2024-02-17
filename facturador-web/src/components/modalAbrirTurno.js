import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import AbrirTurno from "../Services/getServices/AbrirTurno";
import "./styles/home.css";
import "./styles/modal.css";

const ModalAbrirTurno = (props) => {
  const [showModalAbrirTurno, setShowModalAbrirTurno] = useState(false);
  const handleCloseModalAbrirTurno = () => setShowModalAbrirTurno(false);
  const handleShowModalAbrirTurno = () => setShowModalAbrirTurno(true);
  const codigoEmpleado = props.codigoEmpleado;
  const handleChangeCodigoEmpleado = props.handleChangeCodigoEmpleado;
  const handleSetShowAlertError = props.handleSetShowAlertError;
  const islaSelect = props.islaSelect;

  return (
    <>
      <Button
        className="botton-green m-1 right-botton"
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
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Código de Empleado
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={codigoEmpleado}
                  onChange={(event) =>
                    handleChangeCodigoEmpleado(event.target.value)
                  }
                ></input>
              </div>
            </div>

            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Isla Seleccionada
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="nombre"
                  value={props.islaSelectName}
                  disabled
                ></input>
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseModalAbrirTurno();
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseModalAbrirTurno();
              let respuesta = await AbrirTurno(islaSelect, codigoEmpleado);
              if(respuesta === "fail"){
                handleSetShowAlertError(true);
              }
            }}
          >
            Abrir Turno
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalAbrirTurno;
