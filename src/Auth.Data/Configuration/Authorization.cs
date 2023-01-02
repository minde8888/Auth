namespace Auth.Data.Configuration
{
    public class Authorization
    {

        public const string default_username = "user";
        public const string default_email = "user@secureapi.com";
        public const string default_password = "Pa$$w0rd.";
        public const Roles default_role = Roles.SuperAdmin;

        public enum Roles
        {
            SuperAdmin = 0,
            Admin = 1,
            Moderator = 2,
            Basic = 3
        }
    }
}
