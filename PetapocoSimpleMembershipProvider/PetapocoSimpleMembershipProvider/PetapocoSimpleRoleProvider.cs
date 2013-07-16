// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using PetapocoSimpleMembershipProvider.Resources;
using PetapocoSimpleMembershipProvider.DAL;
using System.Collections.Specialized;

namespace PetapocoSimpleMembershipProvider
{
    public class PetapocoSimpleRoleProvider : RoleProvider
    {
        internal DatabaseWrapper _database { get; set; }

        public PetapocoSimpleRoleProvider()
        {
        }

        #region Initialize
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (String.IsNullOrEmpty(name))
            {
                name = "PetapocoSimpleRoleProvider";
            }
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Petapoco Simple Role Provider");
            }

            base.Initialize(name, config);

            ApplicationName = GetValueOrDefault(config, "applicationName", o => o.ToString(), "MySampleApp");

            //取得连接字符串
            this.ConnectionStringName = GetValueOrDefault(config, "connectionStringName", o => o.ToString(), string.Empty);

            //取得数据库的providerName
            //this.ProviderName = GetValueOrDefault(config, "providerName", o => o.ToString(), string.Empty);

            config.Remove("name");
            config.Remove("description");
            config.Remove("applicationName");
            config.Remove("connectionStringName");
            //config.Remove("providerName");

            if (config.Count <= 0)
                return;
            var key = config.GetKey(0);
            if (string.IsNullOrEmpty(key))
                return;

            throw new ProviderException(string.Format(CultureInfo.CurrentCulture,
                                                      "The role provider does not recognize the configuration attribute {0}.",
                                                      key));
        }

        private string ConnectionStringName { get; set; }
        private string ProviderName { get; set; }
        #endregion


        private string SafeUserTableName
        {
            get { return "[" + UserTableName + "]"; }
        }

        private string SafeUserNameColumn
        {
            get { return "[" + UserNameColumn + "]"; }
        }

        private string SafeUserIdColumn
        {
            get { return "[" + UserIdColumn + "]"; }
        }

        internal static string RoleTableName
        {
            get { return "webpages_Roles"; }
        }

        internal static string UsersInRoleTableName
        {
            get { return "webpages_UsersInRoles"; }
        }

        // represents the User table for the app
        public string UserTableName 
        { 
            get {return "UserProfile";}
            set { UserTableName = value; } 
        }

        // represents the User created UserName column, i.e. Email
        public string UserNameColumn 
        {
            get { return "UserName"; }
            set { UserNameColumn = value; } 
        }

        // Represents the User created id column, i.e. ID;
        // REVIEW: we could get this from the primary key of UserTable in the future
        public string UserIdColumn
        {
            get { return "UserId"; }
            set { UserIdColumn = value; }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ApplicationName { get; set; }
        
        private DatabaseWrapper ConnectToDatabase()
        {
            if (_database == null)
            {
                _database = new DatabaseWrapper(this.ConnectionStringName);
            }
            return _database;
        }

        internal void CreateTablesIfNeeded()
        {
            using (var db = ConnectToDatabase())
            {
                if (!PetapocoSimpleMembershipProvider.CheckTableExists(db, RoleTableName))
                {
                    db.Execute(@"CREATE TABLE " + RoleTableName + @" (
                        RoleId                                  int                 NOT NULL PRIMARY KEY IDENTITY,
                        RoleName                                nvarchar(256)       NOT NULL UNIQUE)");

                    db.Execute(@"CREATE TABLE " + UsersInRoleTableName + @" (
                        UserId                                  int                 NOT NULL,
                        RoleId                                  int                 NOT NULL,
                        PRIMARY KEY (UserId, RoleId),
                        CONSTRAINT fk_UserId FOREIGN KEY (UserId) REFERENCES " + SafeUserTableName + "(" + SafeUserIdColumn + @"),
                        CONSTRAINT fk_RoleId FOREIGN KEY (RoleId) REFERENCES " + RoleTableName + "(RoleId) )");
                }
            }
        }

        private List<int> GetUserIdsFromNames(DatabaseWrapper db, string[] usernames)
        {
            List<int> userIds = new List<int>(usernames.Length);
            foreach (string username in usernames)
            {
                int id = PetapocoSimpleMembershipProvider.GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (id == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }
                userIds.Add(id);
            }
            return userIds;
        }

        private static List<int> GetRoleIdsFromNames(DatabaseWrapper db, string[] roleNames)
        {
            List<int> roleIds = new List<int>(roleNames.Length);
            foreach (string role in roleNames)
            {
                int id = FindRoleId(db, role);
                if (id == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvider_NoRoleFound, role));
                }
                roleIds.Add(id);
            }
            return roleIds;
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (var db = ConnectToDatabase())
            {
                int userCount = usernames.Length;
                int roleCount = roleNames.Length;
                List<int> userIds = GetUserIdsFromNames(db, usernames);
                List<int> roleIds = GetRoleIdsFromNames(db, roleNames);

                // Generate a INSERT INTO for each userid/rowid combination, where userIds are the first params, and roleIds follow
                for (int uId = 0; uId < userCount; uId++)
                {
                    for (int rId = 0; rId < roleCount; rId++)
                    {
                        if (IsUserInRole(usernames[uId], roleNames[rId]))
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvder_UserAlreadyInRole, usernames[uId], roleNames[rId]));
                        }

                        // REVIEW: is there a way to batch up these inserts?
                        int rows = db.Execute("INSERT INTO " + UsersInRoleTableName + " VALUES (" + userIds[uId] + "," + roleIds[rId] + "); ");
                        if (rows != 1)
                        {
                            throw new ProviderException(WebDataResources.Security_DbFailure);
                        }
                    }
                }
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void CreateRole(string roleName)
        {
            using (var db = ConnectToDatabase())
            {
                int roleId = FindRoleId(db, roleName);
                if (roleId != -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.SimpleRoleProvider_RoleExists, roleName));
                }

                int rows = db.Execute("INSERT INTO " + RoleTableName + " (RoleName) VALUES (@0)", roleName);
                if (rows != 1)
                {
                    throw new ProviderException(WebDataResources.Security_DbFailure);
                }
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (var db = ConnectToDatabase())
            {
                int roleId = FindRoleId(db, roleName);
                if (roleId == -1)
                {
                    return false;
                }

                if (throwOnPopulatedRole)
                {
                    int usersInRole = db.Query(@"SELECT * FROM " + UsersInRoleTableName + " WHERE (RoleId = @0)", roleId).Count();
                    if (usersInRole > 0)
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.SimpleRoleProvder_RolePopulated, roleName));
                    }
                }
                else
                {
                    // Delete any users in this role first
                    db.Execute(@"DELETE FROM " + UsersInRoleTableName + " WHERE (RoleId = @0)", roleId);
                }

                int rows = db.Execute(@"DELETE FROM " + RoleTableName + " WHERE (RoleId = @0)", roleId);
                return (rows == 1); // REVIEW: should this ever be > 1?
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var db = ConnectToDatabase())
            {
                // REVIEW: Is there any way to directly get out a string[]?
                IEnumerable<dynamic> userNames = db.Query(@"SELECT u." + SafeUserNameColumn + " FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (r.RoleName = @0 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + " and u." + SafeUserNameColumn + " LIKE @1)", new object[] { roleName, usernameToMatch }).ToList();
                string[] users = new string[userNames.Count()];
                int i = 0;
                foreach (dynamic user in userNames)
                {
                    users[i] = user.UserName;
                    i++;
                }
                return users;
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetAllRoles()
        {
            using (var db = ConnectToDatabase())
            {
                return db.Query(@"SELECT RoleName FROM " + RoleTableName).Select<dynamic, string>(d => (string)d.RoleName).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetRolesForUser(string username)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = PetapocoSimpleMembershipProvider.GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }

                string query = @"SELECT r.RoleName FROM " + UsersInRoleTableName + " u, " + RoleTableName + " r Where (u.UserId = @0 and u.RoleId = r.RoleId) GROUP BY RoleName";
                return db.Query(query, new object[] { userId }).Select<dynamic, string>(d => (string)d.RoleName).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetUsersInRole(string roleName)
        {
            using (var db = ConnectToDatabase())
            {
                string query = @"SELECT u." + SafeUserNameColumn + " FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (r.RoleName = @0 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + ")";
                return db.Query(query, new object[] { roleName }).Select<dynamic, string>(d => (string)d.UserName).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool IsUserInRole(string username, string roleName)
        {
            using (var db = ConnectToDatabase())
            {
                var count = db.QuerySingle("SELECT COUNT(*) as cnt FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (u." + SafeUserNameColumn + " = @0 and r.RoleName = @1 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + ")", username, roleName);
                return (count.cnt == 1);
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (string rolename in roleNames)
            {
                if (!RoleExists(rolename))
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvider_NoRoleFound, rolename));
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in roleNames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvder_UserNotInRole, username, rolename));
                    }
                }
            }

            using (var db = ConnectToDatabase())
            {
                List<int> userIds = GetUserIdsFromNames(db, usernames);
                List<int> roleIds = GetRoleIdsFromNames(db, roleNames);

                foreach (int userId in userIds)
                {
                    foreach (int roleId in roleIds)
                    {
                        // Review: Is there a way to do these all in one query?
                        int rows = db.Execute("DELETE FROM " + UsersInRoleTableName + " WHERE UserId = " + userId + " and RoleId = " + roleId);
                        if (rows != 1)
                        {
                            throw new ProviderException(WebDataResources.Security_DbFailure);
                        }
                    }
                }
            }
        }

        private static int FindRoleId(DatabaseWrapper db, string roleName)
        {
            var result = db.QuerySingle(@"SELECT RoleId FROM " + RoleTableName + " WHERE (RoleName = @0)", roleName);
            if (result == null)
            {
                return -1;
            }
            return (int)result.RoleId;
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RoleExists(string roleName)
        {
            using (var db = ConnectToDatabase())
            {
                return (FindRoleId(db, roleName) != -1);
            }
        }

        private static T GetValueOrDefault<T>(NameValueCollection nvc, string key, Func<object, T> converter, T defaultIfNull)
        {
            var val = nvc[key];

            if (val == null)
                return defaultIfNull;

            try
            {
                return converter(val);
            }
            catch
            {
                return defaultIfNull;
            }
        }
    }
}
