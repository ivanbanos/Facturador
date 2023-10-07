import React, { useState, useEffect } from "react";
import "./styles/home.css";
import "./styles/terceros.css";
import PostTercero from "../services/getServices/PostTercero";
import GetTiposDeIdentificacion from "../services/getServices/GetTiposDeIdentificacion";

const Terceros = () => {
  const [tercero, setTercero] = useState({
    terceroId: 0,
    coD_CLI: "",
    nombre: "",
    telefono: "",
    direccion: "",
    identificacion: "",
    correo: "",
    tipoIdentificacion: 0,
  });
  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const handleChangeTercero = (event) => {
    const tempTercero = {
      ...tercero,
      [event.target.name]: event.target.value,
    };
    setTercero(tempTercero);
  };
  useEffect(() => {
    const fetchData = async () => {
      try {
        let tiposDeIdentificacion = await GetTiposDeIdentificacion();
        setTiposDeIdentificacion(tiposDeIdentificacion);
      } catch (error) {}
    };

    fetchData();
  }, []);
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
                      <input
                        type="text"
                        class="form-control tercero-input"
                        name="identificacion"
                        value={tercero.identificacion}
                        onChange={handleChangeTercero}
                      ></input>
                    </div>
                  </div>

                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">
                      Tipo de Identificación
                    </label>
                    <div class="col-sm-10">
                      <select
                        className="form-select w-80 h-50 tercero-input"
                        name="tipoIdentificacion"
                        value={tercero.tipoIdentificacion || ""}
                        onChange={handleChangeTercero}
                      >
                        <option value="">Selecciona tipo identificación</option>
                        {Array.isArray(tiposDeIdentificacion) &&
                          tiposDeIdentificacion.map((elemento) => (
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
                    <label class="col-sm-2 col-form-label">Nombre</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                        name="nombre"
                        value={tercero.nombre}
                        onChange={handleChangeTercero}
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Dirección</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                        name="direccion"
                        value={tercero.direccion}
                        onChange={handleChangeTercero}
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Teléfono</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                        name="telefono"
                        value={tercero.telefono}
                        onChange={handleChangeTercero}
                      ></input>
                    </div>
                  </div>
                  <div class="row mb-3">
                    <label class="col-sm-2 col-form-label">Correo</label>
                    <div class="col-sm-10">
                      <input
                        type="text"
                        class="form-control tercero-input"
                        name="correo"
                        value={tercero.correo}
                        onChange={handleChangeTercero}
                      ></input>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <button
            className="print-button botton-light-blue"
            onClick={() => {
              console.log(tercero);
              PostTercero(tercero);
            }}
          >
            Agregar
          </button>
        </div>
      </div>
    </>
  );
};

export default Terceros;
