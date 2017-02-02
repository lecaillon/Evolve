
/*******************************************************************************
   Chinook Database - Version 1.4
   Script: Chinook_Sqlite_AutoIncrementPKs.sql
   Description: Creates and populates the Chinook database.
   DB Server: Sqlite
   Author: Luis Rocha
   License: http://www.codeplex.com/ChinookDatabase/license
********************************************************************************/


/*******************************************************************************
   Drop Tables
********************************************************************************/
DROP TABLE IF EXISTS [Album];

DROP TABLE IF EXISTS [Artist];

DROP TABLE IF EXISTS [Customer];

DROP TABLE IF EXISTS [Employee];

DROP TABLE IF EXISTS [Genre];

DROP TABLE IF EXISTS [Invoice];

DROP TABLE IF EXISTS [InvoiceLine];

DROP TABLE IF EXISTS [MediaType];

DROP TABLE IF EXISTS [Playlist];

DROP TABLE IF EXISTS [PlaylistTrack];

DROP TABLE IF EXISTS [Track];


/*******************************************************************************
   Create Tables
********************************************************************************/
CREATE TABLE [Album]
(
    [AlbumId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Title] NVARCHAR(160)  NOT NULL,
    [ArtistId] INTEGER  NOT NULL,
    FOREIGN KEY ([ArtistId]) REFERENCES [Artist] ([ArtistId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE VIEW [V_Album]
AS
SELECT * FROM [Album];


CREATE TABLE [Artist]
(
    [ArtistId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] NVARCHAR(120)
);

CREATE TABLE [Customer]
(
    [CustomerId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [FirstName] NVARCHAR(40)  NOT NULL,
    [LastName] NVARCHAR(20)  NOT NULL,
    [Company] NVARCHAR(80),
    [Address] NVARCHAR(70),
    [City] NVARCHAR(40),
    [State] NVARCHAR(40),
    [Country] NVARCHAR(40),
    [PostalCode] NVARCHAR(10),
    [Phone] NVARCHAR(24),
    [Fax] NVARCHAR(24),
    [Email] NVARCHAR(60)  NOT NULL,
    [SupportRepId] INTEGER,
    FOREIGN KEY ([SupportRepId]) REFERENCES [Employee] ([EmployeeId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE [Employee]
(
    [EmployeeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [LastName] NVARCHAR(20)  NOT NULL,
    [FirstName] NVARCHAR(20)  NOT NULL,
    [Title] NVARCHAR(30),
    [ReportsTo] INTEGER,
    [BirthDate] DATETIME,
    [HireDate] DATETIME,
    [Address] NVARCHAR(70),
    [City] NVARCHAR(40),
    [State] NVARCHAR(40),
    [Country] NVARCHAR(40),
    [PostalCode] NVARCHAR(10),
    [Phone] NVARCHAR(24),
    [Fax] NVARCHAR(24),
    [Email] NVARCHAR(60),
    FOREIGN KEY ([ReportsTo]) REFERENCES [Employee] ([EmployeeId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE [Genre]
(
    [GenreId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] NVARCHAR(120)
);

CREATE TABLE [Invoice]
(
    [InvoiceId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [CustomerId] INTEGER  NOT NULL,
    [InvoiceDate] DATETIME  NOT NULL,
    [BillingAddress] NVARCHAR(70),
    [BillingCity] NVARCHAR(40),
    [BillingState] NVARCHAR(40),
    [BillingCountry] NVARCHAR(40),
    [BillingPostalCode] NVARCHAR(10),
    [Total] NUMERIC(10,2)  NOT NULL,
    FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([CustomerId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE [InvoiceLine]
(
    [InvoiceLineId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [InvoiceId] INTEGER  NOT NULL,
    [TrackId] INTEGER  NOT NULL,
    [UnitPrice] NUMERIC(10,2)  NOT NULL,
    [Quantity] INTEGER  NOT NULL,
    FOREIGN KEY ([InvoiceId]) REFERENCES [Invoice] ([InvoiceId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY ([TrackId]) REFERENCES [Track] ([TrackId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE [MediaType]
(
    [MediaTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] NVARCHAR(120)
);

CREATE TABLE [Playlist]
(
    [PlaylistId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] NVARCHAR(120)
);

CREATE TABLE [PlaylistTrack]
(
    [PlaylistId] INTEGER  NOT NULL,
    [TrackId] INTEGER  NOT NULL,
    CONSTRAINT [PK_PlaylistTrack] PRIMARY KEY  ([PlaylistId], [TrackId]),
    FOREIGN KEY ([PlaylistId]) REFERENCES [Playlist] ([PlaylistId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY ([TrackId]) REFERENCES [Track] ([TrackId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE [Track]
(
    [TrackId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] NVARCHAR(200)  NOT NULL,
    [AlbumId] INTEGER,
    [MediaTypeId] INTEGER  NOT NULL,
    [GenreId] INTEGER,
    [Composer] NVARCHAR(220),
    [Milliseconds] INTEGER  NOT NULL,
    [Bytes] INTEGER,
    [UnitPrice] NUMERIC(10,2)  NOT NULL,
    FOREIGN KEY ([AlbumId]) REFERENCES [Album] ([AlbumId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY ([GenreId]) REFERENCES [Genre] ([GenreId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY ([MediaTypeId]) REFERENCES [MediaType] ([MediaTypeId]) 
		ON DELETE NO ACTION ON UPDATE NO ACTION
);


/*******************************************************************************
   Create Foreign Keys
********************************************************************************/
CREATE INDEX [IFK_AlbumArtistId] ON [Album] ([ArtistId]);

CREATE INDEX [IFK_CustomerSupportRepId] ON [Customer] ([SupportRepId]);

CREATE INDEX [IFK_EmployeeReportsTo] ON [Employee] ([ReportsTo]);

CREATE INDEX [IFK_InvoiceCustomerId] ON [Invoice] ([CustomerId]);

CREATE INDEX [IFK_InvoiceLineInvoiceId] ON [InvoiceLine] ([InvoiceId]);

CREATE INDEX [IFK_InvoiceLineTrackId] ON [InvoiceLine] ([TrackId]);

CREATE INDEX [IFK_PlaylistTrackTrackId] ON [PlaylistTrack] ([TrackId]);

CREATE INDEX [IFK_TrackAlbumId] ON [Track] ([AlbumId]);

CREATE INDEX [IFK_TrackGenreId] ON [Track] ([GenreId]);

CREATE INDEX [IFK_TrackMediaTypeId] ON [Track] ([MediaTypeId]);


/*******************************************************************************
   Populate Tables
********************************************************************************/
INSERT INTO [Genre] ([Name]) VALUES ('Rock');
INSERT INTO [Genre] ([Name]) VALUES ('Jazz');
INSERT INTO [Genre] ([Name]) VALUES ('Metal');
INSERT INTO [Genre] ([Name]) VALUES ('Alternative & Punk');
INSERT INTO [Genre] ([Name]) VALUES ('Rock And Roll');
INSERT INTO [Genre] ([Name]) VALUES ('Blues');
INSERT INTO [Genre] ([Name]) VALUES ('Latin');
INSERT INTO [Genre] ([Name]) VALUES ('Reggae');
INSERT INTO [Genre] ([Name]) VALUES ('Pop');
INSERT INTO [Genre] ([Name]) VALUES ('Soundtrack');
INSERT INTO [Genre] ([Name]) VALUES ('Bossa Nova');
INSERT INTO [Genre] ([Name]) VALUES ('Easy Listening');
INSERT INTO [Genre] ([Name]) VALUES ('Heavy Metal');
INSERT INTO [Genre] ([Name]) VALUES ('R&B/Soul');
INSERT INTO [Genre] ([Name]) VALUES ('Electronica/Dance');
INSERT INTO [Genre] ([Name]) VALUES ('World');
INSERT INTO [Genre] ([Name]) VALUES ('Hip Hop/Rap');
INSERT INTO [Genre] ([Name]) VALUES ('Science Fiction');
INSERT INTO [Genre] ([Name]) VALUES ('TV Shows');
INSERT INTO [Genre] ([Name]) VALUES ('Sci Fi & Fantasy');
INSERT INTO [Genre] ([Name]) VALUES ('Drama');
INSERT INTO [Genre] ([Name]) VALUES ('Comedy');
INSERT INTO [Genre] ([Name]) VALUES ('Alternative');
INSERT INTO [Genre] ([Name]) VALUES ('Classical');
INSERT INTO [Genre] ([Name]) VALUES ('Opera');

INSERT INTO [MediaType] ([Name]) VALUES ('MPEG audio file');
INSERT INTO [MediaType] ([Name]) VALUES ('Protected AAC audio file');
INSERT INTO [MediaType] ([Name]) VALUES ('Protected MPEG-4 video file');
INSERT INTO [MediaType] ([Name]) VALUES ('Purchased AAC audio file');
INSERT INTO [MediaType] ([Name]) VALUES ('AAC audio file');

INSERT INTO [Artist] ([Name]) VALUES ('AC/DC');
INSERT INTO [Artist] ([Name]) VALUES ('Accept');
INSERT INTO [Artist] ([Name]) VALUES ('Aerosmith');
INSERT INTO [Artist] ([Name]) VALUES ('Alanis Morissette');
INSERT INTO [Artist] ([Name]) VALUES ('Alice In Chains');
INSERT INTO [Artist] ([Name]) VALUES ('Antônio Carlos Jobim');
INSERT INTO [Artist] ([Name]) VALUES ('Apocalyptica');
INSERT INTO [Artist] ([Name]) VALUES ('Audioslave');
INSERT INTO [Artist] ([Name]) VALUES ('BackBeat');

INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('For Those About To Rock We Salute You', 1);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Balls to the Wall', 2);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Restless and Wild', 2);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Let There Be Rock', 1);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Big Ones', 3);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Jagged Little Pill', 4);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Facelift', 5);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Warner 25 Anos', 6);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Plays Metallica By Four Cellos', 7);
INSERT INTO [Album] ([Title], [ArtistId]) VALUES ('Audioslave', 8);

INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('For Those About To Rock (We Salute You)', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 343719, 11170334, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Balls to the Wall', 2, 2, 1, 342562, 5510424, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Fast As a Shark', 3, 2, 1, 'F. Baltes, S. Kaufman, U. Dirkscneider & W. Hoffman', 230619, 3990994, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Restless and Wild', 3, 2, 1, 'F. Baltes, R.A. Smith-Diesel, S. Kaufman, U. Dirkscneider & W. Hoffman', 252051, 4331779, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Princess of the Dawn', 3, 2, 1, 'Deaffy & R.A. Smith-Diesel', 375418, 6290521, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Put The Finger On You', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 205662, 6713451, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Let''s Get It Up', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 233926, 7636561, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Inject The Venom', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 210834, 6852860, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Snowballed', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 203102, 6599424, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Evil Walks', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 263497, 8611245, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('C.O.D.', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 199836, 6566314, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Breaking The Rules', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 263288, 8596840, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Night Of The Long Knives', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 205688, 6706347, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Spellbound', 1, 1, 1, 'Angus Young, Malcolm Young, Brian Johnson', 270863, 8817038, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Go Down', 4, 1, 1, 'AC/DC', 331180, 10847611, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Dog Eat Dog', 4, 1, 1, 'AC/DC', 215196, 7032162, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Let There Be Rock', 4, 1, 1, 'AC/DC', 366654, 12021261, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Bad Boy Boogie', 4, 1, 1, 'AC/DC', 267728, 8776140, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Problem Child', 4, 1, 1, 'AC/DC', 325041, 10617116, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Overdose', 4, 1, 1, 'AC/DC', 369319, 12066294, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Hell Ain''t A Bad Place To Be', 4, 1, 1, 'AC/DC', 254380, 8331286, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Whole Lotta Rosie', 4, 1, 1, 'AC/DC', 323761, 10547154, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Walk On Water', 5, 1, 1, 'Steven Tyler, Joe Perry, Jack Blades, Tommy Shaw', 295680, 9719579, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Love In An Elevator', 5, 1, 1, 'Steven Tyler, Joe Perry', 321828, 10552051, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Rag Doll', 5, 1, 1, 'Steven Tyler, Joe Perry, Jim Vallance, Holly Knight', 264698, 8675345, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('What It Takes', 5, 1, 1, 'Steven Tyler, Joe Perry, Desmond Child', 310622, 10144730, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Dude (Looks Like A Lady)', 5, 1, 1, 'Steven Tyler, Joe Perry, Desmond Child', 264855, 8679940, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Janie''s Got A Gun', 5, 1, 1, 'Steven Tyler, Tom Hamilton', 330736, 10869391, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Cryin''', 5, 1, 1, 'Steven Tyler, Joe Perry, Taylor Rhodes', 309263, 10056995, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Amazing', 5, 1, 1, 'Steven Tyler, Richie Supa', 356519, 11616195, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Blind Man', 5, 1, 1, 'Steven Tyler, Joe Perry, Taylor Rhodes', 240718, 7877453, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Deuces Are Wild', 5, 1, 1, 'Steven Tyler, Jim Vallance', 215875, 7074167, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('The Other Side', 5, 1, 1, 'Steven Tyler, Jim Vallance', 244375, 7983270, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Crazy', 5, 1, 1, 'Steven Tyler, Joe Perry, Desmond Child', 316656, 10402398, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Eat The Rich', 5, 1, 1, 'Steven Tyler, Joe Perry, Jim Vallance', 251036, 8262039, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Angel', 5, 1, 1, 'Steven Tyler, Desmond Child', 307617, 9989331, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Livin'' On The Edge', 5, 1, 1, 'Steven Tyler, Joe Perry, Mark Hudson', 381231, 12374569, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('All I Really Want', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 284891, 9375567, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('You Oughta Know', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 249234, 8196916, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Perfect', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 188133, 6145404, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Hand In My Pocket', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 221570, 7224246, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Right Through You', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 176117, 5793082, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Forgiven', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 300355, 9753256, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('You Learn', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 239699, 7824837, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Head Over Feet', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 267493, 8758008, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Mary Jane', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 280607, 9163588, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Ironic', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 229825, 7598866, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Not The Doctor', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 227631, 7604601, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Wake Up', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 293485, 9703359, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('You Oughta Know (Alternate)', 6, 1, 1, 'Alanis Morissette & Glenn Ballard', 491885, 16008629, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('We Die Young', 7, 1, 1, 'Jerry Cantrell', 152084, 4925362, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Man In The Box', 7, 1, 1, 'Jerry Cantrell, Layne Staley', 286641, 9310272, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Sea Of Sorrow', 7, 1, 1, 'Jerry Cantrell', 349831, 11316328, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Bleed The Freak', 7, 1, 1, 'Jerry Cantrell', 241946, 7847716, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('I Can''t Remember', 7, 1, 1, 'Jerry Cantrell, Layne Staley', 222955, 7302550, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Love, Hate, Love', 7, 1, 1, 'Jerry Cantrell, Layne Staley', 387134, 12575396, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('It Ain''t Like That', 7, 1, 1, 'Jerry Cantrell, Michael Starr, Sean Kinney', 277577, 8993793, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Sunshine', 7, 1, 1, 'Jerry Cantrell', 284969, 9216057, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Put You Down', 7, 1, 1, 'Jerry Cantrell', 196231, 6420530, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Confusion', 7, 1, 1, 'Jerry Cantrell, Michael Starr, Layne Staley', 344163, 11183647, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('I Know Somethin (Bout You)', 7, 1, 1, 'Jerry Cantrell', 261955, 8497788, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Real Thing', 7, 1, 1, 'Jerry Cantrell, Layne Staley', 243879, 7937731, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Desafinado', 8, 1, 2, 185338, 5990473, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Garota De Ipanema', 8, 1, 2, 285048, 9348428, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Samba De Uma Nota Só (One Note Samba)', 8, 1, 2, 137273, 4535401, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Por Causa De Você', 8, 1, 2, 169900, 5536496, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Ligia', 8, 1, 2, 251977, 8226934, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Fotografia', 8, 1, 2, 129227, 4198774, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Dindi (Dindi)', 8, 1, 2, 253178, 8149148, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Se Todos Fossem Iguais A Você (Instrumental)', 8, 1, 2, 134948, 4393377, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Falando De Amor', 8, 1, 2, 219663, 7121735, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Angela', 8, 1, 2, 169508, 5574957, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Corcovado (Quiet Nights Of Quiet Stars)', 8, 1, 2, 205662, 6687994, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Outra Vez', 8, 1, 2, 126511, 4110053, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('O Boto (Bôto)', 8, 1, 2, 366837, 12089673, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Canta, Canta Mais', 8, 1, 2, 271856, 8719426, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Enter Sandman', 9, 1, 3, 'Apocalyptica', 221701, 7286305, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Master Of Puppets', 9, 1, 3, 'Apocalyptica', 436453, 14375310, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Harvester Of Sorrow', 9, 1, 3, 'Apocalyptica', 374543, 12372536, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('The Unforgiven', 9, 1, 3, 'Apocalyptica', 322925, 10422447, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Sad But True', 9, 1, 3, 'Apocalyptica', 288208, 9405526, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Creeping Death', 9, 1, 3, 'Apocalyptica', 308035, 10110980, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Wherever I May Roam', 9, 1, 3, 'Apocalyptica', 369345, 12033110, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Welcome Home (Sanitarium)', 9, 1, 3, 'Apocalyptica', 350197, 11406431, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Cochise', 10, 1, 1, 'Audioslave/Chris Cornell', 222380, 5339931, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Show Me How to Live', 10, 1, 1, 'Audioslave/Chris Cornell', 277890, 6672176, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Gasoline', 10, 1, 1, 'Audioslave/Chris Cornell', 279457, 6709793, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('What You Are', 10, 1, 1, 'Audioslave/Chris Cornell', 249391, 5988186, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Like a Stone', 10, 1, 1, 'Audioslave/Chris Cornell', 294034, 7059624, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Set It Off', 10, 1, 1, 'Audioslave/Chris Cornell', 263262, 6321091, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Shadow on the Sun', 10, 1, 1, 'Audioslave/Chris Cornell', 343457, 8245793, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('I am the Highway', 10, 1, 1, 'Audioslave/Chris Cornell', 334942, 8041411, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Exploder', 10, 1, 1, 'Audioslave/Chris Cornell', 206053, 4948095, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Hypnotize', 10, 1, 1, 'Audioslave/Chris Cornell', 206628, 4961887, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Bring''em Back Alive', 10, 1, 1, 'Audioslave/Chris Cornell', 329534, 7911634, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Light My Way', 10, 1, 1, 'Audioslave/Chris Cornell', 303595, 7289084, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('Getaway Car', 10, 1, 1, 'Audioslave/Chris Cornell', 299598, 7193162, 0.99);
INSERT INTO [Track] ([Name], [AlbumId], [MediaTypeId], [GenreId], [Composer], [Milliseconds], [Bytes], [UnitPrice]) VALUES ('The Last Remaining Light', 10, 1, 1, 'Audioslave/Chris Cornell', 317492, 7622615, 0.99);

INSERT INTO [Employee] ([LastName], [FirstName], [Title], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Adams', 'Andrew', 'General Manager', '1962-02-18 00:00:00', '2002-08-14 00:00:00', '11120 Jasper Ave NW', 'Edmonton', 'AB', 'Canada', 'T5K 2N1', '+1 (780) 428-9482', '+1 (780) 428-3457', 'andrew@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Edwards', 'Nancy', 'Sales Manager', 1, '1958-12-08 00:00:00', '2002-05-01 00:00:00', '825 8 Ave SW', 'Calgary', 'AB', 'Canada', 'T2P 2T3', '+1 (403) 262-3443', '+1 (403) 262-3322', 'nancy@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Peacock', 'Jane', 'Sales Support Agent', 2, '1973-08-29 00:00:00', '2002-04-01 00:00:00', '1111 6 Ave SW', 'Calgary', 'AB', 'Canada', 'T2P 5M5', '+1 (403) 262-3443', '+1 (403) 262-6712', 'jane@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Park', 'Margaret', 'Sales Support Agent', 2, '1947-09-19 00:00:00', '2003-05-03 00:00:00', '683 10 Street SW', 'Calgary', 'AB', 'Canada', 'T2P 5G3', '+1 (403) 263-4423', '+1 (403) 263-4289', 'margaret@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Johnson', 'Steve', 'Sales Support Agent', 2, '1965-03-03 00:00:00', '2003-10-17 00:00:00', '7727B 41 Ave', 'Calgary', 'AB', 'Canada', 'T3B 1Y7', '1 (780) 836-9987', '1 (780) 836-9543', 'steve@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Mitchell', 'Michael', 'IT Manager', 1, '1973-07-01 00:00:00', '2003-10-17 00:00:00', '5827 Bowness Road NW', 'Calgary', 'AB', 'Canada', 'T3B 0C5', '+1 (403) 246-9887', '+1 (403) 246-9899', 'michael@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('King', 'Robert', 'IT Staff', 6, '1970-05-29 00:00:00', '2004-01-02 00:00:00', '590 Columbia Boulevard West', 'Lethbridge', 'AB', 'Canada', 'T1K 5N8', '+1 (403) 456-9986', '+1 (403) 456-8485', 'robert@chinookcorp.com');
INSERT INTO [Employee] ([LastName], [FirstName], [Title], [ReportsTo], [BirthDate], [HireDate], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email]) VALUES ('Callahan', 'Laura', 'IT Staff', 6, '1968-01-09 00:00:00', '2004-03-04 00:00:00', '923 7 ST NW', 'Lethbridge', 'AB', 'Canada', 'T1H 1Y8', '+1 (403) 467-3351', '+1 (403) 467-8772', 'laura@chinookcorp.com');

INSERT INTO [Customer] ([FirstName], [LastName], [Company], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email], [SupportRepId]) VALUES ('Luís', 'Gonçalves', 'Embraer - Empresa Brasileira de Aeronáutica S.A.', 'Av. Brigadeiro Faria Lima, 2170', 'São José dos Campos', 'SP', 'Brazil', '12227-000', '+55 (12) 3923-5555', '+55 (12) 3923-5566', 'luisg@embraer.com.br', 3);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Leonie', 'Köhler', 'Theodor-Heuss-Straße 34', 'Stuttgart', 'Germany', '70174', '+49 0711 2842222', 'leonekohler@surfeu.de', 5);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [State], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('François', 'Tremblay', '1498 rue Bélanger', 'Montréal', 'QC', 'Canada', 'H2G 1A7', '+1 (514) 721-4711', 'ftremblay@gmail.com', 3);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Bjørn', 'Hansen', 'Ullevålsveien 14', 'Oslo', 'Norway', '0171', '+47 22 44 22 22', 'bjorn.hansen@yahoo.no', 4);
INSERT INTO [Customer] ([FirstName], [LastName], [Company], [Address], [City], [Country], [PostalCode], [Phone], [Fax], [Email], [SupportRepId]) VALUES ('František', 'Wichterlová', 'JetBrains s.r.o.', 'Klanova 9/506', 'Prague', 'Czech Republic', '14700', '+420 2 4172 5555', '+420 2 4172 5555', 'frantisekw@jetbrains.com', 4);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Helena', 'Holý', 'Rilská 3174/6', 'Prague', 'Czech Republic', '14300', '+420 2 4177 0449', 'hholy@gmail.com', 5);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Astrid', 'Gruber', 'Rotenturmstraße 4, 1010 Innere Stadt', 'Vienne', 'Austria', '1010', '+43 01 5134505', 'astrid.gruber@apple.at', 5);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Daan', 'Peeters', 'Grétrystraat 63', 'Brussels', 'Belgium', '1000', '+32 02 219 03 03', 'daan_peeters@apple.be', 4);
INSERT INTO [Customer] ([FirstName], [LastName], [Address], [City], [Country], [PostalCode], [Phone], [Email], [SupportRepId]) VALUES ('Kara', 'Nielsen', 'Sønder Boulevard 51', 'Copenhagen', 'Denmark', '1720', '+453 3331 9991', 'kara.nielsen@jubii.dk', 4);
INSERT INTO [Customer] ([FirstName], [LastName], [Company], [Address], [City], [State], [Country], [PostalCode], [Phone], [Fax], [Email], [SupportRepId]) VALUES ('Eduardo', 'Martins', 'Woodstock Discos', 'Rua Dr. Falcão Filho, 155', 'São Paulo', 'SP', 'Brazil', '01007-010', '+55 (11) 3033-5446', '+55 (11) 3033-4564', 'eduardo@woodstock.com.br', 4);

INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (2, '2009-01-01 00:00:00', 'Theodor-Heuss-Straße 34', 'Stuttgart', 'Germany', '70174', 1.98);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (4, '2009-01-02 00:00:00', 'Ullevålsveien 14', 'Oslo', 'Norway', '0171', 3.96);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (8, '2009-01-03 00:00:00', 'Grétrystraat 63', 'Brussels', 'Belgium', '1000', 5.94);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (2, '2009-02-11 00:00:00', 'Theodor-Heuss-Straße 34', 'Stuttgart', 'Germany', '70174', 13.86);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (4, '2009-04-06 00:00:00', 'Ullevålsveien 14', 'Oslo', 'Norway', '0171', 5.94);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingState], [BillingCountry], [BillingPostalCode], [Total]) VALUES (10, '2009-04-09 00:00:00', 'Rua Dr. Falcão Filho, 155', 'São Paulo', 'SP', 'Brazil', '01007-010', 8.91);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (6, '2009-07-11 00:00:00', 'Rilská 3174/6', 'Prague', 'Czech Republic', '14300', 8.91);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (8, '2009-08-24 00:00:00', 'Grétrystraat 63', 'Brussels', 'Belgium', '1000', 0.99);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (9, '2009-09-06 00:00:00', 'Sønder Boulevard 51', 'Copenhagen', 'Denmark', '1720', 1.98);
INSERT INTO [Invoice] ([CustomerId], [InvoiceDate], [BillingAddress], [BillingCity], [BillingCountry], [BillingPostalCode], [Total]) VALUES (2, '2009-10-12 00:00:00', 'Theodor-Heuss-Straße 34', 'Stuttgart', 'Germany', '70174', 8.91);

INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (1, 2, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (1, 4, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (2, 6, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (2, 8, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (2, 10, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (2, 12, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 16, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 20, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 24, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 28, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 32, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (3, 36, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 42, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 48, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 54, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 60, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 66, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 72, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 78, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 84, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (4, 90, 0.99, 1);
INSERT INTO [InvoiceLine] ([InvoiceId], [TrackId], [UnitPrice], [Quantity]) VALUES (5, 99, 0.99, 1);

INSERT INTO [Playlist] ([Name]) VALUES ('Music');
INSERT INTO [Playlist] ([Name]) VALUES ('Movies');
INSERT INTO [Playlist] ([Name]) VALUES ('TV Shows');
INSERT INTO [Playlist] ([Name]) VALUES ('Audiobooks');
INSERT INTO [Playlist] ([Name]) VALUES ('90’s Music');
INSERT INTO [Playlist] ([Name]) VALUES ('Audiobooks');
INSERT INTO [Playlist] ([Name]) VALUES ('Movies');
INSERT INTO [Playlist] ([Name]) VALUES ('Music');
INSERT INTO [Playlist] ([Name]) VALUES ('Music Videos');
INSERT INTO [Playlist] ([Name]) VALUES ('TV Shows');
INSERT INTO [Playlist] ([Name]) VALUES ('Brazilian Music');
INSERT INTO [Playlist] ([Name]) VALUES ('Classical');
INSERT INTO [Playlist] ([Name]) VALUES ('Classical 101 - Deep Cuts');
INSERT INTO [Playlist] ([Name]) VALUES ('Classical 101 - Next Steps');
INSERT INTO [Playlist] ([Name]) VALUES ('Classical 101 - The Basics');
INSERT INTO [Playlist] ([Name]) VALUES ('Grunge');
INSERT INTO [Playlist] ([Name]) VALUES ('Heavy Metal Classic');
INSERT INTO [Playlist] ([Name]) VALUES ('On-The-Go 1');

INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 99);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 63);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 64);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 65);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 66);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 67);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 68);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 69);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 70);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 71);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 72);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 73);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 74);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 75);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (1, 76);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 23);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 24);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 25);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 26);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 27);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 28);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 29);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 30);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 31);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 32);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 33);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 34);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 35);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 36);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (5, 37);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (16, 52);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (17, 1);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (17, 2);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (17, 3);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (17, 4);
INSERT INTO [PlaylistTrack] ([PlaylistId], [TrackId]) VALUES (17, 5);
