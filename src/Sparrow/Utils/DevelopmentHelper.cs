﻿using System.Diagnostics;

namespace Sparrow.Utils;

internal static class DevelopmentHelper
{
    [Conditional("DEBUG")]
    internal static void ToDo(Feature feature, TeamMember member, Severity severity, string message)
    {
        // nothing to do here
    }

    [Conditional("DEBUG")]
    internal static void ShardingToDo(TeamMember member, Severity severity, string message) => ToDo(Feature.Sharding, member, severity, message);

    internal enum Feature
    {
        Sharding,
        Cluster
    }

    internal enum TeamMember
    {
        Karmel,
        Aviv,
        Pawel,
        Arek,
        Grisha,
        Efrat,
        Marcin,
        Egor,
        Stav,
        Shiran
    }

    internal enum Severity
    {
        Minor,
        Normal,
        Major,
        Critical
    }
}
