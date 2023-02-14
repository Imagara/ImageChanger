using System;

namespace ImageChanger
{
    public class Settings
    {
        //Режим работы
        public int Mode { get; set; } = 1;

        //Директория в которой хранятся изображения
        public string PicturesDirectoryPath { get; set; } = Environment.CurrentDirectory;

        //Частота смены изображений в секундах (Режим №2)
        public int Rate { get; set; } = 30;

        //Расширения файлов
        public string[] Extensions { get; set; } = { "png", "jpeg", "jpg", "wav", "bmp", "tiff", "jfif", "webp" };
    }
}