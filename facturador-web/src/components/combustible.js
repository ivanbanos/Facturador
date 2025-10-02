import React, { useState, useEffect } from "react";
import "./styles/home.css";
import GetIslas from "../Services/getServices/GetIslas";
import GetTurnoIsla from "../Services/getServices/GetTurnoIsla";
import GetCarasPorIsla from "../Services/getServices/GetCarasPorIsla";
import GetFormasDePago from "../Services/getServices/GetFormasDePago";
import GetTiposDeIdentificacion from "../Services/getServices/GetTiposDeIdentificacion";
import GetUltimaFacturaPorCara from "../Services/getServices/GetUltimaFacturaporCara";
import GetUltimaFacturaPorCaraTexto from "../Services/getServices/GetUltimaFacturaPorCaraTexto";
import ModalImprimir from "./modalImprimir";
import ModalFacturaElectronica from "./modalFacturaElectronica";
import AlertTercero from "./alertaTercero";
import ModalAddTercero from "./modalAddTercero";
import ModalAbrirTurno from "./modalAbrirTurno";
import ModalCerrarTurno from "./modalCerrarTurno";
import ModalAgregarBolsa from "./modalAgregarBolsa";
import CerrarTurno from "../Services/getServices/CerrarTurno";
import GetTercero from "../Services/getServices/GetTercero";
import FidelizarVenta from "../Services/getServices/FidelizarVenta";
import AlertError from "./alertaError";
import ModalImprimirPorConsecutivo from "./modalImprimirPorConsecutivo";
import ModalReimprimirTurno from "./modalReimprimirTurno";
import ModalFidelizarVenta from "./modalFidelizarVenta";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

const Combustible = () => {
  // Estado para mostrar error de placa
  const [placaError, setPlacaError] = useState("");
  // Opciones permitidas para placa

  // Estado para modo de placa: "PLACA" o "PALABRA"
  const [modoPlaca, setModoPlaca] = useState("PLACA");
  const [showAlertError, setShowAlertError] = useState(false);
  const handleSetShowAlertError = (show) => setShowAlertError(show);
  const [codigoEmpleado, setCodigoEmpleado] = useState("");
  const handleChangeCodigoEmpleado = (codigo) => setCodigoEmpleado(codigo);
  const [showAddTercero, setShowAddTercero] = useState(false);
  const [disableNumeroTrans, setDisableNumeroTrans] = useState(false);
  const [disableForma, setDisableForma] = useState(false);
  const handleShowAddTercero = (show) => setShowAddTercero(show);

  const [identificacion, setIdentificacion] = useState("");
  const [bloqueado, setUsuarioBloqueado] = useState(false);
  const [showFacturaElectronica, setShowFacturaElectronica] = useState(false);

  const handleCloseFacturaElectronica = () => setShowFacturaElectronica(false);
  const handleShowFacturaElectrónica = () => {
    setShowFacturaElectronica(true);
  };
  const [showTerceroNoExiste, setShowTerceroNoExiste] = useState(false);

  function handleShowTerceroNoExiste(showTerceroNoExiste) {
    setShowTerceroNoExiste(showTerceroNoExiste);
  }
  function handleNoCambiarTercero() {
    setIdentificacion(ultimaFactura.tercero.identificacion);
  }

  const [terceroBusqueda, setTerceroBusqueda] = useState([{}]);
  function onBlurTercero() {
    if (terceroBusqueda.length === 0) {
      setShowTerceroNoExiste(true);
    } else setShowTerceroNoExiste(false);
  }
  const [islas, setIslas] = useState([]);

  function GetLocalStorage() {
    try {
      setTurno(JSON.parse(localStorage.getItem("turno")) || null);

      setCaras(JSON.parse(localStorage.getItem("caras")) || null);
      setIslaSelect(JSON.parse(localStorage.getItem("islaSelect")) || null);
      setIslaSelectName(
        JSON.parse(localStorage.getItem("islaSelectName")) || null
      );
    } catch (error) {
      localStorage.clear();
    }
  }

  const [turno, setTurno] = useState(null);
  const [caras, setCaras] = useState([]);

  const [ultimaFacturaTexto, setUltimaFacturaTexto] = useState("");

  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const [formasDePago, setformasDePago] = useState([]);
  const [islaSelect, setIslaSelect] = useState("");
  const [islaSelectName, setIslaSelectName] = useState("");

  const [caraSelect, setCaraSelect] = useState("");

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
  async function handleSetTerceroModalAddTercero(newTercero) {
    setIdentificacion(newTercero.identificacion);
    const tempFactura = { ...ultimaFactura, tercero: newTercero };
    setUltimaFactura(tempFactura);
    let nuevoTercero = await GetTercero(newTercero.identificacion);

    setTerceroBusqueda(nuevoTercero);

    if (nuevoTercero.length > 0) {
      setTercero(nuevoTercero[0]);
      const tempFactura = { ...ultimaFactura, tercero: nuevoTercero[0] };
      setUltimaFactura(tempFactura);
      // setIdentificacion(nuevoTercero[0].identificacion);
      setShowTerceroNoExiste(false);
    } else {
      // Actualiza identificacion aquí
    }
  }
  const ultimaFacturaEstadoInicial = {
    placa: "",
    kilometraje: "",
    codigoFormaPago: "",
    consecutivo: 0,
    ventaId: 1,
    tercero: {
      terceroId: 0,
      coD_CLI: "",
      nombre: "",
      telefono: "",
      direccion: "",
      identificacion: "",
      correo: "",
      tipoIdentificacion: 0,
    },
  };
  const [ultimaFactura, setUltimaFactura] = useState(
    ultimaFacturaEstadoInicial
  );

  const handleSetUltimaFactura = async (factura) => {
    await handleEstadoFactura(factura);
    setUltimaFactura(factura);
  };
  const getFacturaInformacion = () => {
    // handleSetUltimaFactura(ultimaFacturaEstadoInicial);
    // setIdentificacion("");
    // setUltimaFacturaTexto("");
    // setCaraSelect("");
    fetchInformacionCliente(caraSelect);
  };

  const handleChangeTercero = (event) => {
    const tempTercero = { ...tercero, [event.target.name]: event.target.value };
    const tempFactura = { ...ultimaFactura, tercero: tempTercero };
    setTercero(tempTercero);
    setUltimaFactura(tempFactura);
  };

  const handleChangeIdentificacion = async (event) => {
    const nuevaIdentificacion = event.target.value;
    setIdentificacion(nuevaIdentificacion);
    let nuevoTercero = await GetTercero(nuevaIdentificacion);

    setTerceroBusqueda(nuevoTercero);

    if (nuevoTercero.length > 0) {
      setTercero(nuevoTercero[0]);
      const tempFactura = { ...ultimaFactura, tercero: nuevoTercero[0] };
      setUltimaFactura(tempFactura);
      // setIdentificacion(nuevoTercero[0].identificacion);
      setShowTerceroNoExiste(false);
    } else {
      // Actualiza identificacion aquí
    }
  };

  const handleEstadoFactura = async (factura) => {
    if (
      window.ValidaFormasDePago &&
      window.FormasPagos &&
      window.FormasPagos.includes(factura.codigoFormaPago)
    ) {
      let formasPago = await GetFormasDePago();
      setformasDePago(
        formasPago.filter((f) => window.FormasPagos.includes(f.id))
      );
    } else {
      let formasPago = await GetFormasDePago();
      setformasDePago(formasPago);
    }

    // Solo bloquear si la transacción existe Y está finalizada
    if (
      factura.estadoTransaccion === "finalizada"
    ) {
      setDisableNumeroTrans(true);
      setDisableForma(true);
    } else {
      if (
        factura.codigoFormaPago == 1 ||
        factura.codigoFormaPago == 2 ||
        factura.codigoFormaPago == 3
      ) {
        setDisableNumeroTrans(false);
      } else {
        setDisableNumeroTrans(true);
      }
      setDisableForma(false);
    }
    if ((window.BloqueaCredito && factura.codigoFormaPago == 6) || factura.Enviada) {
      setUsuarioBloqueado(true);
    } else {
      setUsuarioBloqueado(false);
    }
  };

  // Función para verificar el estado de la transacción
  const verificarEstadoTransaccion = async (numeroTransaccion) => {
    // Implementa tu lógica aquí para determinar si la transacción está finalizada
    // Ejemplo: verificar por longitud o formato
    if (numeroTransaccion && numeroTransaccion.length >= 8) {
      return true; // Considera finalizada si tiene 8+ caracteres
    }

    // Aquí puedes agregar más lógica según tus criterios:
    // - Consultar una API
    // - Verificar contra una lista predefinida
    // - Verificar por patrón específico

    return false; // Por defecto no está finalizada
  };

  // Función específica para manejar el número de transacción
  const handleBlurNumeroTransaccion = async (event) => {
    const numeroTransaccion = event.target.value;

    // Actualizar la factura con el nuevo número de transacción
    const tempFactura = {
      ...ultimaFactura,
      numeroTransaccion: numeroTransaccion,
    };

    // Si hay número de transacción, verificar si está finalizada
    if (numeroTransaccion && numeroTransaccion.trim() !== "") {
      const estaFinalizada = await verificarEstadoTransaccion(
        numeroTransaccion
      );
      tempFactura.estadoTransaccion = estaFinalizada
        ? "finalizada"
        : "pendiente";
    } else {
      tempFactura.estadoTransaccion = null;
    }

    setUltimaFactura(tempFactura);
    await handleEstadoFactura(tempFactura);
  };

  const handleChangeFactura = async (event) => {
    if (event.target.name === "placa" && modoPlaca === "PLACA") {
      // Permitir que el input se actualice siempre, validando solo al guardar
      const value = event.target.value.toUpperCase();
      const tempFactura = { ...ultimaFactura, placa: value };
      setUltimaFactura(tempFactura);
      // Validar formato y mostrar error visual
      const regex = /^[A-Z]{3}[0-9]{3}$/;
      if (value.length === 6 && !regex.test(value)) {
        setPlacaError("Formato inválido. Ejemplo: ABC123");
      } else {
        setPlacaError("");
      }
    } else if (event.target.name === "placa" && modoPlaca === "PALABRA") {
      // Solo permitir palabras de la lista
      const value = event.target.value;
      if (window.palabrasPermitidas.includes(value)) {
        const tempFactura = { ...ultimaFactura, placa: value };
        setUltimaFactura(tempFactura);
      }
    } else if (
      event.target.name != "codigoFormaPago" ||
      !window.DesabilitaFormasNoCredito
    ) {
      const tempFactura = {
        ...ultimaFactura,
        [event.target.name]: event.target.value,
      };
      setUltimaFactura(tempFactura);
      await handleEstadoFactura(tempFactura);
    }
  };

  const fetcInicial = async () => {
    let islas = await GetIslas();
    setIslas(islas);

    let tiposDeIdentificacion = await GetTiposDeIdentificacion();
    setTiposDeIdentificacion(tiposDeIdentificacion);

    await handleEstadoFactura(ultimaFactura);
    GetLocalStorage();
  };

  const fetcTurnoYCaras = async (idIsla) => {
    let turno = await GetTurnoIsla(idIsla);
    setTurno(turno);

    localStorage.setItem("turno", JSON.stringify(turno));
    localStorage.setItem("empleado", JSON.stringify(turno.empleado));
    let caras = await GetCarasPorIsla(idIsla);
    setCaras(caras);
    localStorage.setItem("caras", JSON.stringify(caras));
  };

  const cerrarTurno = async (isla, codigo) => {
    let respuesta = await CerrarTurno(isla, codigo);
    if (respuesta === "fail") {
      handleSetShowAlertError(true);
    } else {
      if (window.imprimirNativo) {
        await ImprimirNativo(respuesta);
      }
    }
  };

  const fetchInformacionCliente = async (idCara) => {
    let factura = await GetUltimaFacturaPorCara(idCara);
    if (factura) {
      setUltimaFactura(factura);
      setTercero(factura.tercero);
      setIdentificacion(factura.tercero.identificacion);

      await handleEstadoFactura(factura);
    }

    let facturaTexto = await GetUltimaFacturaPorCaraTexto(idCara);
    setUltimaFacturaTexto(facturaTexto);
  };
  useEffect(() => {
    fetcInicial();
  }, []);

  return (
    <div className="contenedor-principal p-0">
      <div className="col-4 col-md-5 pt-4 pb-4 left-column columnas custom-style">
        <div className="d-flex flex-row isla-div">
          <div className="rombo">
            <div></div>
          </div>
          <label className="mx-2 d-inline text-white title-isla">ISLAS</label>
          <select
            className="form-select dark-blue-input d-inline w-50 h-50 text-select-list"
            aria-label="Default select example"
            value={islaSelect}
            onChange={(event) => {
              const selectIsla = event.target.value;
              setIslaSelect(selectIsla);
              localStorage.setItem("islaSelect", selectIsla);
              fetcTurnoYCaras(selectIsla);
              const selectedName =
                event.target.options[event.target.selectedIndex].getAttribute(
                  "data-name"
                );
              setIslaSelectName(selectedName);
              localStorage.setItem(
                "islaSelectName",
                JSON.stringify(selectedName)
              );
            }}
          >
            <option value="">Selecciona la isla</option>

            {Array.isArray(islas) &&
              islas.map((isla) => (
                <option key={isla.id} value={isla.id} data-name={isla.isla}>
                  {isla.isla}
                </option>
              ))}
          </select>
        </div>
        <div className="info-div ">
          <div className="text-white info-isla-div">
            <div className="row info-turno-div pt-2">
              <div className="col-4 turno-xs">
                <p className="text-end texto-turno">Turno: </p>
                <p className="text-end texto-turno">Empleado:</p>
              </div>
              <div className="col-7 turno-xs-info">
                <p className="texto-turno">
                  {turno === null || turno === "" ? "N/A" : turno.fechaApertura}{" "}
                </p>
                <p className="texto-turno">
                  {turno === null || turno === "" ? "N/A" : turno.empleado}
                </p>
              </div>
            </div>
            <div className="d-flex flex-row ms-3 info-cara-xs">
              <label className="mx-3 d-inline  titulo-informacion titulo-cara-xs">
                Cara
              </label>
              <select
                className="form-select d-inline w-50 altura-select select-white-blue text-select-list select-cara-xs"
                aria-label="Default select example"
                value={caraSelect}
                onChange={(event) => {
                  const selectCara = event.target.value;
                  setCaraSelect(selectCara);
                  fetchInformacionCliente(selectCara);
                }}
              >
                <option value="">Selecciona la Cara</option>
                {Array.isArray(caras) &&
                  caras.map((cara) => (
                    <option key={cara.id} value={cara.id}>
                      {cara.descripcion}
                    </option>
                  ))}
              </select>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="d-flex div-titulo-info-cliente titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="titulo-informacion text-white">
                Información del Cliente
              </div>
            </div>

            <form className="div-tercero-identificacion">
              <select
                className="form-select w-100 h-50 select-white-blue text-select-list"
                name="tipoIdentificacion"
                value={ultimaFactura.tercero.tipoIdentificacion || ""}
                onChange={handleChangeTercero}
                disabled={bloqueado}
                onkeydown="return /[a-zA-Z0-9]/i.test(event.key)"
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
              <div className="mt-2 p-0">
                <input
                  type="text"
                  className="form-control dark-blue-input w-100 input-identificacion "
                  placeholder="Identificación"
                  name="identificacion"
                  value={identificacion || ""}
                  onkeydown="return /[a-zA-Z0-9]/i.test(event.key)"
                  onChange={handleChangeIdentificacion}
                  onBlur={onBlurTercero}
                  disabled={bloqueado}
                ></input>
                <AlertTercero
                  showTerceroNoExiste={showTerceroNoExiste}
                  handleShowTerceroNoExiste={handleShowTerceroNoExiste}
                  handleNoCambiarTercero={handleNoCambiarTercero}
                  handleShowAddTercero={handleShowAddTercero}
                ></AlertTercero>
              </div>
            </form>
            <div className="mt-2 formulario-datos-cliente">
              <div className="form-control dark-blue-input input-datos-cliente d-flex">
                <div className="col-3 me-1 datos-cliente-xs">
                  <p className="text-end texto-datos-cliente">Nombre: </p>
                  <p className="text-end texto-datos-cliente">Teléfono:</p>
                  <p className="text-end texto-datos-cliente">Correo:</p>
                  <p className="text-end texto-datos-cliente">Dirección:</p>
                </div>
                <div className="col-8 datos-cliente">
                  <p className="texto-datos-cliente">
                    {ultimaFactura.tercero.nombre || ""}
                  </p>
                  <p className="texto-datos-cliente">
                    {ultimaFactura.tercero.telefono || ""}
                  </p>
                  <p className="texto-datos-cliente">
                    {ultimaFactura.tercero.correo || ""}
                  </p>
                  <p className="texto-datos-cliente">
                    {ultimaFactura.tercero.direccion || ""}
                  </p>
                </div>
              </div>
            </div>
          </div>
          <div className="info-venta-div">
            <div className="d-flex titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="titulo-informacion text-white">
                Información de la Venta
              </div>
            </div>

            <div className="info-venta-div d-flex flex-column ">
              <div className="div-info-venta ">
                <label className="label-info-venta ">Forma de Pago</label>
                <select
                  className="form-select  w-75 altura-select select-white-blue text-select-list"
                  aria-label="Default select example"
                  name="codigoFormaPago"
                  disabled={disableForma || bloqueado}
                  value={ultimaFactura.codigoFormaPago || ""}
                  onChange={handleChangeFactura}
                >
                  {Array.isArray(formasDePago) &&
                    formasDePago.map((forma) => (
                      <option key={forma.id} value={forma.id}>
                        {forma.descripcion}
                      </option>
                    ))}
                </select>
              </div>
              <div className="div-info-venta ">
                <label className="label-info-venta ">N. trans</label>
                <input
                  type="text"
                  className="form-control select-white-blue w-75 altura-select text-select-list"
                  name="numeroTransaccion"
                  value={ultimaFactura.numeroTransaccion || ""}
                  disabled={disableNumeroTrans || bloqueado}
                  onChange={(event) => {
                    // Solo actualizar el valor sin validaciones
                    const tempFactura = {
                      ...ultimaFactura,
                      numeroTransaccion: event.target.value,
                    };
                    setUltimaFactura(tempFactura);
                  }}
                  onBlur={handleBlurNumeroTransaccion}
                  placeholder="Ingrese número de transacción"
                ></input>
              </div>
              <div className="div-info-venta ">
                <label className="label-info-venta ">Placa</label>
                <div className="d-flex flex-row align-items-center">
                  <select
                    className="form-select w-25 me-2"
                    value={modoPlaca}
                    onChange={(e) => setModoPlaca(e.target.value)}
                  >
                    <option value="PLACA">PLACA</option>
                    <option value="PALABRA">PALABRA</option>
                  </select>
                  {modoPlaca === "PLACA" ? (
                    <div className="w-100">
                      <input
                        type="text"
                        className={`form-control select-white-blue w-50 altura-select text-select-list ${
                          placaError ? "is-invalid" : ""
                        }`}
                        name="placa"
                        disabled={bloqueado}
                        value={ultimaFactura.placa || ""}
                        maxLength={6}
                        placeholder="ABC123"
                        onChange={handleChangeFactura}
                      />
                      {placaError && (
                        <div
                          className="invalid-feedback d-block"
                          style={{ fontSize: "0.9em" }}
                        >
                          {placaError}
                        </div>
                      )}
                    </div>
                  ) : (
                    <select
                      className="form-select select-white-blue w-50 altura-select text-select-list"
                      name="placa"
                      disabled={bloqueado}
                      value={
                        window.palabrasPermitidas.includes(ultimaFactura.placa)
                          ? ultimaFactura.placa
                          : ""
                      }
                      onChange={handleChangeFactura}
                    >
                      <option value="">Seleccione palabra</option>
                      {window.palabrasPermitidas.map((palabra) => (
                        <option key={palabra} value={palabra}>
                          {palabra}
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              </div>
              <div className="div-info-venta ">
                <label className="label-info-venta ">Kilometraje</label>
                <input
                  type="text"
                  className="form-control select-white-blue w-75 altura-select text-select-list"
                  name="kilometraje"
                  disabled={bloqueado}
                  value={ultimaFactura.kilometraje || ""}
                  onChange={handleChangeFactura}
                ></input>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 col-md-5 center-column columnas custom-style">
        <div className="container container-factura contenedor-factura">
          <div className=" factura px-2 w-100 h-100">
            <p className="texto-factura">
              {ultimaFacturaTexto ? ultimaFacturaTexto : " "}
            </p>
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <ModalImprimir
            ultimaFactura={ultimaFactura}
            handleShowFacturaElectrónica={handleShowFacturaElectrónica}
            handleSetUltimaFactura={handleSetUltimaFactura}
            handleSetShowAlertError={handleSetShowAlertError}
          ></ModalImprimir>
          <ModalFacturaElectronica
            handleCloseFacturaElectronica={handleCloseFacturaElectronica}
            showFacturaElectronica={showFacturaElectronica}
            ultimaFactura={ultimaFactura}
            getFacturaInformacion={getFacturaInformacion}
            handleSetShowAlertError={handleSetShowAlertError}
          ></ModalFacturaElectronica>
          <ModalAddTercero
            showAddTercero={showAddTercero}
            handleShowAddTercero={handleShowAddTercero}
            identificacionActualizada={identificacion}
            tiposDeIdentificacion={tiposDeIdentificacion}
            handleNoCambiarTercero={handleNoCambiarTercero}
            handleSetShowAlertError={handleSetShowAlertError}
            handleSetTerceroModalAddTercero={handleSetTerceroModalAddTercero}
          ></ModalAddTercero>
        </div>
      </div>
      <div className="col-3 col-md-2 right-column columnas  ">
        <div className="d-flex  flex-column button-container custom-style">
          <ModalImprimirPorConsecutivo
            handleSetShowAlertError={handleSetShowAlertError}
          ></ModalImprimirPorConsecutivo>
          {(turno === null || turno === "") && (
            <ModalAbrirTurno
              islaSelect={islaSelect}
              islaSelectName={islaSelectName}
              codigoEmpleado={codigoEmpleado}
              handleChangeCodigoEmpleado={handleChangeCodigoEmpleado}
              handleSetShowAlertError={handleSetShowAlertError}
            ></ModalAbrirTurno>
          )}
          {turno && (
            <ModalCerrarTurno
              islaSelect={islaSelect}
              islaSelectName={islaSelectName}
              cerrarTurno={cerrarTurno}
            ></ModalCerrarTurno>
          )}
          {turno && (
            <ModalAgregarBolsa
              islaSelect={islaSelect}
              islaSelectName={islaSelectName}
              handleSetShowAlertError={handleSetShowAlertError}
            ></ModalAgregarBolsa>
          )}
          {turno && (
            <ModalFidelizarVenta
              handleSetShowAlertError={handleSetShowAlertError}
              ventaId={ultimaFactura.ventaId}
              getFacturaInformacion={getFacturaInformacion}
            ></ModalFidelizarVenta>
          )}
          <ModalReimprimirTurno
            islaSelect={islaSelect}
            islaSelectName={islaSelectName}
            handleSetShowAlertError={handleSetShowAlertError}
          ></ModalReimprimirTurno>
        </div>
      </div>
      <AlertError
        showAlertError={showAlertError}
        handleSetShowAlertError={handleSetShowAlertError}
      ></AlertError>
    </div>
  );
};

export default Combustible;
