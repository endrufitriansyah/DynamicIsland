; ============================================================
; Dynamic Island - Inno Setup Script
; Version: 1.0.0
; Target: Windows 10/11 64-bit
; Mode: Self-Contained (.NET runtime included)
; ============================================================

#define MyAppName "Dynamic Island"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Dynamic Island"
#define MyAppExeName "DynamicIsland.exe"
#define MyAppSourceDir "..\publish-self-contained"
#define MyAppDataFolder "DynamicIsland"

[Setup]
AppId={{B5E2F8A1-3C7D-4E9F-A6B8-1D2E3F4A5B6C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\installer-output
OutputBaseFilename=DynamicIsland-Setup
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
MinVersion=10.0
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline dialog
RestartIfNeededByRun=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  RemoveUserData: Boolean;

function InitializeUninstall(): Boolean;
var
  AppDataPath: String;
  Msg: String;
  CrLf: String;
begin
  Result := True;
  RemoveUserData := False;
  CrLf := Chr(13) + Chr(10);

  AppDataPath := ExpandConstant('{localappdata}\{#MyAppDataFolder}');

  // In silent mode, preserve user data by default without prompting
  if UninstallSilent() then
  begin
    RemoveUserData := False;
    Exit;
  end;

  if DirExists(AppDataPath) then
  begin
    Msg := 'Do you also want to delete your Dynamic Island settings and data?' + CrLf +
           CrLf +
           'Location: ' + AppDataPath + CrLf +
           CrLf +
           'Click "Yes" to delete everything (application + data).' + CrLf +
           'Click "No" to keep your data (you can restore it later).' + CrLf +
           'Click "Cancel" to abort uninstallation.';

    case MsgBox(Msg, mbConfirmation, MB_YESNOCANCEL or MB_DEFBUTTON2) of
      IDYES: RemoveUserData := True;
      IDNO: RemoveUserData := False;
      IDCANCEL: Result := False;
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if RemoveUserData then
    begin
      AppDataPath := ExpandConstant('{localappdata}\{#MyAppDataFolder}');
      if DirExists(AppDataPath) then
      begin
        DelTree(AppDataPath, True, True, True);
      end;
    end;
  end;
end;
