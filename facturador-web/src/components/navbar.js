import React from "react";
import "./styles/home.css";
import "./styles/navbar.css";
import { Link, useLocation } from "react-router-dom";

const NavBar = () => {
  const location = useLocation();

  const selectedItem =
    location.pathname === "/canastilla"
      ? "canastilla"
      : location.pathname === "/terceros"
        ? "terceros"
        : "combustible";

  return (
    // <nav className="">
    //   <ul className="menu">
    //     <li>
    //       <Link
    //         className="links"
    //         to="/"
    //         onClick={() => handleItemClick("combustible")}
    //       >
    //         <div
    //           className={`pt-1 d-flex ${
    //             selectedItem === "combustible"
    //               ? "item-active"
    //               : "item-no-active"
    //           }`}
    //         >
    //           <div className="circulo mb-2 mx-2"></div>
    //           <div>COMBUSTIBLE </div>
    //         </div>
    //       </Link>
    //     </li>
    //     <li>
    //       <Link
    //         className="links"
    //         to="/canastilla"
    //         onClick={() => handleItemClick("canastilla")}
    //       >
    //         <div
    //           className={`item-second pt-2 ${
    //             selectedItem === "canastilla" ? "item-active" : "item-no-active"
    //           }`}
    //         >
    //           CANASTILLA{" "}
    //         </div>
    //       </Link>
    //     </li>
    //     <li>
    //       <Link
    //         className="links"
    //         to="/terceros"
    //         onClick={() => handleItemClick("terceros")}
    //       >
    //         <div
    //           className={`icono-third pt-2 ${
    //             selectedItem === "terceros" ? "item-active" : "item-no-active"
    //           }`}
    //         >
    //           TERCEROS
    //         </div>
    //       </Link>
    //     </li>
    //   </ul>
    // </nav>
    <div className="navbar pb-0">
      <div className="card-header">
        <ul className="nav nav-tabs card-header-tabs">
          <li className="nav-item">
            <Link
              className={`nav-link ${
                selectedItem === "combustible"
                  ? "item-active"
                  : "item-no-active"
              }`}
              to="/"
            >
              COMBUSTIBLE
            </Link>
          </li>
          <li className="nav-item">
            <Link
              className={`nav-link ${
                selectedItem === "canastilla" ? "item-active" : "item-no-active"
              }`}
              to="/canastilla"
            >
              CANASTILLA
            </Link>
          </li>
          <li className="nav-item">
            <Link
              className={`nav-link ${
                selectedItem === "terceros" ? "item-active" : "item-no-active"
              }`}
              to="/terceros"
            >
              TERCEROS
            </Link>
          </li>
        </ul>
      </div>
    </div>
  );
};

export default NavBar;
