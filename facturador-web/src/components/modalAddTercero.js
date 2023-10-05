import React, { useState } from "react";
import { Modal, Button } from "react-bootstrap";

import "./styles/modal.css";

const ModalAddTercero = (props) => {
  const tiposDeIdentificacion = props.tiposDeIdentificacion;
  console.log(tiposDeIdentificacion);
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
              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">
                  Tipo de Identificación
                </label>
                <div class="col-sm-8">
                  <select
                    className="form-select modal-tercero-input"
                    name="tipoIdentificacion"
                    value={""}
                    // onChange={handleChangeTercero}
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
              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">Identificación</label>
                <div class="col-sm-8">
                  <input
                    type=""
                    class="form-control modal-tercero-input"
                  ></input>
                </div>
              </div>

              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">Nombre</label>
                <div class="col-sm-8">
                  <input
                    type="text"
                    class="form-control modal-tercero-input"
                  ></input>
                </div>
              </div>
              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">Dirección</label>
                <div class="col-sm-8">
                  <input
                    type="text"
                    class="form-control modal-tercero-input"
                  ></input>
                </div>
              </div>
              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">Teléfono</label>
                <div class="col-sm-8">
                  <input
                    type="text"
                    class="form-control modal-tercero-input"
                  ></input>
                </div>
              </div>
              <div class="row mb-3">
                <label class="col-sm-4 col-form-label">Correo</label>
                <div class="col-sm-8">
                  <input
                    type="text"
                    class="form-control modal-tercero-input"
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
                props.handleShowAddTercero(false);
              }}
            >
              No Enviar e Imprimir
            </Button>
          </Modal.Footer>
        </Modal>
      </div>
    </>
  );
};

export default ModalAddTercero;
