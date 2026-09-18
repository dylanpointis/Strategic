using BE;
using DAL;
using Microsoft.Win32;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BLL
{
    // Digito verificador de las tablas controladas (las que figuran en la
    // tabla DigitoVerificador). El DVH global es el hash de todas las filas
    // concatenadas y el DVV global el de todas las columnas.
    //Ademas se guarda un DVH por fila y un DVV por columna
    // para poder decir que registro y que campo cambiaron.

    // - DVH (horizontal): permite detectar cambios en los registros.
    // - DVV (vertical): permite detectar cambios en las columnas.
    public class BLLDigitoVerificador
    {
        private const string ModuloIntegridad = "Integridad";
        private const int CriticidadGrave = 3;
        private const string SeparadorClave = "|";

        private readonly DALDigitoVerificador dalDigitoVerificador = new DALDigitoVerificador();

        // Calcula el DV completo de una tabla a partir de sus datos
        //DVH: Se concatenan los valores de cada fila y se calcula un hash SHA256. Se guarda un DVH global de la tabla y un DVH de cada fila
        //DVV: Se concatenan los valores de cada columna y se calcula un hash SHA256. Se guarda un DVV global de la tabla y un DVV de cada columna
        public BEDigitoVerificador Calcular(DataTable tabla, List<string> columnasClave)
        {
            BEDigitoVerificador dv = new BEDigitoVerificador();
            dv.Tabla = tabla.TableName;

            // DVH: cada fila concatena sus columnas; el global concatena las filas
            StringBuilder horizontal = new StringBuilder();

            // Cálculo del DVH
            // Se recorren las filas y se concatenan sus columnas.
            foreach (DataRow fila in tabla.Rows)
            {
                StringBuilder cadenaFila = new StringBuilder();

                foreach (DataColumn columna in tabla.Columns)
                {
                    cadenaFila.Append(Texto(fila[columna]));
                }
                //guarda el DVH individual de la fila.
                horizontal.Append(cadenaFila);
                dv.Filas.Add(new BEDigitoVerificadorFila(
                    dv.Tabla,
                    ArmarClave(fila, columnasClave),
                    Encriptacion.EncriptarSHA256(cadenaFila.ToString())));
            }

            //guarda el DVH global de la tabla.
            dv.DVH = Encriptacion.EncriptarSHA256(horizontal.ToString());

            // Cálculo del DVV
            // DVV: cada columna concatena sus valores; el global concatena las columnas
            StringBuilder vertical = new StringBuilder();
            // Se recorre cada columna y se concatenan los valores de todos sus registros
            foreach (DataColumn columna in tabla.Columns)
            {
                StringBuilder cadenaColumna = new StringBuilder();

                foreach (DataRow fila in tabla.Rows)
                {
                    cadenaColumna.Append(Texto(fila[columna]));
                }

                vertical.Append(cadenaColumna);
                //guarda el DVV individual de la columna.
                dv.Columnas.Add(new BEDigitoVerificadorColumna(
                    dv.Tabla,
                    columna.ColumnName,
                    Encriptacion.EncriptarSHA256(cadenaColumna.ToString())));
            }
            //guardar el DVV global de la tabla.
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

        // Compara el DV guardado de cada tabla con el calculado sobre los datos actuales. 
        // Si ambos coinciden, se considera que no se detectaron cambios.
        // Si no coinciden devuelve las inconsistencias encontradas (lista vacia si no hay). 
        // Una tabla que todavia no tiene DV guardado (base recien creadas) se inicializa con lo que hay.
        public List<BEInconsistenciaDV> CompararDV()
        {
            List<BEInconsistenciaDV> inconsistencias = new List<BEInconsistenciaDV>();

            foreach (BEDigitoVerificador guardado in dalDigitoVerificador.TraerTablaDV())
            {
                // Calcula nuevamente los DVH y DVV utilizando los datos actuales de la tabla.
                BEDigitoVerificador calculado = CalcularActual(guardado.Tabla);

                // Si la tabla todavía no tenía un DV calculado, se inicializa con los datos actuales
                if (!guardado.Calculado)
                {
                    dalDigitoVerificador.PersistirDV(calculado);
                    continue;
                }

                // Si el DVH y el DVV global coinciden con los almacenados,no se detectaron modificaciones en la tabla.
                if (guardado.DVH == calculado.DVH && guardado.DVV == calculado.DVV)
                {
                    continue;
                }
                // Si los valores globales no coinciden, se analiza el detalle de
                // cada fila y columna para determinar dónde ocurrió la inconsistencia.
                inconsistencias.AddRange(Detallar(guardado.Tabla, calculado));
            }

            return inconsistencias;
        }

        // Toma los datos actuales como correctos y vuelve a calcular el DV de todas las tablas controladas
        public void RecalcularDV(string nombreUsuario)
        {
            new BLLEvento().RegistrarEvento(new Evento( nombreUsuario,ModuloIntegridad,"Digito verificador recalculado", CriticidadGrave));

            foreach (BEDigitoVerificador dv in dalDigitoVerificador.TraerTablaDV())
            {
                PersistirDV(dv.Tabla);
            }
        }

        // Analiza en detalle una tabla cuyo DVH o DVV global no coincide.
        // Compara los DVH de las filas almacenados anteriormente contra los actuales. 
        // y Lo mismo para los DVV de las columnas
        // Con los DVH de las filas se pueden detectar registros modificados, agregados o eliminados.
        // Con los DVV de las columnas se puede detectar qué columna (campo) cambió.
        private List<BEInconsistenciaDV> Detallar(string tabla, BEDigitoVerificador calculado)
        {
            List<BEInconsistenciaDV> lista = new List<BEInconsistenciaDV>();
            // Recupera los DVH almacenados anteriormente
            Dictionary<string, string> filasGuardadas = dalDigitoVerificador.TraerDVHFilas(tabla)
                .ToDictionary(f => f.Clave, f => f.DVH);
            // Recupera los DVV almacenados anteriormente
            Dictionary<string, string> columnasGuardadas = dalDigitoVerificador.TraerDVVColumnas(tabla)
                .ToDictionary(c => c.Columna, c => c.DVV);

            List<string> modificadas = new List<string>();
            List<string> agregadas = new List<string>();

            // Compara cada fila actual contra su DVH almacenado.
            foreach (BEDigitoVerificadorFila fila in calculado.Filas)
            {
                string dvhGuardado;
                // Si la clave (PK) no existía anteriormente, el registro fue agregado.
                if (!filasGuardadas.TryGetValue(fila.Clave, out dvhGuardado))
                {
                    agregadas.Add(fila.Clave);
                }
                // Si la clave existía pero el DVH cambió, el registro fue modificado.
                else if (dvhGuardado != fila.DVH)
                {
                    modificadas.Add(fila.Clave);
                }
            }

            //La clave estaba almacenada pero ahora no aparece en los datos actuales: fue eliminado.
            List<string> eliminadas = filasGuardadas.Keys
                .Except(calculado.Filas.Select(f => f.Clave))
                .ToList();

            // Las columnas solo se pueden identificar cuando no hubo altas ni bajas de registros. 
            //Ya que una fila agregada o eliminada modifica los valores de todas las columnas
            string columnas = string.Empty;

            if (agregadas.Count == 0 && eliminadas.Count == 0)
            {
                // Compara el DVV almacenado de cada columna contra el DVV calculado actualmente.
                columnas = string.Join(", ", calculado.Columnas
                    .Where(c => columnasGuardadas.ContainsKey(c.Columna) && columnasGuardadas[c.Columna] != c.DVV)
                    .Select(c => c.Columna));
            }

            // Informar registros modificados, agregados o eliminados (Agregar a la lista de inconsistencias)
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
            return Calcular(dalDigitoVerificador.TraerTabla(tabla), dalDigitoVerificador.TraerColumnasClave(tabla));
        }

        // Construye una clave que permite identificar un registro (PK o PK compuesta)
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
