using System;
using System.Collections.Generic;
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

        private static readonly Regex IniSectionLineMatcher = new Regex(
            @"^([A-Za-z]+=[0-9]+)|([A-Za-z]+=[A-Za-z]+)|([A-Za-z]+=([0-1][0-9]|[2][1-3])[:./\s-][0-5][0-9])|([[0-9A-Za-z]+])",
            RegexOptions.Compiled);

        private static readonly Regex IniLineMatcher = new Regex(
            @"^([A-Za-z]+=[0-9]+)|([A-Za-z]+=[A-Za-z]+)|([A-Za-z]+=([0-1][0-9]|[2][1-3])[:./\s-][0-5][0-9])",
            RegexOptions.Compiled);

        //Конструктор, принимающий путь к INI-файлу
        public FileManager(string aPath)
        {
            _path = aPath;
        }

        public string GetPrivateString(string key)
        {
            string result = "";

            try
            {
                IEnumerable<string> iniLines = File.ReadLines(_path);

                foreach (var line in iniLines)
                {
                    var trimmedLine = line.Replace("  ", " ").Trim();

                    if (trimmedLine.StartsWith("#") || !IniLineMatcher.IsMatch(trimmedLine))
                        continue;

                    var equalsIndex = trimmedLine.IndexOf("=", StringComparison.Ordinal);

                    if (equalsIndex != -1)
                    {
                        var currentKey = trimmedLine.Substring(0, equalsIndex);
                        if (currentKey == key)
                        {
                            result = trimmedLine.Substring(equalsIndex + 1);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading ini file (key = {key}): " + ex.Message);
                return "";
            }

            return result;
        }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string section, string key)
        {
            string result = "";

            try
            {
                IEnumerable<string> iniLines = File.ReadLines(_path);

                bool isInDesiredSection = false;

                foreach (var line in iniLines)
                {
                    var trimmedLine = line.Replace("  ", " ").Trim();

                    if (trimmedLine.StartsWith("#") || !IniSectionLineMatcher.IsMatch(trimmedLine))
                        continue;

                    if (trimmedLine == $"[{section}]")
                    {
                        isInDesiredSection = true;
                        continue;
                    }

                    if (isInDesiredSection && trimmedLine.StartsWith("["))
                        break;

                    if (trimmedLine.StartsWith("["))
                    {
                        isInDesiredSection = false;
                        continue;
                    }

                    var equalsIndex = trimmedLine.IndexOf("=", StringComparison.Ordinal);

                    if (equalsIndex != -1 && isInDesiredSection)
                    {
                        var currentKey = trimmedLine.Substring(0, equalsIndex);
                        if (currentKey == key)
                        {
                            result = trimmedLine.Substring(equalsIndex + 1);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading ini file (section = {section}, key = {key}): " + ex.Message);
                return "";
            }

            //Вернуть полученное значение
            return result;
        }

        public bool IsHistoryContains(string aFileName, DateTime aLastWriteTime)
        {
            try
            {
                if (File.ReadLines(_path).Contains(aFileName + "|" + aLastWriteTime))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while reading file({_path}): " + ex.Message);
                return false;
            }
        }
        public void WriteHistoryString(string fileName, DateTime creationTime)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(_path);
                if (fileInfo.Exists)
                {
                    using(StreamWriter writer = new StreamWriter(_path, true))
                    {
                        writer.WriteLine($"{fileName}|{creationTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"Error while writing file({_path}): {ex.Message}");
            }
        }

    }
}