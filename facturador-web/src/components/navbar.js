import React, { useState } from "react";
import "./styles/home.css";
import "./styles/navbar.css";
import circle from "./styles/circle.svg";
import { Link } from "react-router-dom";
import Nav from "react-bootstrap/Nav";

const NavBar = () => {
  const [selectedItem, setSelectedItem] = useState("combustible");

  const handleItemClick = (item) => {
    setSelectedItem(item);
  };

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
              onClick={() => handleItemClick("combustible")}
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
              onClick={() => handleItemClick("canastilla")}
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
              onClick={() => handleItemClick("terceros")}
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
