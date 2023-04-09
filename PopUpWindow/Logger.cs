using System;
using System.IO;
using System.Linq;

namespace PopUpWindow;

public class Logger
{
    // The path where log files will be stored.
    private static readonly string DirPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}logs";

    // The name of the log file. This will be the current date in the format MM/dd/yyyy.
    private static readonly string LogFileName = $"{DateTime.Now.ToShortDateString()}.log";

    // The full path to the log file.
    private readonly string _logFilePath = $"{DirPath}{Path.DirectorySeparatorChar}{LogFileName}";

    // Constructor. Creates the log directory if it doesn't exist and creates the log file if it doesn't exist.
    public Logger()
    {
        DirectoryInfo dir = new DirectoryInfo(DirPath);
        if (!dir.Exists)
        {
            Directory.CreateDirectory(DirPath);
        }

        FileInfo logFile = new FileInfo(_logFilePath);
        if (!logFile.Exists)
        {
            CreateLogFile();
        }
    }

    // Creates a log file and deletes all old log files if there are more than 3.
    private void CreateLogFile()
    {
        try
        {
            FileInfo logFile = new FileInfo(_logFilePath);
            if (!logFile.Exists)
            {
                logFile.Create().Close();
            }

            DirectoryInfo dir = new DirectoryInfo(DirPath);
            FileInfo[] logFiles = dir.GetFiles("*.log");

            if (logFiles.Length > 3)
            {
                FileInfo logFileDelete = logFiles.OrderBy(item => item.CreationTime).FirstOrDefault();
                logFileDelete?.Delete();
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Writes the specified error message to the log file.
    public void CreateLog(string error)
    {
        FileInfo logFile = new FileInfo(_logFilePath);

        if (logFile.Exists)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_logFilePath))
                {
                    sw.WriteLine($"{DateTime.Now} | {error}");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"An error occurred while writing to the log file: {e.Message}");
            }
        }
    }
}