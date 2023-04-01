using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PopUpWindow
{
    public class FileManager
    {
        //Поля класса
        private readonly string _path; //Для хранения пути к INI-файлу
        private readonly Logger _logger = new();

        //Конструктор, принимающий путь к INI-файлу
        public FileManager(string aPath)
        {
            _path = aPath;
        }

        public string GetPrivateString(string aKey)
        {
            string result = "";

            Regex regex = new Regex(
                @"^([A-Za-z]+=[0-9]+)|([A-Za-z]+=[A-Za-z]+)|([A-Za-z]+=([0-1][0-9]|[2][1-3])[:./\s-][0-5][0-9])",
                RegexOptions.Compiled);

            try
            {
                FileInfo ini = new(_path);
                if (ini.Exists)
                {
                    StreamReader sr = new(_path);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine()!.Replace("  ", " ").Trim();

                        if (str.StartsWith("#") ||
                            !regex.IsMatch(str) ||
                            str.IndexOf("[", StringComparison.Ordinal) != -1)
                            continue;

                        if (str.IndexOf("=", StringComparison.Ordinal) != -1)
                        {
                            string key = str.Substring(0, str.IndexOf("=", StringComparison.Ordinal));

                            if (key == aKey)
                            {
                                result = str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1,
                                    str.Length - str.IndexOf("=", StringComparison.Ordinal) - 1);
                                break;
                            }
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading ini file (key = {aKey}): " + ex.Message);
                return "";
            }

            return result;
        }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aSection, string aKey)
        {
            string result = "";
            Regex regex = new Regex(
                @"^([A-Za-z]+=[0-9]+)|([A-Za-z]+=[A-Za-z]+)|([A-Za-z]+=([0-1][0-9]|[2][1-3])[:./\s-][0-5][0-9])|([[0-9A-Za-z]+])",
                RegexOptions.Compiled);
            try
            {
                FileInfo ini = new(_path);
                if (ini.Exists)
                {
                    StreamReader sr = new(_path);

                    bool isDesiredSection = false;

                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine()!.Replace("  ", " ").Trim();

                        if (str.StartsWith("#") ||
                            !regex.IsMatch(str))
                            continue;

                        if (str == $"[{aSection}]")
                        {
                            isDesiredSection = true;
                            continue;
                        }

                        if (str.IndexOf("[", StringComparison.Ordinal) != -1 && isDesiredSection)
                            break;
                        if (str.IndexOf("[", StringComparison.Ordinal) != -1)
                            isDesiredSection = false;

                        if (isDesiredSection && str.IndexOf("=", StringComparison.Ordinal) != -1)
                        {
                            string key = str.Substring(0, str.IndexOf("=", StringComparison.Ordinal));
                            if (key == aKey)
                            {
                                result = str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1,
                                    str.Length - str.IndexOf("=", StringComparison.Ordinal) - 1);
                                break;
                            }
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading ini file (section = {aSection}, key = {aKey}): " + ex.Message);
                return "";
            }

            //Вернуть полученное значение
            return result;
        }

        public bool IsHistoryContains(string aFileName, DateTime aLastWriteTime)
        {
            try
            {
                FileInfo historyFile = new(_path);
                if (historyFile.Exists)
                {
                    StreamReader sr = new(_path);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        if (str == aFileName + "|" + aLastWriteTime)
                        {
                            sr.Close();
                            return true;
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading file({_path}): " + ex.Message);
                return false;
            }

            return false;
        }

        //Пишет значение в INI-файл (по указанным секции и ключу) 
        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
        }

        public void WriteHistoryString(string aFileName, DateTime aCreationTime)
        {
            try
            {
                FileInfo historyFile = new FileInfo(_path);
                if (historyFile.Exists)
                {
                    StreamWriter sw = new StreamWriter(_path, true);
                    sw.WriteLine(aFileName + "|" + aCreationTime);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while writing file({_path}): " + ex.Message);
            }
        }
    }
}