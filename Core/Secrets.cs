using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Core
{
    internal class Secrets 
    {
        public static readonly string FlashKey = "мой_секретный_флеш_ключ_12345"; //Текст внутри секртеного файла на флешке для авторизации
        public static readonly byte[] DocumentKey = new byte[32] //32-байтовый ключ шифрования необходимый для алгоритма AES-256
        {
            0x8E, 0x2C, 0x7A, 0x9F, 0x3B, 0xD1, 0x56, 0xE0,
            0xF4, 0x6A, 0x1C, 0x88, 0x2D, 0x5E, 0x97, 0xC3,
            0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0,
            0x0F, 0x1E, 0x2D, 0x3C, 0x4B, 0x5A, 0x69, 0x78
        };
    }
}
