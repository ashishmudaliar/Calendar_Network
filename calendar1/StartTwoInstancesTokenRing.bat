del "%~dp0instance1\BitCalendar.db"
xcopy "%~dp0bin" "%~dp0instance1" /E /i /Y
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0instance1 & BitCalendarService.exe 5555 1" 
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0instance1 & BitCalendar.exe 5555"

del "%~dp0instance2\BitCalendar.db"
xcopy "%~dp0bin" "%~dp0instance2" /E /i /Y
START "runas /user:administrator" cmd /t:05 /K  "cd %~dp0instance2 & BitCalendarService.exe 8888 1"
START "runas /user:administrator" cmd /t:05 /K  "cd %~dp0instance2 & BitCalendar.exe 8888"