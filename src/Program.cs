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
            if (args.Length > 3)
            {
                if (args.Length < 7)
                {
                    string method = args[0]; // method of working program
                    string pathOpen = args[1]; // path to opening file
                    string pathSave = args[2]; // path to saving file
                    string pathBuffer = args[3]; // path to buffer file
                    int begin = 0;
                    int lengthOriginal = 0;
                    if (args.Length > 4)
                    {
                        begin = int.Parse(args[4]); // begin address
                        if (args.Length > 5)
                        {
                            lengthOriginal = int.Parse(args[5]); // length of required sequence
                        }
                    }
                    switch (method)
                    {
                        case "-u":
                            LZSS.Unpack(pathOpen, pathSave, pathBuffer, begin, lengthOriginal);
                            break;
                        case "-p":
                            LZSS.Pack(pathOpen, pathSave, pathBuffer, begin);
                            break;
                        default:
                            break;
                    }
                    Console.ReadLine();
                }
            }
        }
    }
}
