UPDATE [FidsDemo].[dbo].[AHT_FlightInformation]
SET ScheduledTime = DATEADD(DAY, DATEDIFF(DAY, ScheduledTime, GETDATE()), ScheduledTime),
    EstimatedTime = DATEADD(DAY, DATEDIFF(DAY, EstimatedTime, GETDATE()), EstimatedTime),
    ActualTime    = DATEADD(DAY, DATEDIFF(DAY, ActualTime, GETDATE()), ActualTime);
