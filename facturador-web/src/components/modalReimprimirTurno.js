import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import ReimprimirTurno from "../Services/getServices/ReimprimirTurno";
import "./styles/home.css";
import "./styles/modal.css";
import { Alert } from "react-bootstrap";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

const ModalReimprimirTurno = (props) => {
  const [showModalReimpirmirTurno, setShowModalReimprimirTurno] =
    useState(false);
  const handleCloseModalReimprimirTurno = () =>
    setShowModalReimprimirTurno(false);
  const handleShowModalReimprimirTurno = () =>
    setShowModalReimprimirTurno(true);
  const [fechaTurno, setFechaTurno] = useState("");
  const [fechaSelected, setFechaSelected] = useState("");
  const [posicion, setPosicion] = useState("");
  const [showAlertImpresionExitosa, setShowAlertImpresionExitosa] =
    useState(false);

  const islaSelect = props.islaSelect;

  const handleDateChange = (event) => {
    const selectedDate = event.target.value;
    setFechaSelected(selectedDate);
    // Asegurémonos de que la fecha sea válida antes de guardarla
    if (isValidDate(selectedDate)) {
      // Realizamos la conversión al formato mes-día-año (MM-DD-YYYY)
      const parts = selectedDate.split("-");
      if (parts.length === 3) {
        const formattedDate = `${parts[1]}-${parts[2]}-${parts[0]}`;
        setFechaTurno(formattedDate);
      }
    }
  };

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/;
    return pattern.test(date);
  };

  return (
    <>
      <Button
        className="botton-green m-1 right-botton"
        onClick={() => {
          handleShowModalReimprimirTurno();
        }}
      >
        Reimprimir Turno
      </Button>

      <Modal
        show={showModalReimpirmirTurno}
        onHide={handleCloseModalReimprimirTurno}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Reimpimir Turno</Modal.Title>
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
                  value={props.islaSelectName}
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
            onClick={() => {
              handleCloseModalReimprimirTurno();
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseModalReimprimirTurno();
              const respuestaReImprimir = await ReimprimirTurno(
                fechaTurno,
                islaSelect,
                posicion
              );
              if (respuestaReImprimir === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                
                await ImprimirNativo(respuestaReImprimir);
                setPosicion("");
                setFechaSelected("");
                setFechaTurno("");
                setShowAlertImpresionExitosa(true);
              }
            }}
          >
            Reimpimir Turno
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
          <Alert.Heading>Turno reimpreso de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalReimprimirTurno;
