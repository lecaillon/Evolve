IF OBJECT_ID('CALENDRIER') IS NOT NULL DROP TABLE CALENDRIER

CREATE TABLE CALENDRIER
(
	JOUR						date				NOT NULL PRIMARY KEY CLUSTERED,
	SEMAINE					tinyint			NOT NULL,
	MOIS						tinyint			NOT NULL,
	TRIMESTRE				tinyint			NOT NULL,
	QUADRIMESTRE			tinyint			NOT NULL,
	ANNEE						smallint			NOT NULL,
	JOUR_SEMAINE			tinyint			NOT NULL,
	JOUR_MOIS				tinyint			NOT NULL,
	JOUR_AN					smallint			NOT NULL,
	ANNEE_FISCAL			smallint			NOT NULL,
	JOUR_CHOME				bit				NOT NULL DEFAULT 0
)

ALTER TABLE CALENDRIER
ADD CONSTRAINT [CK_CALENDRIER] CHECK 
(
	(ANNEE					> 1900)					AND
	(TRIMESTRE				BETWEEN 1 AND 4)		AND
	(QUADRIMESTRE			BETWEEN 1 AND 3)		AND
	(MOIS						BETWEEN 1 AND 12)		AND
	(SEMAINE					BETWEEN 1 AND 53)		AND
	(JOUR_SEMAINE			BETWEEN 1 AND 7)		AND
	(JOUR_MOIS				BETWEEN 1 AND 31)		AND
	(JOUR_AN					BETWEEN 1 AND 366)	AND
	(ANNEE_FISCAL			> 1900)	
)
GO
SET DATEFIRST 1;

DECLARE @currentDate as date
DECLARE @dateFin as date

SET @dateFin = '20301231'
SELECT @currentDate = GETDATE()

WHILE @currentDate <= @dateFin
BEGIN
	INSERT INTO CALENDRIER (JOUR,SEMAINE,MOIS,TRIMESTRE,QUADRIMESTRE,ANNEE,JOUR_SEMAINE,JOUR_MOIS,JOUR_AN,ANNEE_FISCAL) VALUES 
										(@currentDate, 
										 DATEPART(ISO_WEEK, @currentDate),
										 DATEPART(MM, @currentDate), 
										 DATEPART(QQ, @currentDate), 
										 CASE WHEN DATEPART(MM, @currentDate) <= 4 THEN 1
												WHEN DATEPART(MM, @currentDate) <= 8 THEN 2
												WHEN DATEPART(MM, @currentDate) <= 12 THEN 3 END,
										 DATEPART(YY, @currentDate),
										 DATEPART(DW, @currentDate),
										 DATEPART(DD, @currentDate),
										 DATEPART(DY, @currentDate),
										 CASE WHEN DATEPART(ISO_WEEK, @currentDate) >= 51 AND DATEPART(DY, @currentDate) < 7 THEN DATEPART(YY, @currentDate) - 1
												WHEN DATEPART(ISO_WEEK, @currentDate) <= 2 AND DATEPART(DY, @currentDate) > 350 THEN DATEPART(YY, @currentDate) + 1
												ELSE DATEPART(YY, @currentDate) END)
	
	SET @currentDate = DATEADD(DAY, 1, @currentDate)
END

GO