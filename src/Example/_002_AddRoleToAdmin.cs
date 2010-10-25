using Newtonsoft.Json.Linq;
using Raven.Client;
using Raven.Migrations;

namespace Example
{
    [Migration(2)]
    public class _002_AddRoleToAdmin : Migration
    {
        public override void Up(IDocumentSession session)
        {
            var doc = session.Advanced.DatabaseCommands.Get("users/1");
            doc.DataAsJson["Roles"] = new JArray("admin");
            session.Advanced.DatabaseCommands.Put("users/1", null, doc.DataAsJson, doc.Metadata);
        }

        public override void Down(IDocumentSession session)
        {
            var doc = session.Advanced.DatabaseCommands.Get("users/1");
            doc.DataAsJson.Remove("Roles");
            session.Advanced.DatabaseCommands.Put("users/1", null, doc.DataAsJson, doc.Metadata);
        }
    }
}
