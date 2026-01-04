namespace PokeAByte.Web.Logger;

internal class LogFileWriter : IDisposable
{
    private FileStream _stream;
    private StreamWriter _writer;
    private Lock _lock = new();

    internal LogFileWriter(string path, int maxSize)
    {
        if (!File.Exists(path))
        {
            File.Create(path);
        }

        // Truncate logfile to below half of max size, if max size is exceeded:
        var lines = File.ReadAllLines(path).ToList();
        int currentSize = lines.Sum(x => x.Length);
        if (currentSize >= maxSize)
        {
            int skipped = 0;
            string[] content = lines.SkipWhile(x =>
                {
                    skipped += x.Length;
                    return currentSize - skipped > (maxSize / 2);
                })
                .SkipWhile(x => !x.Contains("Application is shutting down..."))
                .ToArray();
            File.WriteAllLines(path, content);
        }

        _stream = File.Open(path, FileMode.OpenOrCreate);
        _stream.Seek(0, SeekOrigin.End);
        _writer = new(_stream) { AutoFlush = true };
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _writer.Flush();
            _writer.Dispose();
            _stream.Dispose();
        }
    }

    internal void Log(DateTime time, LogLevel level, string context, string logLines)
    {
        string entry = $"{time:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] ({context}) {logLines}";
        lock (_lock)
        {
            _writer.WriteLine(entry);
        }
    }
}
