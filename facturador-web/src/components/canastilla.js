import React, { useState, useEffect } from "react";
import "./styles/home.css";
import GetCanastilla from "../Services/getServices/GetCanastilla";
import GetTercero from "../Services/getServices/GetTercero";
import PostCanastilla from "../Services/getServices/PostCanastilla";
import GetTiposDeIdentificacion from "../Services/getServices/GetTiposDeIdentificacion";
import GetFormasDePago from "../Services/getServices/GetFormasDePago";
import AlertTerceroNoExisteCnastilla from "./alertTerceroNoExisteCanastilla";
import ModalAddTercero from "./modalAddTercero";
const Canastilla = () => {
  const [productos, setProductos] = useState([]);
  const valorInicialObjetoPostCanastilla = {
    terceroId: 0,
    codigoFormaPago: 0,
    descuento: 0,
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
    telefono: "",
    direccion: "",
    identificacion: "",
    correo: "",
    tipoIdentificacion: 0,
  };
  const [tercero, setTercero] = useState(valorInicialTercero);

  const [identificacion, setIdentificacion] = useState("");
  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const [formasDePago, setformasDePago] = useState([]);
  const [terceroBusqueda, setTerceroBusqueda] = useState([{}]);
  const handleChangeIdentificacion = async (event) => {
    const nuevaIdentificacion = event.target.value;
    setIdentificacion(nuevaIdentificacion);
    let nuevoTercero = await GetTercero(nuevaIdentificacion);
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
      console.log(nuevaIdentificacion);
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
    setIdentificacion("");
  }
  const [showAddTercero, setShowAddTercero] = useState(false);
  const handleShowAddTercero = (show) => setShowAddTercero(show);
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
      console.log(tempObjetoPostCanastilla);
      setCantidadSeleccionada(0);
      setProductoSeleccionado(null);
      let counterTotalItems = 0;
      let counterSubTotal = 0;
      for (let item of tempCanastillas) {
        counterTotalItems += item.cantidad;
        counterSubTotal += item.precio * item.cantidad;
      }
      setTotalItems(counterTotalItems);
      setSubTotal(counterSubTotal);
      console.log(counterSubTotal);
    }
  }
  const handleChangeFormaPago = (event) => {
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      codigoFormaPago: event.target.value,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);
    console.log(tempObjetoPostCanastilla);
  };
  const onClickGenerarVenta = (canastilla) => {
    console.log(canastilla);
    PostCanastilla(canastilla);
    // setObjetoPostCanastilla(valorInicialObjetoPostCanastilla)
  };
  const resetValues = () => {
    setIdentificacion("");
    setSubTotal(0);
    setTotalItems(0);
    setObjetoCanastillas(valorInicialObjetoCanastillas);
    setCanastillas([]);
    setObjetoPostCanastilla(valorInicialObjetoPostCanastilla);
    setTercero(valorInicialTercero);
  };
  useEffect(() => {
    const fetchData = async () => {
      try {
        let productos = await GetCanastilla();
        setProductos(productos);
        let tiposDeIdentificacion = await GetTiposDeIdentificacion();
        setTiposDeIdentificacion(tiposDeIdentificacion);
        let formasPago = await GetFormasDePago();
        setformasDePago(formasPago);
      } catch (error) {}
    };

    fetchData();
  }, []);
  return (
    <>
      <div className="col-4 pt-4 pb-4 left-column columnas">
        <div className="info-div ">
          <div className="text-white">
            <label className="titulo-informacion text-white">
              Agregar Producto
            </label>
            <select
              className="form-select d-inline w-80 h-50 select-white-blue text-select-list"
              aria-label="Default select example"
              value={productoSeleccionado?.canastillaId || ""}
              onChange={(event) => {
                const selectedProductId = event.target.value;
                const selectedProduct = productos.find(
                  (product) =>
                    product.canastillaId === parseInt(selectedProductId, 10)
                );
                setProductoSeleccionado(selectedProduct);
              }}
            >
              <option value="">Selecciona el producto</option>
              {Array.isArray(productos) &&
                productos.map((elemento) => (
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
              <label className="mx-3 d-inline titulo-informacion">
                Cantidad
              </label>

              <input
                value={cantidadSeleccionada || ""}
                onChange={(event) => {
                  const newCantidad = parseInt(event.target.value, 10);
                  if (!isNaN(newCantidad) || event.target.value === "") {
                    setCantidadSeleccionada(newCantidad);
                  }
                }}
                className="form-control w-50 h-50 select-white-blue text-select-list"
              />
            </div>
            <div className="d-flex justify-content-center">
              <button
                className="print-button botton-light-blue"
                onClick={onClickAgregarProducto}
              >
                Agregar
              </button>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="titulo-informacion text-white">
              Información del Cliente
            </div>

            <div className="mt-2 p-0">
              <div className=" ">
                <input
                  type="text"
                  className="form-control dark-blue-input "
                  placeholder="Identificación"
                  name="identificacion"
                  value={identificacion || ""}
                  onChange={handleChangeIdentificacion}
                  onBlur={onBlurTercero}
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
              <div className="form-control dark-blue-input">
                <p>Nombre: {tercero?.nombre}</p>
                <p>Teléfono: {tercero?.telefono} </p>
                <p>Correo: {tercero?.correo}</p>
                <p>Dirección: {tercero?.direccion}</p>
                {/* <p>
                  Tipo de identificación:{" "}
                  {objetoPostCanastilla.terceroId.tipoIdentificacion}
                </p> */}
              </div>
              <div className="info-venta-div d-flex flex-column align-items-end">
                <select
                  className="form-select  w-75 h-50 select-white-blue text-select-list"
                  aria-label="Default select example"
                  name="codigoFormaPago"
                  value={objetoPostCanastilla.codigoFormaPago || ""}
                  onChange={handleChangeFormaPago}
                >
                  <option value="">Forma de pago</option>
                  {Array.isArray(formasDePago) &&
                    formasDePago.map((forma) => (
                      <option key={forma.id} value={forma.id}>
                        {forma.descripcion}
                      </option>
                    ))}
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 center-column columnas">
        <div className="container container-factura my-4">
          <div className=" factura px-2 h-100">
            <p>Vendido a: {tercero.nombre} </p>
            <p>Nit/CC: {tercero.identificacion}</p>

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
          </div>
        </div>
      </div>
      <div className="col-3  right-column columnas">
        <div className="button-container1"></div>
        <div className="d-flex flex-column align-items-center button-container">
          <button
            className="botton-green m-3 right-botton "
            onClick={() => {
              onClickGenerarVenta(objetoPostCanastilla);
              resetValues();
            }}
          >
            <span className="">Generar</span> <span>Venta</span>
          </button>
          <button
            className="botton-medium-blue m-3 right-botton"
            onClick={() => {
              resetValues();
            }}
          >
            <span>Borrar</span>
          </button>
        </div>
        <ModalAddTercero
          showAddTercero={showAddTercero}
          handleShowAddTercero={handleShowAddTercero}
          tiposDeIdentificacion={tiposDeIdentificacion}
          handleSetTerceroModalAddTercero={handleSetTerceroModalAddTercero}
          handleNoCambiarTercero={handleNoCambiarTercero}
        ></ModalAddTercero>
      </div>
    </>
  );
};

export default Canastilla;
