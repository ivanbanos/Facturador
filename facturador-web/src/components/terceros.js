import React from "react";
import "./styles/home.css";
import "./styles/terceros.css";

const Terceros = () => {
  return (
    <>
      <div className="col-12 pt-4 pb-4  columnas terceros-box row">
        <div className="boder-div col-7 ">
          <div className="row add-tercero-div">
            <div className="col-5 icono-add-div">
              <h1 className="text-white title-add">Agregar Tercero</h1>
            </div>
            <div className="col-7 form-tercero-div">
              <div className="formulario ">
                <form>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">
                      Identificación
                    </label>
                    <div class="col-sm-10">
                      <input type="" class="form-control tercero-input"></input>
                    </div>
                  </div>

                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">
                      Tipo de Identificación
                    </label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Nombre</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Dirección</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Teléfono</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Correo</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                      ></input>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <button className="print-button botton-light-blue">Agregar</button>
        </div>
      </div>
    </>
  );
};

export default Terceros;
