import React, { useEffect, useMemo, useState } from "react";
import { Button, Modal } from "react-bootstrap";
import "./styles/modal.css";

const toNumberOrNull = (value) => {
  if (value === "" || value === null || value === undefined) {
    return null;
  }
  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
};

const ModalAgregarFormaPago = ({
  show,
  handleClose,
  onSave,
  formasDePago,
  factura,
}) => {
  const [codigoFormaPago2, setCodigoFormaPago2] = useState("");
  const [total2, setTotal2] = useState("");

  const totalFactura = useMemo(() => {
    const total = Number(factura?.total);
    return Number.isNaN(total) ? null : total;
  }, [factura]);

  useEffect(() => {
    if (!show) {
      return;
    }
    setCodigoFormaPago2(
      factura?.codigoFormaPago2 !== null && factura?.codigoFormaPago2 !== undefined
        ? String(factura.codigoFormaPago2)
        : ""
    );
    setTotal2(
      factura?.total2 !== null && factura?.total2 !== undefined
        ? String(factura.total2)
        : ""
    );
  }, [show, factura]);

  const formasDisponibles = Array.isArray(formasDePago)
    ? formasDePago.filter((forma) => Number(forma.id) !== Number(factura?.codigoFormaPago))
    : [];

  const handleGuardarYCerrar = () => {
    const formaPago2 = codigoFormaPago2 === "" ? null : Number(codigoFormaPago2);
    const valor2 = toNumberOrNull(total2);

    let total1 = null;
    if (totalFactura !== null && valor2 !== null) {
      total1 = totalFactura - valor2;
    }

    onSave({
      codigoFormaPago2: formaPago2,
      total2: valor2,
      total1,
    });

    handleClose();
  };

  return (
    <Modal
      show={show}
      onHide={handleGuardarYCerrar}
      backdrop="static"
      keyboard={false}
      dialogClassName="custom-modal"
      aria-labelledby="contained-modal-title-vcenter"
      centered
    >
      <Modal.Header className="header-modal" closeButton>
        <Modal.Title>Agregar segunda forma de pago</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <div className="row mb-3">
          <label className="col-sm-5 col-form-label">Forma de pago 2</label>
          <div className="col-sm-7">
            <select
              className="form-select modal-tercero-input"
              value={codigoFormaPago2}
              onChange={(event) => setCodigoFormaPago2(event.target.value)}
            >
              <option value="">Seleccione forma de pago</option>
              {formasDisponibles.map((forma) => (
                <option key={forma.id} value={forma.id}>
                  {forma.descripcion}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="row mb-3">
          <label className="col-sm-5 col-form-label">Valor segunda forma</label>
          <div className="col-sm-7">
            <input
              type="number"
              min="0"
              step="0.01"
              className="form-control modal-tercero-input"
              value={total2}
              onChange={(event) => setTotal2(event.target.value)}
              placeholder="0"
            />
          </div>
        </div>

        {totalFactura !== null && (
          <div className="row mb-1">
            <label className="col-sm-5 col-form-label">Total factura</label>
            <div className="col-sm-7">
              <input
                type="text"
                className="form-control modal-tercero-input"
                value={totalFactura}
                disabled
              />
            </div>
          </div>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button className="botton-medium-blue-modal" onClick={handleGuardarYCerrar}>
          Guardar y cerrar
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default ModalAgregarFormaPago;