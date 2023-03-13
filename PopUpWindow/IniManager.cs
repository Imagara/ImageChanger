using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PopUpWindow
{
    public class INIManager
    {
        //Конструктор, принимающий путь к INI-файлу
        public INIManager(string aPath)
        {
            path = aPath;
        }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aSection, string aKey)
        {
            string result = "";
            Regex regex = new Regex(@"([A-Za-z]+=[0-9]+)|([A-Za-z]+=[A-Za-z]+)|([[0-9A-Za-z]+])", RegexOptions.Compiled);
            try
            {
                FileInfo ini = new(path);
                if (ini.Exists)
                {
                    StreamReader sr = new(path);

                    bool isDesiredSection = false;

                    while (!sr.EndOfStream)
                    {
                        string  str = sr.ReadLine().ToLower().Replace(" ", "");
                        
                        if (str.StartsWith("#") ||
                        if(str.StartsWith("#") ||
                           !regex.IsMatch(str))
                            continue;
                        
                        
                        if (str == $"[{aSection}]")
                            isDesiredSection = true;
                        else if(str.IndexOf("[") != -1)
                            isDesiredSection = false;
                        

                        if (isDesiredSection)
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
                //new InfoWindow($"Error while reading ini file (key = {aKey}): " + ex.Message).Show();
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
        //Поля класса
        private string path; //Для хранения пути к INI-файлу
    }
}
