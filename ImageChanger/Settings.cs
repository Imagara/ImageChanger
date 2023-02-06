using System;

namespace ImageChanger
{
    public class Settings
    {
        //Режим работы
        public int Mode = 1;
        //Директория в которой хранятся изображения
        public string PicturesDirectoryPath = Environment.CurrentDirectory;
        //Частота смены изображений в секундах (Режим №2)
        public int Rate = 30;
        //Расширения файлов
        public string[] Extensions = { "png", "jpeg", "jpg", "wav", "bmp", "tiff", "jfif", "webp" };
    }
}