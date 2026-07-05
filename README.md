# Testador NEON HIO115

## What It Does

Industrial software project for testing, validating, and diagnosing NEON/HI Tecnologia CLP units with digital I/O modules. Built with C# and Windows Forms, it provides a practical bench-top validation interface for hardware testing workflows.

---

## Why It Exists

When testing industrial CLPs in field conditions, you need a reliable, repeatable way to validate digital inputs/outputs and other hardware modules. This tool provides:
- Structured test flows for CLP validation
- Visual feedback for I/O operations
- Organized logging and test evidence
- Portable Windows-based interface

---

## Features

- ✓ Digital I/O testing (inputs and outputs)
- ✓ Analog input monitoring (if supported by module)
- ✓ Serial communication diagnostics
- ✓ Structured test profiles and workflows
- ✓ Test logging and evidence collection
- ✓ Windows Forms GUI for field use
- ✓ Modular project structure for extensions

---

## Tech Stack

- **Language:** C#
- **Framework:** .NET 8 / .NET Framework
- **UI:** Windows Forms (WinForms)
- **Hardware:** NEON HIO115 Module
- **Protocol:** Serial Communication / Modbus
- **Version Control:** Git, GitHub

---

## Project Status

**Status:** Work in Progress / Experimental  
**Phase:** Testing and validation  
**Last Updated:** June 2026

This is an active portfolio project focused on practical industrial automation diagnostics and field validation workflows.

---

## How to Run

### Prerequisites

- Windows 10 or later
- .NET 8 SDK (or appropriate .NET Framework)
- Visual Studio or VS Code with C# support
- NEON HIO115 hardware (or simulated environment)

### Build & Run

```bash
# Clone the repository
git clone https://github.com/AtsonMelo/testador-neon-hio115.git
cd testador-neon-hio115

# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run (GUI)
dotnet run -- --gui

# Or run from compiled executable
./bin/Release/net8.0/testador-neon-hio115.exe
```

### Project Structure

```
testador-neon-hio115/
├── src/                          # Main application code
│   ├── Forms/                    # WinForms UI components
│   ├── Models/                   # Data models and structures
│   ├── Services/                 # Business logic services
│   └── Program.cs                # Entry point
├── tests/                        # Unit tests
├── profiles/                     # Hardware test profiles
├── scripts/                      # Helper scripts
├── docs/                         # Technical documentation
├── TESTADOR_NEON_HIO115_1_1/    # Legacy/reference builds
└── README.md
```

---

## Roadmap

### Completed

- [x] Digital I/O test interface (DO00-DO03, DI00-DI07)
- [x] Basic hardware communication
- [x] Test logging
- [x] Project structure and organization

### In Progress / Planned

- [ ] Analog input support (AI00-AI02)
- [ ] Serial communication testing UI
- [ ] Test report generation (PDF/HTML)
- [ ] Automated test sequences
- [ ] Hardware profile library
- [ ] Data export to CSV

---

## Security and privacy

- No real credentials, tokens, client data or production environment details should be committed.
- Examples use placeholders or simulated data.
- Sensitive values must stay in local environment variables or ignored files.

### Additional repository notes

⚠️ **Important:**
- Do not commit credentials, API keys, or authentication tokens
- Do not expose internal IP addresses or company network details
- Do not commit customer data, serial numbers, or site-specific information
- Do not include real hardware configurations that might reveal operational systems
- Use `.gitignore` for `.env`, `*.log`, and config files with real values

---

## Contributing

This is a personal portfolio project. For improvements, suggestions, or bug reports:

1. Open an issue describing the problem or feature
2. Feel free to fork and experiment
3. Submit PRs for documentation improvements

---

## License

License: pending decision.

---

## Author

**Atson Melo**  
Focus: Automation, Industrial Diagnostics, C#/.NET, Python, PowerShell  
GitHub: [@AtsonMelo](https://github.com/AtsonMelo)

---

## See Also

- [atson-powershell-toolkit](https://github.com/AtsonMelo/atson-powershell-toolkit) – PowerShell automation
- [monitor-hardware](https://github.com/AtsonMelo/monitor-hardware) – Hardware monitoring
- [monitor-com](https://github.com/AtsonMelo/monitor-com) – Serial communication monitoring
