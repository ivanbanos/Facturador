import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import PostTercero from "../Services/getServices/PostTercero";

import "./styles/modal.css";

const ModalAddTercero = (props) => {
  const [errores, setErrores] = useState({});
  const tiposDeIdentificacion = props.tiposDeIdentificacion;
  const terceroInicial = {
    terceroId: 0,
    coD_CLI: "",
    nombre: "",
    telefono: "",
    direccion: "",
    identificacion: "",
    correo: "",
    tipoIdentificacion: "",
  };
  const [nuevoTercero, setNuevoTercero] = useState(terceroInicial);

  const handleChangeTercero = (event) => {
    const tempTercero = {
      ...nuevoTercero,
      [event.target.name]: event.target.value,
    };
    setNuevoTercero(tempTercero);
  };
  function formIsValid() {
    const _errores = {};
    if (!nuevoTercero.nombre) _errores.nombre = "Se requiere el nombre";
    if (!nuevoTercero.telefono) _errores.telefono = "Se requiere el teléfono";
    if (!nuevoTercero.direccion)
      _errores.direccion = "Se requiere la dirección";
    if (!nuevoTercero.identificacion)
      _errores.identificacion = "Se requiere la identificación";
    if (!nuevoTercero.tipoIdentificacion)
      _errores.tipoIdentificacion = "Se requiere el tipo de identificación";
    if (!nuevoTercero.correo) _errores.correo = "Se requiere el correo";
    setErrores(_errores);
    return Object.keys(_errores).length === 0;
  }
  const onSubmitTercero = (newTercero) => {
    if (!formIsValid()) return;
    PostTercero(newTercero);
    props.handleSetTerceroModalAddTercero &&
      props.handleSetTerceroModalAddTercero(newTercero);
    setNuevoTercero(terceroInicial);
    props.handleShowAddTercero(false);
  };

  return (
    <>
      <div className="div-modal-add-tercero">
        <Modal
          show={props.showAddTercero}
          onHide={() => {
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
                    onChange={handleChangeTercero}
                    placeholder={errores.identificacion || "Identificación"}
                  ></input>
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
                    onChange={handleChangeTercero}
                    placeholder={errores.nombre || "Nombre"}
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
                    onChange={handleChangeTercero}
                    placeholder={errores.telefono || "Teléfono"}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Correo</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className={`form-control modal-tercero-input ${
                      errores.correo ? "is-invalid" : ""
                    }`}
                    name="correo"
                    value={nuevoTercero.correo}
                    onChange={handleChangeTercero}
                    placeholder={errores.correo || "Correo"}
                  ></input>
                </div>
              </div>
            </form>
          </Modal.Body>
          <Modal.Footer>
            <Button
              className="botton-light-blue-modal"
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
