using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {
            string method = args[0]; // метод работы
            string pathOpen = args[1]; // путь к открываемому файлу
            string pathSave = args[2]; // путь к сохраняемому файлу
            string pathBuffer = args[3]; // путь к файлу буфера
            int lengthOriginal = int.Parse(args[4]); // длина распаковываемой последовательности
            int begin = int.Parse(args[5]); // адрес начала

            switch (method)
            {
                case "-u":
                    LZSS.Unpack(pathOpen, pathSave, pathBuffer, lengthOriginal, begin);
                    break;
                case "-p":
                    LZSS.Pack(pathOpen, pathSave, pathBuffer, begin);
                    break;
                default:
                    break;
            } 
        }
    }
}
