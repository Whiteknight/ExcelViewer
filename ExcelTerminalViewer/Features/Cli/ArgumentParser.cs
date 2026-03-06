namespace ExcelTerminalViewer.Features.Cli;

public static class ArgumentParser
{
    public static Result<CliOptions, CliError> Parse(string[] args)
    {
        if (args.Length == 0)
            return new CliError("Usage: ExcelTerminalViewer <file-path> [--max-width N] [--go-to ROW]");

        var filePath = args[0];

        var maxWidthResult = ParseMaxWidth(args);
        if (maxWidthResult.IsError)
            return maxWidthResult.Match(static _ => default!, static e => e);

        var maxWidth = maxWidthResult.Match(static v => v, static _ => 30);

        if (!HasFlag(args, "--go-to"))
            return new CliOptions(filePath, maxWidth);

        var goToRowResult = ParseGoToRow(args);
        if (goToRowResult.IsError)
            return goToRowResult.Match(static _ => default!, static e => e);

        var goToRow = goToRowResult.Match(static v => v, static _ => 0);
        return new CliOptions(filePath, maxWidth, goToRow);
    }

    internal static bool HasFlag(string[] args, string flag) => Array.IndexOf(args, flag) >= 0;

    private static Result<int, CliError> ParseMaxWidth(string[] args)
    {
        var index = Array.IndexOf(args, "--max-width");
        if (index < 0)
            return 30;

        if (index + 1 >= args.Length)
            return new CliError("--max-width requires a value.");

        if (!int.TryParse(args[index + 1], out var value))
            return new CliError($"--max-width value must be an integer, got: {args[index + 1]}");

        if (value < 5 || value > 200)
            return new CliError("--max-width must be between 5 and 200.");

        return value;
    }

    private static Result<int, CliError> ParseGoToRow(string[] args)
    {
        var index = Array.IndexOf(args, "--go-to");

        if (index + 1 >= args.Length)
            return new CliError("--go-to requires a value.");

        if (!int.TryParse(args[index + 1], out var value))
            return new CliError($"--go-to value must be an integer, got: {args[index + 1]}");

        if (value < 1)
            return new CliError("--go-to must be at least 1.");

        return value;
    }
}
