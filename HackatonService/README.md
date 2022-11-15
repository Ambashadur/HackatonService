# HackatonService
## Builidng Windows Service
For building Windows Service run this command: 
<br/> sc.exe create "{service_name}" binpath= "{path_to_project_exe}"
## Manage Windows Service
Start service: sc.exe start "{service_name}"<br>
Stop service: sc.exe stop "{service_name}"<br>
Delete service: sc.exe delete "{service_name}"