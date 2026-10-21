using System;
using System.Configuration;
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

        // AES-256 en modo CBC para datos que hay que volver a leer, como el
        // numero de operacion de la suscripcion. Cada encriptacion usa un
        // vector de inicializacion aleatorio que se guarda adelante del texto
        // encriptado; el resultado va en base64. La clave sale del Web.config
        public static string EncriptarAES(string texto)
        {
            using (Aes aes = CrearAES())
            {
                aes.GenerateIV();

                byte[] bytesTexto = Encoding.UTF8.GetBytes(texto);
                byte[] encriptado;

                using (ICryptoTransform encriptador = aes.CreateEncryptor())
                {
                    encriptado = encriptador.TransformFinalBlock(bytesTexto, 0, bytesTexto.Length);
                }

                byte[] resultado = new byte[aes.IV.Length + encriptado.Length];
                Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
                Buffer.BlockCopy(encriptado, 0, resultado, aes.IV.Length, encriptado.Length);

                return Convert.ToBase64String(resultado);
            }
        }

        public static string DesencriptarAES(string textoEncriptado)
        {
            byte[] datos = Convert.FromBase64String(textoEncriptado);

            using (Aes aes = CrearAES())
            {
                // Los primeros 16 bytes son el vector de inicializacion
                byte[] iv = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(datos, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (ICryptoTransform desencriptador = aes.CreateDecryptor())
                {
                    byte[] bytesTexto = desencriptador.TransformFinalBlock(datos, iv.Length, datos.Length - iv.Length);
                    return Encoding.UTF8.GetString(bytesTexto);
                }
            }
        }

        private static Aes CrearAES()
        {
            string clave = ConfigurationManager.AppSettings["ClaveAES"];

            if (string.IsNullOrEmpty(clave))
            {
                throw new InvalidOperationException("Falta la clave ClaveAES en el Web.config");
            }

            Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Convert.FromBase64String(clave);

            return aes;
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
