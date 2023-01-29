using Avalonia.Controls.Shapes;
using Avalonia.Media.TextFormatting.Unicode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ImageChanger
{
    public class INIManagerV
    {
        //Конструктор, принимающий путь к INI-файлу
        public INIManagerV(string aPath)
        {
            path = aPath;
        }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aKey, string aSection = "")
        {
            string result = "";
            try
            {
                FileInfo ini = new FileInfo(path);
                if (ini.Exists)
                {
                    StreamReader sr = new StreamReader(path);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        if (str.StartsWith("#"))
                            continue;
                        else
                        {
                            string key = str.Substring(0, str.IndexOf("="));
                            if (key == aKey)
                                result = str.Substring(str.IndexOf("=") + 1, str.Length - str.IndexOf("=") - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new InfoWindow(ex.Message).Show();
                return "";
            }


            //Вернуть полученное значение
            return result;
        }

        //Пишет значение в INI-файл (по указанным секции и ключу) 
        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
            //Записать значение в INI-файл

        }

        //Возвращает или устанавливает путь к INI файлу
        public string Path { get { return path; } set { path = value; } }

        //Поля класса
        private string path = Environment.CurrentDirectory + "\\settings.ini"; //Для хранения пути к INI-файлу
    }
}
