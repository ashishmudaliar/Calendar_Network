del "%~dp0\bin\BitCalendar.db"
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0\bin & BitCalendarService.exe"
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0\bin & BitCalendar.exe" 