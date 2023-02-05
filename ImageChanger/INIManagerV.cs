using System;
using System.IO;

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
        public string GetPrivateString(string aSection, string aKey)
        {
            string result = "";
            try
            {
                FileInfo ini = new(path);
                if (ini.Exists)
                {
                    StreamReader sr = new(path);

                    bool isDesiredSection = false;

                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine().Replace(" ", "");

                        if (str == $"[{aSection}]")
                            isDesiredSection = true;
                        else if(str.IndexOf("[") != -1)
                            isDesiredSection = false;

                        if (str.StartsWith("#") ||
                            !isDesiredSection ||
                            str == string.Empty ||
                            str.IndexOf("=") == -1 ||
                            str.Length <= 3)
                            continue;
                        else
                        {
                            string key = str.Substring(0, str.IndexOf("="));
                            if (key == aKey)
                                result = str.Substring(str.IndexOf("=") + 1, str.Length - str.IndexOf("=") - 1);
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                new InfoWindow("Ошибка при чтении ini файла: " + ex.Message).Show();
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
        private string path; //Для хранения пути к INI-файлу
    }
}
