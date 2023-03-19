using System;
using System.IO;
using System.Linq;

namespace PopUpWindow;

public class Logger
{
    private static readonly string DirPath = Environment.CurrentDirectory + "\\logs";
    private static readonly string LogFileName = $"{DateTime.Now.ToShortDateString()}.log";
    private readonly string _logFilePath = DirPath + "\\" + LogFileName;
    public Logger()
    {
        FileInfo logFile = new FileInfo(_logFilePath);
        
        DirectoryInfo dir = new DirectoryInfo(DirPath);
        if (!dir.Exists)
            Directory.CreateDirectory(DirPath);
        
        if (!logFile.Exists)
            CreateLogFile();
    }

    private void CreateLogFile()
    {
        try
        {
           
            FileInfo logFile = new FileInfo(_logFilePath);
            if (!logFile.Exists)
                logFile.Create();

           DirectoryInfo dir = new DirectoryInfo(DirPath);
           if (dir.GetFiles().Count(item => item.Extension == ".log") > 3)
            {
                FileInfo logFileDelete = dir.GetFiles().OrderBy(item => item.CreationTime).First();
                logFileDelete.Delete();
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    public void CreateLog(string error)
    {
        FileInfo logFile = new FileInfo(_logFilePath);

        if (logFile.Exists)
        {
            try
            {
                StreamWriter sw = new StreamWriter(_logFilePath, true);
                sw.WriteLine(DateTime.Now + "|" + error);
                sw.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}