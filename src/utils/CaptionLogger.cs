using System;
using System.IO;
using System.Text.Json;

namespace LiveCaptionsTranslator.utils
{
    public class CaptionEntry
    {
        public DateTime Timestamp { get; set; }
        public string Caption { get; set; }
        public string? Translation { get; set; }

        public CaptionEntry(string caption, string? translation = null)
        {
            Timestamp = DateTime.Now;
            Caption = caption;
            Translation = translation;
        }
    }

    public class CaptionLogger
    {
        private static CaptionLogger? instance;
        private readonly List<CaptionEntry> entries;
        private string logFilePath;
        private bool isLogging;
        private readonly object lockObject = new();

        public bool IsLogging
        {
            get { return isLogging; }
            set
            {
                if (isLogging != value)
                {
                    isLogging = value;
                    if (isLogging)
                    {
                        StartNewLog();
                    }
                }
            }
        }

        private CaptionLogger()
        {
            entries = new List<CaptionEntry>();
            logFilePath = GetNewLogFilePath();
            isLogging = false;
        }

        public static CaptionLogger GetInstance()
        {
            instance ??= new CaptionLogger();
            return instance;
        }

        private string GetNewLogFilePath()
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LiveCaptionsTranslator",
                "Logs"
            );
            Directory.CreateDirectory(baseDir);
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.Combine(baseDir, $"captions_{timestamp}.json");
        }

        private void StartNewLog()
        {
            lock (lockObject)
            {
                entries.Clear();
                logFilePath = GetNewLogFilePath();
            }
        }

        public void LogCaption(string caption, string? translation = null)
        {
            if (!IsLogging) return;

            lock (lockObject)
            {
                var entry = new CaptionEntry(caption, translation);
                entries.Add(entry);
                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(entries, options);
                File.WriteAllText(logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving caption log: {ex.Message}");
            }
        }

        public string GetCurrentLogPath()
        {
            return logFilePath;
        }
    }
} 