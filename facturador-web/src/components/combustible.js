import React from "react";
import "./styles/home.css";

const Combustible = () => {
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
          >
            <option selected></option>
            <option value="1">Isla 1</option>
            <option value="2">Isla 2</option>
            <option value="3">Isla 3</option>
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
                <p>09/25/2023 16:30:38 </p>
                <p>Karen Vergara</p>
              </div>
            </div>
            <div className="d-flex flex-row">
              <label className="mx-3 d-inline fs-3">Cara</label>
              <select
                className="form-select d-inline w-50 h-50 select-white-blue"
                aria-label="Default select example"
              >
                <option selected>Selecciona la cara</option>
                <option value="1">Cara 1</option>
                <option value="2">Cara 2</option>
              </select>
            </div>
          </div>
          <div className="info-cliente-div">
            <div className="d-flex my-3 titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="fs-3 text-white">Información de la Venta</div>
            </div>
            <select
              className="form-select w-80 h-50 select-white-blue"
              aria-label="Default select example"
            >
              <option selected></option>
              <option value="1">Ejemplo 1</option>
              <option value="2">Ejemplo 2</option>
            </select>
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
          <div className="info-venta-div">
            <div className="d-flex titulos-circulo">
              <div className="circulo my-2 mx-3"></div>
              <div className="fs-3 text-white">Información de la Venta</div>
            </div>

            <div className="info-venta-div d-flex flex-column align-items-end">
              <select
                className="form-select  w-75 h-50 select-white-blue"
                aria-label="Default select example"
              >
                <option selected>Efectivo</option>
                <option value="1">Débito</option>
                <option value="2">Cupón</option>
                <option value="3">Isla 3</option>
              </select>
              <select
                className="form-select  w-75 h-50 select-white-blue"
                aria-label="Default select example"
              >
                <option selected>CBP248</option>
              </select>
              <select
                className="form-select  w-50 h-50 select-white-blue"
                aria-label="Default select example"
              >
                <option selected>Kilometraje</option>
              </select>
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
