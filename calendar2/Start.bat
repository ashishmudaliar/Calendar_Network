del "%~dp0\bin\BitCalendar.db"
START "runas /user:administrator" cmd /t:04 /K  "cd %~dp0\bin\Client & java -jar BitCalendar.jar" 
START "runas /user:administrator" cmd /t:04 /K  "cd %~dp0\bin\Service & java -jar BitCalendarService.jar" 