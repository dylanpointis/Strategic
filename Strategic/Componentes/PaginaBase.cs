using Services;
using System;
using System.Web.UI;

namespace Strategic.Componentes
{

    /// Página base de todas las pantallas. Se suscribe al Traductor para
    /// enterarse si el usuario cambia el idioma mientras la está viendo.
    ///
    /// Los textos del aspx escritos con Traductor.T se resuelven solos al
    /// dibujar la página. ActualizarIdioma es para los que se ponen desde
    /// código: textos de botones, labels, validadores, opciones de combos.

    public abstract class PaginaBase : Page, IIdiomaObserver
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Traductor.Suscribir(this);
            ActualizarIdioma();
        }

        public virtual void ActualizarIdioma()
        {
        }
    }
}
