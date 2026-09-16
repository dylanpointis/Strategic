using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BLL
{
    // CU-008-039 Realizar Respaldo (Backup) y CU-008-040 Realizar Restore
    public class BLLRespaldo
    {
        private const string ModuloRespaldos = "Respaldos";

        // Criticidad de los eventos: 1 informativo, 2 cambio de datos, 3 grave
        private const int CriticidadInformativa = 1;
        private const int CriticidadGrave = 3;

        private const string PrefijoArchivo = "Strategic_Backup_";
        private const string Extension = ".bak";

        // Cantidad de respaldos que se conservan en la carpeta. Al generar uno
        // nuevo se borran los mas viejos que sobren
        public const int MaximoRespaldos = 30;

        private readonly DALRespaldo dalRespaldo = new DALRespaldo();
        private readonly BLLEvento bllEvento = new BLLEvento();

        // Genera el .bak en la carpeta indicada y devuelve sus datos
        public BERespaldo RealizarBackup(string carpeta, string nombreUsuario)
        {
            ValidarCarpeta(carpeta);

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string nombreArchivo = PrefijoArchivo + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Extension;
            string ruta = Path.Combine(carpeta, nombreArchivo);

            dalRespaldo.RealizarBackup(ruta);

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuario,
                ModuloRespaldos,
                "Backup realizado: " + nombreArchivo,
                CriticidadInformativa));

            BorrarRespaldosViejos(carpeta);

            return Armar(new FileInfo(ruta));
        }

        // Respaldos que hay en la carpeta, del mas nuevo al mas viejo
        public List<BERespaldo> TraerRespaldos(string carpeta)
        {
            ValidarCarpeta(carpeta);

            if (!Directory.Exists(carpeta))
            {
                return new List<BERespaldo>();
            }

            return new DirectoryInfo(carpeta)
                .GetFiles(PrefijoArchivo + "*" + Extension)
                .Select(Armar)
                .OrderByDescending(respaldo => respaldo.Fecha)
                .ToList();
        }

        // Busca un respaldo por nombre dentro de la carpeta. El nombre viene de
        // la pantalla, asi que no se acepta nada que salga de la carpeta
        public BERespaldo TraerRespaldo(string carpeta, string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo) || nombreArchivo != Path.GetFileName(nombreArchivo))
            {
                return null;
            }

            return TraerRespaldos(carpeta)
                .FirstOrDefault(respaldo => string.Equals(respaldo.NombreArchivo, nombreArchivo, StringComparison.OrdinalIgnoreCase));
        }

        // Reemplaza la base actual por el contenido del archivo .bak
        public void RealizarRestore(string ruta, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
            {
                throw new Exception("No se encontró el archivo de respaldo");
            }

            if (!string.Equals(Path.GetExtension(ruta), Extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("El archivo de respaldo debe tener extensión .bak");
            }

            dalRespaldo.RealizarRestore(ruta);

            // La bitacora vuelve al estado del respaldo, por eso el evento se
            // registra despues de restaurar
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuario,
                ModuloRespaldos,
                "Restore realizado desde: " + Path.GetFileName(ruta),
                CriticidadGrave));
        }

        private void BorrarRespaldosViejos(string carpeta)
        {
            List<BERespaldo> sobrantes = TraerRespaldos(carpeta).Skip(MaximoRespaldos).ToList();

            foreach (BERespaldo respaldo in sobrantes)
            {
                File.Delete(respaldo.Ruta);
            }
        }

        private void ValidarCarpeta(string carpeta)
        {
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                throw new Exception("No está configurada la carpeta de respaldos");
            }
        }

        private BERespaldo Armar(FileInfo archivo)
        {
            BERespaldo respaldo = new BERespaldo();
            respaldo.NombreArchivo = archivo.Name;
            respaldo.Ruta = archivo.FullName;
            respaldo.Fecha = archivo.LastWriteTime;
            respaldo.TamanioBytes = archivo.Length;
            return respaldo;
        }
    }
}
