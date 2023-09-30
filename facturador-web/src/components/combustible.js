import React, { useState, useEffect } from "react";
import "./styles/home.css";
import GetIslas from "../services/getServices/GetIslas";
import GetTurnoIsla from "../services/getServices/GetTurnoIsla";
import GetCarasPorIsla from "../services/getServices/GetCarasPorIsla";
import GetFormasDePago from "../services/getServices/GetFormasDePago";
import GetTiposDeIdentificacion from "../services/getServices/GetTiposDeIdentificacion";
import GetUltimaFacturaPorCara from "../services/getServices/GetUltimaFacturaporCara";
import GetUltimaFacturaPorCaraTexto from "../services/getServices/GetUltimaFacturaPorCaraTexto";

const Combustible = () => {
  const [islas, setIslas] = useState([]);

  const [turno, setTurno] = useState(null);
  const [ultimaFactura, setUltimaFactura] = useState(null);
  const [ultimaFacturaTexto, setUltimaFacturaTexto] = useState("");
  const [caras, setCaras] = useState([]);
  const [tiposDeIdentificacion, setTiposDeIdentificacion] = useState([]);
  const [formasDePago, setformasDePago] = useState([]);
  const [formaDePagoSelect, setformaDePagoSelect] = useState("");

  const [islaSelect, setIslaSelect] = useState("");
  const [caraSelect, setCaraSelect] = useState(null);
  const [placa, setPlaca] = useState(null);
  const [kilometraje, setkilometraje] = useState(null);

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
    let factura = await GetUltimaFacturaPorCara(idCara);
    setUltimaFactura(factura);

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
                onChange={(event) => {
                  const selectCara = event.target.value;
                  setCaraSelect(selectCara);
                  fetchInformacionCliente(selectCara);
                }}
              >
                <option value="">Selecciona la Cara</option>
                {Array.isArray(caras) &&
                  caras.map((cara) => (
                    <option key={cara.id} value={cara.tipoIdentificacionId}>
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
              value={
                ultimaFactura ? ultimaFactura.tercero.tipoIdentificacion : ""
              }
            >
              <option value="">Selecciona un elemento</option>
              {Array.isArray(tiposDeIdentificacion) &&
                tiposDeIdentificacion.map((elemento, index) => (
                  <option key={index} value={elemento.tipoIdentificacionId}>
                    {elemento.descripcion}
                  </option>
                ))}
            </select>
            <div className="mt-2 p-0">
              <div className="form-control dark-blue-input">
                <p>
                  222222222222{" "}
                  {ultimaFactura === null
                    ? "N/A"
                    : ultimaFactura.tercero.identificacion}
                </p>
              </div>
            </div>
            <div className="mt-2">
              <div className="form-control dark-blue-input">
                <p>
                  {ultimaFactura === null ? (
                    <br></br>
                  ) : (
                    "Nombre:" + ultimaFactura.tercero.nombre
                  )}
                </p>
                <p>
                  {ultimaFactura === null ? (
                    <br></br>
                  ) : (
                    "Teléfono: " + ultimaFactura.tercero.telefono
                  )}
                </p>
                <p>
                  {ultimaFactura === null ? (
                    <br></br>
                  ) : (
                    "Correo: " + ultimaFactura.tercero.correo
                  )}
                </p>
                <p>
                  {ultimaFactura === null ? (
                    <br></br>
                  ) : (
                    "Dirección: " + ultimaFactura.tercero.direccion
                  )}
                </p>
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
                value={
                  ultimaFactura
                    ? ultimaFactura.codigoFormadePago
                    : formaDePagoSelect
                }
                onChange={(event) => {
                  const selectForma = event.target.value;
                  setformaDePagoSelect(selectForma);
                }}
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
                value={ultimaFactura ? ultimaFactura.placa : placa}
                onChange={(event) => {
                  setPlaca(event.target.value);
                }}
              ></input>

              <input
                type="text"
                className="form-control select-white-blue w-50 h-40"
                placeholder="Kilometraje"
                value={ultimaFactura ? ultimaFactura.kilometraje : kilometraje}
                onChange={(event) => {
                  setkilometraje(event.target.value);
                }}
              ></input>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 center-column columnas">
        <div className="container container-factura my-4">
          <div className=" factura px-2 w-100 h-100">
            <p>{ultimaFacturaTexto ? ultimaFacturaTexto : " "}</p>
            {/* <p>Facr=tura de Venta P.O.s No. 111111</p>
            <p>Vendido a: consumidor final </p>
            <p>Nit/CC: 22222222222</p>
            <p>Placa: placa</p>
            <p>Surtidor: surtidor 2</p>
            <p>Cara: cara 4</p>
            <p>Manguera: manguera 4</p>
            <p>Vendedor: Karen Vergara</p>
            <p>Producto Cantidad Precio Total</p>
            <p>GNV 21.200 2200,00 46.640</p>
            <p>DISCRIMINACION TARIFAS IVA</p>
            <p>Producto Cantidad Tarifa Total</p>
            <p>GNV 21.200 0% 46.640</p>
            <p>Subtotal sin IVA: 46.640</p>
            <p>Descuento: 0,00</p>
            <p>Subtotal IVA: 0,00</p>
            <p>TOTAL: 46.640</p>
            <p>Forma de pago: Efectivo</p> */}
          </div>
        </div>
        <div className="d-flex justify-content-center">
          <button className="print-button botton-light-blue">
            Imprimir Factura
          </button>
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
