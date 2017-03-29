IF OBJECT_ID ('GetLatestSunday') IS NOT NULL DROP FUNCTION GetLatestSunday;

GO

CREATE FUNCTION GetLatestSunday(@datepivot date) RETURNS date
BEGIN 

	RETURN CASE WHEN DATEPART(WEEKDAY, @datepivot) = 7 THEN @datepivot 
					ELSE (SELECT DATEADD(DAY, -DATEPART(DW, @datepivot), @datepivot)) END;
	
END

GO