namespace Services
{
    // Observer del cambio de idioma. Lo implementan la pagina actual, el
    // master y los componentes que ponen textos desde codigo
    public interface IIdiomaObserver
    {
        void ActualizarIdioma();
    }
}
