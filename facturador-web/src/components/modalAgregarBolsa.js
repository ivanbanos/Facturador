import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import AgregarBolsa from "../Services/getServices/AgregarBolsa";
import "./styles/home.css";
import "./styles/modal.css";
import { Alert } from "react-bootstrap";
import ImprimirNativo from "../Services/getServices/ImprimirNativo";

const ModalAgregarBolsa = (props) => {
  const [showModalReimpirmirTurno, setShowModalReimprimirTurno] =
    useState(false);
  const handleCloseModalReimprimirTurno = () =>
    setShowModalReimprimirTurno(false);
  const handleShowModalReimprimirTurno = () =>
    setShowModalReimprimirTurno(true);
    const [codigoEmpleado, setCodigoEmpleado] = useState("");
    const [cantidad, setCantidad] = useState("0");
    const [moneda, setMoneda] = useState("0");
    const [numero, setNumero] = useState("0");
  
  const handleChangeCodigoEmpleado = (codigo) => setCodigoEmpleado(codigo);
  const handleChangeCantidad = (cantidad) => setCantidad(cantidad);
  const handleChangeMoneda = (moneda) => setMoneda(moneda);
  const handleChangeNumero = (numero) => setNumero(numero);
  const [showAlertImpresionExitosa, setShowAlertImpresionExitosa] =
    useState(false);

  const islaSelect = props.islaSelect;



  return (
    <>
      <Button
        className="botton-green m-1 right-botton"
        onClick={() => {
          handleShowModalReimprimirTurno();
        }}
      >
        Agregar Bolsa
      </Button>

      <Modal
        show={showModalReimpirmirTurno}
        onHide={handleCloseModalReimprimirTurno}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Agregar Bolsa</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Isla Seleccionada
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="isla"
                  value={props.islaSelectName}
                  disabled
                ></input>
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Código de Empleado
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={codigoEmpleado}
                  onChange={(event) =>
                    handleChangeCodigoEmpleado(event.target.value)
                  }
                ></input>
              </div>
            </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Billetes
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={cantidad}
                  onChange={(event) =>
                    handleChangeCantidad(event.target.value)
                  }
                ></input>
              </div>
            </div>
            <div className="row mb-3">
            <label className="col-sm-5 col-form-label">
                Moneda
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={moneda}
                  onChange={(event) =>
                    handleChangeMoneda(event.target.value)
                  }
                ></input>
              </div>
              </div>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">
                Número de bolsa
              </label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={numero}
                  onChange={(event) =>
                    handleChangeNumero(event.target.value)
                  }
                ></input>
              </div>
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            className="botton-light-blue-modal"
            onClick={() => {
              handleCloseModalReimprimirTurno();
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={async () => {
              handleCloseModalReimprimirTurno();
              const respuestaReImprimir = await AgregarBolsa(
                islaSelect,
                codigoEmpleado,
                cantidad,
                moneda,
                numero
              );
              if (respuestaReImprimir === "fail") {
                props.handleSetShowAlertError(true);
              } else {
                
      await ImprimirNativo(respuestaReImprimir);
                setShowAlertImpresionExitosa(true);
              }
            }}
          >
            Agregar bolsa
          </Button>
        </Modal.Footer>
      </Modal>
      <div
        className={`alert-container ${
          showAlertImpresionExitosa ? "active" : ""
        }`}
      >
        <Alert
          variant="info"
          show={showAlertImpresionExitosa}
          onClose={() => setShowAlertImpresionExitosa(false)}
          dismissible
        >
          <Alert.Heading>Bolsa agregada de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalAgregarBolsa;
