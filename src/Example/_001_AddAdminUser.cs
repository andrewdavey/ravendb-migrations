using Raven.Client;
using Raven.Migrations;

namespace Example
{
    [Migration(1)]
    public class _001_AddAdminUser : Migration
    {
        public override void Up(IDocumentSession session)
        {
            session.Store(new { Id = "users/1", Name = "Admin" });
            session.SaveChanges();
        }

        public override void Down(IDocumentSession session)
        {
            session.Advanced.DatabaseCommands.Delete("users/1", null);
        }
    }

}
