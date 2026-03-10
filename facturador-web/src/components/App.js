import React from "react";
import "./styles/home.css";
import NavBar from "./navbar";
import logo from "./icono.png";
import Combustible from "./combustible";
import Canastilla from "./canastilla.js";
import VehiculosSICOMModal from "./rabbitWebSocket";
import Terceros from "./terceros";
import ReloadToRootOnRefreshGuard from "./ReloadToRootOnRefreshGuard";
import { Routes, Route, BrowserRouter, Navigate } from "react-router-dom";

function AppRoutes() {
  return (
    <Routes>
      <Route path="/" exact element={<Combustible />} />
      <Route path="/canastilla" exact element={<Canastilla />} />
      <Route path="/terceros" exact element={<Terceros />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <>
      <BrowserRouter>
        <ReloadToRootOnRefreshGuard />
        <NavBar></NavBar>
        <div className=" main-box">
          <div className="icon-container">
            <img
              className="icono1"
              src={logo}
              alt="Texto alternativo para la imagen"
            ></img>
          </div>
          <div className="row box mx-2">
            <AppRoutes />
          </div>
        </div>
      </BrowserRouter>
      <VehiculosSICOMModal />
    </>
  );
}

export default App;
