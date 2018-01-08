using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Evolve.Migration;
using Xunit;

namespace Evolve.Test.Migration
{
    [Collection("File migrations")]
    public class FileAndResourceIntegrationTest
    {
        [Fact]
        public void TestSameMigrationMatches()
        {
            IList<IMigrationScript> scripts = new FileMigrationLoader().GetMigrations(new List<string> { "Resources/scripts." }, "V", "__", ".sql").ToList();
            IList<IMigrationScript> embedded = new EmbeddedResourceMigrationLoader(GetType().Assembly).GetMigrations(new List<string> { "Evolve.Test.Resources.scripts." }, "V","__", ".sql").ToList();

            Assert.Equal(2,scripts.Count);
            Assert.Equal(2,embedded.Count);
            Assert.True(scripts.Zip(embedded, Tuple.Create).All(AreEqual));
        }
        
        [Fact]
        public void TestSameMigrationMatchesWithNormalization()
        {
            IList<IMigrationScript> scripts = new FileMigrationLoader(normalizeLineEndingsForChecksum:true).GetMigrations(new List<string> { "Resources/scripts." }, "V", "__", ".sql").ToList();
            IList<IMigrationScript> embedded = new EmbeddedResourceMigrationLoader(GetType().Assembly,normalizeLineEndingsForChecksum:true).GetMigrations(new List<string> { "Evolve.Test.Resources.scripts." }, "V","__", ".sql").ToList();

            Assert.Equal(2,scripts.Count);
            Assert.Equal(2,embedded.Count);
            Assert.True(scripts.Zip(embedded, Tuple.Create).All(AreEqual));
        }

        private static bool AreEqual(Tuple<IMigrationScript,IMigrationScript> t)
        {
            Assert.Equal(t.Item1.Version, t.Item2.Version);
            Assert.Equal(t.Item1.CheckSum, t.Item2.CheckSum);
            Assert.Equal(t.Item1.Name, t.Item2.Name);
            Assert.Equal(t.Item1.Description, t.Item2.Description);
            return true;
        }

        [Fact]
        public void TestExistingChecksumsValid()
        {
            IList<IMigrationScript> scripts = new FileMigrationLoader().GetMigrations(new List<string> { "Resources/scripts." }, "V", "__", ".sql").ToList();
            foreach (var migrationScript in scripts)
            {
                var script = (FileMigrationScript) migrationScript;
                Assert.Equal(CalculateChecksum(script.Path),script.CheckSum);
            }
        }
        
        
        /// <summary>
        /// Existing file/stream based checksum calculator
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string CalculateChecksum(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }
    }
}