# Evolve [![Build status](https://ci.appveyor.com/api/projects/status/oj9wf4bk0p0npggu?svg=true)](https://ci.appveyor.com/project/lecaillon/evolve)
Database migration tool for .NET. Inspired by [Flyway](https://flywaydb.org/).

Evolve is an easy migration tool that uses plain old sql scripts. Its purpose is to automate your database changes, and help keep those changes synchronized through all your environments and developpement teams.
This makes it an ideal tool for continuous integration / delivery.

Over all Evolve embraces simplicity. Every time you build your project, it will automatically ensure that your database is up-to-date, without having to do anything other than build. Install it and forget it !

## Supported Databases
- [x] PostgreSQL
- [x] SQLite
- [x] SQL Server
- [ ] MySQL / MariaDB
- [ ] Oracle support (for .NET Framework only)

## Supported Modes
- [x] MSBuild
- [x] In-app
- [ ] Evolve CLI tool

## Supported Frameworks
- [x] .NET 3.5+
- [x] .NET 4.5+
- [ ] .NET Core (more generally .NET Standard 1.3)

## Installation
```
PM> Install-Package Evolve
```

## Quick Start
1. Add a reference to the NuGet Evolve package in your project.
2. Add at least those variables to your Web.config/App.config and update their values according to your environment:
```xml
<appSettings>
  <add key="Evolve.ConnectionString" value="Server=127.0.0.1;Port=5432;Database=my_db;User Id=postgres;Password=postgres;" />
  <add key="Evolve.Driver" value="npgsql" /> <!-- or sqlserver or microsoftdatasqlite or sqlite or mysql or mariadb -->
  <add key="Evolve.Locations" value="Sql_Scripts" />
  <add key="Evolve.Command" value="migrate" />
</appSettings>
```
3. Create a folder *Sql_Scripts* at the root of your project directory.
4. And add your sql migration scripts following this file name structure. Example: *V1_3_1__Create_table.sql*:
- **prefix**: configurable, default: **V**
- **version**: numbers separated by _ (one underscore)
- **separator**: configurable, default: **__** (two underscores)
- **description**: words separated by underscores
- **suffix**: configurable, default: **.sql** 
5. Next time you build your project, Evolve will start and run your migration scripts automatically.

Please refer to the [wiki](https://github.com/lecaillon/Evolve/wiki) pages for a complete [documentation](https://github.com/lecaillon/Evolve/wiki) on how to master Evolve.

## Feedback and issues
Feedback, improvements, ideas are welcomed.
Feel free to create new [issues](https://github.com/lecaillon/Evolve/issues) at the issues section and/or send emails to evolve-db@hotmail.com

## Credits
Again, many thanks to the [Flyway](https://flywaydb.org/) project. Great idea, big inspiration, a tool that I could not do without in .NET.
