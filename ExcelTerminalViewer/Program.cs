using ExcelTerminalViewer;
using ExcelTerminalViewer.Features.Cli;
using ExcelTerminalViewer.Features.Display;
using ExcelTerminalViewer.Features.FileLoading;
using Terminal.Gui;

return ProgramRunner.Run(args, true);

internal static class ProgramRunner
{
    internal static int Run(string[] args, bool isReal = false)
    {
        if (isReal)
            Console.CursorVisible = false;
        var optionsResult = ArgumentParser.Parse(args);
        if (optionsResult.IsError)
            return WriteErrorAndExit(optionsResult);

        var options = optionsResult.Match(static o => o, static _ => throw new InvalidOperationException());

        if (!File.Exists(options.FilePath))
            return WriteErrorAndExit(new CliError($"File not found: {options.FilePath}"));

        var dataResult = FileLoader.Load(options.FilePath);
        if (dataResult.IsError)
            return WriteErrorAndExit(dataResult);

        var data = dataResult.Match(static d => d, static _ => throw new InvalidOperationException());

        Application.Init();
        var top = ViewBuilder.Build(data, options.MaxWidth, options.FilePath, options.GoToRow);
        Application.Run(top);
        Application.Shutdown();
        return 0;
    }

    private static int WriteErrorAndExit<T, TError>(Result<T, TError> result) where TError : Error
    {
        result.Switch(
            static _ => { },
            static error => Console.Error.WriteLine(error.Message));
        return 1;
    }

    private static int WriteErrorAndExit(Error error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}
