using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.CellSearch;
using System.Data;
using Terminal.Gui;

namespace ExcelTerminalViewer.Features.Display;

public static class ViewBuilder
{
    public static Toplevel Build(SpreadsheetData data, int maxWidth, string filePath, int? goToRow = null)
    {
        var top = new Toplevel();
        top.ColorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.White, Color.Black),
            HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
        };

        var tableView = CreateTableView(data, maxWidth);
        var previewLabel = CreatePreviewLabel();
        var statusBar = CreateStatusBar(data, filePath);

        var searchHighlightSet = new HashSet<(int Row, int Column)>();
        var searchPrompt = CreateSearchPrompt();

        SearchCoordinator? coordinatorRef = null;
        var coordinator = new SearchCoordinator(data, state =>
        {
            Application.Invoke(() =>
            {
                UpdateSearchStatusLabel(statusBar, state);
                UpdateSearchHighlightSet(searchHighlightSet, coordinatorRef!, state, tableView);
            });
        });
        coordinatorRef = coordinator;

        ConfigureSearchHighlighting(tableView, searchHighlightSet);

        WireSelectedCellChanged(tableView, previewLabel, statusBar, data, filePath);
        WireExitBindings(top, searchPrompt);
        WireHelpKeyBinding(top, searchPrompt);
        WireSearchKeyBindings(top, tableView, searchPrompt, coordinator);

        top.Closed += (_, _) => coordinator.Dispose();

        top.Add(tableView, previewLabel, statusBar, searchPrompt);
        ApplyGoToRow(tableView, data, goToRow);
        return top;
    }

    internal static int ClampGoToRow(int goToRow, int rowCount)
    {
        return Math.Clamp(goToRow - 1, 0, Math.Max(rowCount - 1, 0));
    }

    private static void ApplyGoToRow(TableView tableView, SpreadsheetData data, int? goToRow)
    {
        if (goToRow is null)
            return;

        var selected = ClampGoToRow(goToRow.Value, data.RowCount);
        tableView.SetSelection(0, selected, false);
        tableView.EnsureSelectedCellIsVisible();
    }

    private static TableView CreateTableView(SpreadsheetData data, int maxWidth)
    {
        var dataTable = ToDataTable(data, maxWidth);

        var tableView = new TableView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(3),
            Table = new DataTableSource(dataTable),
            FullRowSelect = false,
            Style = new TableStyle
            {
                ShowHorizontalHeaderOverline = false,
                ShowHorizontalHeaderUnderline = true,
                AlwaysShowHeaders = true,
                InvertSelectedCellFirstCharacter = false,
            },
        };

        ConfigureRowHighlighting(tableView);

        tableView.SelectedColumn = 1;

        return tableView;
    }

    private static void ConfigureRowHighlighting(TableView tableView)
    {
        var activeRowScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.White, Color.Blue),
            HotNormal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Blue),
        };

        tableView.Style.RowColorGetter = args =>
        {
            if (args.RowIndex != tableView.SelectedRow)
                return null;

            return activeRowScheme;
        };
    }

    private static Label CreatePreviewLabel()
    {
        return new Label
        {
            X = 0,
            Y = Pos.AnchorEnd(3),
            Width = Dim.Fill(),
            Height = 2,
            Text = string.Empty,
        };
    }

    private static StatusBar CreateStatusBar(SpreadsheetData data, string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var initialText = StatusBarHelper.FormatPosition(0, 0, data.RowCount, data.ColumnCount, fileName);

        var statusBar = new StatusBar
        {
            Y = Pos.AnchorEnd(1),
        };

        var statusLabel = new Label
        {
            Text = initialText,
            Width = Dim.Auto(),
        };

        var searchStatusLabel = new Label
        {
            Text = string.Empty,
            X = Pos.Right(statusLabel),
            Width = Dim.Auto(),
        };

        statusBar.Add(statusLabel, searchStatusLabel);

        return statusBar;
    }

    internal static int ToDataColumn(int tableColumn) => Math.Max(tableColumn - 1, 0);

    private static void WireSelectedCellChanged(
        TableView tableView,
        Label previewLabel,
        StatusBar statusBar,
        SpreadsheetData data,
        string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        tableView.SelectedCellChanged += (_, args) =>
        {
            var dataCol = ToDataColumn(args.NewCol);
            var cellContent = data.GetCell(args.NewRow, dataCol);
            previewLabel.Text = cellContent;

            UpdateStatusBarText(statusBar, args.NewRow, dataCol, data, fileName);
        };
    }

    private static void UpdateStatusBarText(StatusBar statusBar, int row, int col, SpreadsheetData data, string fileName)
    {
        var text = StatusBarHelper.FormatPosition(row, col, data.RowCount, data.ColumnCount, fileName);

        if (statusBar.Subviews.FirstOrDefault() is Label label)
            label.Text = text;
    }

    private static void WireExitBindings(Toplevel top, FrameView searchPrompt)
    {
        top.KeyDown += (_, args) =>
        {
            if (!ShouldHandleExitKey(args, searchPrompt.Visible))
                return;

            args.Handled = true;
            Application.RequestStop();
        };
    }

    internal static bool ShouldHandleExitKey(Key key, bool searchPromptVisible)
    {
        if (searchPromptVisible)
            return false;

        return IsExitKey(key);
    }

    internal static bool IsExitKey(Key key)
    {
        if (key == Key.Q)
            return true;
        if (key == Key.C.WithCtrl)
            return true;
        return false;
    }

    internal static bool ShouldHandleHelpKey(Key key, bool searchPromptVisible)
    {
        if (searchPromptVisible)
            return false;

        return key == Key.H;
    }

    private static void WireHelpKeyBinding(Toplevel top, FrameView searchPrompt)
    {
        top.KeyDown += (_, args) =>
        {
            if (!ShouldHandleHelpKey(args, searchPrompt.Visible))
                return;

            args.Handled = true;
            ShowHelpDialog();
        };
    }

    private static void ShowHelpDialog()
    {
        var dialog = new Dialog
        {
            Title = "Keyboard Shortcuts",
            Width = 45,
            Height = HelpContent.Entries.Count + 6,
        };

        var label = new Label
        {
            Text = HelpContent.FormatHelpText(),
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Fill(1),
        };

        var okButton = new Button { Text = "OK", IsDefault = true };
        okButton.Accepting += (_, _) => Application.RequestStop();

        dialog.Add(label);
        dialog.AddButton(okButton);
        Application.Run(dialog);
    }

    // --- Search prompt ---

    private static FrameView CreateSearchPrompt()
    {
        var textField = new TextField
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Id = "SearchTextField",
        };

        var frame = new FrameView
        {
            Title = "Search",
            X = 0,
            Y = Pos.AnchorEnd(6),
            Width = Dim.Fill(),
            Height = 3,
            Visible = false,
            Id = "SearchPrompt",
        };

        frame.Add(textField);
        return frame;
    }

    private static TextField GetSearchTextField(FrameView searchPrompt)
    {
        return (TextField)searchPrompt.Subviews.First(v => v is TextField);
    }

    private static void ShowSearchPrompt(FrameView searchPrompt, TableView tableView)
    {
        var textField = GetSearchTextField(searchPrompt);
        textField.Text = string.Empty;
        searchPrompt.Visible = true;
        textField.SetFocus();
    }

    private static void HideSearchPrompt(FrameView searchPrompt, TableView tableView)
    {
        searchPrompt.Visible = false;
        tableView.SetFocus();
    }

    // --- Key bindings ---

    private static void WireSearchKeyBindings(
        Toplevel top,
        TableView tableView,
        FrameView searchPrompt,
        SearchCoordinator coordinator)
    {
        var textField = GetSearchTextField(searchPrompt);

        WireSearchPromptKeys(textField, searchPrompt, tableView, coordinator);
        WireToplevelSearchKeys(top, tableView, searchPrompt, coordinator);
    }

    private static void WireSearchPromptKeys(
        TextField textField,
        FrameView searchPrompt,
        TableView tableView,
        SearchCoordinator coordinator)
    {
        textField.KeyDown += (_, args) =>
        {
            if (args == Key.Enter)
            {
                args.Handled = true;
                HandleSearchSubmit(textField, searchPrompt, tableView, coordinator);
                return;
            }

            if (args == Key.Esc)
            {
                args.Handled = true;
                HideSearchPrompt(searchPrompt, tableView);
            }
        };
    }

    private static void HandleSearchSubmit(
        TextField textField,
        FrameView searchPrompt,
        TableView tableView,
        SearchCoordinator coordinator)
    {
        var query = textField.Text ?? string.Empty;
        HideSearchPrompt(searchPrompt, tableView);
        _ = coordinator.StartSearchAsync(query);
    }

    private static void WireToplevelSearchKeys(
        Toplevel top,
        TableView tableView,
        FrameView searchPrompt,
        SearchCoordinator coordinator)
    {
        top.KeyDown += (_, args) =>
        {
            if (args == Key.F.WithCtrl)
            {
                args.Handled = true;
                HandleCtrlF(searchPrompt, tableView, coordinator);
                return;
            }

            if (args == Key.F3)
            {
                args.Handled = true;
                NavigateToResult(coordinator.NavigateNext(), tableView);
                return;
            }

            if (args == Key.F3.WithShift)
            {
                args.Handled = true;
                NavigateToResult(coordinator.NavigatePrevious(), tableView);
                return;
            }

            if (args == Key.Esc && !searchPrompt.Visible && HasActiveSearch(coordinator))
            {
                args.Handled = true;
                coordinator.CancelSearch();
            }
        };
    }

    internal static bool HasActiveSearch(SearchCoordinator coordinator) =>
        coordinator.IsSearching || coordinator.Results.Count > 0;

    private static void HandleCtrlF(
        FrameView searchPrompt,
        TableView tableView,
        SearchCoordinator coordinator)
    {
        if (coordinator.IsSearching)
            coordinator.CancelSearch();

        ShowSearchPrompt(searchPrompt, tableView);
    }

    private static void NavigateToResult(SearchResult? result, TableView tableView)
    {
        if (result is null)
            return;

        var tableColumn = result.Value.Column + 1; // +1 for the row-number column
        tableView.SetSelection(tableColumn, result.Value.Row, false);
        tableView.EnsureSelectedCellIsVisible();
    }

    // --- Search status bar ---

    private static void UpdateSearchStatusLabel(StatusBar statusBar, SearchDisplayState state)
    {
        var searchLabel = statusBar.Subviews.ElementAtOrDefault(1) as Label;
        if (searchLabel is null)
            return;

        var formatted = SearchStatusFormatter.Format(state);
        searchLabel.Text = string.IsNullOrEmpty(formatted) ? string.Empty : $" | {formatted}";
    }

    // --- Search highlighting ---

    private static void ConfigureSearchHighlighting(
        TableView tableView,
        HashSet<(int Row, int Column)> highlightSet)
    {
        var highlightScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
            HotNormal = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
        };

        if (tableView.Table is null)
            return;

        for (var col = 0; col < tableView.Table.Columns; col++)
        {
            var style = tableView.Style.GetOrCreateColumnStyle(col);
            var colIndex = col;

            style.ColorGetter = args =>
            {
                // Column 0 is the row-number column; data columns start at 1
                var dataCol = colIndex - 1;
                if (dataCol < 0)
                    return null;

                if (highlightSet.Contains((args.RowIndex, dataCol)))
                    return highlightScheme;

                return null;
            };
        }
    }

    private static void UpdateSearchHighlightSet(
        HashSet<(int Row, int Column)> highlightSet,
        SearchCoordinator coordinator,
        SearchDisplayState state,
        TableView tableView)
    {
        RebuildHighlightSet(highlightSet, coordinator.Results, state.Status);
        tableView.SetNeedsDraw();
    }

    internal static void RebuildHighlightSet(
        HashSet<(int Row, int Column)> highlightSet,
        IReadOnlyList<SearchResult> results,
        SearchStatus status)
    {
        highlightSet.Clear();

        if (status != SearchStatus.Complete)
            return;

        foreach (var r in results)
            highlightSet.Add((r.Row, r.Column));
    }

    // --- Data table conversion ---

    internal static DataTable ToDataTable(SpreadsheetData data, int maxWidth)
    {
        var table = new DataTable();

        table.Columns.Add("#");

        foreach (var header in data.Headers)
            table.Columns.Add(TruncationHelper.Truncate(header, maxWidth));

        for (var r = 0; r < data.Rows.Count; r++)
        {
            var row = data.Rows[r];
            var values = new object[data.ColumnCount + 1];
            values[0] = (r + 1).ToString();

            for (var c = 0; c < data.ColumnCount; c++)
            {
                var raw = c < row.Count ? row[c] : string.Empty;
                values[c + 1] = TruncationHelper.Truncate(raw, maxWidth);
            }
            table.Rows.Add(values);
        }

        return table;
    }

}
