// Buscador de los selectores de Strategic (SelectorProducto y SelectorCompetidor).
// Mientras el usuario escribe, deja en el desplegable solo las opciones que
// coinciden. Cada opcion trae en data-busqueda el texto por el que se la busca.

var Strategic = Strategic || {};

Strategic.selector = (function () {

    // Minusculas y sin tildes, para que "camara" encuentre "Cámara"
    function normalizar(texto) {
        return (texto || '').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '');
    }

    function iniciar(idBuscador, idLista, idAviso) {
        var buscador = document.getElementById(idBuscador);
        var lista = document.getElementById(idLista);
        var aviso = document.getElementById(idAviso);

        if (!buscador || !lista) {
            return;
        }

        // Se guarda una copia de todas las opciones: el filtro las saca y las
        // vuelve a poner, sin crear ninguna nueva
        var opciones = Array.prototype.slice.call(lista.options);
        var vacia = opciones.length > 0 && opciones[0].value === '' ? opciones[0] : null;

        function filtrar() {
            var texto = normalizar(buscador.value.trim());
            var seleccionada = lista.options[lista.selectedIndex];
            var coincidencias = 0;

            while (lista.options.length > 0) {
                lista.remove(0);
            }

            opciones.forEach(function (opcion) {
                var coincide = opcion !== vacia && normalizar(opcion.getAttribute('data-busqueda') || opcion.text).indexOf(texto) >= 0;

                if (coincide) {
                    coincidencias++;
                }

                // La opcion vacia y la elegida quedan siempre, para no perder la seleccion
                if (opcion === vacia || opcion === seleccionada || coincide) {
                    lista.add(opcion);
                }
            });

            if (seleccionada) {
                lista.value = seleccionada.value;
            }

            if (aviso) {
                aviso.textContent = texto === ''
                    ? ''
                    : coincidencias === 0
                        ? aviso.getAttribute('data-sin-coincidencias')
                        : coincidencias === 1
                            ? aviso.getAttribute('data-una-coincidencia')
                            : aviso.getAttribute('data-coincidencias').replace('{0}', coincidencias);
            }
        }

        buscador.addEventListener('input', filtrar);

        // Enter en el buscador no envia el formulario
        buscador.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
            }
        });

        // Despues de un postback el buscador conserva el texto: se vuelve a aplicar
        if (buscador.value.trim() !== '') {
            filtrar();
        }
    }

    return {
        iniciar: iniciar
    };
})();
