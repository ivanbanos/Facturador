import React, { useState, useEffect } from "react";
import "./styles/home.css";
import "./styles/terceros.css";
import PostTercero from "../Services/getServices/PostTercero";
import GetTiposDeIdentificacion from "../Services/getServices/GetTiposDeIdentificacion";
import AlertError from "./alertaError";
import AlertTerceroAgregadoExitosamente from "./alertTerceroAgregadoExitosamente";
import GetTercero from "../Services/getServices/GetTercero";

const Terceros = () => {
  const [errores, setErrores] = useState({});
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
  const [identificacion, setIdentificacion] = useState("");
  const handleChangeTercero = (event) => {
    const tempTercero = {
      ...tercero,
      [event.target.name]: event.target.value,
    };
    setTercero(tempTercero);
  };
  function formIsValid() {
    const _errores = {};
    if (!tercero.nombre) _errores.nombre = "Se requiere el nombre";
    // if (!tercero.telefono) _errores.telefono = "Se requiere el teléfono";
    // if (!tercero.direccion) _errores.direccion = "Se requiere la dirección";
    if (!tercero.identificacion)
      _errores.identificacion = "Se requiere la identificación";
    if (!tercero.tipoIdentificacion)
      _errores.tipoIdentificacion = "Se requiere el tipo de identificación";
    // if (!tercero.correo) _errores.correo = "Se requiere el correo";
    setErrores(_errores);
    return Object.keys(_errores).length === 0;
  }
  const handleOnBlurIdentificacion = async (event) => {
    event.preventDefault();
    const nuevaIdentificacion = event.target.value;

    let nuevoTercero = await GetTercero(nuevaIdentificacion);

    if (nuevoTercero.length > 0) {
      setTercero(nuevoTercero[0]);
    } else {
      const tempTercero = {
        ...tercero,
        identificacion: nuevaIdentificacion,
      };
      setTercero(tempTercero);
    }
  };
  const handleChangeIdentificacion = async (event) => {
    const nuevaIdentificacion = event.target.value;
    setIdentificacion(nuevaIdentificacion);
  };

  const [showAlertError, setShowAlertError] = useState(false);
  const handleSetShowAlertError = (show) => setShowAlertError(show);
  const [
    showAlertTerceroAgregadoExitosamente,
    setShowAlertTerceroAgregadoExitosamente,
  ] = useState(false);
  const handleSetShowAlertTerceroAgregadoExitosamente = (show) =>
    setShowAlertTerceroAgregadoExitosamente(show);

  const addTercero = async () => {
    if (!formIsValid()) return;
    const respuesta = await PostTercero(tercero);

    if (respuesta === "fail") {
      handleSetShowAlertError(true);
    } else {
      handleSetShowAlertTerceroAgregadoExitosamente(true);
    }
    setTercero({
      terceroId: 0,
      coD_CLI: "",
      nombre: "",
      telefono: "",
      direccion: "",
      identificacion: "",
      correo: "",
      tipoIdentificacion: 0,
    });
    setIdentificacion("");
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
        <div className="boder-div col-10 ">
          <div className="row add-tercero-div">
            <div className="icono-add-div ">
              <h1 className="text-white title-add ">Agregar Tercero</h1>
            </div>
            <div className="col-9 form-tercero-div">
              <div className="formulario p-auto">
                <form className="formulario-div">
                  <div className="row margen-input-3 p-auto">
                    <label className="col-sm-4 col-form-label">
                      Identificación
                    </label>
                    <div className="col-sm-8">
                      <input
                        type="text"
                        className={`form-control tercero-input ${
                          errores.identificacion ? "is-invalid" : ""
                        }`}
                        name="identificacion"
                        value={identificacion}
                        onChange={handleChangeIdentificacion}
                        onBlur={handleOnBlurIdentificacion}
                        required
                        placeholder={errores.identificacion || "Identificación"}
                      ></input>
                    </div>
                  </div>

                  <div className="row margen-input-3 d-flex align-items-center">
                    <label className="col-sm-4 col-form-label">
                      Tipo de Identificación
                    </label>
                    <div className="col-sm-8">
                      <select
                        className="form-select w-80 h-50 tercero-input"
                        name="tipoIdentificacion"
                        value={tercero.tipoIdentificacion || ""}
                        onChange={handleChangeTercero}
                        required
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
                      {errores.tipoIdentificacion && (
                        <div className="alert alert-danger error-container">
                          {errores.tipoIdentificacion}
                        </div>
                      )}
                    </div>
                  </div>
                  <div className="row margen-input-3">
                    <label className="col-sm-4 col-form-label">Nombre</label>
                    <div className="col-sm-8">
                      <input
                        type="text"
                        className={`form-control tercero-input ${
                          errores.nombre ? "is-invalid" : ""
                        }`}
                        name="nombre"
                        value={tercero.nombre}
                        onChange={handleChangeTercero}
                        required
                        placeholder={errores.nombre || "Nombre"}
                      ></input>
                    </div>
                  </div>
                  <div className="row margen-input-3">
                    <label className="col-sm-4 col-form-label">Dirección</label>
                    <div className="col-sm-8">
                      <input
                        type="text"
                        className={`form-control tercero-input ${
                          errores.direccion ? "is-invalid" : ""
                        }`}
                        name="direccion"
                        value={tercero.direccion}
                        onChange={handleChangeTercero}
                        required
                        placeholder={errores.direccion || "Dirección"}
                      ></input>
                    </div>
                  </div>
                  <div className="row margen-input-3">
                    <label className="col-sm-4 col-form-label">Teléfono</label>
                    <div className="col-sm-8">
                      <input
                        type="text"
                        className={`form-control tercero-input ${
                          errores.telefono ? "is-invalid" : ""
                        }`}
                        name="telefono"
                        value={tercero.telefono}
                        onChange={handleChangeTercero}
                        required
                        placeholder={errores.telefono || "Teléfono"}
                      ></input>
                    </div>
                  </div>
                  <div className="row mb-3">
                    <label className="col-sm-4 col-form-label">Correo</label>
                    <div className="col-sm-8">
                      <input
                        type="text"
                        className={`form-control tercero-input ${
                          errores.correo ? "is-invalid" : ""
                        }`}
                        name="correo"
                        value={tercero.correo}
                        onChange={handleChangeTercero}
                        required
                        placeholder={errores.correo || "Correo"}
                      ></input>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <button className="add-button botton-light-blue" onClick={addTercero}>
            Actualizar Tercero
          </button>
        </div>
      </div>
      <AlertError
        showAlertError={showAlertError}
        handleSetShowAlertError={handleSetShowAlertError}
      ></AlertError>
      <AlertTerceroAgregadoExitosamente
        showAlertTerceroAgregadoExitosamente={
          showAlertTerceroAgregadoExitosamente
        }
        handleSetShowAlertTerceroAgregadoExitosamente={
          handleSetShowAlertTerceroAgregadoExitosamente
        }
      ></AlertTerceroAgregadoExitosamente>
    </>
  );
};

export default Terceros;
