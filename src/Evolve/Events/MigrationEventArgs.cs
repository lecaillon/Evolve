using System;
using EvolveDb.Migration;

namespace EvolveDb.Events;

public sealed class MigrationEventArgs : EventArgs
{
    public MigrationScript? Migration { get; }
    

    public MigrationEventArgs(MigrationScript migration)
    {
        Migration = migration;
    }
}