; �ű��� Inno Setup �ű��� ���ɣ�
; �йش��� Inno Setup �ű��ļ�����ϸ��������İ����ĵ���

#define MyAppName "FileOnlineEdit"
#define MyAppVersion "1.0"
#define MyAppPublisher "DreamWorks"
#define MyAppExeName "FileOnlineEdit.exe"

[Setup]
; ע: AppId��ֵΪ������ʶ��Ӧ�ó���
; ��ҪΪ������װ����ʹ����ͬ��AppIdֵ��
; (�����µ�GUID����� ����|��IDE������GUID��)
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
Source: "F:\�����\Github\UpDownFile\UpDownFile2\bin\Release\FileOnlineEdit.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "F:\�����\Github\UpDownFile\UpDownFile2\bin\Release\FileOnlineEdit.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; ע��: ��Ҫ���κι���ϵͳ�ļ���ʹ�á�Flags: ignoreversion��

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[registry]
;���δ��������ע����еļ�ֵ
Root:HKCR;Subkey:Ycsy;ValueType:string;ValueName:"URL Protocol";ValueData:"{app}\FileOnlineEdit.exe"
Root:HKCR;Subkey:"Ycsy\DefaultIcon";ValueType:string;ValueData:"%SystemRoot%\\system32\\url.dll,0"
Root:HKCR;Subkey:"Ycsy\shell\open\command";ValueType:string;ValueData:"""{app}\FileOnlineEdit.exe\"" ""%1"""

[Code]
//ж��ʱɾ����װ����ӵ�ע�����
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RegDeleteKeyIncludingSubkeys(HKEY_CLASSES_ROOT, 'Ycsy')
end;