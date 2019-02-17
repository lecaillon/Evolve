# Evolve
<img align="right" width="173px" height="173px" src="https://raw.githubusercontent.com/lecaillon/Evolve/master/images/logo.png">

Database migration tool for .NET and .NET Core. Inspired by [Flyway](https://flywaydb.org/).

Evolve is an easy migration tool that uses plain old sql scripts. Its purpose is to automate your database changes, and help keep those changes synchronized through all your environments and development teams.
This makes it an ideal tool for continuous integration / delivery.

Over all Evolve embraces simplicity. Every time you build or run your project, it will automatically ensure that your database is up-to-date. Install it and forget it!

## Evolve v2
Evolve 2.0 is the first major rewrite version. It will help simplify the overall design by getting rid of the hard to maintain dynamic database driver loading. The benefits: a simpler code base, a simpler test infrastructure, more time to develop new features. **Evole** is now decoupled from its **MSBuild** part, is fully compatible with .NET Standard 2.0 and comes with a standalone **CLI**.

## Installation

| Package | Repository |
|---------|------------|
| Evolve | [![NuGet](https://buildstats.info/nuget/Evolve)](https://www.nuget.org/packages/Evolve) |
| Evolve MSBuild | [![NuGet](https://buildstats.info/nuget/Evolve.MSBuild.Windows.x64)](https://www.nuget.org/packages/Evolve.MSBuild.Windows.x64) |
| Evolve CLI | [Evolve Releases](https://github.com/lecaillon/Evolve/releases) |

## Continuous integration
| Build server    | Platform | Build status |
|-----------------|----------|--------------|
| AppVeyor        | Windows  | [![Build status](https://img.shields.io/appveyor/ci/lecaillon/Evolve.svg?label=appveyor&branch=master)](https://ci.appveyor.com/project/lecaillon/evolve) |
| Azure Pipelines | Linux    | [![Build Status](https://lecaillon.visualstudio.com/Evolve-CI/_apis/build/status/Evolve-CI?branchName=master)](https://lecaillon.visualstudio.com/Evolve-CI/_build/latest?definitionId=2&branchName=master) |

## Documentation
- [Concepts](https://evolve-db.netlify.com/concepts/)
- [Evolve](https://evolve-db.netlify.com/getting-started/)
- [Evolve MSBuild](https://evolve-db.netlify.com/msbuild/)
- [Evolve CLI](https://evolve-db.netlify.com/cli/)
- [Samples](https://evolve-db.netlify.com/samples/)

## Supported Databases
- [x] PostgreSQL
- [x] SQL Server
- [x] MySQL / MariaDB
- [x] SQLite
- [x] Cassandra

## Supported Frameworks
- [x] .NET 3.5+
- [x] .NET 4.6.1+
- [x] .NET Standard 2.0

## Feedback and issues
Feedback, improvements, ideas are welcomed.
Feel free to create new [issues](https://github.com/lecaillon/Evolve/issues) at the issues section and/or send emails to evolve-db@hotmail.com

## Credits
Again, many thanks to the [Flyway](https://flywaydb.org/) project. Great idea, big inspiration, a tool that I could not do without in .NET. 
