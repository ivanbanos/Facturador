import React from "react";
import "./styles/home.css";

const Canastilla = () => {
  return (
    <>
      <div className="col-4 pt-4 pb-4 left-column columnas">
        <div className="info-div ">
          <div className="text-white">
            <label className="fs-3 text-white">Agregar Producto</label>
            <select
              className="form-select d-inline w-80 h-50 select-white-blue"
              aria-label="Default select example"
            >
              <option value="1">Producto 1</option>
              <option value="2">Producto 2</option>
            </select>
            <div className="d-flex flex-row">
              <label className="mx-3 d-inline fs-3">Cantidad</label>

              <div className="form-control w-50 h-50 select-white-blue">
                <input></input>
              </div>
            </div>
            <div className="d-flex justify-content-center">
              <button className="print-button botton-light-blue">
                Agregar
              </button>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="fs-3 text-white">Información del Cliente</div>

            <div className="mt-2 p-0">
              <div className="form-control dark-blue-input">
                <p>222222222222</p>
              </div>
            </div>
            <div className="mt-2">
              <div className="form-control dark-blue-input">
                <p>Nombre: Consumidor Final</p>
                <p>Teléfono: 2345678 </p>
                <p>Correo: prueba@gmail.com</p>
                <p>Dirección: C1 N1-5</p>
                <p>Tipo de identificación: NIT</p>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="col-5 center-column columnas">
        <div className="container container-factura my-4">
          <div className=" factura px-2">
            <p>Facr=tura de Venta P.O.s No. 111111</p>
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
            <p>Forma de pago: Efectivo</p>
          </div>
        </div>
      </div>
      <div className="col-3  right-column columnas">
        <div className="button-container1"></div>
        <div className="d-flex flex-column align-items-center button-container">
          <button className="botton-green m-3 right-botton">
            <span className="">Generar</span> <span>Venta</span>
          </button>
          <button className="botton-medium-blue m-3 right-botton">
            <span>Borrar</span>
          </button>
        </div>
      </div>
    </>
  );
};

export default Canastilla;
