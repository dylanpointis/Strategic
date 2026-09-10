using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class DALRol
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BERol> TraerListaRoles()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerListaRoles", null);
            List<BERol> roles = new List<BERol>();

            foreach (DataRow row in tabla.Rows)
            {
                roles.Add(new BERol(
                    Convert.ToInt32(row["CodRol"]),
                    row["Nombre"].ToString()));
            }

            return roles;
        }
    }
}
