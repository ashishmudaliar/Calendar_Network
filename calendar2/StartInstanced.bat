del "%~dp0instance1\BitCalendar.db"
xcopy "%~dp0bin" "%~dp0instance1" /E /i /Y
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0instance1\Client & java -jar BitCalendar.jar" 
START "runas /user:administrator" cmd /t:03 /K  "cd %~dp0instance1\Service & java -jar BitCalendarService.jar" 