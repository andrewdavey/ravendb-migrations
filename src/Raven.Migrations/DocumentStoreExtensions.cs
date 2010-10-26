using System;
using System.Net;
using System.Text.RegularExpressions;
using Raven.Client.Document;

namespace Raven.Migrations
{
    static class DocumentStoreExtensions
    {
        public static void ConfigureUsingConnectionString(this DocumentStore store, string connectionString)
        {
            // Copied (and tweaked a bit) from RavenDB source: DocumentStore.cs
            string user = null;
            string pass = null;
            var strings = connectionStringArgumentsSplitterRegex.Split(connectionString);
            foreach (var arg in strings)
            {
                var match = connectionStringRegex.Match(arg);
                if (match.Success == false)
                    throw new ArgumentException("Connection string could not be parsed");
                switch (match.Groups[1].Value.ToLower())
                {
                    case "memory":
                        bool result;
                        if (bool.TryParse(match.Groups[2].Value, out result) == false)
                            throw new ArgumentException("Could not understand memory setting: " +
                                match.Groups[2].Value);
                        store.RunInMemory = result;
                        break;
                    case "datadir":
                        store.DataDirectory = match.Groups[2].Value.Trim();
                        break;
                    case "resourcemanagerid":
                        store.ResourceManagerId = new Guid(match.Groups[2].Value.Trim());
                        break;
                    case "url":
                        store.Url = match.Groups[2].Value.Trim();
                        break;

                    case "user":
                        user = match.Groups[2].Value.Trim();
                        break;
                    case "password":
                        pass = match.Groups[2].Value.Trim();
                        break;

                    default:
                        throw new ArgumentException("Connection string could not be parsed, unknown option: " + match.Groups[1].Value);
                }
            }

            if (user == null && pass == null)
                return;

            if (user == null || pass == null)
                throw new ArgumentException("User and Password must both be specified in the connection string");
            store.Credentials = new NetworkCredential(user, pass);
        }

        // Copied from RavenDB source: DocumentStore.cs
        static readonly Regex connectionStringRegex = new Regex(@"(\w+) \s* = \s* (.*)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        static readonly Regex connectionStringArgumentsSplitterRegex = new Regex(@"; (?=\s* \w+ \s* =)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    }
}
