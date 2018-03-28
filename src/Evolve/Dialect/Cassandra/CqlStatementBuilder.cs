﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Evolve.Dialect.Cassandra
{
    //Limitation: a statement must end with a ";" that is the last character on the line
    // this means that a line cannot end with a ";" that is inside a comment or a multi-line literal
    public sealed class CqlStatementBuilder : SqlStatementBuilderBase
    {
        private const string StatementTerminationCharacter = ";";

        public override string BatchDelimiter => null;

        protected override IEnumerable<SqlStatement> Parse(string sqlScript)
        {
            int lineNumber = 0;
            int currentStatementLineStart = 0;
            var sb = new StringBuilder();
            foreach (var line in getLines(sqlScript))
            {
                sb.Append(line + Environment.NewLine);

                if (line.TrimEnd(' ').EndsWith(StatementTerminationCharacter))
                {
                    yield return new SqlStatement(sb.ToString(), false, currentStatementLineStart);
                    currentStatementLineStart = lineNumber + 1;
                    sb = new StringBuilder();
                }

                lineNumber++;
            }

            IEnumerable<string> getLines(string s)
            {
                using (var sr = new StringReader(s))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        yield return line;
                }
            }
        }
    }
}
