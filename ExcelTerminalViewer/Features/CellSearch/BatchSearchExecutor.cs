using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.Cli;
using ExcelTerminalViewer.Features.FileLoading;

namespace ExcelTerminalViewer.Features.CellSearch;

public static class BatchSearchExecutor
{
    public static async Task<int> ExecuteAsync(CliOptions options)
    {
        // Task 4.2: Load the spreadsheet file
        var loadResult = FileLoader.Load(options.FilePath);
        if (loadResult.IsError)
        {
            var error = loadResult.GetErrorOrDefault(new FileLoadError("Unknown error"));
            await Console.Error.WriteLineAsync($"Error loading file: {error.Message}");
            return 1;
        }

        var data = loadResult.GetValueOrDefault(null!);

        // Task 4.3: Execute the search
        try
        {
            var searchResults = await CellSearchEngine.SearchAsync(
                data,
                options.SearchTerm!,
                CancellationToken.None);

            // Task 4.4: Output results and stream them
            foreach (var result in searchResults)
            {
                var formattedOutput = SearchResultFormatter.Format(result, data);
                await Console.Out.WriteLineAsync(formattedOutput);
            }

            // Return exit code 0 on success (including empty results)
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error during search: {ex.Message}");
            return 1;
        }
    }
}
