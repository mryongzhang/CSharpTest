; 脚本由 Inno Setup 脚本向导 生成！
; 有关创建 Inno Setup 脚本文件的详细资料请查阅帮助文档！

#define MyAppName "FileOnlineEdit"
#define MyAppVersion "1.0"
#define MyAppPublisher "DreamWorks"
#define MyAppExeName "FileOnlineEdit.exe"

[Setup]
; 注: AppId的值为单独标识该应用程序。
; 不要为其他安装程序使用相同的AppId值。
; (生成新的GUID，点击 工具|在IDE中生成GUID。)
AppId={{AF207FD8-8685-42B3-875A-4426D62C62F8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "F:\代码库\Github\UpDownFile\UpDownFile2\bin\Release\FileOnlineEdit.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "F:\代码库\Github\UpDownFile\UpDownFile2\bin\Release\FileOnlineEdit.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; 注意: 不要在任何共享系统文件上使用“Flags: ignoreversion”

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[registry]
;本段处理程序在注册表中的键值
Root:HKCR;Subkey:Ycsy;ValueType:string;ValueName:"URL Protocol";ValueData:"{app}\FileOnlineEdit.exe"
Root:HKCR;Subkey:"Ycsy\DefaultIcon";ValueType:string;ValueData:"%SystemRoot%\\system32\\url.dll,0"
Root:HKCR;Subkey:"Ycsy\shell\open\command";ValueType:string;ValueData:"""{app}\FileOnlineEdit.exe\"" ""%1"""

[Code]
//卸载时删除安装是添加的注册表项
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RegDeleteKeyIncludingSubkeys(HKEY_CLASSES_ROOT, 'Ycsy')
end;