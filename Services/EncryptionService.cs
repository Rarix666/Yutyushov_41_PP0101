using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Services
{
    internal class EncryptionService
    {
        private readonly byte[] _key; // 32 байта для AES-256

        public EncryptionService(byte[] key)
        {
            if (key.Length != 32)
                throw new ArgumentException("Ключ должен быть 32 байта для AES-256");
            _key = key;
        }

        /// <summary>
        /// Шифрует данные. Возвращает массив: [IV 16 байт] + [зашифрованные данные]
        /// </summary>
        public byte[] Encrypt(byte[] plainData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Пишем IV в начало
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainData, 0, plainData.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Расшифровывает данные, полученные методом Encrypt
        /// </summary>
        public byte[] Decrypt(byte[] encryptedData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                // Извлекаем IV (первые 16 байт)
                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var result = new MemoryStream())
                {
                    cs.CopyTo(result);
                    return result.ToArray();
                }
            }
        }
    }
}
