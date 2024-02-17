import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import FidelizarVenta from "../Services/getServices/FidelizarVenta";
import "./styles/home.css";
import "./styles/modal.css";
import { Alert } from "react-bootstrap";
import GetTercero from "../Services/getServices/GetTercero";

const ModalFidelizarVenta = (props) => {
  const [showModalFidelizarVenta, setShowModalFidelizarVenta] = useState(false);
  const handleCloseModalFidelizarVenta = () =>
    setShowModalFidelizarVenta(false);
  const handleShowModalFidelizarVenta = () => setShowModalFidelizarVenta(true);
  const [identificacionFidelizar, setIdentificacionFidelizar] = useState("");
  const [showAlertFidelizacionExitosa, setShowAlertFidelizacionExitosa] =
    useState(false);
  const [terceroBusqueda, setTerceroBusqueda] = useState([{}]);
  const [tercero, setTercero] = useState({
    nombre: "",
  });
  const [hasError, setHasError] = useState(false);
  
  const onClickFidelizarVenta = async () => {
    if (!hasError) {
      handleCloseModalFidelizarVenta();
      const respuestaFidelizar = await FidelizarVenta(
        identificacionFidelizar,
        props.manguera
      );
      if (respuestaFidelizar === "fail") {
        props.handleSetShowAlertError(true);
      } else {
        setIdentificacionFidelizar("");
        setTerceroBusqueda([{}]);
        setTercero({
          nombre: "",
        });
        setHasError(false);
        props.getFacturaInformacion();
        setShowAlertFidelizacionExitosa(true);
      }
    }
  };

  return (
    <>
      <Button
        className="botton-light-blue right-botton m-1"
        onClick={() => {
          handleShowModalFidelizarVenta();
        }}
      >
        Fidelizar Venta
      </Button>

      <Modal
        show={showModalFidelizarVenta}
        onHide={handleCloseModalFidelizarVenta}
        backdrop="static"
        keyboard={false}
        dialogClassName="custom-modal"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header className="header-modal" closeButton>
          <Modal.Title>Fidelizar Venta</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form>
            <div className="row mb-3">
              <label className="col-sm-5 col-form-label">Identificación</label>
              <div className="col-sm-7">
                <input
                  type="text"
                  className="form-control modal-tercero-input"
                  name="identificacion"
                  value={identificacionFidelizar}
                  onChange={(event) =>
                    setIdentificacionFidelizar(event.target.value)
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
              handleCloseModalFidelizarVenta();
              setIdentificacionFidelizar("");
              setTerceroBusqueda([{}]);
              setTercero({
                nombre: "",
              });
              setHasError(false);
            }}
          >
            Cancelar
          </Button>
          <Button
            className="botton-medium-blue-modal"
            onClick={onClickFidelizarVenta}
          >
            Fidelizar
          </Button>
        </Modal.Footer>
      </Modal>
      <div
        className={`alert-container ${
          showAlertFidelizacionExitosa ? "active" : ""
        }`}
      >
        <Alert
          variant="info"
          show={showAlertFidelizacionExitosa}
          onClose={() => setShowAlertFidelizacionExitosa(false)}
          dismissible
        >
          <Alert.Heading>Fidelización exitosa</Alert.Heading>
        </Alert>
      </div>
    </>
  );
};

export default ModalFidelizarVenta;
