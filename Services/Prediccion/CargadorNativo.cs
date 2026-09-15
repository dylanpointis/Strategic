using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Services.Prediccion
{
    
    /// ML.NET calcula con una librería nativa (CpuMathNative.dll) que trae en
    /// dos versiones, x64 y x86. Una aplicación web no puede cargarla sola:
    /// Windows busca las nativas en la carpeta del proceso (IIS Express), no
    /// en el bin del sitio. Acá se carga a mano, eligiendo la versión que
    /// corresponde al proceso, antes del primer uso.
    
    internal static class CargadorNativo
    {
        private const string NombreLibreria = "CpuMathNative.dll";

        private static readonly object candado = new object();
        private static bool cargada;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string ruta);

        public static void AsegurarCpuMath()
        {
            lock (candado)
            {
                if (cargada)
                {
                    return;
                }

                string arquitectura = Environment.Is64BitProcess ? "x64" : "x86";

                foreach (string ruta in Candidatas(arquitectura))
                {
                    if (File.Exists(ruta) && LoadLibrary(ruta) != IntPtr.Zero)
                    {
                        cargada = true;
                        return;
                    }
                }

                throw new InvalidOperationException(string.Format(
                    "No se encontro {0} para {1}. Tiene que estar en la carpeta {1} junto a las dll de la aplicacion.",
                    NombreLibreria, arquitectura));
            }
        }

        private static IEnumerable<string> Candidatas(string arquitectura)
        {
            foreach (string carpeta in CarpetasBase())
            {
                yield return Path.Combine(carpeta, arquitectura, NombreLibreria);
                yield return Path.Combine(carpeta, NombreLibreria);
                yield return Path.Combine(carpeta, "runtimes", "win-" + arquitectura, "nativeassets", "netstandard2.0", NombreLibreria);
            }
        }

        private static IEnumerable<string> CarpetasBase()
        {
            // En un sitio web las dll viven en bin; en una consola, junto al exe
            string bin = AppDomain.CurrentDomain.RelativeSearchPath;

            if (!string.IsNullOrEmpty(bin))
            {
                yield return bin;
            }

            yield return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
