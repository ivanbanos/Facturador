import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";
import PostTercero from "../services/getServices/PostTercero";

import "./styles/modal.css";

const ModalAddTercero = (props) => {
  const tiposDeIdentificacion = props.tiposDeIdentificacion;
  const identificacionRecibida = props.identificacionActualizada;
  const [identificacionModalAddTercero, setIdentificacionModalAddTercero] =
    useState(identificacionRecibida);
  //   console.log(identificacionModalAddTercero);
  //   console.log(tiposDeIdentificacion);
  const [nuevoTercero, setNuevoTercero] = useState({
    terceroId: 0,
    coD_CLI: "",
    nombre: "",
    telefono: "",
    direccion: "",
    identificacion: "",
    correo: "",
    tipoIdentificacion: "",
  });

  const handleChangeTercero = (event) => {
    const tempTercero = {
      ...nuevoTercero,
      [event.target.name]: event.target.value,
    };
    setNuevoTercero(tempTercero);
  };
  const handleChangeIdentificacion = (event) => {
    setIdentificacionModalAddTercero(event.target.value);
    const tempTercero = {
      ...nuevoTercero,
      [event.target.name]: event.target.value,
    };
    setNuevoTercero(tempTercero);
  };

  const onSubmitTercero = (newTercero) => {
    console.log("submittercero");
    console.log(newTercero);
    PostTercero(newTercero);
    props.handleSetTerceroModalAddTercero(newTercero);
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
                    {tiposDeIdentificacion?.map((elemento) => (
                      <option
                        key={elemento.tipoIdentificacionId}
                        value={elemento.tipoIdentificacionId}
                      >
                        {elemento.descripcion}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">
                  Identificación
                </label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className="form-control modal-tercero-input"
                    name="identificacion"
                    value={nuevoTercero.identificacion}
                    onChange={handleChangeTercero}
                  ></input>
                </div>
              </div>

              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Nombre</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className="form-control modal-tercero-input"
                    name="nombre"
                    value={nuevoTercero.nombre}
                    onChange={handleChangeTercero}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Dirección</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className="form-control modal-tercero-input"
                    name="direccion"
                    value={nuevoTercero.direccion}
                    onChange={handleChangeTercero}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Teléfono</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    class="form-control modal-tercero-input"
                    name="telefono"
                    value={nuevoTercero.telefono}
                    onChange={handleChangeTercero}
                  ></input>
                </div>
              </div>
              <div className="row mb-3">
                <label className="col-sm-4 col-form-label">Correo</label>
                <div className="col-sm-8">
                  <input
                    type="text"
                    className="form-control modal-tercero-input"
                    name="correo"
                    value={nuevoTercero.correo}
                    onChange={handleChangeTercero}
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
              }}
            >
              Cancelar
            </Button>
            <Button
              className="botton-medium-blue-modal"
              onClick={() => {
                onSubmitTercero(nuevoTercero);
                props.handleShowAddTercero(false);
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
