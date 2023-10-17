import Alert from "react-bootstrap/Alert";
import "./styles/home.css";

function AlertTerceroAgregadoExitosamente(props) {
  if (props.showAlertTerceroAgregadoExitosamente) {
    return (
      <div className="alert-tercero-agregado">
        <Alert
          variant="info"
          onClose={() =>
            props.handleSetShowAlertTerceroAgregadoExitosamente(false)
          }
          dismissible
        >
          <Alert.Heading>Tercero actualizado de forma exitosa</Alert.Heading>
        </Alert>
      </div>
    );
  }
}

export default AlertTerceroAgregadoExitosamente;
