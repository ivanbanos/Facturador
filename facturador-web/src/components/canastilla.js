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
    facturasCanastillaId: 0,
    fecha: new Date().toISOString(),
    resolucion: {
      descripcionResolucion: "string",
      fechaInicioResolucion: "2023-10-09T16:49:52.939Z",
      fechaFinalResolucion: "2023-10-09T16:49:52.939Z",
      consecutivoInicial: 0,
      consecutivoFinal: 0,
      consecutivoActual: 0,
      tipo: 0,
      habilitada: true,
    },
    consecutivo: 0,
    estado: "",
    terceroId: {
      terceroId: 0,
      coD_CLI: "",
      nombre: "",
      telefono: "",
      direccion: "",
      identificacion: "",
      correo: "",
      tipoIdentificacion: 0,
    },
    impresa: 0,
    enviada: 0,
    codigoFormaPago: {
      id: 0,
      descripcion: "",
    },
    canastillas: [],
    subtotal: 0,
    descuento: 0,
    iva: 0,
    total: 0,
  };
  const [objetoPostCanastilla, setObjetoPostCanastilla] = useState(
    valorInicialObjetoPostCanastilla
  );
  const [canastillas, setCanastillas] = useState([]);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [cantidadSeleccionada, setCantidadSeleccionada] = useState(0);
  const valorInicialObjetoCanastillas = {
    facturasCanastillaId: 0,
    fecha: new Date().toISOString(),
    resolucion: {
      descripcionResolucion: "string",
      fechaInicioResolucion: "2023-10-09T16:49:52.939Z",
      fechaFinalResolucion: "2023-10-09T16:49:52.939Z",
      consecutivoInicial: 0,
      consecutivoFinal: 0,
      consecutivoActual: 0,
      tipo: 0,
      habilitada: true,
    },
    consecutivo: 0,
    estado: "",
    terceroId: {
      terceroId: 0,
      coD_CLI: "",
      nombre: "",
      telefono: "",
      direccion: "",
      identificacion: "",
      correo: "",
      tipoIdentificacion: 0,
    },
    impresa: 0,
    enviada: 0,
    codigoFormaPago: {
      id: 0,
      descripcion: "",
    },
    canastillas: [],
    subtotal: 0,
    descuento: 0,
    iva: 0,
    total: 0,
  };
  const [objetoCanastillas, setObjetoCanastillas] = useState(
    valorInicialObjetoCanastillas
  );
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
        terceroId: nuevoTercero[0],
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
    console.log(objetoPostCanastilla.terceroId.identificacion);
    setIdentificacion(objetoPostCanastilla.terceroId.identificacion);
  }
  const [showAddTercero, setShowAddTercero] = useState(false);
  const handleShowAddTercero = (show) => setShowAddTercero(show);
  function handleSetTerceroModalAddTercero(newTercero) {
    setTercero(newTercero);
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      terceroId: newTercero,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);

    setIdentificacion(newTercero.identificacion);
  }
  const [totalItems, setTotalItems] = useState(0);
  const [subTotal, setSubTotal] = useState(0);
  function onClickAgregarProducto() {
    if (productoSeleccionado) {
      let tempObjetoCanastillas = {
        ...objetoCanastillas,
        canastilla: productoSeleccionado,
        cantidad: cantidadSeleccionada,
      };

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
        counterSubTotal += item.canastilla.precio * item.cantidad;
      }
      setTotalItems(counterTotalItems);
      setSubTotal(counterSubTotal);
      console.log(counterSubTotal);
    }
  }
  const handleChangeFormaPago = (event) => {
    const selectedPFormaId = event.target.value;
    const selectedFormaPago = formasDePago.find(
      (forma) => forma.id === parseInt(selectedPFormaId, 10)
    );
    const tempObjetoPostCanastilla = {
      ...objetoPostCanastilla,
      codigoFormaPago: selectedFormaPago,
    };
    setObjetoPostCanastilla(tempObjetoPostCanastilla);
    console.log(tempObjetoPostCanastilla);
  };
  const onClickGenerarVenta = (canastilla) => {
    console.log(canastilla);
    PostCanastilla(canastilla);
    // setObjetoPostCanastilla(valorInicialObjetoPostCanastilla)
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
            <label className="fs-3 text-white">Agregar Producto</label>
            <select
              className="form-select d-inline w-80 h-50 select-white-blue"
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
              <label className="mx-3 d-inline fs-3">Cantidad</label>

              <input
                value={cantidadSeleccionada || ""}
                onChange={(event) => {
                  const newCantidad = parseInt(event.target.value, 10);
                  if (!isNaN(newCantidad) || event.target.value === "") {
                    setCantidadSeleccionada(newCantidad);
                  }
                }}
                className="form-control w-50 h-50 select-white-blue"
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
            <div className="fs-3 text-white">Información del Cliente</div>

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
                <p>Nombre: {objetoPostCanastilla?.terceroId?.nombre}</p>
                <p>Teléfono: {objetoPostCanastilla?.terceroId?.telefono} </p>
                <p>Correo: {objetoPostCanastilla?.terceroId?.correo}</p>
                <p>Dirección: {objetoPostCanastilla?.terceroId?.direccion}</p>
                {/* <p>
                  Tipo de identificación:{" "}
                  {objetoPostCanastilla.terceroId.tipoIdentificacion}
                </p> */}
              </div>
              <div className="info-venta-div d-flex flex-column align-items-end">
                <select
                  className="form-select  w-75 h-50 select-white-blue"
                  aria-label="Default select example"
                  name="codigoFormaPago"
                  value={objetoPostCanastilla.codigoFormaPago.id || ""}
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
            <p>Vendido a: {objetoPostCanastilla.terceroId.nombre} </p>
            <p>Nit/CC: {objetoPostCanastilla.terceroId.identificacion}</p>
            <p>Fecha: {objetoPostCanastilla.fecha}</p>
            <p>Fecha: {objetoPostCanastilla.codigoFormaPago.descripcion}</p>
            {objetoPostCanastilla.canastillas.length > 0 && (
              <p>PRODUCTOS AGREGADOS</p>
            )}
            {Array.isArray(objetoPostCanastilla.canastillas) &&
              objetoPostCanastilla.canastillas.map((elemento) => (
                <div key={elemento.canastilla.canastillaId}>
                  <hr></hr>
                  <p>Producto: {elemento.canastilla.descripcion}</p>
                  <p>Precio: {elemento.canastilla.precio}</p>
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
            }}
          >
            <span className="">Generar</span> <span>Venta</span>
          </button>
          <button className="botton-medium-blue m-3 right-botton">
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
