using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageChanger
{
    public static class Settings
    {
        public static string OS { get; set; }
        //Режим работы программы
        public static int Mode = 1;
        //Директория в которой хранятся изображения
        public static string PicturesDirectoryPath = Environment.CurrentDirectory;
        //Частота смены изображений в секундах (Режим №2)
        public static int Rate = 30;
        //Расширения файлов
        public static string[] Extensions = { "png", "jpeg", "jpg", "wav", "bmp", "tiff", "jfif", "webp" };
    }
}
