using System;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public class Encriptacion
    {
        // PBKDF2 con HMAC-SHA256: 100.000 iteraciones, sal de 16 bytes y clave
        // derivada de 32 bytes. Se usa solo para las claves de los usuarios
        private const int Iteraciones = 100000;
        private const int BytesSal = 16;
        private const int BytesHash = 32;

        // Genera un salt aleatorio para una nueva contraseña.
        // Luego llama a Derivar para obtener el hash mediante PBKDF2.
        // Devuelve el hash y devuelve el salt mediante el parámetro out.
        // Ambos valores se almacenan en la base de datos.
        public static string EncriptarPBKDF2(string clave, out string salt)
        {
            byte[] bytesSal = new byte[BytesSal];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytesSal);
            }

            salt = Convert.ToBase64String(bytesSal);
            return Derivar(clave, bytesSal);
        }

        // Verifica una contraseña utilizando el salt almacenado en la base de datos.
        // Vuelve a derivar el hash mediante PBKDF2 y lo compara
        // con el hash guardado en la base de datos.
        public static bool VerificarPBKDF2(string clave, string salt, string hashGuardado)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hashGuardado))
            {
                return false;
            }

            string hashGenerado = Derivar(clave, Convert.FromBase64String(salt));

            if (hashGenerado == hashGuardado)
            {
                return true;
            }

            return false;
        }

        private static string Derivar(string clave, byte[] bytesSal)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(clave, bytesSal, Iteraciones, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(BytesHash));
            }
        }

        // SHA256 usado para digito verificador
        public static string EncriptarSHA256(string clave)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(clave));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
