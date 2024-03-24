using System;
using FluentMigrator;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Sample.Migrations
{
    [Migration(1, description:"Creates main working schema")]
    public class CreateSchema:Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Create.Schema("for-test");
        }

        /// <inheritdoc />
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
