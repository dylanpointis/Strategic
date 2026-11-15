// Comportamiento del CampoClave en el navegador: muestra u oculta lo escrito,
// marca los requisitos de la clave que se van cumpliendo y avisa si la
// confirmacion coincide. Los textos vienen en los atributos data- del control.

var Strategic = Strategic || {};

Strategic.campoClave = (function () {

    // Las mismas reglas que valida el servidor
    var reglas = {
        largo: function (v) { return v.length >= 8; },
        mayuscula: function (v) { return /[A-ZÁÉÍÓÚÜÑ]/.test(v); },
        numero: function (v) { return /[0-9]/.test(v); },
        especial: function (v) { return /[^A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ]/.test(v); }
    };

    function iniciar(idContenedor) {
        var contenedor = document.getElementById(idContenedor);

        if (!contenedor) {
            return;
        }

        var principal = contenedor.querySelector('[data-campo-clave="principal"]');
        var confirmacion = contenedor.querySelector('[data-campo-clave="confirmacion"]');
        var aviso = contenedor.querySelector('[data-coincide]');
        var botones = contenedor.querySelectorAll('[data-ver]');

        // Boton mostrar/ocultar de cada campo
        Array.prototype.forEach.call(botones, function (boton) {
            var campo = contenedor.querySelector('[data-campo-clave="' + boton.getAttribute('data-ver') + '"]');

            if (!campo) {
                return;
            }

            boton.textContent = contenedor.getAttribute('data-mostrar');

            boton.addEventListener('click', function () {
                var oculto = campo.type === 'password';

                campo.type = oculto ? 'text' : 'password';
                boton.textContent = contenedor.getAttribute(oculto ? 'data-ocultar' : 'data-mostrar');
                campo.focus();
            });
        });

        function revisarRequisitos() {
            var items = contenedor.querySelectorAll('[data-regla]');

            Array.prototype.forEach.call(items, function (item) {
                var cumple = reglas[item.getAttribute('data-regla')](principal.value);
                item.classList.toggle('cumple', cumple);
            });
        }

        function revisarCoincidencia() {
            if (!confirmacion || !aviso) {
                return;
            }

            if (!confirmacion.value) {
                aviso.textContent = '';
                aviso.className = 'campo-clave-coincide';
                return;
            }

            var coinciden = confirmacion.value === principal.value;

            aviso.textContent = contenedor.getAttribute(coinciden ? 'data-coinciden' : 'data-no-coinciden');
            aviso.className = 'campo-clave-coincide ' + (coinciden ? 'coinciden' : 'no-coinciden');
        }

        if (principal) {
            principal.addEventListener('input', function () {
                revisarRequisitos();
                revisarCoincidencia();
            });
        }

        if (confirmacion) {
            confirmacion.addEventListener('input', revisarCoincidencia);
        }

        revisarRequisitos();
        revisarCoincidencia();
    }

    return { iniciar: iniciar };
})();
