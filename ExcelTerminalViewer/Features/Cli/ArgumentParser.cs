namespace ExcelTerminalViewer.Features.Cli;

public static class ArgumentParser
{
    public static Result<CliOptions, CliError> Parse(string[] args)
    {
        if (args.Length == 0)
            return new CliError("Usage: ExcelTerminalViewer <file-path> [--max-width N] [--go-to ROW] [--search TERM]");

        var validationResult = ValidateIncompatibleArguments(args);
        if (validationResult.IsError)
            return validationResult.Match(static _ => default!, static e => e);

        var filePath = args[0];

        var maxWidthResult = ParseMaxWidth(args);
        if (maxWidthResult.IsError)
            return maxWidthResult.Match(static _ => default!, static e => e);

        var maxWidth = maxWidthResult.Match(static v => v, static _ => 30);

        string? searchTerm = null;
        if (HasFlag(args, "--search"))
        {
            var searchTermResult = ParseSearchTerm(args);
            if (searchTermResult.IsError)
                return searchTermResult.Match(static _ => default!, static e => e);

            searchTerm = searchTermResult.Match(static v => v, static _ => (string?)null);
        }

        if (!HasFlag(args, "--go-to"))
            return new CliOptions(filePath, maxWidth, null, searchTerm);

        var goToRowResult = ParseGoToRow(args);
        if (goToRowResult.IsError)
            return goToRowResult.Match(static _ => default!, static e => e);

        var goToRow = goToRowResult.Match(static v => v, static _ => 0);
        return new CliOptions(filePath, maxWidth, goToRow, searchTerm);
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

    private static Result<bool, CliError> ValidateIncompatibleArguments(string[] args)
    {
        var hasSearch = HasFlag(args, "--search");
        var hasGoTo = HasFlag(args, "--go-to");
        var hasMaxWidth = HasFlag(args, "--max-width");

        if (hasSearch && hasGoTo)
            return new CliError("Arguments --search and --go-to are incompatible.");

        if (hasSearch && hasMaxWidth)
            return new CliError("Arguments --search and --max-width are incompatible.");

        return true;
    }

    private static Result<string?, CliError> ParseSearchTerm(string[] args)
    {
        var index = Array.IndexOf(args, "--search");

        if (index + 1 >= args.Length)
            return new CliError("--search requires a value.");

        var searchTerm = args[index + 1];

        if (string.IsNullOrWhiteSpace(searchTerm))
            return new CliError("--search value cannot be empty.");

        return searchTerm;
    }
}
