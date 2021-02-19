CREATE TABLE [Enquiry] (
  [EnquiryId] int PRIMARY KEY NOT NULL IDENTITY(1, 1),
  [IdempotenceKey] uniqueidentifier NOT NULL UNIQUE,
  [CompanyId] nchar(1) NOT NULL
    FOREIGN KEY REFERENCES [SalesCompany],
  [SalesCentreId] nvarchar(30) NOT NULL
    FOREIGN KEY REFERENCES [SalesCentre],
  [TradingPartnerId] int NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [CreatedDate] datetimeoffset NOT NULL,
  [PartnerReference] nvarchar(30) NULL
    CHECK (LEN([PartnerReference]) > 0),
  [ValueEstimateThousandsCzk] int NULL
    CHECK (LEN(ValueEstimateThousandsCzk) > 0),
  [Comment] nvarchar(4000) NOT NULL
)
GO

CREATE TABLE [EnquiryAttachment] (
  [EnquiryId] int NOT NULL
    FOREIGN KEY REFERENCES [Enquiry],
  [FileName] nvarchar(255) NOT NULL
    CHECK (LEN([FileName]) > 0),
  [Content] varbinary(max) NOT NULL
    CHECK (LEN([Content]) > 0),
  [CreatedDate] datetimeoffset NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],

  PRIMARY KEY ([EnquiryId], [FileName])
)
GO

CREATE TABLE [EnquirySource] (
  [EnquiryId] int PRIMARY KEY NOT NULL
    FOREIGN KEY REFERENCES [Enquiry],
  [ContactName] nvarchar(200) NULL
    CHECK (LEN([ContactName]) > 0),
  [ReceivedDate] datetimeoffset NOT NULL,
  [Type] varchar(30) NOT NULL
    CHECK ([Type] IN ('Email', 'Call', 'InternetCall', 'TextMessage', 'Fax', 'InPerson', 'Other')),
  [Comment] nvarchar(4000) NOT NULL,
)
GO

CREATE TABLE [EnquirySourceMessage] (
  [EnquiryId] int PRIMARY KEY NOT NULL
    FOREIGN KEY REFERENCES [EnquirySource],
  [Type] varchar(30) NOT NULL
    CHECK ([Type] IN ('OutlookMessageFormatUnicode')),
  [Content] varbinary(max) NOT NULL
    CHECK (LEN([Content]) > 0)
)
GO

CREATE TABLE [EnquiryItem] (
  [EnquiryItemId] int PRIMARY KEY NOT NULL IDENTITY(1, 1),
  [EnquiryId] int NOT NULL
    FOREIGN KEY REFERENCES [Enquiry],
  [InitiallyBypassed] bit NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [AssigneeUserId] int NULL
    FOREIGN KEY REFERENCES [User],
  [AssigneeCompanyId] nchar(1)
    FOREIGN KEY REFERENCES [ManufacturingCompany],
  [Position] smallint NOT NULL,

  [ArticleAbbr] nvarchar(30) NULL
    CHECK (LEN([ArticleAbbr]) > 0),
  [DrawingId] nvarchar(30) NULL
    CHECK (LEN([DrawingId]) > 0),
  [Description] nvarchar(4000) NOT NULL,
  [Dimensions] nvarchar(30) NOT NULL,
  [MaterialId] nvarchar(30) NULL
    FOREIGN KEY REFERENCES [Material],

  [MeasurementUnit] nvarchar(8) NOT NULL DEFAULT N'pc'
    CHECK ([MeasurementUnit] = N'pc'),
  [CreatedDate] datetimeoffset NOT NULL,

  CHECK (IIF([AssigneeUserId] IS NOT NULL, 1, 0) + IIF([AssigneeCompanyId] IS NOT NULL, 1, 0) = 1)
)
GO

CREATE TABLE [EnquiryItemAttachment] (
  [EnquiryItemId] int NOT NULL
    FOREIGN KEY REFERENCES [EnquiryItem],
  [FileName] nvarchar(255) NOT NULL
    CHECK (LEN([FileName]) > 0),
  [Content] varbinary(max) NOT NULL
    CHECK (LEN([Content]) > 0),
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [CreatedDate] datetimeoffset NOT NULL,

  PRIMARY KEY ([EnquiryItemId], [FileName])
)
GO

CREATE TABLE [EnquiryItemEnquiredQuantity] (
  [EnquiryItemId] int NOT NULL
    FOREIGN KEY REFERENCES [EnquiryItem],
  [Quantity] decimal(9, 4) NOT NULL
    CHECK ([Quantity] = FLOOR([Quantity])),

  PRIMARY KEY ([EnquiryItemId], [Quantity])
)
GO

CREATE TABLE [EnquiryItemOwnerChange] (
  [EnquiryItemId] int NOT NULL
    FOREIGN KEY REFERENCES [EnquiryItem],
  [CreatedDate] datetimeoffset NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [OwnerUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],

  PRIMARY KEY ([EnquiryItemId], [CreatedDate])
)
GO

CREATE TABLE [EnquiryItemComment] (
  [EnquiryItemId] int NOT NULL
    FOREIGN KEY REFERENCES [EnquiryItem],
  [CreatedDate] datetimeoffset NOT NULL,
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [Comment] nvarchar(4000) NOT NULL,

  PRIMARY KEY ([EnquiryItemId], [CreatedDate])
)
GO

CREATE TABLE [EnquiryItemTransition] (
  [EnquiryItemId] int NOT NULL
    FOREIGN KEY REFERENCES [EnquiryItem],
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL
    CHECK ([TransitionType] IN ('Assign', 'Accept', 'Price', 'Reject', 'Approve', 'Bypass', 'Invalidate')),
  [AuthorUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],
  [Comment] nvarchar(4000) NOT NULL,

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  UNIQUE ([EnquiryItemId], [CreatedDate], [TransitionType]) -- enforce is-a relationship
)
GO

CREATE TABLE [EnquiryItemAssignTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Assign'
    CHECK ([TransitionType] = 'Assign'),
  [AssigneeUserId] int NULL
    FOREIGN KEY REFERENCES [User],
  [AssigneeCompanyId] nchar(1) NULL
    FOREIGN KEY REFERENCES [ManufacturingCompany],

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType]),
  CHECK (IIF([AssigneeUserId] IS NOT NULL, 1, 0) + IIF([AssigneeCompanyId] IS NOT NULL, 1, 0) = 1)
)
GO

CREATE TABLE [EnquiryItemAcceptTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Accept'
    CHECK ([TransitionType] = 'Accept'),

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO

CREATE TABLE [EnquiryItemPriceTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Price',
    CHECK ([TransitionType] = 'Price'),

  [ArticleAbbr] nvarchar(30) NULL
    CHECK (LEN([ArticleAbbr]) > 0),
  [DrawingId] nvarchar(30) NULL
    CHECK (LEN([DrawingId]) > 0),
  [Description] nvarchar(4000) NOT NULL,
  [Dimensions] nvarchar(30) NOT NULL,
  [MaterialId] nvarchar(30) NULL
    FOREIGN KEY REFERENCES [Material],

  [ValidUntilDate] date NOT NULL,
  [ApproverUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO

CREATE TABLE [EnquiryItemPriceCalculation] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [EurCzkFxRate] decimal(9, 4) NOT NULL,
  [FileName] nvarchar(255) NOT NULL
    CHECK (LEN([FileName]) > 0),
  [Content] varbinary(max) NOT NULL
    CHECK (LEN([Content]) > 0),

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate]) REFERENCES [EnquiryItemPriceTransition]
)
GO

CREATE TABLE [EnquiryItemPricedQuantity] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [Quantity] decimal(9, 4) NOT NULL
    CHECK ([Quantity] = FLOOR([Quantity])),
  [Price] money NOT NULL,
  [ManufacturingProfitRatio] decimal(9, 3) NOT NULL,

  PRIMARY KEY ([EnquiryItemId], [CreatedDate], [Quantity]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate]) REFERENCES [EnquiryItemPriceTransition]
)
GO

CREATE TABLE [EnquiryItemApproveTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Approve'
    CHECK ([TransitionType] = 'Approve'),
  [ApproverUserId] int NOT NULL
    FOREIGN KEY REFERENCES [User],

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO

CREATE TABLE [EnquiryItemRejectTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Reject'
    CHECK ([TransitionType] = 'Reject'),
  [Reason] nvarchar(30) NOT NULL,

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO

CREATE TABLE [EnquiryItemBypassTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Bypass'
    CHECK ([TransitionType] = 'Bypass'),

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO

CREATE TABLE [EnquiryItemInvalidateTransition] (
  [EnquiryItemId] int NOT NULL,
  [CreatedDate] datetimeoffset NOT NULL,
  [TransitionType] varchar(30) NOT NULL DEFAULT 'Invalidate',
    CHECK ([TransitionType] = 'Invalidate'),

  PRIMARY KEY ([EnquiryItemId], [CreatedDate]),
  FOREIGN KEY ([EnquiryItemId], [CreatedDate], [TransitionType]) REFERENCES [EnquiryItemTransition] ([EnquiryItemId], [CreatedDate], [TransitionType])
)
GO
