CREATE TABLE AHT_FlightInformation (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FlightNumber NVARCHAR(20) NOT NULL,
    Airline NVARCHAR(100) NULL,
    Destination NVARCHAR(100) NOT NULL,
    Gate NVARCHAR(10) NULL,
    Counter NVARCHAR(10) NULL,
    Status NVARCHAR(50) NULL,
    ScheduledTime DATETIME NOT NULL,
    EstimatedTime DATETIME NULL,
    ActualTime DATETIME NULL,
    Carousel NVARCHAR(10) NULL,
    Terminal NVARCHAR(10) NULL,
    IsDeparture BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
