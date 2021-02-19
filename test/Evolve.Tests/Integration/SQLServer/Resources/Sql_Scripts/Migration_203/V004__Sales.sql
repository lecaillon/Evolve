CREATE TABLE [Article] (
  [ArticleId] int PRIMARY KEY NOT NULL,
  [Abbr] nvarchar(30) NOT NULL,
  [Abbr2] nvarchar(30) NOT NULL,
  [Kind] nvarchar(12),
  [MeasurementUnit] nvarchar(8),
  [DimLength] money,
  [DimWidth] money,
  [DimThickness] money,
  [Valid] bit NOT NULL,
  [GoodsGroupId] nvarchar(12),
  [DrawingId] nvarchar(30)
)
GO

CREATE TABLE [TradingPartner] (
  [TradingPartnerId] int PRIMARY KEY NOT NULL,
  [Abbr] nvarchar(30) NOT NULL,
  [Name] nvarchar(100) NOT NULL,
  [AddressCity] nvarchar(4000),
  [AddressStreet] nvarchar(4000),
  [AddressPostcode] nvarchar(30),
  [AddressCountryIso2] char(2),
  [SalesCentreId] nvarchar(30),
  [SalesRepUserId] int,
  [Currency] varchar(3),
  [Rating] tinyint
)
GO
