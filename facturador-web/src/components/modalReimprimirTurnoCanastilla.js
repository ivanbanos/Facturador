import React, { useState } from "react";
import { Modal, Button, Alert } from "react-bootstrap";
import ReimprimirTurnoCanastilla from "../Services/getServices/ReimprimirTurnoCanastilla";
import "./styles/home.css";
import "./styles/modal.css";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

const ModalReimprimirTurnoCanastilla = (props) => {
  const [showModal, setShowModal] = useState(false);
  const handleCloseModal = () => setShowModal(false);
  const handleShowModal = () => setShowModal(true);
  const [fechaTurno, setFechaTurno] = useState("");
  const [fechaSelected, setFechaSelected] = useState("");
  const [posicion, setPosicion] = useState("");
  const [showAlertExitosa, setShowAlertExitosa] = useState(false);

  const handleDateChange = (event) => {
    const selectedDate = event.target.value;
    setFechaSelected(selectedDate);
    if (isValidDate(selectedDate)) {
      const parts = selectedDate.split("-");
      if (parts.length === 3) {
        const formattedDate = `${parts[1]}-${parts[2]}-${parts[0]}`;
        setFechaTurno(formattedDate);
      }
    }
  };

  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/;
    return pattern.test(date);
  };

  return (
    <>
      <Button
        className="botton-green m-3 right-botton right-botton-xs"
        onClick={handleShowModal}
      >
        <span>Reimprimir</span> <span>Turno</span>
      </Button>

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
          <Modal.Title>Reimprimir Turno Canastilla</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Isla Seleccionada
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="isla"
                  value={localStorage.getItem("islaSelectName") || "No seleccionada"}
                  disabled
                ></input>
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Número de Turno</label>
              <div className="col-sm-7">
                <select
                  className="form-select modal-tercero-input"
                  aria-label="Default select example"
                  value={posicion}
                  onChange={(event) => setPosicion(event.target.value)}
                >
                  <option value="">Selecciona el número de turno</option>
                  <option value="1">1</option>
                  <option value="2">2</option>
                  <option value="3">3</option>
                  <option value="4">4</option>
                </select>
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Fecha del Turno</label>
              <div className="col-sm-7">
                <input
                  type="date"
                  className="form-control modal-tercero-input"
                  name="fechaTurno"
                  value={fechaSelected}
                  onChange={handleDateChange}
                ></input>
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={handleCloseModal}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseModal();
              const islaSelect = localStorage.getItem("islaSelect");
              if (!islaSelect || !fechaTurno || !posicion) {
                props.handleSetShowAlertError(true);
                return;
              }
              const respuesta = await ReimprimirTurnoCanastilla(
                fechaTurno,
                islaSelect,
                posicion
              );
              if (respuesta === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                if (window.imprimirNativo) {
                  await ImprimirNativo(respuesta);
                }
                setPosicion("");
                setFechaSelected("");
                setFechaTurno("");
                setShowAlertExitosa(true);
              }
            }}
          >
            Reimprimir Turno
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
          <Alert.Heading>Turno canastilla reimpreso de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalReimprimirTurnoCanastilla;
