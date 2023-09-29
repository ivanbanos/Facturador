import React from "react";
import "./styles/home.css";
import "./styles/navbar.css";
import circle from "./styles/circle.svg";
import { Link } from "react-router-dom";

const NavBar = () => {
  return (
    <nav className="">
      <ul className="menu ">
        <li className="menu-item">
          <Link className="links" to="/">
            <div className="item-active pt-1 d-flex">
              <div className="circulo mb-2 mx-3"></div>
              <div>COMBUSTIBLE</div>
            </div>
          </Link>
        </li>
        <li className="menu-item ">
          <Link className="links" to="/canastilla">
            <div className="item-no-active item-second pt-2">CANASTILLA</div>
          </Link>
        </li>
        <li className="menu-item">
          <Link className="links" to="/terceros">
            <div className="item-no-active icono-third pt-2">TERCEROS</div>
          </Link>
        </li>
      </ul>
    </nav>
  );
};

export default NavBar;
