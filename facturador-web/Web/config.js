var SERVER_URL = "https://localhost:7269";
var RabbitWebSocket = "ws://192.168.1.174:15674/ws";
var ConvertirAFactura = false;
var ConvertirAOrden = false;
var GenerarFacturaelectronica = true;
var DesabilitaFormasNoCredito=true;
var PlacaObligatoriaCanastillaCredito = true;
var FormasPagos = [1,4,98];
var palabrasPermitidas = [
    "PIMPINAS",
    "CANECAS",
    "BIDONES",
    "PLANTA",
    "MÁQUINA",
    "PRUEBA",
    "AIRE",
    "TANQUE",
    "BALDE"
  ];