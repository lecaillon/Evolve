# Evolve [![Build status](https://ci.appveyor.com/api/projects/status/oj9wf4bk0p0npggu/branch/master?svg=true)](https://ci.appveyor.com/project/lecaillon/evolve) [![Build Status](https://lecaillon.visualstudio.com/Evolve-CI/_apis/build/status/Evolve-CI?branchName=master)](https://lecaillon.visualstudio.com/Evolve-CI/_build/latest?definitionId=2&branchName=master) [![Build status](https://img.shields.io/azure-devops/coverage/lecaillon/evolve-ci/2.svg?color=brightgreen)](https://lecaillon.visualstudio.com/Evolve-CI/_build/latest?definitionId=2&branchName=master) [![Donate](https://img.shields.io/badge/Support%20this%20project-yellow.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=US3ZY9FJVKMW4&source=url)
<img align="right" width="173px" height="173px" src="https://raw.githubusercontent.com/lecaillon/Evolve/master/images/logo.png">

Database migration tool for .NET and .NET Core. Inspired by [Flyway](https://flywaydb.org/).

Evolve is an easy migration tool that uses plain SQL scripts. Its purpose is to automate your database changes, and help keep those changes synchronized through all your environments and development teams.
This makes it an ideal tool for continuous integration / delivery.

Over all Evolve embraces simplicity. Every time you build or run your project, it will automatically ensure that your database is up-to-date. Install it and forget it!

## Installation

| Package | Repository |
|---------|------------|
| Evolve | [![NuGet](https://buildstats.info/nuget/Evolve)](https://www.nuget.org/packages/Evolve) |
| Evolve MSBuild | [![NuGet](https://buildstats.info/nuget/Evolve.MSBuild.Windows.x64)](https://www.nuget.org/packages/Evolve.MSBuild.Windows.x64) |
| Evolve CLI | [![NuGet](https://img.shields.io/badge/GitHub-releases-brightgreen.svg?logo=github)](https://github.com/lecaillon/Evolve/releases) |

## Documentation
You can read the latest documentation at [https://evolve-db.netlify.com](https://evolve-db.netlify.com).

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
