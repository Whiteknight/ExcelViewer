# ExcelTerminalViewer

A terminal-based spreadsheet viewer for Excel and CSV files. Navigate, search, and view your spreadsheets directly in the terminal with a clean TUI interface.

## Features

- View Excel (.xlsx, .xls) and CSV files in the terminal
- Navigate cells with arrow keys
- Search functionality with result navigation
- Adjustable column width
- Jump to specific rows
- Keyboard-driven interface

## Supported Formats

- `.xlsx` - Excel 2007+ format
- `.xls` - Legacy Excel format
- `.csv` - Comma-separated values

## Installation

Requires .NET 10.0 or later.

```bash
dotnet build
```

## Usage

```bash
ExcelTerminalViewer <file-path> [--max-width N] [--go-to ROW]
```

### Options

- `<file-path>` - Path to the spreadsheet file (required)
- `--max-width N` - Maximum column width (5-200, default: 30)
- `--go-to ROW` - Jump to specific row on startup (1-based index)

### Examples

```bash
# View a spreadsheet
ExcelTerminalViewer data.xlsx

# View with custom column width
ExcelTerminalViewer data.csv --max-width 50

# Open and jump to row 100
ExcelTerminalViewer report.xlsx --go-to 100
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Arrow keys` | Navigate cells |
| `Ctrl+F` | Open search |
| `Enter` | Submit search query |
| `F3` | Next search result |
| `Shift+F3` | Previous search result |
| `Esc` | Close search / clear results |
| `h` | Toggle help |
| `q` or `Ctrl+C` | Quit |

## Dependencies

- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) - Terminal UI framework
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) - Excel file handling
- [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader) - Legacy Excel format support
- [CsvHelper](https://github.com/JoshClose/CsvHelper) - CSV parsing

## License

See [LICENSE](LICENSE) file for details.
