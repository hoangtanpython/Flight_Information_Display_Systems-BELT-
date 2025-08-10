/* =========================
   FIDS Arrivals – Wipe & Reload from PDF (Today)
   ========================= */
SET NOCOUNT ON;
BEGIN TRY
  BEGIN TRAN;

  -- 1) XÓA toàn bộ ARRIVALS hiện có
  DELETE FROM FidsDemo.dbo.AHT_FlightInformation
  WHERE IsDeparture = 0;

  -- 2) Tạo dữ liệu NGUỒN theo PDF (giờ = hôm nay + giờ PDF)
  DECLARE @today date = CONVERT(date, GETDATE());
  DECLARE @base  datetime2(0) = CAST(@today AS datetime2(0));

  ;WITH src AS (
    SELECT
      v.FlightNumber,
      v.Destination,
      v.Airline,
      Terminal    = v.Terminal,
      Carousel    = CASE WHEN v.CarouselNum IS NULL THEN NULL ELSE CONCAT('B', v.CarouselNum) END,
      ActualTime  = DATEADD(SECOND,
                     DATEDIFF(SECOND, CAST('00:00:00' AS time), CAST(v.SchedTime AS time)),
                     @base)
    FROM (VALUES
      -- p1
      ('7C2275','Seoul (ICN)','JEJU AIR',                        2, 3,'23:55'),
      ('TW175', 'Seoul (ICN)','T''WAY AIR',                      2, 4,'00:20'),
      ('7C2217','Seoul (ICN)','JEJU AIR',                        2, 1,'01:00'),
      ('KE2093','Busan (PUS)','KOREAN AIRLINES',                 2, 2,'00:35'),
      ('VJ875', 'Seoul (ICN)','VIETJET AIR',                     2, 4,'01:10'),
      ('KC259', 'Astana (NQZ)','AIR ASTANA',                     2, 3,'08:25'),
      ('VJ881', 'Seoul (ICN)','VIETJET AIR',                     2, 4,'08:55'),
      -- p2
      ('DV5340','Almaty (ALA)','SCAT AIRLINES',                  2, 1,'09:00'),
      ('UO552', 'Hong Kong (HKG)','HONG KONG EXPRESS AIRWAYS',   2, 2,'09:20'),
      ('FD634', 'Bangkok (DMK)','THAI AIR ASIA',                 2, 3,'09:30'),
      ('VJ879', 'Seoul (ICN)','VIETJET AIR',                     2, 1,'09:40'),
      ('VZ964', 'Bangkok (BKK)','THAI VIETJET',                  2, 4,'09:55'),
      ('DV5342','Astana (NQZ)','SCAT AIRLINES',                  2, 2,'10:00'),
      ('TW171', 'Seoul (ICN)','T''WAY AIR',                      2, 3,'10:45'),
      ('AK642', 'Kuala Lumpur (KUL)','AIR ASIA',                 2, 4,'10:50'),
      ('FD636', 'Bangkok (DMK)','THAI AIR ASIA',                 2, 2,'11:30'),
      -- p3
      ('BR383', 'Taipei (TPE)','EVA AIR',                        2, 3,'11:35'),
      ('AK648', 'Kuala Lumpur (KUL)','AIR ASIA',                 2, 2,'12:05'),
      ('OD502', 'Kuala Lumpur (KUL)','BATIK AIR MALAYSIA',       2, 4,'12:05'),
      ('7C2261','Busan (PUS)','JEJU AIR',                        2, 1,'12:10'),
      ('VN337', 'Osaka (KIX)','VIETNAM AIRLINES',                2, 1,'12:25'),
      ('VZ960', 'Bangkok (BKK)','THAI VIETJET',                  2, 4,'12:30'),
      ('VJ989', 'Busan (PUS)','VIETJET AIR',                     2, 3,'12:30'),
      ('VN319', 'Tokyo (NRT)','VIETNAM AIRLINES',                2, 2,'12:40'),
      -- p4
      ('VJ970', 'Singapore (SIN)','VIETJET AIR',                 2, 3,'13:10'),
      ('7C2211','Seoul (ICN)','JEJU AIR',                        2, 1,'13:20'),
      ('MH748', 'Kuala Lumpur (KUL)','MALAYSIA AIRLINES',        2, 4,'13:40'),
      ('KE457', 'Seoul (ICN)','KOREAN AIRLINES',                 2, 2,'13:40'),
      ('VN431', 'Seoul (ICN)','VIETNAM AIRLINES',                2, 1,'13:50'),
      ('AK640', 'Kuala Lumpur (KUL)','AIR ASIA',                 2, 3,'15:15'),
      ('HX548', 'Hong Kong (HKG)','HONG KONG AIRLINES',          2, 4,'15:25'),
      ('SQ174', 'Singapore (SIN)','SINGAPORE AIRLINES',          2, 1,'15:45'),
      -- p5
      ('CI789', 'Taipei (TPE)','CHINA AIRLINES',                 2, 2,'16:35'),
      ('VJ8891','Macau (MFM)','VIETJET AIR',                     2, 2,'17:20'),
      ('UO558', 'Hong Kong (HKG)','HONG KONG EXPRESS AIRWAYS',   2, 1,'17:25'),
      ('VZ962', 'Bangkok (BKK)','THAI VIETJET',                  2, 3,'17:30'),
      ('JX703', 'Taipei (TPE)','STARLUX AIRLINES',               2, 2,'17:35'),
      ('FD638', 'Bangkok (DMK)','THAI AIR ASIA',                 2, 4,'17:40'),
      ('IT551', 'Taipei (TPE)','TIGERAIR TAIWAN',                2, 1,'18:40'),
      ('VN626', 'Bangkok (BKK)','VIETNAM AIRLINES',              2, 2,'19:45'),
      ('NX978', 'Macau (MFM)','AIR MACAU',                       2, 1,'20:30'),
      -- p6
      ('KE461', 'Seoul (ICN)','KOREAN AIRLINES',                 2, 2,'21:05'),
      ('UO560', 'Hong Kong (HKG)','HONG KONG EXPRESS AIRWAYS',   2, 4,'21:25'),
      ('5J5756','Manila (MNL)','CEBU PACIFIC',                   2, 1,'21:30'),
      ('OZ755', 'Seoul (ICN)','ASIANA AIRLINES',                 2, 3,'21:35'),
      ('EK370', 'Bangkok (BKK)','EMIRATES',                      2, 2,'21:50'),
      ('QH9589','Macau (MFM)','BAMBOO AIRWAYS',                  2, 1,'22:05'),
      ('TW173', 'Seoul (ICN)','T''WAY AIR',                      2, 4,'22:40'),
      ('LJ81',  'Seoul (ICN)','JIN AIR',                         2, 1,'23:05'),
      ('NX986', 'Macau (MFM)','AIR MACAU',                       2, 2,'23:15'),
      -- p7 (map thiếu cột cuối là Carousel: B3/B4; Terminal giả định = 2)
      ('ZE5932','Seoul (ICN)','EASTAR JET',                      2, 3,'23:25'),
      ('RS5112','Seoul (ICN)','AIR SEOUL',                       2, 4,'23:30'),
      ('TW181', 'Cheongju (CJJ)','T''WAY AIR',                   2, 4,'23:50'),
      ('BX773', 'Busan (PUS)','AIR BUSAN',                       2, 2,'23:50'),
      ('RF5312','Cheongju (CJJ)','AERO-K AIRLINES',              2, 3,'23:55'),
      ('TW183', 'Daegu (TAE)','T''WAY AIR',                      2, 3,'23:55'),
      ('LJ111', 'Busan (PUS)','JIN AIR',                         2, 1,'23:55')
    ) AS v(FlightNumber, Destination, Airline, Terminal, CarouselNum, SchedTime)
  )
  -- 3) CHÈN MỚI
  INSERT INTO FidsDemo.dbo.AHT_FlightInformation
  (
    FlightNumber, Airline, Destination, Gate, Counter, Status,
    ScheduledTime, EstimatedTime, ActualTime, Carousel, Terminal,
    IsDeparture, CreatedAt, UpdatedAt
  )
  SELECT
    s.FlightNumber,
    s.Airline,
    s.Destination,
    NULL AS Gate,
    NULL AS Counter,
    CASE
      WHEN GETDATE() <  s.ActualTime                                  THEN 'Waiting for Baggage'
      WHEN DATEDIFF(MINUTE, s.ActualTime, GETDATE()) BETWEEN 0 AND 4  THEN 'Waiting for Baggage'
      WHEN DATEDIFF(MINUTE, s.ActualTime, GETDATE()) BETWEEN 5 AND 9  THEN 'Baggage Claiming'
      WHEN DATEDIFF(MINUTE, s.ActualTime, GETDATE()) >= 10            THEN 'Baggage Delivered'
      ELSE 'Waiting for Baggage'
    END AS Status,
    s.ActualTime AS ScheduledTime,   -- nếu bạn muốn giữ “Scheduled” đúng nghĩa: thay bằng NULL
    s.ActualTime AS EstimatedTime,
    s.ActualTime,
    s.Carousel,
    s.Terminal,
    0 AS IsDeparture,
    GETDATE() AS CreatedAt,
    GETDATE() AS UpdatedAt
  FROM src AS s;

  COMMIT TRAN;
  PRINT CONCAT('Reloaded arrivals: ', @@ROWCOUNT, ' rows.');
END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0 ROLLBACK TRAN;
  THROW;
END CATCH;
