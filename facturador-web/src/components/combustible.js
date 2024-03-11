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

const Combustible = () => {
  const [showAlertError, setShowAlertError] = useState(false);
  const handleSetShowAlertError = (show) => setShowAlertError(show);
  const [codigoEmpleado, setCodigoEmpleado] = useState("");
  const handleChangeCodigoEmpleado = (codigo) => setCodigoEmpleado(codigo);
  const [showAddTercero, setShowAddTercero] = useState(false);
  const handleShowAddTercero = (show) => setShowAddTercero(show);

  const [identificacion, setIdentificacion] = useState("");
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
  function handleSetTerceroModalAddTercero(newTercero) {
    setTercero(newTercero);
    const tempFactura = { ...ultimaFactura, tercero: newTercero };
    setUltimaFactura(tempFactura);
    setIdentificacion(newTercero.identificacion);
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

  const handleSetUltimaFactura = (factura) => {
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

  const handleChangeFactura = (event) => {
    const tempFactura = {
      ...ultimaFactura,
      [event.target.name]: event.target.value,
    };
    setUltimaFactura(tempFactura);
  };

  const fetcInicial = async () => {
    let islas = await GetIslas();
    setIslas(islas);

    let tiposDeIdentificacion = await GetTiposDeIdentificacion();
    setTiposDeIdentificacion(tiposDeIdentificacion);

    let formasPago = await GetFormasDePago();
    setformasDePago(formasPago);
    GetLocalStorage();
  };

  const fetcTurnoYCaras = async (idIsla) => {
    let turno = await GetTurnoIsla(idIsla);
    setTurno(turno);

    localStorage.setItem("turno", JSON.stringify(turno));
    let caras = await GetCarasPorIsla(idIsla);
    setCaras(caras);
    localStorage.setItem("caras", JSON.stringify(caras));
  };

  const cerrarTurno = async (isla, codigo) => {
    let respuesta = await CerrarTurno(isla, codigo);
    if (respuesta === "fail") {
      handleSetShowAlertError(true);
    }
  };

  const fetchInformacionCliente = async (idCara) => {
    let factura = await GetUltimaFacturaPorCara(idCara);
    if (factura) {
      setUltimaFactura(factura);
      setTercero(factura.tercero);
      setIdentificacion(factura.tercero.identificacion);
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
                  onChange={handleChangeIdentificacion}
                  onBlur={onBlurTercero}
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
                  value={ultimaFactura.codigoFormaPago || ""}
                  onChange={handleChangeFactura}
                >
                  <option value=""></option>
                  {Array.isArray(formasDePago) &&
                    formasDePago.map((forma) => (
                      <option key={forma.id} value={forma.id}>
                        {forma.descripcion}
                      </option>
                    ))}
                </select>
              </div>
              <div className="div-info-venta ">
                <label className="label-info-venta ">Placa</label>
                <input
                  type="text"
                  className="form-control select-white-blue w-75 altura-select text-select-list"
                  name="placa"
                  value={ultimaFactura.placa || ""}
                  onChange={handleChangeFactura}
                ></input>
              </div>
              <div className="div-info-venta ">
                <label className="label-info-venta ">Kilometraje</label>
                <input
                  type="text"
                  className="form-control select-white-blue w-75 altura-select text-select-list"
                  name="kilometraje"
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
