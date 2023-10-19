import Alert from "react-bootstrap/Alert";
import "./styles/home.css";

function AlertError(props) {
  if (props.showAlertError) {
    return (
      <div className="alert-error">
        <Alert
          variant="danger"
          onClose={() => props.handleSetShowAlertError(false)}
          dismissible
        >
          <Alert.Heading>Se ha presentado un error!</Alert.Heading>
        </Alert>
      </div>
    );
  }
}

export default AlertError;
