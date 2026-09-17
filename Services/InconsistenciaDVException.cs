using BE;
using System;
using System.Collections.Generic;

namespace Services
{
    // Se lanza en el login cuando el digito verificador no coincide. Lleva el
    // usuario que intentaba entrar, para decidir si puede reparar, y el
    // detalle de lo que no coincide
    public class InconsistenciaDVException : Exception
    {
        public BEUsuario Usuario { get; private set; }
        public List<BEInconsistenciaDV> Inconsistencias { get; private set; }

        public InconsistenciaDVException(BEUsuario usuario, List<BEInconsistenciaDV> inconsistencias)
            : base("Se detectaron inconsistencias en los datos del sistema")
        {
            Usuario = usuario;
            Inconsistencias = inconsistencias;
        }
    }
}
