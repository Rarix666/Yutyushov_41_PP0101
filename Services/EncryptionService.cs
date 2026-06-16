using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.Services
{
    public class EncryptionService //Сервис для шифрования и расшифрования документов
    {
        private const int IvSize = 16; //Размер IV для AES
        private readonly byte[] _key;

        public EncryptionService(byte[] key) //Проверка соответсвия количества байтов в массиве для алгоритма AES-256
        {
            if (key.Length != 32)
                throw new ArgumentException("Ключ должен быть 32 байта");
            _key = key;
        }

        private Aes CreateAes(bool forEncryption)
        {
            var algorithm = Aes.Create();
            algorithm.Key = _key; //Подставляем ключ которым будем шифровать данные
            if (!forEncryption)
                algorithm.IV = new byte[IvSize];
            else
                algorithm.GenerateIV(); //генерация случайного IV из 16 байт
            return algorithm;
        }

        /// <summary>
        /// Шифрует данные. Возвращает массив: IV 16 байт + зашифрованные данные
        /// </summary>
        public byte[] Encrypt(byte[] plainData) // plainData - незашифрованные данные
        {

            using (var algorithm = CreateAes(true)) // получаем AES с сгенерированным IV
            using (var encryptor = algorithm.CreateEncryptor())
            using (var memoryStream = new MemoryStream()) // заносим все зашифрованные данные в оперативную память для отправки
            {
                // Пишем IV в начало
                memoryStream.Write(algorithm.IV, 0, IvSize);
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainData, 0, plainData.Length);
                    cryptoStream.FlushFinalBlock(); // завершение работы шифровальщика
                }
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Расшифровывает данные, полученные методом Encrypt
        /// </summary>
        public byte[] Decrypt(byte[] encryptedData) //encryptedData - зашифрованные данные
        {
            if (encryptedData == null || encryptedData.Length < IvSize)
            {
                WpfMessageBox.Show("Данные повреждены!");
                throw new ArgumentException($"Данные повреждены: недостаточная длина. Минимум {IvSize} байт.");
            }

            // Извлекаем IV (первые 16 байт)
            byte[] iv = new byte[IvSize];
            Array.Copy(encryptedData, 0, iv, 0, IvSize);

            using (var algorithm = CreateAes(false)) // AES без генерации IV
            {
                algorithm.IV = iv; // устанавливаем извлечённый IV
                using (var decryptor = algorithm.CreateDecryptor())
                using (var memoryStream = new MemoryStream(encryptedData, IvSize, encryptedData.Length - IvSize)) // Передаётся весь оставшийся массив, начиная с 16-го байта
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) // расшифровываем данные
                using (var resultStream = new MemoryStream()) // передаём результат
                {
                    cryptoStream.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
        }
    }
}
