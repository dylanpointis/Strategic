using BE;
using DAL;
using System.Collections.Generic;

namespace BLL
{
    public class BLLRol
    {
        private readonly DALRol dalRol = new DALRol();

        public List<BERol> TraerListaRoles()
        {
            return dalRol.TraerListaRoles();
        }
    }
}
