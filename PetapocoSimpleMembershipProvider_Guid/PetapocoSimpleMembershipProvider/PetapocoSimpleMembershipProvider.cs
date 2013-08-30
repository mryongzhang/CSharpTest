﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using WebMatrix.WebData;
using WebMatrix.Data;
using PetapocoSimpleMembershipProvider.Resources;
using PetapocoSimpleMembershipProvider.Extensions;
using PetapocoSimpleMembershipProvider.DAL;

namespace PetapocoSimpleMembershipProvider
{
    public class PetapocoSimpleMembershipProvider : ExtendedMembershipProvider
    {
        private const int TokenSizeInBytes = 16;
        private string ConnectionStringName { get; set; }
        private string HashAlgorithmType { get; set; }

        public PetapocoSimpleMembershipProvider()
        {
        }

        // Public properties
        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordRetrieval
        {
            get { return EnablePasswordRetrievalInternal; }
        }
        private bool EnablePasswordRetrievalInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordReset
        {
            get { return EnablePasswordResetInternal; }
        }
        private bool EnablePasswordResetInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RequiresQuestionAndAnswer
        {
            get { return RequiresQuestionAndAnswerInternal; }
        }
        private bool RequiresQuestionAndAnswerInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RequiresUniqueEmail
        {
            get { return RequiresUniqueEmailInternal; }
        }
        private bool RequiresUniqueEmailInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return PasswordFormatInternal; }
        }
        private MembershipPasswordFormat PasswordFormatInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MaxInvalidPasswordAttempts
        {
            get { return MaxInvalidPasswordAttemptsInternal; }
        }
        private int MaxInvalidPasswordAttemptsInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int PasswordAttemptWindow
        {
            get { return PasswordAttemptWindowInternal; }
        }
        private int PasswordAttemptWindowInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredPasswordLength
        {
            get { return MinRequiredPasswordLengthInternal; }
        }
        private int MinRequiredPasswordLengthInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return MinRequiredNonAlphanumericCharactersInternal; }
        }
        private int MinRequiredNonAlphanumericCharactersInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string PasswordStrengthRegularExpression
        {
            get { return PasswordStrengthRegularExpressionInternal; }
        }
        private string PasswordStrengthRegularExpressionInternal { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ApplicationName { get; set; }

        internal static string MembershipTableName
        {
            get { return "webpages_Membership"; }
        }

        internal static string OAuthMembershipTableName
        {
            get { return "webpages_OAuthMembership"; }
        }

        internal static string OAuthTokenTableName 
        {
            get { return "webpages_OAuthToken"; }
        }

        private string SafeUserTableName
        {
            get { return UserTableName; }
        }

        private string SafeUserIdColumn
        {
            get { return UserIdColumn; }
        }

        private string SafeUserNameColumn
        {
            get { return UserNameColumn; }
        }

        // represents the User table for the app
        public string UserTableName
        {
            get { return "UserProfile"; }
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
            set { UserIdColumn = value;}
        }

        internal DatabaseWrapper _database { get; set; }
        internal bool InitializeCalled { get; set; }

        // Inherited from ProviderBase - The "previous provider" we get has already been initialized by the Config system,
        // so we shouldn't forward this call
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (String.IsNullOrEmpty(name))
            {
                name = "PetapocoSimpleMembershipProvider";
            }
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Petapoco Simple Membership Provider");
            }
            base.Initialize(name, config);

            //取得连接字符串
            this.ConnectionStringName = GetValueOrDefault(config, "connectionStringName", o => o.ToString(), string.Empty);

            // membership settings
            this.EnablePasswordRetrievalInternal = GetValueOrDefault(config, "enablePasswordRetrieval", Convert.ToBoolean, false);
            this.EnablePasswordResetInternal = GetValueOrDefault(config, "enablePasswordReset", Convert.ToBoolean, true);
            this.RequiresQuestionAndAnswerInternal = GetValueOrDefault(config, "requiresQuestionAndAnswer", Convert.ToBoolean, false);
            this.RequiresUniqueEmailInternal = GetValueOrDefault(config, "requiresUniqueEmail", Convert.ToBoolean, true);
            this.MaxInvalidPasswordAttemptsInternal = GetValueOrDefault(config, "maxInvalidPasswordAttempts", Convert.ToInt32, 3);
            this.PasswordAttemptWindowInternal = GetValueOrDefault(config, "passwordAttemptWindow", Convert.ToInt32, 10);
            this.PasswordFormatInternal = GetValueOrDefault(config, "passwordFormat", o =>
            {
                MembershipPasswordFormat format;
                return Enum.TryParse(o.ToString(), true, out format) ? format : MembershipPasswordFormat.Hashed;
            }, MembershipPasswordFormat.Hashed);
            this.MinRequiredPasswordLengthInternal = GetValueOrDefault(config, "minRequiredPasswordLength", Convert.ToInt32, 6);
            this.MinRequiredNonAlphanumericCharactersInternal = GetValueOrDefault(config, "minRequiredNonalphanumericCharacters",
                                                                          Convert.ToInt32, 1);
            this.PasswordStrengthRegularExpressionInternal = GetValueOrDefault(config, "passwordStrengthRegularExpression",
                                                                       o => o.ToString(), string.Empty);
            this.HashAlgorithmType = GetValueOrDefault(config, "hashAlgorithmType", o => o.ToString(), "SHA1");


            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("description");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("hashAlgorithmType");

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleMembership_ProviderUnrecognizedAttribute, attribUnrecognized));
                }
            }
        }

        internal static bool CheckTableExists(DatabaseWrapper db, string tableName)
        {
            var query = db.QuerySingle(@"SELECT * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @0", tableName);
            return query != null;
        }

        internal void CreateTablesIfNeeded()
        {
            using (var db = ConnectToDatabase())
            {
                if (!CheckTableExists(db, UserTableName))
                {
                    db.Execute(@"CREATE TABLE " + SafeUserTableName + "(" + SafeUserIdColumn + " varchar(32) NOT NULL PRIMARY KEY, " + SafeUserNameColumn + " nvarchar(56) NOT NULL UNIQUE)");
                }

                if (!CheckTableExists(db, OAuthMembershipTableName))
                {
                    db.Execute(@"CREATE TABLE " + OAuthMembershipTableName + " (Provider nvarchar(30) NOT NULL, ProviderUserId nvarchar(100) NOT NULL, UserId varchar(32) NOT NULL, PRIMARY KEY (Provider, ProviderUserId))");
                }

                if (!CheckTableExists(db, MembershipTableName))
                {
                    db.Execute(@"CREATE TABLE " + MembershipTableName + @" (
                        UserId                                  varchar(32)                 NOT NULL PRIMARY KEY,
                        CreateDate                              datetime            ,
                        ConfirmationToken                       nvarchar(128)       ,
                        IsConfirmed                             bit                 DEFAULT 0,
                        LastPasswordFailureDate                 datetime            ,
                        PswdFailuresSinceLastSuccess            int                 NOT NULL DEFAULT 0,
                        Password                                nvarchar(128)       NOT NULL,
                        PasswordChangedDate                     datetime            ,
                        PasswordSalt                            nvarchar(128)       NOT NULL,
                        PasswordVerificationToken               nvarchar(128)       ,
                        PswdVerificationTokenExpDate            datetime)");
                    // TODO: Do we want to add FK constraint to user table too?
                    //                        CONSTRAINT fk_UserId FOREIGN KEY (UserId) REFERENCES "+UserTableName+"("+UserIdColumn+"))");
                }
            }
        }

        private static void CreateOAuthTokenTableIfNeeded(DatabaseWrapper db)
        {
            if (!CheckTableExists(db, OAuthTokenTableName))
            {
                db.Execute(@"CREATE TABLE " + OAuthTokenTableName + " (Token nvarchar(100) NOT NULL, Secret nvarchar(100) NOT NULL, PRIMARY KEY (Token))");
            }
        }

        // Not an override ==> Simple Membership MUST be enabled to use this method
        public string GetUserId(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                return GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
            }
        }

        internal static string GetUserId(DatabaseWrapper db, string userTableName, string userNameColumn, string userIdColumn, string userName)
        {
            // Casing is normalized in Sql to allow the database to normalize username according to its collation. The common issue 
            // that can occur here is the 'Turkish i problem', where the uppercase of 'i' is not 'I' in Turkish.
            var result = db.QueryValue(@"SELECT " + userIdColumn + " FROM " + userTableName + " WHERE (UPPER(" + userNameColumn + ") = UPPER(@0))", userName);
            if (result != null)
            {
                return (string)result;
            }
            return null;
        }

        // 此处代码有问题，基类接口的定义中返回值应为int，而此处必须返回string，暂时先返回int，后续要修改基类定义
        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override int GetUserIdFromPasswordResetToken(string token)
        {

            using (var db = ConnectToDatabase())
            {
                var result = db.QuerySingle(@"SELECT UserId FROM " + MembershipTableName + " WHERE (PasswordVerificationToken = @0)", token);
                if (result != null && result.USERID != null)
                {
                    return (int)result.USERID;
                }
                return -1;
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
        /// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {

            using (var db = ConnectToDatabase())
            {
                // We need to compare the token using a case insensitive comparison however it seems tricky to do this uniformly across databases when representing the token as a string. 
                // Therefore verify the case on the client
                var row = db.QuerySingle("SELECT m.UserId, m.ConfirmationToken FROM " + MembershipTableName + " m JOIN " + SafeUserTableName + " u"
                                         + " ON m.UserId = u." + SafeUserIdColumn
                                         + " WHERE m.ConfirmationToken = @0 AND"
                                         + " u." + SafeUserNameColumn + " = @1", accountConfirmationToken, userName);
                if (row == null)
                {
                    return false;
                }
                string userId = row.USERID;
                string expectedToken = row.CONFIRMATIONTOKEN;

                if (String.Equals(accountConfirmationToken, expectedToken, StringComparison.Ordinal))
                {
                    int affectedRows = db.Execute("UPDATE " + MembershipTableName + " SET IsConfirmed = 1 WHERE UserId = @0", userId);
                    return affectedRows > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
        /// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method.
        /// There is a tiny possibility where this method fails to work correctly. Two or more users could be assigned the same token but specified using different cases.
        /// A workaround for this would be to use the overload that accepts both the user name and confirmation token.
        /// </remarks>
        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            using (var db = ConnectToDatabase())
            {
                // We need to compare the token using a case insensitive comparison however it seems tricky to do this uniformly across databases when representing the token as a string. 
                // Therefore verify the case on the client
                var rows = db.Query("SELECT UserId, ConfirmationToken FROM " + MembershipTableName + " WHERE ConfirmationToken = @0", accountConfirmationToken)
                    .Where(r => ((string)r.CONFIRMATIONTOKEN).Equals(accountConfirmationToken, StringComparison.Ordinal))
                    .ToList();
                Debug.Assert(rows.Count < 2, "By virtue of the fact that the ConfirmationToken is random and unique, we can never have two tokens that are identical.");
                if (!rows.Any())
                {
                    return false;
                }
                var row = rows.First();
                string userId = row.USERID;
                int affectedRows = db.Execute("UPDATE " + MembershipTableName + " SET IsConfirmed = 1 WHERE UserId = @0", userId);
                return affectedRows > 0;
            }
        }

        internal virtual DatabaseWrapper ConnectToDatabase()
        {
            if (_database == null)
            {
                _database = new DatabaseWrapper(this.ConnectionStringName);
            }
            return _database;
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            if (password.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            string hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            if (userName.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            using (var db = ConnectToDatabase())
            {
                // Step 1: Check if the user exists in the Users table
                string uid = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (uid == null)
                {
                    // User not found
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }

                // Step 2: Check if the user exists in the Membership table: Error if yes.
                var result = db.QuerySingle(@"SELECT COUNT(*) as cnt FROM " + MembershipTableName + " WHERE UserId = @0", uid);
                if (result.CNT > 0)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
                }

                // Step 3: Create user in Membership table
                string token = null;
                object dbtoken = DBNull.Value;
                if (requireConfirmationToken)
                {
                    token = GenerateToken();
                    dbtoken = token;
                }
                int defaultNumPasswordFailures = 0;

                int insert = db.Execute(@"INSERT INTO " + MembershipTableName + " (UserId, Password, PasswordSalt, IsConfirmed, ConfirmationToken, CreateDate, PasswordChangedDate, PswdFailuresSinceLastSuccess)"
                                        + " VALUES (@0, @1, @2, @3, @4, @5, @5, @6)", uid, hashedPassword, String.Empty /* salt column is unused */, !requireConfirmationToken, dbtoken, DateTime.UtcNow, defaultNumPasswordFailures);
                if (insert != 1)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
                return token;
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotSupportedException();
        }

        private void CreateUserRow(DatabaseWrapper db, string userName, IDictionary<string, object> values)
        {
            // Make sure user doesn't exist
            string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
            if (userId != null)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
            }

            StringBuilder columnString = new StringBuilder();
            columnString.Append(SafeUserIdColumn);
            columnString.Append(",").Append(SafeUserNameColumn);
            StringBuilder argsString = new StringBuilder();
            argsString.Append("@0");
            argsString.Append(",@1");
            List<object> argsArray = new List<object>();
            argsArray.Add(Guid.NewGuid().ToString("N"));   // userid
            argsArray.Add(userName);
            if (values != null)
            {
                int index = 2;
                foreach (string key in values.Keys)
                {
                    // Skip the user name column since we always generate that
                    if (String.Equals(UserNameColumn, key, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    columnString.Append(",").Append(key);
                    argsString.Append(",@").Append(index++);
                    object value = values[key];
                    if (value == null)
                    {
                        value = DBNull.Value;
                    }
                    argsArray.Add(value);
                }
            }

            int rows = db.Execute("INSERT INTO " + SafeUserTableName + " (" + columnString.ToString() + ") VALUES (" + argsString.ToString() + ")", argsArray.ToArray());
            if (rows != 1)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            using (var db = ConnectToDatabase())
            {
                CreateUserRow(db, userName, values);
                return CreateAccount(userName, password, requireConfirmation);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        private static bool SetPassword(DatabaseWrapper db, string userId, string newPassword)
        {
            string hashedPassword = Crypto.HashPassword(newPassword);
            if (hashedPassword.Length > 128)
            {
                throw new ArgumentException(WebDataResources.SimpleMembership_PasswordTooLong);
            }

            // Update new password
            int result = db.Execute(@"UPDATE " + MembershipTableName + " SET Password=@0, PasswordSalt=@1, PasswordChangedDate=@2 WHERE UserId = @3", hashedPassword, String.Empty /* salt column is unused */, DateTime.UtcNow, userId);
            return result > 0;
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            // REVIEW: are commas special in the password?
            if (username.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
            }
            if (oldPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "oldPassword");
            }
            if (newPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
            }

            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == null)
                {
                    return false; // User not found
                }

                // First check that the old credentials match
                if (!CheckPassword(db, userId, oldPassword))
                {
                    return false;
                }

                return SetPassword(db, userId, newPassword);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            // Due to a bug in v1, GetUser allows passing null / empty values.
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == null)
                {
                    return null; // User not found
                }

                return new MembershipUser(Membership.Provider.Name, username, userId, null, null, null, true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool DeleteAccount(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == null)
                {
                    return false; // User not found
                }

                int deleted = db.Execute(@"DELETE FROM " + MembershipTableName + " WHERE UserId = @0", userId);
                return (deleted == 1);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == null)
                {
                    return false; // User not found
                }

                int deleted = db.Execute(@"DELETE FROM " + SafeUserTableName + " WHERE " + SafeUserIdColumn + " = @0", userId);
                bool returnValue = (deleted == 1);

                //if (deleteAllRelatedData) {
                // REVIEW: do we really want to delete from the user table?
                //}
                return returnValue;
            }
        }

        internal bool DeleteUserAndAccountInternal(string userName)
        {
            return (DeleteAccount(userName) && DeleteUser(userName, false));
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        private static int GetPasswordFailuresSinceLastSuccess(DatabaseWrapper db, string userId)
        {
            var failure = db.QueryValue(@"SELECT PswdFailuresSinceLastSuccess FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
            if (failure != null)
            {
                return (int)failure;
            }
            return -1;
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                return GetPasswordFailuresSinceLastSuccess(db, userId);
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetCreateDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var createDate = db.QueryValue(@"SELECT CreateDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (createDate != null)
                {
                    return (DateTime)createDate;
                }
                return DateTime.MinValue;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetPasswordChangedDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var pwdChangeDate = db.QuerySingle(@"SELECT PasswordChangedDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (pwdChangeDate != null && pwdChangeDate.PASSWORDCHANGEDDATE != null)
                {
                    return (DateTime)pwdChangeDate.PASSWORDCHANGEDDATE;
                }
                return DateTime.MinValue;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var failureDate = db.QuerySingle(@"SELECT LastPasswordFailureDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (failureDate != null && failureDate.LASTPASSWORDFAILUREDATE != null)
                {
                    return (DateTime)failureDate.LASTPASSWORDFAILUREDATE;
                }
                return DateTime.MinValue;
            }
        }

        private bool CheckPassword(DatabaseWrapper db, string userId, string password)
        {
            string hashedPassword = GetHashedPassword(db, userId);
            bool verificationSucceeded = (hashedPassword != null && Crypto.VerifyHashedPassword(hashedPassword, password));
            if (verificationSucceeded)
            {
                // Reset password failure count on successful credential check
                db.Execute(@"UPDATE " + MembershipTableName + " SET PswdFailuresSinceLastSuccess = 0 WHERE (UserId = @0)", userId);
            }
            else
            {
                int failures = GetPasswordFailuresSinceLastSuccess(db, userId);
                if (failures != -1)
                {
                    db.Execute(@"UPDATE " + MembershipTableName + " SET PswdFailuresSinceLastSuccess = @1, LastPasswordFailureDate = @2 WHERE (UserId = @0)", userId, failures + 1, DateTime.UtcNow);
                }
            }
            return verificationSucceeded;
        }

        private string GetHashedPassword(DatabaseWrapper db, string userId)
        {
            var pwdQuery = db.Query(@"SELECT m.Password " +
                                    @"FROM " + MembershipTableName + " m, " + SafeUserTableName + " u " +
                                    @"WHERE m.UserId = @0 AND m.UserId = u." + SafeUserIdColumn, userId).ToList();
            // REVIEW: Should get exactly one match, should we throw if we get > 1?
            if (pwdQuery.Count != 1)
            {
                return null;
            }
            return pwdQuery[0].PASSWORD;
        }

        // Ensures the user exists in the accounts table
        private string VerifyUserNameHasConfirmedAccount(DatabaseWrapper db, string username, bool throwException)
        {
            string userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
            if (userId == null)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }
                else
                {
                    return null;
                }
            }

            int result = (int)db.QueryValue(@"SELECT COUNT(*) FROM " + MembershipTableName + " WHERE (UserId = @0 AND IsConfirmed = 1)", userId);
            if (result == 0)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoAccountFound, username));
                }
                else
                {
                    return null;
                }
            }
            return userId;
        }

        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            byte[] tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            if (userName.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
            }
            using (var db = ConnectToDatabase())
            {
                string userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: true);
                if (userId == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                string token = (string)db.QueryValue(@"SELECT PasswordVerificationToken FROM " + MembershipTableName + " WHERE (UserId = @0 AND PswdVerificationTokenExpDate > @1)", userId, DateTime.UtcNow);
                if (token == null)
                {
                    token = GenerateToken();

                    int rows = db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordVerificationToken = @0, PswdVerificationTokenExpDate = @1 WHERE (UserId = @2)", token, DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow), userId);
                    if (rows != 1)
                    {
                        throw new ProviderException(WebDataResources.Security_DbFailure);
                    }
                }
                else
                {
                    // TODO: should we update expiry again?
                }
                return token;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool IsConfirmed(string userName)
        {
            if (userName.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
            }

            using (var db = ConnectToDatabase())
            {
                string userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: false);
                return (userId != null);
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            if (newPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
            }
            using (var db = ConnectToDatabase())
            {
                string userId = (string)db.QueryValue(@"SELECT UserId FROM " + MembershipTableName + " WHERE (PasswordVerificationToken = @0 AND PswdVerificationTokenExpDate > @1)", token, DateTime.UtcNow);
                if (userId != null)
                {
                    bool success = SetPassword(db, userId, newPassword);
                    if (success)
                    {
                        // Clear the Token on success
                        int rows = db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordVerificationToken = NULL, PswdVerificationTokenExpDate = NULL WHERE (UserId = @0)", userId);
                        if (rows != 1)
                        {
                            throw new ProviderException(WebDataResources.Security_DbFailure);
                        }
                    }
                    return success;
                }
                else
                {
                    return false;
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool UnlockUser(string userName)
        {
            throw new NotSupportedException();
        }

        internal void ValidateUserTable()
        {
            using (var db = ConnectToDatabase())
            {
                // GetUser will fail with an exception if the user table isn't set up properly
                try
                {
                    GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, "z");
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.Security_FailedToFindUserTable, UserTableName), e);
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ValidateUser(string username, string password)
        {
            if (username.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
            }
            if (password.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "password");
            }

            using (var db = ConnectToDatabase())
            {
                string userId = VerifyUserNameHasConfirmedAccount(db, username, throwException: false);
                if (userId == null)
                {
                    return false;
                }
                else
                {
                    return CheckPassword(db, userId, password);
                }
            }
        }

        public string GetUserNameFromId(string userId)
        {
            using (var db = ConnectToDatabase())
            {
                dynamic username = db.QueryValue("SELECT " + SafeUserNameColumn + " FROM " + SafeUserTableName + " WHERE (" + SafeUserIdColumn + "=@0)", userId);
                return (string)username;
            }
        }

        public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName)
        {
            if (userName.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
            }

            string userId = GetUserId(userName);
            if (userId == null)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            var oldUserId = GetUserIdFromOAuth(provider, providerUserId);
            using (var db = ConnectToDatabase())
            {
                if (oldUserId == null)
                {
                    // account doesn't exist. create a new one.
                    int insert = db.Execute(@"INSERT INTO " + OAuthMembershipTableName + " (Provider, ProviderUserId, UserId) VALUES (@0, @1, @2)", provider, providerUserId, userId);
                    if (insert != 1)
                    {
                        throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                    }
                }
                else
                {
                    // account already exist. update it
                    int insert = db.Execute(@"UPDATE " + OAuthMembershipTableName + " SET UserId = @2 WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider, providerUserId, userId);
                    if (insert != 1)
                    {
                        throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                    }
                }
            }
        }

        public override void DeleteOAuthAccount(string provider, string providerUserId)
        {
            using (var db = ConnectToDatabase())
            {
                // account doesn't exist. create a new one.
                int insert = db.Execute(@"DELETE FROM " + OAuthMembershipTableName + " WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider, providerUserId);
                if (insert != 1)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
            }
        }

        public string GetUserIdFromOAuth(string provider, string providerUserId)
        {
            using (var db = ConnectToDatabase())
            {
                dynamic id = db.QueryValue(@"SELECT UserId FROM " + OAuthMembershipTableName + " WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider.ToUpperInvariant(), providerUserId.ToUpperInvariant());
                if (id != null)
                {
                    return (string)id;
                }

                return null;
            }
        }

        public override string GetOAuthTokenSecret(string token)
        {
            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive
                dynamic secret = db.QueryValue(@"SELECT Secret FROM " + OAuthTokenTableName + " WHERE Token=@0", token);
                return (string)secret;
            }
        }

        public override void StoreOAuthRequestToken(string requestToken, string requestTokenSecret)
        {
            string existingSecret = GetOAuthTokenSecret(requestToken);
            if (existingSecret != null)
            {
                if (existingSecret == requestTokenSecret)
                {
                    // the record already exists
                    return;
                }

                using (var db = ConnectToDatabase())
                {
                    CreateOAuthTokenTableIfNeeded(db);

                    // the token exists with old secret, update it to new secret
                    db.Execute(@"UPDATE " + OAuthTokenTableName + " SET Secret = @1 WHERE Token = @0", requestToken, requestTokenSecret);
                }
            }
            else
            {
                using (var db = ConnectToDatabase())
                {
                    CreateOAuthTokenTableIfNeeded(db);

                    // insert new record
                    int insert = db.Execute(@"INSERT INTO " + OAuthTokenTableName + " (Token, Secret) VALUES(@0, @1)", requestToken, requestTokenSecret);
                    if (insert != 1)
                    {
                        throw new ProviderException(WebDataResources.SimpleMembership_FailToStoreOAuthToken);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the request token with access token and secret.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        public override void ReplaceOAuthRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret)
        {
            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // insert new record
                db.Execute(@"DELETE FROM " + OAuthTokenTableName + " WHERE Token = @0", requestToken);

                // Although there are two different types of tokens, request token and access token,
                // we treat them the same in database records.
                StoreOAuthRequestToken(accessToken, accessTokenSecret);
            }
        }

        /// <summary>
        /// Deletes the OAuth token from the backing store from the database.
        /// </summary>
        /// <param name="token">The token to be deleted.</param>
        public override void DeleteOAuthToken(string token)
        {
            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive
                db.Execute(@"DELETE FROM " + OAuthTokenTableName + " WHERE Token=@0", token);
            }
        }

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            string userId = GetUserId(userName);
            if (userId != null)
            {
                using (var db = ConnectToDatabase())
                {
                    IEnumerable<dynamic> records = db.Query(@"SELECT Provider, ProviderUserId FROM " + OAuthMembershipTableName + " WHERE UserId=@0", userId);
                    if (records != null)
                    {
                        var accounts = new List<OAuthAccountData>();
                        foreach (dynamic row in records)
                        {
                            accounts.Add(new OAuthAccountData((string)row.PROVIDER, (string)row.PROVIDERUSERID));
                        }
                        return accounts;
                    }
                }
            }

            return new OAuthAccountData[0];
        }

        /// <summary>
        /// Determines whether there exists a local account (as opposed to OAuth account) with the specified userId.
        /// </summary>
        /// <param name="userId">The user id to check for local account.</param>
        /// <returns>
        ///   <c>true</c> if there is a local account with the specified user id]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasLocalAccount(string userId)
        {
            using (var db = ConnectToDatabase())
            {
                dynamic id = db.QueryValue(@"SELECT UserId FROM " + MembershipTableName + " WHERE UserId=@0", userId);
                return id != null;
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