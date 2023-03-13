using System;

namespace PopUpWindow
{
    public class Settings
    {

        //Директория в которой хранятся изображения
        public string DirectoryPath { get; set; } = Environment.CurrentDirectory;

        //Частота смены изображений в секундах (Режим №2)
        public int Rate { get; set; } = 30;

        //Расширения файлов
        public string[] Extensions { get; set; } = { "png", "jpeg", "jpg", "wav", "bmp", "tiff", "jfif", "webp" };
        public bool LeftPanel { get; set; } = true;
    }
}