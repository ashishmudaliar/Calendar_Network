netsh http delete urlacl url=http://+:%1/BitCalendarService
netsh http add urlacl url=http://+:%1/BitCalendarService user=everyone