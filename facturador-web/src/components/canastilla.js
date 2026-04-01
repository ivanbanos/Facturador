import React, { useState, useEffect, useRef } from "react";
import "./styles/home.css";
import GetCanastilla from "../Services/getServices/GetCanastilla";
import GetTercero from "../Services/getServices/GetTercero";
import PostCanastilla from "../Services/getServices/PostCanastilla";
import PostImprimirTurnoCanastilla from "../Services/getServices/PostImprimirTurnoCanastilla";
import GetTiposDeIdentificacion from "../Services/getServices/GetTiposDeIdentificacion";
import GetFormasDePago from "../Services/getServices/GetFormasDePago";
import AlertTerceroNoExisteCnastilla from "./alertTerceroNoExisteCanastilla";
import ModalAddTercero from "./modalAddTercero";
import AlertError from "./alertaError";
import AlertVentaExitosa from "./AlertVentaExitosa";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";
import ModalReimprimirFacturaCanastilla from "./modalReimprimirFacturaCanastilla";
import ModalReimprimirTurnoCanastilla from "./modalReimprimirTurnoCanastilla";
import ModalAgregarFormaPago from "./modalAgregarFormaPago";

const Canastilla = () => {
  const habilitarSegundaFormaPago = window.HabilitarSegundaFormaPago !== false;
  const placaObligatoriaCanastillaCredito =
    window.PlacaObligatoriaCanastillaCredito === undefined ||
    window.PlacaObligatoriaCanastillaCredito === true ||
    String(window.PlacaObligatoriaCanastillaCredito).toLowerCase() ===
      "true";

  // Estado para turno y empleado
  const [turno, setTurno] = useState(null);

  const [productos, setProductos] = useState([]);
  const [filtroProducto, setFiltroProducto] = useState(""); // Estado para el filtro
  const [productosFiltrados, setProductosFiltrados] = useState([]); // Productos filtrados
  const valorInicialObjetoPostCanastilla = {
    terceroId: 0,
    codigoFormaPago: 4,
    numeroTransaccion: "",
    descuento: 0,
    vendedor: 0,
    isla: 0,
    placa: "",
    canastillas: [],
  };
  const [objetoPostCanastilla, setObjetoPostCanastilla] = useState(
    valorInicialObjetoPostCanastilla
  );
  const [canastillas, setCanastillas] = useState([]);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [cantidadSeleccionada, setCantidadSeleccionada] = useState(0);
  const valorInicialObjetoCanastillas = {
    canastillaId: 0,
    canastillaGuid: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    descripcion: "string",
    unidad: "string",
    precio: 0,
    deleted: "string",
    cantidad: 0,
    iva: 0,
  };
  const [objetoCanastillas, setObjetoCanastillas] = useState(
    valorInicialObjetoCanastillas
  );
  const valorInicialTercero = {
    terceroId: 0,
    coD_CLI: "",
    nombre: "",
    apellidos: "",
    telefono: "",
    direccion: "",
    identificacion: "222222222222",
    correo: "",
    tipoIdentificacion: 1,
  };
  const [tercero, setTercero] = useState(valorInicialTercero);
  const [showAlertError, setShowAlertError] = useState(false);
  const handleSetShowAlertError = (show) => setShowAlertError(show);
  const [identificacion, setIdentificacion] = useState("");
  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const [formasDePago, setformasDePago] = useState([]);
  const [terceroBusqueda, setTerceroBusqueda] = useState([{}]);
  const terceroRequestVersion = useRef(0);
  const handleChangeIdentificacion = async (event) => {
    const nuevaIdentificacion = event.target.value;
    setIdentificacion(nuevaIdentificacion);
    const requestVersion = ++terceroRequestVersion.current;
    let nuevoTercero = await GetTercero(nuevaIdentificacion);
    if (requestVersion !== terceroRequestVersion.current) {
      return;
    }
    setTerceroBusqueda(nuevoTercero);
    if (nuevoTercero.length > 0) {
      setTercero(nuevoTercero[0]);
      const tempObjetoPostCanastilla = {
        ...objetoPostCanastilla,
        terceroId: nuevoTercero[0].terceroId,
      };
      setObjetoPostCanastilla(tempObjetoPostCanastilla);

      // setShowTerceroNoExiste(false);
    } else {
    }
  };
  function onBlurTercero() {
    if (terceroBusqueda.length === 0) {
      setShowTerceroNoExiste(true);
    } else setShowTerceroNoExiste(false);
  }
  const [showTerceroNoExiste, setShowTerceroNoExiste] = useState(false);
  function handleShowTerceroNoExiste(showTerceroNoExiste) {
    setShowTerceroNoExiste(showTerceroNoExiste);
  }
  function handleNoCambiarTercero() {
    setIdentificacion("222222222222");
  }
  const [showAlertVentaExitosa, setShowAlertVentaExitosa] = useState(false);
  const [mensajeAlerta, setMensajeAlerta] = useState("Venta Generada Exitosamente");
  const handleSetShowAlertVentaExitosa = (show) =>
    setShowAlertVentaExitosa(show);
  const [showModalSegundaFormaPago, setShowModalSegundaFormaPago] =
    useState(false);
  const [showAddTercero, setShowAddTercero] = useState(false);
  const handleShowAddTercero = (show) => setShowAddTercero(show);
  const handleCloseModalSegundaFormaPago = () =>
    setShowModalSegundaFormaPago(false);
  const handleShowModalSegundaFormaPago = () => {
    if (!habilitarSegundaFormaPago) {
      return;
    }
    setShowModalSegundaFormaPago(true);
  };
  function handleSetTerceroModalAddTercero(newTercero) {
    setTercero(newTercero);
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      terceroId: newTercero.terceroId,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);

    setIdentificacion(newTercero.identificacion);
  }
  const [totalItems, setTotalItems] = useState(0);
  const [subTotal, setSubTotal] = useState(0);
  const [isGeneratingVenta, setIsGeneratingVenta] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [isPrintingTurno, setIsPrintingTurno] = useState(false);

  const isActionInProgress =
    isGeneratingVenta || isResetting || isPrintingTurno;

  const limpiarEstadoCanastilla = (terceroActual) => {
    setSubTotal(0);
    setTotalItems(0);
    setObjetoCanastillas(valorInicialObjetoCanastillas);
    setCanastillas([]);
    setCantidadSeleccionada(0);
    setProductoSeleccionado(null);
    setFiltroProducto("");
    setProductosFiltrados(productos);

    setObjetoPostCanastilla({
      ...valorInicialObjetoPostCanastilla,
      terceroId: terceroActual?.terceroId || 0,
    });
  };

  const restaurarEstadoCanastilla = (snapshot) => {
    setSubTotal(snapshot.subTotal);
    setTotalItems(snapshot.totalItems);
    setCanastillas(snapshot.canastillas);
    setObjetoPostCanastilla(snapshot.objetoPostCanastilla);
    setCantidadSeleccionada(snapshot.cantidadSeleccionada);
    setProductoSeleccionado(snapshot.productoSeleccionado);
    setFiltroProducto(snapshot.filtroProducto);
    setProductosFiltrados(snapshot.productosFiltrados);
  };

  const calcularTotalesCanastilla = (items, descuento = 0) => {
    if (!Array.isArray(items) || items.length === 0) {
      return { subtotal: 0, iva: 0, total: 0 };
    }

    const usaIvaPorcentaje =
      Math.max(...items.map((item) => Number(item.iva || 0))) < 30;
    const subtotal = items.reduce(
      (acumulado, item) =>
        acumulado + Number(item.precio || 0) * Number(item.cantidad || 0),
      0
    );
    const iva = items.reduce((acumulado, item) => {
      const cantidad = Number(item.cantidad || 0);
      const precio = Number(item.precio || 0);
      const valorIva = Number(item.iva || 0);

      if (usaIvaPorcentaje) {
        return acumulado + precio * cantidad * (valorIva / 100);
      }

      return acumulado + cantidad * valorIva;
    }, 0);

    return {
      subtotal,
      iva,
      total: subtotal + iva - Number(descuento || 0),
    };
  };

  const handleGuardarSegundaFormaPago = ({
    codigoFormaPago2,
    total2,
    total1,
  }) => {
    if (!habilitarSegundaFormaPago) {
      return;
    }

    setObjetoPostCanastilla((actual) => ({
      ...actual,
      codigoFormaPago2,
      total2,
      total1,
    }));
  };

  // Función para filtrar productos por descripción
  const handleFiltroProducto = (event) => {
    const filtro = event.target.value;
    setFiltroProducto(filtro);

    if (filtro === "") {
      setProductosFiltrados(productos);
    } else {
      const productosFiltrados = productos.filter((producto) =>
        producto.descripcion.toLowerCase().includes(filtro.toLowerCase())
      );
      setProductosFiltrados(productosFiltrados);
    }
    
    // Limpiar selección si el producto seleccionado ya no está en la lista filtrada
    if (productoSeleccionado && filtro !== "") {
      const productoEnFiltro = productos.filter((producto) =>
        producto.descripcion.toLowerCase().includes(filtro.toLowerCase())
      ).find(p => p.canastillaId === productoSeleccionado.canastillaId);
      
      if (!productoEnFiltro) {
        setProductoSeleccionado(null);
      }
    }
  };

  function onClickAgregarProducto() {
    if (productoSeleccionado) {
      let tempObjetoCanastillas = {
        canastillaId: productoSeleccionado.canastillaId,
        canastillaGuid: productoSeleccionado.guid,
        descripcion: productoSeleccionado.descripcion,
        unidad: productoSeleccionado.unidad,
        precio: productoSeleccionado.precio,
        deleted: "string",
        cantidad: cantidadSeleccionada,
        iva: productoSeleccionado.iva,
      };
      // let tempObjetoCanastillas = {
      //   ...objetoCanastillas,
      //   canastilla: productoSeleccionado,
      //   cantidad: cantidadSeleccionada,
      // };

      setObjetoCanastillas(tempObjetoCanastillas);

      let tempCanastillas = [...canastillas];
      tempCanastillas.push(tempObjetoCanastillas);
      setCanastillas(tempCanastillas);
      let tempObjetoPostCanastilla = {
        ...objetoPostCanastilla,
        canastillas: tempCanastillas,
      };
      setObjetoPostCanastilla(tempObjetoPostCanastilla);

      setCantidadSeleccionada(0);
      setProductoSeleccionado(null);
      // Limpiar filtro después de agregar producto
      setFiltroProducto("");
      setProductosFiltrados(productos);
      
      let counterTotalItems = 0;
      let counterSubTotal = 0;
      for (let item of tempCanastillas) {
        counterTotalItems += item.cantidad;
        counterSubTotal += item.precio * item.cantidad;
      }
      setTotalItems(counterTotalItems);
      setSubTotal(counterSubTotal);
    }
  }
  const handleChangeFormaPago = (event) => {
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      codigoFormaPago: event.target.value,
    };

    if (
      Number(tempObjetoPostCanastilla.codigoFormaPago2) ===
      Number(event.target.value)
    ) {
      tempObjetoPostCanastilla.codigoFormaPago2 = null;
      tempObjetoPostCanastilla.total1 = null;
      tempObjetoPostCanastilla.total2 = null;
    }

    setObjetoPostCanastilla(tempObjetoPostCanastilla);
  };
  const handleChangePlaca = (event) => {
    const placaNormalizada = event.target.value
      .toUpperCase()
      .replace(/[^A-Z0-9]/g, "")
      .slice(0, 6);
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      placa: placaNormalizada,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);
  };

  const handleChangeNumeroTransaccion = (event) => {
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      numeroTransaccion: event.target.value,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);
  };

  const esPlacaColombianaValida = (placa) => {
    const placaLimpia = (placa || "").trim().toUpperCase();
    return /^(?:[A-Z]{3}\d{3}|[A-Z]{3}\d{2}[A-Z])$/.test(placaLimpia);
  };
  const onClickImprimirTurno = async () => {
    if (isPrintingTurno) {
      return;
    }

    setIsPrintingTurno(true);
    try {
      const respuesta = await PostImprimirTurnoCanastilla(
        localStorage.getItem("islaSelect")
      );
      if (respuesta === "fail") {
        handleSetShowAlertError(true);
      } else {
        setMensajeAlerta("Turno mandado a imprimir con éxito");
        handleSetShowAlertVentaExitosa(true);
      }
    } finally {
      setIsPrintingTurno(false);
    }
  };
  const onClickGenerarVenta = async (canastilla) => {
    if (isGeneratingVenta) {
      return;
    }

    const islaSeleccionada = localStorage.getItem("islaSelect");
    if (!islaSeleccionada || islaSeleccionada === "") {
      alert("Debe seleccionar una isla/cara antes de generar la venta.");
    } else if (!canastilla.terceroId || Number(canastilla.terceroId) <= 0) {
      alert("Debe seleccionar un tercero válido antes de generar la venta.");
    } else if (canastilla.codigoFormaPago == 0) {
      handleSetShowAlertError(true);
    } else if (
      placaObligatoriaCanastillaCredito &&
      (canastilla.codigoFormaPago == 6 || canastilla.codigoFormaPago2 == 6) &&
      (!canastilla.placa || canastilla.placa.trim() === "")
    ) {
      alert("La placa es obligatoria cuando la forma de pago es crédito.");
    } else if (
      canastilla.placa &&
      canastilla.placa.trim() !== "" &&
      !esPlacaColombianaValida(canastilla.placa)
    ) {
      alert("La placa debe tener formato colombiano válido (ej: ABC123 o ABC12D).");
    } else {
      const ventaPayload = {
        ...canastilla,
        isla: localStorage.getItem("islaSelect"),
        empleado: localStorage.getItem("empleado"),
        canastillas: Array.isArray(canastilla.canastillas)
          ? [...canastilla.canastillas]
          : [],
      };

      const snapshotCanastilla = {
        subTotal,
        totalItems,
        canastillas,
        objetoPostCanastilla,
        cantidadSeleccionada,
        productoSeleccionado,
        filtroProducto,
        productosFiltrados,
      };

      setIsGeneratingVenta(true);
      limpiarEstadoCanastilla({ terceroId: canastilla.terceroId });

      try {
        const respuesta = await PostCanastilla(ventaPayload);
        if (respuesta === "fail") {
          restaurarEstadoCanastilla(snapshotCanastilla);
          handleSetShowAlertError(true);
          return;
        }

        if (window.imprimirNativo) {
          await ImprimirNativo(respuesta);
        }
        setMensajeAlerta("Venta Generada Exitosamente");
        handleSetShowAlertVentaExitosa(true);
        await resetValues();
      } finally {
        setIsGeneratingVenta(false);
      }
    }
    // setObjetoPostCanastilla(valorInicialObjetoPostCanastilla)
  };
  const resetValues = async () => {
    if (isResetting) {
      return;
    }

    setIsResetting(true);
    setSubTotal(0);
    setTotalItems(0);
    setObjetoCanastillas(valorInicialObjetoCanastillas);
    setCanastillas([]);
    setObjetoPostCanastilla(valorInicialObjetoPostCanastilla);
    setTercero(valorInicialTercero);
    setTerceroBusqueda([]);
    setShowTerceroNoExiste(false);
    setCantidadSeleccionada(0);
    setProductoSeleccionado(null);
    setFiltroProducto("");
    setProductosFiltrados(productos);

    try {
      setIdentificacion("222222222222");
      const requestVersion = ++terceroRequestVersion.current;
      let nuevoTercero = await GetTercero("222222222222");
      if (requestVersion !== terceroRequestVersion.current) {
        return;
      }
      if (nuevoTercero.length > 0) {
        setTercero(nuevoTercero[0]);
        setTerceroBusqueda(nuevoTercero);
        const tempObjetoPostCanastilla = {
          ...valorInicialObjetoPostCanastilla,
          terceroId: nuevoTercero[0].terceroId,
        };
        setObjetoPostCanastilla(tempObjetoPostCanastilla);

        // setShowTerceroNoExiste(false);
      }
    } finally {
      setIsResetting(false);
    }
  };
  useEffect(() => {
    const fetchData = async () => {
      try {
        let productos = await GetCanastilla();
        setProductos(productos);
        setProductosFiltrados(productos); // Inicializar productos filtrados
        let tiposDeIdentificacion = await GetTiposDeIdentificacion();
        setTiposDeIdentificacion(tiposDeIdentificacion);
        let formasPago = await GetFormasDePago();
        setformasDePago(formasPago);
        setIdentificacion("222222222222");
        const requestVersion = ++terceroRequestVersion.current;
        let nuevoTercero = await GetTercero("222222222222");
        if (requestVersion !== terceroRequestVersion.current) {
          return;
        }
        setTerceroBusqueda(nuevoTercero);
        if (nuevoTercero.length > 0) {
          setTercero(nuevoTercero[0]);
          const tempObjetoPostCanastilla = {
            ...objetoPostCanastilla,
            terceroId: nuevoTercero[0].terceroId,
          };
          setObjetoPostCanastilla(tempObjetoPostCanastilla);
        }
      } catch (error) {}
    };
    fetchData();
    // Obtener turno y empleado desde localStorage
    const turnoLocal = JSON.parse(localStorage.getItem("turno"));
    setTurno(turnoLocal);
  }, []);

  const resumenTotalesCanastilla = calcularTotalesCanastilla(
    objetoPostCanastilla.canastillas,
    objetoPostCanastilla.descuento
  );

  return (
    <div className="div-canastilla row">
      {/* Información de turno y empleado */}
      <div className="w-100">
        <div className="text-white info-isla-div">
          <div className="row info-turno-div pt-2">
            <div className="col-4 turno-xs">
              <p className="text-end texto-turno">Turno: </p>
              <p className="text-end texto-turno">Empleado:</p>
            </div>
            <div className="col-7 turno-xs-info">
              <p className="texto-turno">
                {turno === null || turno === "" ? "N/A" : turno.fechaApertura}
              </p>
              <p className="texto-turno">
                {turno === null || turno === "" ? "N/A" : turno.empleado}
              </p>
            </div>
          </div>
        </div>
      </div>
      <div className="col-4 pt-4 pb-4 left-column columnas custom-style-canastilla">
        <div className="info-div ">
          <div className="text-white">
            <label className="titulo-informacion text-white py-1 ms-2">
              AGREGAR PRODUCTO
            </label>
            <input
              type="text"
              className="form-control dark-blue-input my-2"
              placeholder="Buscar producto por descripción..."
              value={filtroProducto}
              onChange={handleFiltroProducto}
            />
            <select
              className="form-select d-inline w-80 altura-select select-white-blue text-select-list my-2 select-producto-xs"
              aria-label="Default select example"
              value={productoSeleccionado?.canastillaId || ""}
              onChange={(event) => {
                const selectedProductId = event.target.value;
                // Buscar en la lista filtrada primero, luego en la lista completa
                let selectedProduct = productosFiltrados.find(
                  (product) =>
                    product.canastillaId === parseFloat(selectedProductId, 10)
                );
                
                // Si no se encuentra en filtrados, buscar en la lista completa
                if (!selectedProduct) {
                  selectedProduct = productos.find(
                    (product) =>
                      product.canastillaId === parseFloat(selectedProductId, 10)
                  );
                }
                
                setProductoSeleccionado(selectedProduct);
              }}
            >
              <option value="">Selecciona el producto</option>
              {Array.isArray(productosFiltrados) &&
                productosFiltrados.map((elemento) => (
                  <option
                    key={elemento.canastillaId}
                    value={elemento.canastillaId}
                    name="canastilla"
                  >
                    {elemento.descripcion}
                  </option>
                ))}
            </select>
            <div className="d-flex flex-row">
              <label className="mx-3 d-inline titulo-informacion my-1">
                Cantidad
              </label>

              <input
                value={cantidadSeleccionada || ""}
                onChange={(event) => {
                  setCantidadSeleccionada(event.target.value);
                }}
                className="form-control altura-select select-white-blue text-select-list select-cantidad"
              />
            </div>
            <div className="d-flex justify-content-center">
              <button
                className="print-button botton-light-blue botton-agregar-producto"
                onClick={onClickAgregarProducto}
                disabled={isActionInProgress}
              >
                Agregar
              </button>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="titulo-informacion text-white my-2">
              Información del Cliente
            </div>

            <div className="mt-2 p-0">
              <div className=" ">
                <input
                  type="text"
                  className="form-control dark-blue-input my-2 "
                  placeholder="Identificación"
                  name="identificacion"
                  value={identificacion || ""}
                  onChange={handleChangeIdentificacion}
                  onBlur={onBlurTercero}
                  onkeydown="return /[a-zA-Z0-9]/i.test(event.key)"
                ></input>
                <AlertTerceroNoExisteCnastilla
                  showTerceroNoExiste={showTerceroNoExiste}
                  handleShowTerceroNoExiste={handleShowTerceroNoExiste}
                  handleShowAddTercero={handleShowAddTercero}
                  handleNoCambiarTercero={handleNoCambiarTercero}
                ></AlertTerceroNoExisteCnastilla>
              </div>
            </div>
            <div className="mt-2">
              <div className="form-control dark-blue-input input-datos-cliente-canastilla my-3">
                <p className="texto-datos-cliente-canastilla">
                  Nombre: {[tercero?.nombre, tercero?.apellidos]
                    .filter(Boolean)
                    .join(" ")}
                </p>
                <p className="texto-datos-cliente-canastilla">
                  Teléfono: {tercero?.telefono}{" "}
                </p>
                <p className="texto-datos-cliente-canastilla">
                  Correo: {tercero?.correo}
                </p>
                <p className="texto-datos-cliente-canastilla">
                  Dirección: {tercero?.direccion}
                </p>
              </div>
              <div className="info-venta-div d-flex flex-column w-100">
                <div className="div-info-venta-canastilla canastilla-venta-row">
                  <label className="label-info-venta-canastilla ">
                    Forma de Pago
                  </label>
                  <select
                    className="form-select altura-select select-white-blue text-select-list canastilla-venta-control"
                    aria-label="Default select example"
                    name="codigoFormaPago"
                    value={objetoPostCanastilla.codigoFormaPago || "0"}
                    onChange={handleChangeFormaPago}
                  >
                    <option key={0} value={0}>
                      Selecione forma de pago
                    </option>
                    {Array.isArray(formasDePago) &&
                      formasDePago.map((forma) => (
                        <option key={forma.id} value={forma.id}>
                          {forma.descripcion}
                        </option>
                      ))}
                  </select>
                  {habilitarSegundaFormaPago && (
                    <button
                      className="botton-light-blue-modal mt-2"
                      onClick={handleShowModalSegundaFormaPago}
                      disabled={isActionInProgress}
                    >
                      Agregar forma de pago
                    </button>
                  )}
                  {habilitarSegundaFormaPago &&
                    objetoPostCanastilla.codigoFormaPago2 && (
                      <small className="d-block text-white mt-1">
                        Forma 2: {objetoPostCanastilla.codigoFormaPago2} | Valor: {objetoPostCanastilla.total2 || 0}
                      </small>
                    )}
                </div>
                <div className="div-info-venta-canastilla mt-2 canastilla-venta-row">
                  <label className="label-info-venta-canastilla">
                    Placa{" "}
                    {placaObligatoriaCanastillaCredito &&
                      (objetoPostCanastilla.codigoFormaPago == 6 ||
                        objetoPostCanastilla.codigoFormaPago2 == 6) &&
                      <span style={{ color: "red" }}>*</span>}
                  </label>
                  <input
                    type="text"
                    className="form-control altura-select select-white-blue text-select-list canastilla-venta-control"
                    placeholder="Ingrese la placa"
                    name="placa"
                    value={objetoPostCanastilla.placa}
                    onChange={handleChangePlaca}
                    maxLength={6}
                  />
                </div>
                <div className="div-info-venta-canastilla mt-2 canastilla-venta-row">
                  <label className="label-info-venta-canastilla">
                    N transacción
                  </label>
                  <input
                    type="text"
                    className="form-control altura-select select-white-blue text-select-list canastilla-venta-control"
                    placeholder="Ingrese N transacción"
                    name="numeroTransaccion"
                    value={objetoPostCanastilla.numeroTransaccion || ""}
                    onChange={handleChangeNumeroTransaccion}
                    maxLength={50}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 center-column columnas custom-style-canastilla">
        <div className="container container-factura my-4">
          <div className=" factura px-2 h-100 texto-canastilla">
            <p>
              Vendido a: {[tercero?.nombre, tercero?.apellidos]
                .filter(Boolean)
                .join(" ")}
            </p>
            <p>Nit/CC: {tercero?.identificacion}</p>

            {objetoPostCanastilla.canastillas.length > 0 && (
              <p>PRODUCTOS AGREGADOS</p>
            )}
            {Array.isArray(objetoPostCanastilla.canastillas) &&
              objetoPostCanastilla.canastillas.map((elemento) => (
                <div key={elemento.canastillaId}>
                  <hr></hr>
                  <p>Producto: {elemento.descripcion}</p>
                  <p>Precio: {elemento.precio}</p>
                  <p>Cantidad: {elemento.cantidad}</p>
                </div>
              ))}
            <hr></hr>
            <p>Total Items: {totalItems}</p>
            <p>Subtotal: {subTotal}</p>
            <p>IVA: {resumenTotalesCanastilla.iva.toFixed(2)}</p>
            <p>Total: {resumenTotalesCanastilla.total.toFixed(2)}</p>
          </div>
        </div>
      </div>
      <div className="col-3  right-column columnas custom-style-canastilla">
        <div className="button-container1"></div>
        <div className=" align-items-center button-container-canastilla">
          <button
            className="botton-green m-3 right-botton right-botton-xs"
            onClick={() => {
              onClickGenerarVenta(objetoPostCanastilla);
            }}
            disabled={isActionInProgress}
          >
            <span className="">Generar</span> <span>Venta</span>
          </button>
          <button
            className="botton-medium-blue m-3 right-botton right-botton-xs"
            onClick={async () => {
              await resetValues();
            }}
            disabled={isActionInProgress}
          >
            <span>Borrar</span>
          </button>
          <button
            className="botton-green m-3 right-botton right-botton-xs"
            onClick={() => {
              onClickImprimirTurno();
            }}
            disabled={isActionInProgress}
          >
            <span className="">Imprimir ultimo</span> <span>turno</span>
          </button>
          <ModalReimprimirFacturaCanastilla
            handleSetShowAlertError={handleSetShowAlertError}
          />
          <ModalReimprimirTurnoCanastilla
            handleSetShowAlertError={handleSetShowAlertError}
          />
        </div>
        <ModalAddTercero
          showAddTercero={showAddTercero}
          handleShowAddTercero={handleShowAddTercero}
          tiposDeIdentificacion={tiposDeIdentificacion}
          handleSetTerceroModalAddTercero={handleSetTerceroModalAddTercero}
          handleNoCambiarTercero={handleNoCambiarTercero}
        ></ModalAddTercero>
        {habilitarSegundaFormaPago && (
          <ModalAgregarFormaPago
            show={showModalSegundaFormaPago}
            handleClose={handleCloseModalSegundaFormaPago}
            onSave={handleGuardarSegundaFormaPago}
            formasDePago={formasDePago}
            factura={{
              ...objetoPostCanastilla,
              total: resumenTotalesCanastilla.total,
            }}
          />
        )}
      </div>
      <AlertError
        showAlertError={showAlertError}
        handleSetShowAlertError={handleSetShowAlertError}
      ></AlertError>
      <AlertVentaExitosa
        showAlertVentaExitosa={showAlertVentaExitosa}
        handleSetShowAlertVentaExitosa={handleSetShowAlertVentaExitosa}
        mensaje={mensajeAlerta}
      ></AlertVentaExitosa>
    </div>
  );
};

export default Canastilla;
