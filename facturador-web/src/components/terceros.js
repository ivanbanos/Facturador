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
    let value = event.target.value;
    // Forzar mayúsculas en nombre, dirección y coD_CLI
    if (["nombre", "direccion", "coD_CLI"].includes(event.target.name)) {
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
    // Correo a mayúsculas
    if (event.target.name === "correo") {
      value = value.toUpperCase();
    }
    const tempTercero = {
      ...tercero,
      [event.target.name]: value,
    };
    setTercero(tempTercero);
  };
  function formIsValid() {
    const _errores = {};
    if (!tercero.nombre) _errores.nombre = "Se requiere el nombre";
    if (!tercero.telefono) {
      _errores.telefono = "Se requiere el teléfono";
    } else if (!/^\d{7,}$/.test(tercero.telefono)) {
      _errores.telefono = "Teléfono inválido (mínimo 7 dígitos)";
    }
    if (!tercero.direccion) {
      _errores.direccion = "Se requiere la dirección";
    }
    if (!tercero.identificacion) {
      _errores.identificacion = "Se requiere la identificación";
    } else if (!/^\d+$/.test(tercero.identificacion)) {
      _errores.identificacion = "Identificación inválida";
    }
    if (!tercero.tipoIdentificacion)
      _errores.tipoIdentificacion = "Se requiere el tipo de identificación";
    if (!tercero.correo) {
      _errores.correo = "Se requiere el correo";
    } else if (!/^([A-Z0-9_\.-]+)@([A-Z0-9\.-]+)\.([A-Z]{2,})$/i.test(tercero.correo)) {
      _errores.correo = "Correo inválido";
    }
    
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
    // Solo permitir números
    if (/^\d*$/.test(nuevaIdentificacion)) {
      setIdentificacion(nuevaIdentificacion);
    }
  };

  // Función para validar entrada de identificación en tiempo real
  const handleKeyPressIdentificacion = (event) => {
    // Solo permitir números (0-9), backspace, delete, tab, enter
    if (!/[0-9]/.test(event.key) && 
        !['Backspace', 'Delete', 'Tab', 'Enter', 'ArrowLeft', 'ArrowRight'].includes(event.key)) {
      event.preventDefault();
    }
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
                        onKeyPress={handleKeyPressIdentificacion}
                        required
                        placeholder={errores.identificacion || "Identificación"}
                      ></input>
                      {errores.identificacion && (
                        <div className="alert alert-danger error-container">
                          {errores.identificacion}
                        </div>
                      )}
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
                        onkeydown="return /[a-zA-Z0-9]/i.test(event.key)"
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
                        maxLength={15}
                        required
                        placeholder={errores.telefono || "Teléfono"}
                      />
                    </div>
                  </div>
                  <div className="row mb-3">
                    <label className="col-sm-4 col-form-label">Correo</label>
                    <div className="col-sm-8">
                      <input
                        type="email"
                        className={`form-control tercero-input ${
                          errores.correo ? "is-invalid" : ""
                        }`}
                        name="correo"
                        value={tercero.correo}
                        onChange={handleChangeTercero}
                        required
                        placeholder={errores.correo || "Correo"}
                        style={{ textTransform: "uppercase" }}
                      />
                      {errores.correo && (
                        <div className="alert alert-danger error-container">
                          {errores.correo}
                        </div>
                      )}
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
