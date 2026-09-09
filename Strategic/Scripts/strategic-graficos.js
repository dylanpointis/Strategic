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

    function opcionesBase(prefijoValor) {
        return {
            responsive: true,
            maintainAspectRatio: false,
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
                x: {
                    grid: { color: colores.grilla },
                    ticks: { color: colores.texto, maxRotation: 0, autoSkip: true }
                },
                y: {
                    beginAtZero: false,
                    grid: { color: colores.grilla },
                    ticks: {
                        color: colores.texto,
                        callback: function (valor) { return prefijoValor + valor; }
                    }
                }
            }
        };
    }

    function dibujar(idCanvas, tipo, datos, etiquetaSerie, prefijoValor) {
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
                    borderWidth: 2,
                    fill: tipo === 'line',
                    tension: 0.25
                }]
            },
            options: opcionesBase(prefijoValor)
        });
    }

    return {
        linea: function (idCanvas, datos, etiquetaSerie, prefijoValor) {
            return dibujar(idCanvas, 'line', datos, etiquetaSerie, prefijoValor);
        },
        barras: function (idCanvas, datos, etiquetaSerie, prefijoValor) {
            return dibujar(idCanvas, 'bar', datos, etiquetaSerie, prefijoValor);
        }
    };
})();
