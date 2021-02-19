CREATE TABLE [GroupCompany] (
  [CompanyId] nchar(1) PRIMARY KEY NOT NULL
    CHECK (UPPER([CompanyId]) = [CompanyId]),
  [ShortName] nvarchar(30) NOT NULL
)
GO

CREATE TABLE [SalesCompany] (
  [CompanyId] nchar(1) PRIMARY KEY NOT NULL
    FOREIGN KEY REFERENCES [GroupCompany]
)
GO

CREATE TABLE [ManufacturingCompany] (
  [CompanyId] nchar(1) PRIMARY KEY NOT NULL
    FOREIGN KEY REFERENCES [GroupCompany]
)
GO

CREATE TABLE [User] (
  [UserId] int PRIMARY KEY NOT NULL,
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [GroupCompany],
  [Active] bit NOT NULL DEFAULT (1)
)
GO

CREATE TABLE [SalesCentre] (
  [SalesCentreId] nvarchar(30) PRIMARY KEY NOT NULL,
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [SalesCompany],
  [Name] nvarchar(30) NOT NULL,
  [Active] bit NOT NULL DEFAULT (1)
)
GO

CREATE TABLE [SalesUser] (
  [UserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [SalesCompany],
  [SalesCentreId] nvarchar(30) NOT NULL
    FOREIGN KEY REFERENCES [SalesCentre],
  [SalesPosition] char(30) NOT NULL,

  PRIMARY KEY ([UserId], [CompanyId])
)
GO
