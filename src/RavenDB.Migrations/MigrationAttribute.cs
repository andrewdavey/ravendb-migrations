using System;

namespace RavenDB.Migrations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(int version)
        {
            Version = version;
        }

        public int Version { get; private set; }
    }
}
