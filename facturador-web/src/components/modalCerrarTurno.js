import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";

import "./styles/home.css";
import "./styles/modal.css";

const ModalCerrarTurno = (props) => {
  const [showModalCerrarTurno, setShowModalCerrarTurno] = useState(false);
  const handleCloseModalCerrarTurno = () => setShowModalCerrarTurno(false);
  const handleShowModalCerrarTurno = () => setShowModalCerrarTurno(true);
  const [codigoEmpleado, setCodigoEmpleado] = useState("");
  const handleChangeCodigoEmpleado = (codigo) => setCodigoEmpleado(codigo);
  const islaSelect = props.islaSelect;
  const cerrarTurno = props.cerrarTurno;
  console.log(islaSelect);
  console.log(props.islaSelectName);
  return (
    <>
      <Button
        className="botton-green m-1 right-botton"
        onClick={() => {
          handleShowModalCerrarTurno();
        }}
      >
        Cerrar Turno
      </Button>

      <Modal
        show={showModalCerrarTurno}
        onHide={handleCloseModalCerrarTurno}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Cerrar Turno</Modal.Title>
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
              handleCloseModalCerrarTurno();
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={() => {
              handleCloseModalCerrarTurno();
              cerrarTurno(islaSelect, codigoEmpleado);
            }}
          >
            Cerrar Turno
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalCerrarTurno;
