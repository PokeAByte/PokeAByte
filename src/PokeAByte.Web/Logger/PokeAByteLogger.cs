using Jint.Native;
using Jint.Runtime;

namespace PokeAByte.Web.Logger;

internal sealed class PokeAByteLogger(string name, LoggerConfiguration config, LogFileWriter? fileWriter) : ILogger
{
    private string Category = name.StartsWith("PokeAByte.")
        ? name.Split(".").Last()
        : name;
    private LogLevel _loggerLevel = config.GetMinimumLevel(name);
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _loggerLevel;

    /// <summary>
    /// Cuts out entries of the stacktrace that start outside of the PokeAByte namespace and simplifies the entries 
    /// that belong to the PokeAByte namespace. <br/>
    /// <code>
    ///    at System.Text.Json[...]
    ///    at PokeAByte.Web.Controller.MapperEndpoints.GetProperties([...]
    /// </code>
    /// becomes:
    /// <code>
    ///    at MapperEndpoints.GetProperties([...]
    /// </code>
    /// </summary>
    /// <param name="stackTrace"> The stacktrace string to reformat. </param>
    /// <returns> The reformatted stacktrace. </returns>
    private string ReformatStacktrace(string stackTrace)
    {
        return string.Join("\n",
            stackTrace.Split('\n')
                .SkipWhile(x => !x.Trim().StartsWith("at PokeAByte."))
                .Select(line =>
                {
                    if (!line.Trim().StartsWith("at PokeAByte."))
                    {
                        return line;
                    }

                    var chunks = line[6..].Split(") in ");
                    var position = chunks.Length == 2
                        ? "in " + chunks.Last()
                        : "";
                    var method = string.Join(".", chunks.First().Split('.').TakeLast(2));
                    return $"    at {method}) {position}";
                })
        );
    }

    private Exception GetJintBaseException(Exception ex)
    {
        
        while(ex.InnerException != null && ex is not JavaScriptException)
        {
            ex = ex.InnerException;
        }
        return ex;
    }

    /// <summary>
    /// Format given exception for logging: <br/>
    /// <c>{ExceptionName}: {ExceptionMessage}\n{StackTrace}</c>
    /// Exceptions derived from <see cref="JavaScriptException"/> are formatted slightly different to make it clearer
    /// what error occured and that the error came from the mapper javascript and not PokeAByte internals.
    /// </summary>
    /// <param name="exception"> The exception to format. </param>
    /// <returns> The exception information as a string. </returns>
    private string FormatException(Exception exception)
    {
        string stacktrace = ReformatStacktrace(exception.StackTrace ?? "");
        var baseException = GetJintBaseException(exception);
        string name = baseException.GetType().Name;
        string message = exception.Message;
        if (baseException is JavaScriptException jsException && jsException.Error is JsError error)
        {
            name = "(JavaScript) " + error.Prototype;
            if (error.HasProperty("message"))
            {
                message = error["message"].ToString();
            }
            else
            {
                message = error.ToString();
            }
            stacktrace = jsException.JavaScriptStackTrace + "\n" + stacktrace;
        }
        return $"{name}: {message}\n{stacktrace}";
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var now = DateTime.Now;
        var entry = exception == null
            ? formatter(state, exception)
            : formatter(state, null) + "\n" + FormatException(exception);
        Console.Write(now.ToString("HH:mm"));
        Console.ForegroundColor = logLevel switch
        {
            <= LogLevel.Information => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            >= LogLevel.Error => ConsoleColor.Red,
        };
        Console.Write($" [{logLevel}] ".PadLeft(15));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"({Category}) {entry}");

        if (fileWriter != null)
        {
            var fullEntry = exception == null
                ? formatter(state, exception)
                : formatter(state, null) + "\n" + exception.ToString();
            fileWriter.Log(now, logLevel, name, fullEntry);
        }
    }
}
