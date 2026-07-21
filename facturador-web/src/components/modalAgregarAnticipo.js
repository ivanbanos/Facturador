import React, { useState } from "react";
import { Modal, Button, Alert } from "react-bootstrap";
import AgregarAnticipo from "../Services/getServices/AgregarAnticipo";
import "./styles/home.css";
import "./styles/modal.css";

const ModalAgregarAnticipo = (props) => {
  const [show, setShow] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [nombre, setNombre] = useState("");
  const [placa, setPlaca] = useState("");
  const [monto, setMonto] = useState("");
  const [error, setError] = useState("");
  const [exito, setExito] = useState(false);

  const handleOpen = () => {
    setNombre("");
    setPlaca("");
    setMonto("");
    setError("");
    setExito(false);
    setShow(true);
  };

  const handleClose = () => {
    if (!isProcessing) setShow(false);
  };

  const handleGuardar = async () => {
    setError("");
    const montoNum = parseFloat(monto);
    if (!monto || isNaN(montoNum) || montoNum <= 0) {
      setError("Ingrese un monto válido mayor a cero.");
      return;
    }
    if (!nombre.trim() && !placa.trim()) {
      setError("Debe ingresar al menos un nombre o una placa.");
      return;
    }

    setIsProcessing(true);
    try {
      const turno = props.turno;
      const anticipo = {
        idIsla: props.islaSelect,
        numTurno: turno ? turno.numero ?? turno.Numero ?? 0 : 0,
        fechaTurno: turno ? (turno.fechaApertura ?? turno.FechaApertura ?? new Date().toISOString()) : new Date().toISOString(),
        turnoGuid: turno ? (turno.turnoGuid ?? turno.TurnoGuid ?? "") : "",
        nombre: nombre.trim(),
        placa: placa.trim(),
        monto: montoNum,
      };
      const resultado = await AgregarAnticipo(anticipo);
      if (resultado === "ok") {
        setExito(true);
        setTimeout(() => setShow(false), 1500);
      } else {
        setError(resultado !== "fail" ? resultado : "Error al guardar el anticipo.");
      }
    } catch {
      setError("Error inesperado al guardar el anticipo.");
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <>
      <Button
        className="botton-green m-1 right-botton"
        disabled={isProcessing}
        onClick={handleOpen}
      >
        Anticipo Efectivo
      </Button>

      <Modal
        show={show}
        onHide={handleClose}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Anticipo de Efectivo</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {error && <Alert variant="danger">{error}</Alert>}
          {exito && <Alert variant="success">Anticipo guardado correctamente.</Alert>}
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Isla</label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  value={props.islaSelectName}
                  disabled
                />
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Nombre / Persona</label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  placeholder="Opcional"
                  value={nombre}
                  disabled={isProcessing}
                  onChange={(e) => setNombre(e.target.value)}
                />
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Placa</label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  placeholder="Opcional"
                  value={placa}
                  disabled={isProcessing}
                  onChange={(e) => setPlaca(e.target.value.toUpperCase())}
                />
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Monto ($)</label>
              <div className="col-sm-7">
                <input
                  type="number"
                  min="1"
                  step="any"
                  className="form-control modal-tercero-input"
                  placeholder="0"
                  value={monto}
                  disabled={isProcessing}
                  onChange={(e) => setMonto(e.target.value)}
                />
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleClose} disabled={isProcessing}>
            Cancelar
          </Button>
          <Button variant="primary" onClick={handleGuardar} disabled={isProcessing}>
            {isProcessing ? "Guardando..." : "Guardar"}
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ModalAgregarAnticipo;
