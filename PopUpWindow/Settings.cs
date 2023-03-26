using System;

namespace PopUpWindow
{
    public class Settings
    {

        //Директория в которой хранятся изображения
        public string DirectoryPath { get; set; } = Environment.CurrentDirectory;

        //Частота смены изображений в секундах (Режим №2)
        public int Rate { get; set; } = 5;
    }
}