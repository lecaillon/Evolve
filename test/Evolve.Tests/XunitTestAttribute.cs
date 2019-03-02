using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Evolve.Tests
{
    public enum Test
    {
        Cassandra,
        MySQL,
        PostgreSQL,
        SQLite,
        SQLServer,

        Cli,
        Configuration,
        Connection,
        Migration,
        Metadata
    }

    [TraitDiscoverer("Evolve.Tests.CategoryDiscover", "Evolve.Tests")]
    [AttributeUsage(AttributeTargets.Method)]
    public class CategoryAttribute : Attribute, ITraitAttribute
    {
        public CategoryAttribute(params Test[] categories)
        {
            Categories = categories;
        }

        public Test[] Categories { get; }
    }

    public class CategoryDiscover : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var categories = traitAttribute.GetNamedArgument<Test[]>("Categories");
            foreach (var category in categories)
            {
                yield return new KeyValuePair<string, string>("Category", category.ToString());
            }
        }
    }
}
