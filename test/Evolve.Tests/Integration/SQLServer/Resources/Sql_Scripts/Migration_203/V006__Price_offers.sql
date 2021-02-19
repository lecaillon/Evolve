CREATE TABLE [Offer] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] AS dbo.OfferSequence([CreatedDate]) PERSISTED NOT NULL
    CHECK ([OfferSequence] LIKE '[0-9][0-9][0-1][0-9]'),
  [OfferCounter] smallint NOT NULL
    CHECK ([OfferCounter] BETWEEN 0 AND 9999),
  [OfferPresentationId] AS dbo.OfferPresentationId([CompanyId], dbo.OfferSequence([CreatedDate]), [OfferCounter])
    PERSISTED NOT NULL UNIQUE,

  [SalesCentreId] nvarchar(30) NOT NULL
    FOREIGN KEY REFERENCES [SalesCentre],
  [CreatedDate] datetimeoffset NOT NULL
    CHECK (YEAR([CreatedDate]) BETWEEN 2000 AND 2099),
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [TradingPartnerId] int NOT NULL
    FOREIGN KEY REFERENCES [TradingPartner],

  [PartnerReference] nvarchar(30) NULL
    CHECK (LEN([PartnerReference]) > 0),
  [Address] nvarchar(4000) NOT NULL,
  [CoverText] nvarchar(max) NOT NULL,
  [ConditionsText] nvarchar(4000) NOT NULL,
  [ClosingText] nvarchar(max) NOT NULL,
  [Comment] nvarchar(max) NOT NULL,

  [OfferTemplateId] nvarchar(30) NOT NULL,
  [LanguageIetfLanguageCode] varchar(30) NOT NULL,
  [CultureIetfLanguageCode] varchar(30) NOT NULL,
  [ValidUntilDate] date NOT NULL,
  [CurrencyIsoCode] char(3) NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter]),
  FOREIGN KEY ([CompanyId], [CurrencyIsoCode]) REFERENCES [OfferCurrency],
  FOREIGN KEY ([CompanyId], [OfferTemplateId]) REFERENCES [OfferTemplate]
)
GO

CREATE TABLE [OfferSourceEnquiry] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [EnquiryId] int NOT NULL
    FOREIGN KEY REFERENCES [Enquiry],

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter]) REFERENCES [Offer]
)
GO

CREATE TABLE [OfferItemGroup] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [GroupPosition] smallint NOT NULL DEFAULT (1)
    CHECK ([GroupPosition] = 1),
  [PresentationColumnsJson] nvarchar(max) NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition])
)
GO

CREATE TABLE [OfferItemPricedQuantity] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [GroupPosition] smallint NOT NULL,
  [ItemPosition] smallint NOT NULL,

  [ArticleAbbr] nvarchar(30) NULL
    CHECK (LEN([ArticleAbbr]) > 0),
  [DrawingId] nvarchar(30) NULL
    CHECK (LEN([DrawingId]) > 0),
  [Description] nvarchar(4000) NOT NULL,
  [Dimensions] nvarchar(30) NOT NULL,
  [MaterialId] nvarchar(30) NULL
    FOREIGN KEY REFERENCES [Material],
  [Comment] nvarchar(4000) NOT NULL,
  [MeasurementUnit] nvarchar(8) NOT NULL DEFAULT N'pc'
    CHECK ([MeasurementUnit] = N'pc'),

  [Quantity] decimal(9, 2) NOT NULL
    CHECK ([Quantity] = FLOOR([Quantity])),
  [UnitPrice] money NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition], [ItemPosition]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition]) REFERENCES [OfferItemGroup]
)
GO

CREATE TABLE [OfferItemCustomColumn] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [GroupPosition] smallint NOT NULL,
  [ColumnName] nvarchar(30) NOT NULL,
  [ColumnType] varchar(30) NOT NULL
    CHECK ([ColumnType] IN ('String', 'Currency')),

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition], [ColumnName]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition]) REFERENCES [OfferItemGroup]
)
GO

CREATE TABLE [OfferItemCustomValue] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [GroupPosition] smallint NOT NULL,
  [ItemPosition] smallint NOT NULL,
  [ColumnName] nvarchar(30) NOT NULL,

  [CurrencyValue] money NULL,
  [StringValue] nvarchar(30) NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition], [ItemPosition], [ColumnName]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter], [GroupPosition], [ColumnName]) REFERENCES [OfferItemCustomColumn],
  CHECK (IIF([StringValue] IS NOT NULL, 1, 0) + IIF([CurrencyValue] IS NOT NULL, 1, 0) = 1)
)
GO

CREATE TABLE [OfferRender] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [FileType] varchar(8) NOT NULL
    CHECK ([FileType] IN ('PDF', 'DOCX')),
  [Content] varbinary(max) NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter], [FileType]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter]) REFERENCES [Offer]
)
GO

CREATE TABLE [OfferInvalidation] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [Reason] nvarchar(30) NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter]) REFERENCES [Offer]
)
GO

CREATE TABLE [OfferResult] (
  [CompanyId] nchar(1) NOT NULL,
  [OfferSequence] char(4) NOT NULL,
  [OfferCounter] smallint NOT NULL,
  [Result] nvarchar(30) NOT NULL,
  [Comment] nvarchar(4000) NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [CreatedDate] datetimeoffset NOT NULL,

  PRIMARY KEY ([CompanyId], [OfferSequence], [OfferCounter]),
  FOREIGN KEY ([CompanyId], [OfferSequence], [OfferCounter]) REFERENCES [Offer]
)
GO
