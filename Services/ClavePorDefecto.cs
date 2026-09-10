using System.Globalization;
using System.Text;

namespace Services
{
    /// <summary>
    /// Arma la contraseña con la que nace un usuario dado de alta desde el
    /// CU-005-018. El formato definido es nombre.apellido en minúsculas, sin
    /// espacios ni acentos: "Juan Pérez" queda como "juan.perez".
    /// </summary>
    public static class ClavePorDefecto
    {
        public static string Generar(string nombre, string apellido)
        {
            return string.Concat(Normalizar(nombre), ".", Normalizar(apellido));
        }

        private static string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            // FormD separa cada letra de su acento y despues se descartan los
            // acentos, que quedan como marcas sin espaciado
            string descompuesto = valor.Trim().ToLower(CultureInfo.InvariantCulture).Normalize(NormalizationForm.FormD);
            StringBuilder limpio = new StringBuilder();

            foreach (char letra in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letra) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsWhiteSpace(letra))
                {
                    continue;
                }

                limpio.Append(letra);
            }

            return limpio.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
