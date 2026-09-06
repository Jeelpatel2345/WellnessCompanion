# Wellness Companion

A Windows desktop wellness companion for people who spend long periods working at a laptop.

## Current version

### Implemented
- Active-work reminder timer using Windows input idle detection.
- Water reminder after the configured active-work interval.
- Mandatory water overlay with a 30-second countdown by default.
- Food reminder with a configurable break duration and 10/15/20-minute shortcuts.
- Camera verification window after the water reminder.
- Local OpenCV face detection using the bundled Haar cascade.
- Face must be visible before the user can confirm the hydration check.
- 15-second retry delay with up to 3 verification attempts by default.
- Local JSON settings in `%LOCALAPPDATA%\\WellnessCompanion\\settings.json`.
- System tray operation with a custom application icon.
- Start-with-Windows support through the current user's Run registry key.
- Single-instance protection.
- Reminder sound option.
- Assets are copied into build/publish output so the camera model is available to the EXE.

## Important AI limitation

The current camera verification confirms **face presence + user confirmation**. It does not claim to prove that water was actually swallowed. Reliable automatic drinking detection requires a separate object/action model (for example bottle/cup detection plus temporal pose/action analysis). That is intentionally kept as Phase 2 instead of presenting face detection as drinking detection.

## Stack
- .NET 8
- WPF
- Windows Forms `NotifyIcon`
- OpenCvSharp 4.13
- P/Invoke `GetLastInputInfo`

## Requirements
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 with .NET desktop development workload, or the `dotnet` CLI

## Run from the repository

From the repository root:

```powershell
dotnet restore .\src\WellnessCompanion\WellnessCompanion.csproj
dotnet build .\src\WellnessCompanion\WellnessCompanion.csproj
dotnet run --project .\src\WellnessCompanion\WellnessCompanion.csproj
```

## Test quickly

Set:
- Water interval: `1` minute
- Water duration: `30` seconds
- Idle threshold: `120` seconds

Keep using the keyboard/mouse. After one minute of active use the water overlay should appear automatically. After its countdown, the verification window opens.

## Publish a Windows EXE

For a self-contained x64 build:

```powershell
dotnet publish .\src\WellnessCompanion\WellnessCompanion.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output:

```text
src\\WellnessCompanion\\bin\\Release\\net8.0-windows\\win-x64\\publish\\
```

The face model is embedded in the application and extracted to the user's local application data on first use, so the published application does not depend on a separate face-model file.

## Project structure

```text
WellnessCompanion/
├── src/WellnessCompanion/
│   ├── Assets/
│   │   ├── haarcascade_frontalface_default.xml  # embedded at build time
│   │   └── logo.ico                             # application/tray icon
│   ├── Models/
│   │   └── AppSettings.cs
│   ├── Services/
│   │   ├── CameraVerificationService.cs
│   │   ├── IdleService.cs
│   │   ├── ReminderService.cs
│   │   ├── SettingsService.cs
│   │   ├── StartupService.cs
│   │   └── TrayService.cs
│   ├── Windows/
│   │   ├── ReminderOverlay.xaml
│   │   └── WaterVerification.xaml
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── WellnessCompanion.csproj
└── README.md
```

## Phase 2 roadmap
1. Automatic bottle/cup detection.
2. Drinking-action detection over a short video window.
3. Better multi-monitor positioning.
4. Fullscreen/game-aware reminder behavior.
5. Daily hydration and break statistics.
6. Installer with Start Menu/Desktop shortcuts.
7. Versioned GitHub releases and automatic updates.

## Download the app

Download the newest `WellnessCompanion-win-x64.zip` from the repository's
[Releases page](../../releases). Extract it anywhere, then run
`WellnessCompanion.exe`. The download is self-contained and does not require a
.NET installation.

### Maintainer: publish a release

1. Update the version in `src/WellnessCompanion/WellnessCompanion.csproj`.
2. Commit and push the change to `main`.
3. Create and push a matching tag, for example `v1.0.0`:

   ```powershell
   git tag v1.0.0
   git push origin v1.0.0
   ```

GitHub Actions builds the self-contained Windows app, creates
`WellnessCompanion-win-x64.zip`, and publishes it on the GitHub Releases page.
