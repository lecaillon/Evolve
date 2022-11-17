using EvolveDb.Migration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EvolveDb.Utilities
{

    internal static class SortMigrationHelper
    {
        internal static IEnumerable<MigrationScript> SortWithDependencies(this IEnumerable<MigrationScript> source)
        {
            List<MigrationScript> sorted = new();
            List<string> stack = new();
            Dictionary<string, bool> visited = new();
            Dictionary<string, MigrationScript> cache = source.ToDictionary(
                item => item.Description.Replace(" ", "_"),
                item => item
            );
            foreach (var item in source)
            {
                Visit(
                    item,
                    sorted,
                    stack,
                    visited,
                    cache
                );
            }
            return sorted;
        }
        internal static void Visit(
            MigrationScript item,
            List<MigrationScript> sorted,
            List<string> stack,
            Dictionary<string, bool> visited,
            Dictionary<string, MigrationScript> cache
        )
        {
            var name = item.Description;
            try
            {
                stack.Add(name);
                if (visited.TryGetValue(name, out bool visiting))
                {
                    if (visiting)
                    {
                        throw new EvolveException($"Circular dependency detected: {string.Join(" -> ", stack)}");
                    }
                    return;
                }
                visited[name] = true;
                foreach (var dep in item.RepeatableDependencies)
                {
                    Visit(
                        cache[dep],
                        sorted,
                        stack,
                        visited,
                        cache
                    );
                }
                visited[name] = false;
                sorted.Add(item);
            }
            finally
            {
                if (stack[stack.Count - 1] != name)
                {
                    throw new Exception("should never happen");
                }
                stack.RemoveAt(stack.Count - 1);
            }
        }
    }
}
