# Evolve [![Build status](https://ci.appveyor.com/api/projects/status/oj9wf4bk0p0npggu?svg=true)](https://ci.appveyor.com/project/lecaillon/evolve)
Database migration tool for .NET. Inspired by Flyway.

Its purpose is to automate your database changes, and help keep those changes synchronized in all your environments.

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
- [ ] .NET Core project support (more generally .NET Standard 1.3 support)

## Getting Started
1. Add a reference to the nuget Evolve package in your project.

1. Add at least those variables to your Web.config/App.config and update their values according to your environment:

```xml
<appSettings>
  <add key="Evolve.ConnectionString" value="Server=127.0.0.1;Port=5432;Database=my_db;User Id=postgres;Password=postgres;" />
  <add key="Evolve.Driver" value="npgsql" />
  <add key="Evolve.Locations" value="Sql_Scripts" />
  <add key="Evolve.Command" value="migrate" />
</appSettings>
```
