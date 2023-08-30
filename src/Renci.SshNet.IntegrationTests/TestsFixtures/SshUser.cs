namespace IntegrationTests.TestsFixtures
{
    public class SshUser
    {
        public string UserName { get; }

        public string Password { get; }

        public SshUser(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}

