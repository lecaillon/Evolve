# Evolve [![Build status](https://img.shields.io/appveyor/ci/lecaillon/Evolve.svg?label=appveyor&branch=master)](https://ci.appveyor.com/project/lecaillon/evolve) [![Build Status](https://img.shields.io/travis/lecaillon/Evolve.svg?label=travis-ci&branch=master)](https://travis-ci.org/lecaillon/Evolve) [![NuGet](https://buildstats.info/nuget/Evolve)](https://www.nuget.org/packages/Evolve)
Database migration tool for .NET and .NET Core. Inspired by [Flyway](https://flywaydb.org/).

Evolve is an easy migration tool that uses plain old sql scripts. Its purpose is to automate your database changes, and help keep those changes synchronized through all your environments and development teams.
This makes it an ideal tool for continuous integration / delivery.

Over all Evolve embraces simplicity. Every time you build or run your project, it will automatically ensure that your database is up-to-date. Install it and forget it!

## Documentation
- [Getting started](https://evolve-db.netlify.com/getting-started/)
- [Concepts](https://evolve-db.netlify.com/concepts/)
- [Samples](https://evolve-db.netlify.com/samples/)

## Supported Databases
- [x] PostgreSQL
- [x] SQL Server
- [x] MySQL / MariaDB
- [x] SQLite
- [x] Cassandra

## Supported Frameworks
- [x] .NET 3.5+
- [x] .NET 4.5.2+
- [x] .NET Core 1.0+
- [x] .NET Core 2.0+
- [x] .NET Core 2.1+

## Feedback and issues
Feedback, improvements, ideas are welcomed.
Feel free to create new [issues](https://github.com/lecaillon/Evolve/issues) at the issues section and/or send emails to evolve-db@hotmail.com

## Credits
Again, many thanks to the [Flyway](https://flywaydb.org/) project. Great idea, big inspiration, a tool that I could not do without in .NET. 
