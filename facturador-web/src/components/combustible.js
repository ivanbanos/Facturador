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

const Combustible = () => {
  const [showFacturaElectronica, setShowFacturaElectronica] = useState(false);

  const handleCloseFacturaElectronica = () => setShowFacturaElectronica(false);
  const handleShowFacturaElectrónica = () => setShowFacturaElectronica(true);

  const [islas, setIslas] = useState([]);

  const [turno, setTurno] = useState(null);
  const [ultimaFacturaTexto, setUltimaFacturaTexto] = useState("");
  const [caras, setCaras] = useState([]);
  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const [formasDePago, setformasDePago] = useState([]);
  const [islaSelect, setIslaSelect] = useState("");
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
  const [ultimaFactura, setUltimaFactura] = useState({
    placa: "",
    kilometraje: "",
    codigoFormaPago: "",
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
  });
  const handleChangeTercero = (event) => {
    const name = event.target.name;
    const value = event.target.value;
    const tempTercero = { ...tercero, [event.target.name]: event.target.value };
    const tempFactura = { ...ultimaFactura, tercero: tempTercero };
    setTercero(tempTercero);
    setUltimaFactura(tempFactura);
  };
  const handleChangeFactura = (event) => {
    const name = event.target.name;
    const value = event.target.value;
    const tempFactura = {
      ...ultimaFactura,
      [event.target.name]: event.target.value,
    };
    setUltimaFactura(tempFactura);
    console.log(tempFactura);
  };

  const fetcInicial = async () => {
    let islas = await GetIslas();
    setIslas(islas);

    let tiposDeIdentificacion = await GetTiposDeIdentificacion();
    setTiposDeIdentificacion(tiposDeIdentificacion);
    console.log(tiposDeIdentificacion);

    let formasPago = await GetFormasDePago();
    setformasDePago(formasPago);
  };

  const fetcTurnoYCaras = async (idIsla) => {
    let turno = await GetTurnoIsla(idIsla);
    setTurno(turno);
    console.log(turno);
    let caras = await GetCarasPorIsla(idIsla);
    setCaras(caras);
    console.log(caras);
  };

  const fetchInformacionCliente = async (idCara) => {
    console.log(idCara);
    let factura = await GetUltimaFacturaPorCara(idCara);
    factura && setUltimaFactura(factura);

    let facturaTexto = await GetUltimaFacturaPorCaraTexto(idCara);
    setUltimaFacturaTexto(facturaTexto);
  };
  useEffect(() => {
    fetcInicial();
  }, []);

  return (
    <>
      <div className="col-4 pt-4 pb-4 left-column columnas">
        <div className="d-flex flex-row isla-div">
          <div className="border-rombo">
            <div className="border-middle-rombo">
              <div className="rombo"></div>
            </div>
          </div>
          <label className="mx-2 fs-3 d-inline text-white">ISLAS</label>
          <select
            className="form-select dark-blue-input d-inline w-50 h-50"
            aria-label="Default select example"
            value={islaSelect}
            onChange={(event) => {
              const selectIsla = event.target.value;
              setIslaSelect(selectIsla);
              fetcTurnoYCaras(selectIsla);
            }}
          >
            <option value="">Selecciona la isla</option>

            {Array.isArray(islas) &&
              islas.map((isla) => (
                <option key={isla.id} value={isla.id}>
                  {isla.isla}
                </option>
              ))}
            <option value="1">opcion1 </option>
          </select>
        </div>
        <div className="info-div ">
          <div className="text-white info-isla-div">
            <div className="row info-turno-div pt-2">
              <div className="col-4">
                <p className="text-end">Turno: </p>
                <p className="text-end">Empleado:</p>
              </div>
              <div className="col-7">
                <p>{turno === null ? "N/A" : turno.fechaApertura} </p>
                <p>{turno === null ? "N/A" : turno.empleado}</p>
              </div>
            </div>
            <div className="d-flex flex-row">
              <label className="mx-3 d-inline fs-3">Cara</label>
              <select
                className="form-select d-inline w-50 h-50 select-white-blue"
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
                <option value="1">opcion1 </option>
              </select>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="d-flex my-3 titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="fs-3 text-white">Información del Cliente</div>
            </div>
            <select
              className="form-select w-80 h-50 select-white-blue"
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
                className="form-control dark-blue-input "
                placeholder="Identificación"
                name="identificacion"
                value={ultimaFactura.tercero.identificacion || ""}
                onChange={handleChangeTercero}
              ></input>
            </div>
            <div className="mt-2">
              <div className="form-control dark-blue-input row d-flex">
                <div className="col-4">
                  <p className="text-end">Nombre: </p>
                  <p className="text-end">Teléfono:</p>
                  <p className="text-end">Correo:</p>
                  <p className="text-end">Dirección:</p>
                </div>
                <div className="col-7">
                  <p>
                    <input
                      type="text"
                      className=" dark-blue-input "
                      name="nombre"
                      value={ultimaFactura.tercero.nombre || ""}
                      onChange={handleChangeTercero}
                    ></input>
                  </p>
                  <p>
                    <input
                      type="tel"
                      className=" dark-blue-input "
                      name="telefono"
                      value={ultimaFactura.tercero.telefono || ""}
                      onChange={handleChangeTercero}
                    ></input>
                  </p>
                  <p>
                    <input
                      type="email"
                      className=" dark-blue-input "
                      name="correo"
                      value={ultimaFactura.tercero.correo || ""}
                      onChange={handleChangeTercero}
                    ></input>
                  </p>
                  <p>
                    <input
                      type="text"
                      className=" dark-blue-input "
                      name="direccion"
                      value={ultimaFactura.tercero.direccion || ""}
                      onChange={handleChangeTercero}
                    ></input>
                  </p>
                </div>
              </div>
            </div>
          </div>
          <div className="info-venta-div">
            <div className="d-flex titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="fs-3 text-white">Información de la Venta</div>
            </div>

            <div className="info-venta-div d-flex flex-column align-items-end">
              <select
                className="form-select  w-75 h-50 select-white-blue"
                aria-label="Default select example"
                name="codigoFormaPago"
                value={ultimaFactura.codigoFormaPago || ""}
                onChange={handleChangeFactura}
              >
                <option value="">Forma de pago</option>
                {Array.isArray(formasDePago) &&
                  formasDePago.map((forma) => (
                    <option key={forma.id} value={forma.id}>
                      {forma.descripcion}
                    </option>
                  ))}
                <option value={"1"}>OpcionFP1</option>
              </select>

              <input
                type="text"
                className="form-control select-white-blue w-50 h-40"
                placeholder="Placa"
                name="placa"
                value={ultimaFactura.placa || ""}
                onChange={handleChangeFactura}
              ></input>

              <input
                type="text"
                className="form-control select-white-blue w-50 h-40"
                placeholder="Kilometraje"
                name="kilometraje"
                value={ultimaFactura.kilometraje || ""}
                onChange={handleChangeFactura}
              ></input>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 center-column columnas">
        <div className="container container-factura my-4">
          <div className=" factura px-2 w-100 h-100">
            <p>{ultimaFacturaTexto ? ultimaFacturaTexto : " "}</p>
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <ModalImprimir
            ultimaFactura={ultimaFactura}
            handleShowFacturaElectrónica={handleShowFacturaElectrónica}
          ></ModalImprimir>
          <ModalFacturaElectronica
            handleCloseFacturaElectronica={handleCloseFacturaElectronica}
            showFacturaElectronica={showFacturaElectronica}
          ></ModalFacturaElectronica>
        </div>
      </div>
      <div className="col-3  right-column columnas">
        <div className="button-container1"></div>
        <div className="d-flex flex-column align-items-center button-container">
          <button className="botton-green m-3 right-botton">
            <span className="">Abrir</span> <span>turno</span>
          </button>
          <button className="botton-medium-blue m-3 right-botton">
            <span>Cerrar</span> <span>turno</span>
          </button>
          <button className="botton-light-blue right-botton m-3">
            <span>Fidelizar</span> <span>Venta</span>
          </button>
        </div>
      </div>
    </>
  );
};

export default Combustible;
