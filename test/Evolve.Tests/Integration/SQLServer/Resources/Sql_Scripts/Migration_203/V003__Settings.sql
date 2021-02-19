CREATE TABLE [OfferCurrency] (
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [SalesCompany],
  [IsoCode] char(3) NOT NULL
    CHECK ([IsoCode] LIKE '[A-Z][A-Z][A-Z]'),
  [Symbol] nvarchar(5) NOT NULL,
  [DisplayDigits] tinyint NOT NULL
    CHECK ([DisplayDigits] BETWEEN 0 AND 4),
  [RoundingDigits] smallint NOT NULL
    CHECK ([RoundingDigits] BETWEEN 0 AND 4),
  [Active] bit NOT NULL DEFAULT (1),

  PRIMARY KEY ([CompanyId], [IsoCode])
)
GO

CREATE TABLE [OfferLanguage] (
  [IetfLanguageCode] varchar(30) PRIMARY KEY NOT NULL
    CHECK (LEN([IetfLanguageCode]) >= 2 AND dbo.IsPrintableAscii([IetfLanguageCode]) = 1),
  [Name] nvarchar(30) NOT NULL
    CHECK (LEN([Name]) > 0),
  [Active] bit NOT NULL DEFAULT (1)
)
GO

CREATE TABLE [OfferStringTranslation] (
  [IetfLanguageCode] varchar(30) NOT NULL
    FOREIGN KEY REFERENCES [OfferLanguage],
  [StringId] nvarchar(30) NOT NULL
    CHECK (LEN([StringId]) > 0),
  [Translation] nvarchar(4000) NOT NULL,

  PRIMARY KEY ([IetfLanguageCode], [StringId])
)
GO

CREATE TABLE [OfferCulture] (
  [IetfLanguageCode] varchar(30) PRIMARY KEY NOT NULL
    CHECK (dbo.IsPrintableAscii([IetfLanguageCode]) = 1),
  [Name] nvarchar(30) NOT NULL
    CHECK (LEN([Name]) > 0),
  [Active] bit NOT NULL DEFAULT (1)
)
GO

CREATE TABLE [OfferTemplate] (
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [SalesCompany],
  [OfferTemplateId] nvarchar(30) NOT NULL
    CHECK (LEN([OfferTemplateId]) > 0),
  [Content] varbinary(max) NOT NULL
    CHECK (LEN([Content]) > 0),
  [Active] bit NOT NULL DEFAULT (1),

  PRIMARY KEY ([CompanyId], [OfferTemplateId])
)
GO

CREATE TABLE [Material] (
  [MaterialId] nvarchar(30) PRIMARY KEY NOT NULL
    CHECK (LEN([MaterialId]) > 0),
  [Name] nvarchar(30) NOT NULL
    CHECK (LEN([Name]) > 0),
  [Active] bit NOT NULL DEFAULT (1)
)
GO
