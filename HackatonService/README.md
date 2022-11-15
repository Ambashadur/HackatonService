# HackatonService
## Builidng Windows Service
For building Windows Service run this command: 
<br/> sc.exe create "{service_name}" binpath= "{path_to_project_exe}"
## Manage Windows Service
Start service: sc.exe start "{service_name}"<br>
Stop service: sc.exe stop "{service_name}"<br>
Delete service: sc.exe delete "{service_name}"
## Usefull Links
[How to create Windows Service](https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service)<br>
[How to implement Web API in Windows Service](https://csharp.christiannagel.com/2022/03/22/windowsservice-2/)<br>
[How to block application via Windows Group Politics](https://pomogaemkompu.temaretik.com/1736067292539127911/kak-zablokirovat-zapusk-programmy-v-windows/)