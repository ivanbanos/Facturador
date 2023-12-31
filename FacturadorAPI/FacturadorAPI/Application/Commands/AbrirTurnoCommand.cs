﻿using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AbrirTurnoCommand : IRequest
    {
        public AbrirTurnoCommand(int isla, string codigo)
        {
            Isla = isla;
            Codigo = codigo;
        }

        public int Isla { get; }
        public string Codigo { get; }
    }
}
