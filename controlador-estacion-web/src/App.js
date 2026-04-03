import React from "react";
import { BrowserRouter, Link, Navigate, Route, Routes } from "react-router-dom";
import DashboardPage from "./pages/DashboardPage";
import ReportesPage from "./pages/ReportesPage";
import "./App.css";

function App() {
  return (
    <BrowserRouter>
      <div className="layout">
        <header className="topbar">
          <h1>Controlador Estacion</h1>
          <nav>
            <Link to="/">Dashboard</Link>
            <Link to="/reportes">Reportes</Link>
          </nav>
        </header>

        <main className="content">
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/reportes" element={<ReportesPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
