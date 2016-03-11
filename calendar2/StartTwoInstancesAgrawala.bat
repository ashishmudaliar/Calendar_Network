del "%~dp0instance1\BitCalendar.db"
xcopy "%~dp0bin" "%~dp0instance1" /E /i /Y
START "runas /user:administrator" cmd /t:04 /K  "cd %~dp0instance1\Service & java -jar BitCalendarService.jar 5555 0" 
START "runas /user:administrator" cmd /t:04 /K  "cd %~dp0instance1\Client & java -jar BitCalendar.jar 5555" 

del "%~dp0instance2\BitCalendar.db"
xcopy "%~dp0bin" "%~dp0instance2" /E /i /Y
START "runas /user:administrator" cmd /t:0A /K  "cd %~dp0instance2\Service & java -jar BitCalendarService.jar 8888 0" 
START "runas /user:administrator" cmd /t:0A /K  "cd %~dp0instance2\Client & java -jar BitCalendar.jar 8888"