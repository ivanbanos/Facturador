import { useState } from "react";
import Alert from "react-bootstrap/Alert";
import Button from "react-bootstrap/Button";

function AlertTercero(props) {
  return (
    <>
      <Alert show={props.showTerceroNoExiste} variant="success">
        <Alert.Heading>Tercero No Existe</Alert.Heading>
        <p>
          La identificación que no ingreso no se encuentra registrada como
          tercero
        </p>
        <hr />
        <div className="d-flex justify-content-end">
          <Button
            onClick={() => {
              props.handleShowTerceroNoExiste(false);
              props.handleNoCambiarTercero();
            }}
            variant="outline-success"
          >
            No Cambiar Tercero
          </Button>
          <Button
            onClick={() => {
              props.handleShowTerceroNoExiste(false);
              props.handleShowAddTercero(true);
            }}
            variant="outline-success"
          >
            Agregar Tercero
          </Button>
        </div>
      </Alert>
    </>
  );
}

export default AlertTercero;
