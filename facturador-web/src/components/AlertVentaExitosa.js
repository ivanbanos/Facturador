import Alert from "react-bootstrap/Alert";
import "./styles/home.css";

function AlertVentaExitosa(props) {
  if (props.showAlertVentaExitosa) {
    return (
      <div className="alert-tercero-agregado">
        <Alert
          variant="info"
          onClose={() => props.handleSetShowAlertVentaExitosa(false)}
          dismissible
        >
          <Alert.Heading>Venta Generada Exitosamente</Alert.Heading>
        </Alert>
      </div>
    );
  }
}

export default AlertVentaExitosa;
