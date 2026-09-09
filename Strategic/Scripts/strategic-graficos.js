// Helpers de graficos de Strategic sobre Chart.js.
// Las paginas le pasan un objeto { etiquetas: [], valores: [] } serializado
// desde el code-behind y estos metodos arman el grafico con la paleta del sistema.

var Strategic = Strategic || {};

Strategic.grafico = (function () {

    var colores = {
        principal: '#5b8fd9',
        relleno: 'rgba(91, 143, 217, 0.16)',
        punto: '#5b8fd9',
        grilla: '#e3ebf5',
        texto: '#6b7f99'
    };

    // Evita romper la pagina si el CDN de Chart.js no cargo o si el canvas no existe
    function obtenerContexto(idCanvas) {
        if (typeof Chart === 'undefined') {
            return null;
        }

        var canvas = document.getElementById(idCanvas);

        if (!canvas) {
            return null;
        }

        return canvas.getContext('2d');
    }

    // Eje que muestra los valores: lleva el prefijo ($, etc) y arranca en cero en las barras.
    // Si todos los valores son enteros (unidades, cantidades) se evitan los decimales
    // en las marcas del eje, que quedarian como 0,5 - 1,5 - 2,5
    function ejeDeValores(prefijoValor, arrancaEnCero, soloEnteros) {
        var marcas = {
            color: colores.texto,
            callback: function (valor) { return prefijoValor + valor; }
        };

        if (soloEnteros) {
            marcas.precision = 0;
        }

        return {
            beginAtZero: arrancaEnCero,
            grid: { color: colores.grilla },
            ticks: marcas
        };
    }

    function todosEnteros(valores) {
        for (var i = 0; i < valores.length; i++) {
            if (valores[i] % 1 !== 0) {
                return false;
            }
        }

        return true;
    }

    // Eje que muestra las etiquetas (fechas, categorias, nombres de producto)
    function ejeDeEtiquetas() {
        return {
            grid: { color: colores.grilla },
            ticks: { color: colores.texto, maxRotation: 0, autoSkip: true }
        };
    }

    function armarOpciones(tipo, horizontal, prefijoValor, soloEnteros) {
        var valores = ejeDeValores(prefijoValor, tipo === 'bar', soloEnteros);
        var etiquetas = ejeDeEtiquetas();

        return {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: horizontal ? 'y' : 'x',
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (contexto) {
                            return prefijoValor + contexto.formattedValue;
                        }
                    }
                }
            },
            scales: {
                x: horizontal ? valores : etiquetas,
                y: horizontal ? etiquetas : valores
            }
        };
    }

    function dibujar(idCanvas, tipo, horizontal, datos, etiquetaSerie, prefijoValor) {
        var contexto = obtenerContexto(idCanvas);

        if (!contexto || !datos || !datos.etiquetas || datos.etiquetas.length === 0) {
            return null;
        }

        prefijoValor = prefijoValor || '';

        return new Chart(contexto, {
            type: tipo,
            data: {
                labels: datos.etiquetas,
                datasets: [{
                    label: etiquetaSerie,
                    data: datos.valores,
                    borderColor: colores.principal,
                    backgroundColor: tipo === 'bar' ? colores.principal : colores.relleno,
                    pointBackgroundColor: colores.punto,
                    pointRadius: 3,
                    borderWidth: tipo === 'bar' ? 0 : 2,
                    borderRadius: tipo === 'bar' ? 4 : 0,
                    fill: tipo === 'line',
                    tension: 0.25
                }]
            },
            options: armarOpciones(tipo, horizontal, prefijoValor, todosEnteros(datos.valores))
        });
    }

    // La torta necesita un color por porcion. Se combinan los dos colores
    // institucionales (azul y magenta) con sus variantes claras.
    function coloresDePorciones(cantidad) {
        var paleta = ['#5b8fd9', '#c6128f', '#3f72b8', '#e7a9d0', '#8fb8e8', '#98a7b8'];
        var asignados = [];

        for (var i = 0; i < cantidad; i++) {
            asignados.push(paleta[i % paleta.length]);
        }

        return asignados;
    }

    function sumar(valores) {
        var total = 0;

        for (var i = 0; i < valores.length; i++) {
            total = total + valores[i];
        }

        return total;
    }

    // Grafico de torta: sirve para relaciones parte-todo, como las unidades
    // vendidas repartidas entre las categorias de producto.
    function dibujarTorta(idCanvas, datos, prefijoValor) {
        var contexto = obtenerContexto(idCanvas);

        if (!contexto || !datos || !datos.etiquetas || datos.etiquetas.length === 0) {
            return null;
        }

        prefijoValor = prefijoValor || '';

        var total = sumar(datos.valores);

        return new Chart(contexto, {
            type: 'doughnut',
            data: {
                labels: datos.etiquetas,
                datasets: [{
                    data: datos.valores,
                    backgroundColor: coloresDePorciones(datos.valores.length),
                    borderColor: '#ffffff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '55%',
                plugins: {
                    legend: {
                        display: true,
                        position: 'right',
                        labels: {
                            color: colores.texto,
                            usePointStyle: true,
                            pointStyle: 'circle',
                            padding: 14,
                            boxWidth: 8
                        }
                    },
                    tooltip: {
                        callbacks: {
                            // El valor de una porcion se lee mejor acompanado del porcentaje
                            label: function (contexto) {
                                var valor = contexto.parsed;
                                var porcentaje = total > 0 ? Math.round((valor / total) * 1000) / 10 : 0;

                                return contexto.label + ': ' + prefijoValor + valor + ' (' + porcentaje + '%)';
                            }
                        }
                    }
                }
            }
        });
    }

    return {
        linea: function (idCanvas, datos, etiquetaSerie, prefijoValor) {
            return dibujar(idCanvas, 'line', false, datos, etiquetaSerie, prefijoValor);
        },
        barras: function (idCanvas, datos, etiquetaSerie, prefijoValor) {
            return dibujar(idCanvas, 'bar', false, datos, etiquetaSerie, prefijoValor);
        },
        barrasHorizontales: function (idCanvas, datos, etiquetaSerie, prefijoValor) {
            return dibujar(idCanvas, 'bar', true, datos, etiquetaSerie, prefijoValor);
        },
        torta: function (idCanvas, datos, prefijoValor) {
            return dibujarTorta(idCanvas, datos, prefijoValor);
        }
    };
})();
