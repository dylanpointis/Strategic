// Detalle del SelectorPermisos: al elegir una familia en cualquiera de las
// dos listas muestra lo que contiene. El contenido ya viene armado y oculto
// desde el servidor, uno por familia, con su codigo en data-codigo.

var Strategic = Strategic || {};

Strategic.selectorPermisos = (function () {

    function iniciar(idDisponibles, idSeleccionados, idDetalle) {
        var detalle = document.getElementById(idDetalle);
        var listas = [document.getElementById(idDisponibles), document.getElementById(idSeleccionados)];

        if (!detalle || !listas[0] || !listas[1]) {
            return;
        }

        // Un permiso simple no tiene bloque propio: no se muestra nada
        function mostrar(codigo) {
            var bloques = detalle.querySelectorAll('[data-codigo]');

            for (var i = 0; i < bloques.length; i++) {
                bloques[i].hidden = bloques[i].getAttribute('data-codigo') !== codigo;
            }
        }

        listas.forEach(function (lista, i) {
            lista.addEventListener('change', function () {
                // Se elige en una sola lista a la vez
                listas[1 - i].selectedIndex = -1;
                mostrar(lista.value);
            });
        });

        // Despues de agregar o quitar queda elegido el elemento que se movio
        for (var i = 0; i < listas.length; i++) {
            if (listas[i].value) {
                mostrar(listas[i].value);
                break;
            }
        }
    }

    return { iniciar: iniciar };
})();
