using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BLL
{
    // Digito verificador de las tablas controladas (las que figuran en la
    // tabla DigitoVerificador). El DVH global es el hash de todas las filas
    // concatenadas y el DVV global el de todas las columnas, como en el
    // esquema clasico. Ademas se guarda un DVH por fila y un DVV por columna
    // para poder decir que registro y que campo cambiaron.
    public class BLLDigitoVerificador
    {
        private const string ModuloIntegridad = "Integridad";
        private const int CriticidadGrave = 3;
        private const string SeparadorClave = "|";

        private readonly DALDigitoVerificador dalDigitoVerificador = new DALDigitoVerificador();

        // Calcula el DV completo de una tabla a partir de sus datos
        public BEDigitoVerificador Calcular(DataTable tabla, List<string> columnasClave)
        {
            BEDigitoVerificador dv = new BEDigitoVerificador();
            dv.Tabla = tabla.TableName;

            // DVH: cada fila concatena sus columnas; el global concatena las filas
            StringBuilder horizontal = new StringBuilder();

            foreach (DataRow fila in tabla.Rows)
            {
                StringBuilder cadenaFila = new StringBuilder();

                foreach (DataColumn columna in tabla.Columns)
                {
                    cadenaFila.Append(Texto(fila[columna]));
                }

                horizontal.Append(cadenaFila);
                dv.Filas.Add(new BEDigitoVerificadorFila(
                    dv.Tabla,
                    ArmarClave(fila, columnasClave),
                    Encriptacion.EncriptarSHA256(cadenaFila.ToString())));
            }

            dv.DVH = Encriptacion.EncriptarSHA256(horizontal.ToString());

            // DVV: cada columna concatena sus valores; el global concatena las columnas
            StringBuilder vertical = new StringBuilder();

            foreach (DataColumn columna in tabla.Columns)
            {
                StringBuilder cadenaColumna = new StringBuilder();

                foreach (DataRow fila in tabla.Rows)
                {
                    cadenaColumna.Append(Texto(fila[columna]));
                }

                vertical.Append(cadenaColumna);
                dv.Columnas.Add(new BEDigitoVerificadorColumna(
                    dv.Tabla,
                    columna.ColumnName,
                    Encriptacion.EncriptarSHA256(cadenaColumna.ToString())));
            }

            dv.DVV = Encriptacion.EncriptarSHA256(vertical.ToString());

            return dv;
        }

        // Recalcula y guarda el DV de una tabla. Se llama despues de cada
        // escritura que hace el sistema sobre esa tabla
        public void PersistirDV(string tabla)
        {
            dalDigitoVerificador.PersistirDV(CalcularActual(tabla));
        }

        public List<BEDigitoVerificador> TraerTablaDV()
        {
            return dalDigitoVerificador.TraerTablaDV();
        }

        // Compara el DV guardado de cada tabla con el calculado sobre los datos
        // actuales. Devuelve las inconsistencias encontradas (vacia si esta todo
        // bien). Una tabla que todavia no tiene DV guardado (base recien creada
        // o datos de prueba recien cargados) se inicializa con lo que hay.
        public List<BEInconsistenciaDV> CompararDV()
        {
            List<BEInconsistenciaDV> inconsistencias = new List<BEInconsistenciaDV>();

            foreach (BEDigitoVerificador guardado in dalDigitoVerificador.TraerTablaDV())
            {
                BEDigitoVerificador calculado = CalcularActual(guardado.Tabla);

                if (!guardado.Calculado)
                {
                    dalDigitoVerificador.PersistirDV(calculado);
                    continue;
                }

                if (guardado.DVH == calculado.DVH && guardado.DVV == calculado.DVV)
                {
                    continue;
                }

                inconsistencias.AddRange(Detallar(guardado.Tabla, calculado));
            }

            return inconsistencias;
        }

        // Toma los datos actuales como correctos y vuelve a calcular el DV de
        // todas las tablas controladas
        public void RecalcularDV(string nombreUsuario)
        {
            new BLLEvento().RegistrarEvento(new Evento(
                nombreUsuario,
                ModuloIntegridad,
                "Digito verificador recalculado",
                CriticidadGrave));

            foreach (BEDigitoVerificador dv in dalDigitoVerificador.TraerTablaDV())
            {
                PersistirDV(dv.Tabla);
            }
        }


        // Con los DVH por fila y DVV por columna guardados arma el detalle de
        // que registros se modificaron, agregaron o eliminaron y, cuando se
        // puede, en que columnas
        private List<BEInconsistenciaDV> Detallar(string tabla, BEDigitoVerificador calculado)
        {
            List<BEInconsistenciaDV> lista = new List<BEInconsistenciaDV>();

            Dictionary<string, string> filasGuardadas = dalDigitoVerificador.TraerDVHFilas(tabla)
                .ToDictionary(f => f.Clave, f => f.DVH);
            Dictionary<string, string> columnasGuardadas = dalDigitoVerificador.TraerDVVColumnas(tabla)
                .ToDictionary(c => c.Columna, c => c.DVV);

            List<string> modificadas = new List<string>();
            List<string> agregadas = new List<string>();

            foreach (BEDigitoVerificadorFila fila in calculado.Filas)
            {
                string dvhGuardado;

                if (!filasGuardadas.TryGetValue(fila.Clave, out dvhGuardado))
                {
                    agregadas.Add(fila.Clave);
                }
                else if (dvhGuardado != fila.DVH)
                {
                    modificadas.Add(fila.Clave);
                }
            }

            List<string> eliminadas = filasGuardadas.Keys
                .Except(calculado.Filas.Select(f => f.Clave))
                .ToList();

            // Las columnas solo se pueden senialar si no hubo altas ni bajas de
            // filas: una fila de mas o de menos cambia el DVV de todas las columnas
            string columnas = string.Empty;

            if (agregadas.Count == 0 && eliminadas.Count == 0)
            {
                columnas = string.Join(", ", calculado.Columnas
                    .Where(c => columnasGuardadas.ContainsKey(c.Columna) && columnasGuardadas[c.Columna] != c.DVV)
                    .Select(c => c.Columna));
            }

            foreach (string clave in modificadas)
            {
                lista.Add(new BEInconsistenciaDV(tabla, clave, columnas, "Registro modificado fuera del sistema"));
            }

            foreach (string clave in agregadas)
            {
                lista.Add(new BEInconsistenciaDV(tabla, clave, string.Empty, "Registro agregado fuera del sistema"));
            }

            foreach (string clave in eliminadas)
            {
                lista.Add(new BEInconsistenciaDV(tabla, clave, string.Empty, "Registro eliminado fuera del sistema"));
            }

            if (lista.Count == 0)
            {
                // El global no coincide pero el detalle por fila si: paso algo con
                // los DV guardados (por ejemplo, se borraron los de las filas)
                lista.Add(new BEInconsistenciaDV(tabla, string.Empty, columnas, "El digito verificador de la tabla no coincide"));
            }

            return lista;
        }

        private BEDigitoVerificador CalcularActual(string tabla)
        {
            return Calcular(
                dalDigitoVerificador.TraerTabla(tabla),
                dalDigitoVerificador.TraerColumnasClave(tabla));
        }

        private string ArmarClave(DataRow fila, List<string> columnasClave)
        {
            if (columnasClave == null || columnasClave.Count == 0)
            {
                return fila.Table.Rows.IndexOf(fila).ToString();
            }

            return string.Join(SeparadorClave, columnasClave.Select(c => Texto(fila[c])));
        }

        // Todos los valores se pasan a texto con cultura invariante, para que el
        // hash no dependa de la configuracion regional del servidor
        private static string Texto(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(valor, CultureInfo.InvariantCulture);
        }
    }
}
