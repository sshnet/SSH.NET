namespace Renci.SshNet.IntegrationTests
{
    public class AuthenticationMethodFactory
    {
        public PasswordAuthenticationMethod CreatePowerUserPasswordAuthenticationMethod()
        {
            var user = Users.Admin;
            return new PasswordAuthenticationMethod(user.UserName, user.Password);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("resources.client.id_rsa");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserMultiplePrivateKeyAuthenticationMethod()
        {
            var privateKeyFile1 = GetPrivateKey("resources.client.id_rsa");
            var privateKeyFile2 = GetPrivateKey("resources.client.id_rsa");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile1, privateKeyFile2);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithPassPhraseAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("resources.client.id_rsa_with_pass", "tester");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithEmptyPassPhraseAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("resources.client.id_rsa_with_pass", null);
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey()
        {
            var privateKeyFile = GetPrivateKey("resources.client.id_noaccess.rsa");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PasswordAuthenticationMethod CreateRegulatUserPasswordAuthenticationMethod()
        {
            return new PasswordAuthenticationMethod(Users.Regular.UserName, Users.Regular.Password);
        }

        public PasswordAuthenticationMethod CreateRegularUserPasswordAuthenticationMethodWithBadPassword()
        {
            return new PasswordAuthenticationMethod(Users.Regular.UserName, "xxx");
        }

        public KeyboardInteractiveAuthenticationMethod CreateRegularUserKeyboardInteractiveAuthenticationMethod()
        {
            var keyboardInteractive = new KeyboardInteractiveAuthenticationMethod(Users.Regular.UserName);
            keyboardInteractive.AuthenticationPrompt += (sender, args) =>
                {
                    foreach (var authenticationPrompt in args.Prompts)
                    {
                        authenticationPrompt.Response = Users.Regular.Password;
                    }
                };
            return keyboardInteractive;
        }

        private PrivateKeyFile GetPrivateKey(string resourceName, string passPhrase = null)
        {
            using (var stream = TestBase.GetData(resourceName))
            {
                return new PrivateKeyFile(stream, passPhrase);
            }
        }
    }
}
