using System.Data.Entity.Migrations;
using WebAPI.Database;

namespace WebAPI.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DatabaseContext context)
        {
            
        }
    }
}
