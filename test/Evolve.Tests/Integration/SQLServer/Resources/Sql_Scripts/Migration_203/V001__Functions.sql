CREATE FUNCTION IsPrintableAscii (@p varchar(max)) RETURNS bit
AS
BEGIN
  RETURN IIF(@p NOT LIKE '%[^' + CHAR(32) + '-' + CHAR(126) + ']%' COLLATE Latin1_General_100_BIN2, 1, 0)
END
GO

-- R function of Speck cipher, adapted to work on 8-bit numbers (insecure)
CREATE FUNCTION dbo.FpR (@x tinyint, @y tinyint, @k tinyint)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN (
  WITH inter(x) AS (SELECT CAST(((((@x * 8) | (@x / 32)) + @y) ^ @k) % 256 AS tinyint))
  SELECT x, CAST((((@y * 4) | (@y / 64)) ^ x) % 256 AS tinyint) AS y FROM inter
);
GO

-- Obscures a 16-bit unsigned int @n given a 16-bit unsigned int key @k.
CREATE FUNCTION dbo.FpObscure16 (@k int, @n int)
RETURNS int
WITH SCHEMABINDING
AS
BEGIN
  DECLARE @y tinyint = @n % 256;
  DECLARE @x tinyint = @n / 256;
  DECLARE @b tinyint = @k % 256;
  DECLARE @a tinyint = @k / 256;

  SELECT @x = x, @y = y FROM dbo.FpR(@x, @y, @b)
  SELECT @a = x, @b = y FROM dbo.FpR(@a, @b, 0)
  SELECT @x = x, @y = y FROM dbo.FpR(@x, @y, @b)

  RETURN @y + (@x * 256);
END
GO

-- Obscures an int @n between 0 and 9999 to one in the same domain, given a 16-bit unsigned int key @k.
CREATE FUNCTION dbo.FpObscureFourDigits (@k int, @n int)
RETURNS int
WITH SCHEMABINDING
AS
BEGIN
  DECLARE @enc int = dbo.FpObscure16(@k, @n)
  WHILE (@enc >= 10000) -- "cycle walking" until we're in our domain
    SET @enc = dbo.FpObscure16(@k, @enc)
  RETURN @enc
END
GO

CREATE FUNCTION dbo.ObscureOfferCounter (@CompanyId nchar(1), @OfferSequence char(4), @OfferCounter smallint)
RETURNS smallint
WITH SCHEMABINDING
AS
BEGIN
  DECLARE @key int = (0xBAD1 + UNICODE(@CompanyId) + CAST(@OfferSequence AS INT)) & 0xFFFF
  RETURN dbo.FpObscureFourDigits(@key, @OfferCounter)
END
GO

CREATE FUNCTION OfferPresentationId (@CompanyId nchar(1), @OfferSequence char(4), @OfferCounter smallint)
RETURNS nchar(9)
WITH SCHEMABINDING
AS
BEGIN
  RETURN @CompanyId + @OfferSequence +
    REPLACE(STR(dbo.ObscureOfferCounter(@CompanyId, @OfferSequence, @OfferCounter), 4), ' ', '0') -- deterministic zero-pad
END
GO

-- Returns yyMM for a date
CREATE FUNCTION OfferSequence(@p date) RETURNS char(4)
WITH SCHEMABINDING
AS
BEGIN
  -- yyyyMMdd truncated to yyMM; style 12 (yyMMdd) won't work here due to non-determinism
  -- TODO: decide how we're going to handle timezones
  RETURN RIGHT(CONVERT(char(6), @p, 112), 4)
END
GO
