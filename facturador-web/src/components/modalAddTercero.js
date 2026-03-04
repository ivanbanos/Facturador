import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import PostTercero from "../Services/getServices/PostTercero";

import "./styles/modal.css";

const ModalAddTercero = (props) => {
  const [errores, setErrores] = useState({});
  const [isProcessing, setIsProcessing] = useState(false);
  const tiposDeIdentificacion = props.tiposDeIdentificacion;
  const terceroInicial = {
    terceroId: 0,
    coD_CLI: "",
    nombre: "",
    apellidos: "",
    telefono: "",
    direccion: "",
    identificacion: "",
    correo: "",
    tipoIdentificacion: "",
  };
  const [nuevoTercero, setNuevoTercero] = useState(terceroInicial);

  const handleChangeTercero = (event) => {
    let value = event.target.value;
    // Forzar mayúsculas en todos los campos excepto identificacion, telefono y correo
    if (["nombre", "apellidos", "direccion", "coD_CLI"].includes(event.target.name)) {
      value = value.toUpperCase();
    }
    // Teléfono solo números
    if (event.target.name === "telefono") {
      value = value.replace(/[^0-9]/g, "");
    }
    // Identificación solo números
    if (event.target.name === "identificacion") {
      value = value.replace(/[^0-9]/g, "");
    }
    // Correo a mayúsculas (opcional, si lo quieres en mayúsculas)
    if (event.target.name === "correo") {
      value = value.toUpperCase();
    }
    const tempTercero = {
      ...nuevoTercero,
      [event.target.name]: value,
    };
    setNuevoTercero(tempTercero);
  };
  function formIsValid() {
    const _errores = {};
    if (!nuevoTercero.nombre) _errores.nombre = "Se requiere el nombre";
    if (!nuevoTercero.apellidos) _errores.apellidos = "Se requieren los apellidos";
    if (nuevoTercero.telefono && !/^\d{7,}$/.test(nuevoTercero.telefono)) {
      _errores.telefono = "Teléfono inválido (mínimo 7 dígitos)";
    }
    if (!nuevoTercero.identificacion) {
      _errores.identificacion = "Se requiere la identificación";
    } else if (!/^\d+$/.test(nuevoTercero.identificacion)) {
      _errores.identificacion = "Identificación inválida";
    }
    if (!nuevoTercero.tipoIdentificacion)
      _errores.tipoIdentificacion = "Se requiere el tipo de identificación";
    if (!nuevoTercero.correo) {
      _errores.correo = "Se requiere el correo";
    } else if (!/^([A-Z0-9_\.-]+)@([A-Z0-9\.-]+)\.([A-Z]{2,})$/i.test(nuevoTercero.correo)) {
      _errores.correo = "Correo inválido";
    }
    setErrores(_errores);
    return Object.keys(_errores).length === 0;
  }
  const onSubmitTercero = async (newTercero) => {
    if (isProcessing) return;
    if (!formIsValid()) return;
    setIsProcessing(true);
    try {
      const respuesta = await PostTercero(newTercero);
      if (respuesta === "fail") {
        props.handleSetShowAlertError && props.handleSetShowAlertError(true);
      } else {
        props.handleSetTerceroModalAddTercero &&
          props.handleSetTerceroModalAddTercero(newTercero);
        setNuevoTercero(terceroInicial);
      }

      props.handleShowAddTercero(false);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <>
      <div className="div-modal-add-tercero">
        <Modal
          show={props.showAddTercero}
          onHide={() => {
            if (isProcessing) return;
            props.handleShowAddTercero(false);
          }}
          backdrop="static"
          keyboard={false}
          dialogClassName="custom-modal"
          aria-labelledby="contained-modal-title-vcenter"
          centered
        >
          <Modal.Header className="header-modal-add-tercero" closeButton>
            <Modal.Title>Agregar Tercero</Modal.Title>
          </Modal.Header>
          <Modal.Body className="body-moda-add-tercero">
            <form>
              <div className="row mb-3">
                <label class="col-sm-4 col-form-label">
                  Tipo de Identificación
                </label>
                <div className="col-sm-8">
                  <select
                    className="form-select modal-tercero-input"
                    name="tipoIdentificacion"
                    value={nuevoTercero.tipoIdentificacion}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                  >
                    <option value="">Selecciona tipo identificación</option>
                    {Array.isArray(tiposDeIdentificacion) &&
                      tiposDeIdentificacion?.map((elemento) => (
                        <option
                          key={elemento.tipoIdentificacionId}
                          value={elemento.tipoIdentificacionId}
                        >
                          {elemento.descripcion}
                        </option>
                      ))}
                  </select>
                  {errores.tipoIdentificacion && (
                    <div className="alert alert-danger error-container">
                      {errores.tipoIdentificacion}
                    </div>
                  )}
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">
                  Identificación
                </label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.identificacion ? "is-invalid" : ""
                    }`}
                    name="identificacion"
                    value={nuevoTercero.identificacion}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.identificacion || "Identificación"}
                    maxLength={15}
                  />
                </div>
              </div>

              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Nombre</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.nombre ? "is-invalid" : ""
                    }`}
                    name="nombre"
                    value={nuevoTercero.nombre}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.nombre || "Nombre"}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Apellidos</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.apellidos ? "is-invalid" : ""
                    }`}
                    name="apellidos"
                    value={nuevoTercero.apellidos}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.apellidos || "Apellidos"}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Dirección</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.direccion ? "is-invalid" : ""
                    }`}
                    name="direccion"
                    value={nuevoTercero.direccion}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.direccion || "Dirección"}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Teléfono</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.telefono ? "is-invalid" : ""
                    }`}
                    name="telefono"
                    value={nuevoTercero.telefono}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.telefono || "Teléfono"}
                    maxLength={15}
                  />
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Correo</label>
                <div className="col-sm-8">
                  <input
                    type="email"
                    className={`form-control modal-tercero-input ${
                      errores.correo ? "is-invalid" : ""
                    }`}
                    name="correo"
                    value={nuevoTercero.correo}
                    disabled={isProcessing}
                    onChange={handleChangeTercero}
                    placeholder={errores.correo || "Correo"}
                    style={{ textTransform: "uppercase" }}
                  />
                </div>
              </div>
            </form>
          </Modal.Body>
          <Modal.Footer>
            <Button
              className="botton-light-blue-modal"
              disabled={isProcessing}
              onClick={() => {
                props.handleShowAddTercero(false);
                props.handleNoCambiarTercero();
                setNuevoTercero(terceroInicial);
              }}
            >
              Cancelar
            </Button>
            <Button
              className="botton-medium-blue-modal"
              disabled={isProcessing}
              onClick={() => {
                onSubmitTercero(nuevoTercero);
              }}
            >
              Agregar
            </Button>
          </Modal.Footer>
        </Modal>
      </div>
    </>
  );
};

export default ModalAddTercero;
