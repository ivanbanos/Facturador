import React from "react";
import "./styles/home.css";
import NavBar from "./navbar";
import logo from "./icono.png";
import Combustible from "./combustible";
import Canastilla from "./canastilla.js";
import VehiculosSICOMModal from "./rabbitWebSocket";
import Terceros from "./terceros";
import { Routes, Route, BrowserRouter } from "react-router-dom";

function App() {
  return (
    <>
      <BrowserRouter>
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
            <Routes>
              <Route path="/" exact element={<Combustible />} />
              <Route path="/canastilla" exact element={<Canastilla />} />
              <Route path="/terceros" exact element={<Terceros />} />
            </Routes>
          </div>
        </div>
      </BrowserRouter>
      <VehiculosSICOMModal />
    </>
  );
}

export default App;
