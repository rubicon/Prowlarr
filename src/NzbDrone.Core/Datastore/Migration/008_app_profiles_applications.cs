using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Applications;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class app_profiles_applications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("AppSyncProfiles").AddColumn("ApplicationIds").AsString().WithDefaultValue("[]");

            Execute.WithConnection(AddApplications);
        }

        private void AddApplications(IDbConnection conn, IDbTransaction tran)
        {
            var appIdsQuery = conn.Query<int>("SELECT Id FROM Applications").ToList();
            var updateSql = "UPDATE AppSyncProfiles SET ApplicationIds = @Id";
            conn.Execute(updateSql, appIdsQuery, transaction: tran);
        }
    }
}
