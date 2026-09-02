; Inno Setup script for PCOptimizer
; Build with: ISCC.exe PCOptimizer.iss
; Requires Inno Setup 6+ on the build machine (or GitHub Actions windows runner)

#define MyAppName "PCOptimizer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "PCOptimizer"
#define MyAppURL "https://github.com/YourOrg/PCOptimizer"
#define MyAppExeName "PCOptimizer.exe"
#define MyUpdaterExeName "PCOptimizer.Updater.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
OutputDir=..\artifacts\installer
OutputBaseFilename=PCOptimizerSetup
SetupIconFile=
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start PCOptimizer with Windows"; GroupDescription: "Startup"; Flags: unchecked

[Files]
; Main application (publish output)
Source: "..\artifacts\publish\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Updater
Source: "..\artifacts\publish\updater\*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Autostart (optional)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PCOptimizer"; ValueData: """{app}\{#MyAppExeName}"" --minimized"; Flags: uninsdeletevalue; Tasks: autostart
; Install location for updater / diagnostics
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallDir"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Optional: remove user data only if user chooses (handled in Code section)
Type: filesandordirs; Name: "{localappdata}\PCOptimizer\Cache"
Type: filesandordirs; Name: "{localappdata}\PCOptimizer\UpdateCache"

[Code]
function InitializeSetup(): Boolean;
var
  InstallDir: String;
  PrevVersion: String;
begin
  Result := True;
  // Detect existing installation via registry
  if RegQueryStringValue(HKLM, 'Software\{#MyAppPublisher}\{#MyAppName}', 'InstallDir', InstallDir) then
  begin
    RegQueryStringValue(HKLM, 'Software\{#MyAppPublisher}\{#MyAppName}', 'Version', PrevVersion);
    // Wizard will offer upgrade path automatically when AppId matches
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Ask whether to keep user data (logs, backups, settings)
    if MsgBox('Do you want to remove user data (logs, backups, settings) as well?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      DelTree(ExpandConstant('{localappdata}\PCOptimizer'), True, True, True);
    end;
  end;
end;
