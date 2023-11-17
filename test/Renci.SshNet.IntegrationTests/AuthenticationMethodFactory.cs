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
            var privateKeyFile = GetPrivateKey("Data.Key.RSA.txt");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserMultiplePrivateKeyAuthenticationMethod()
        {
            var privateKeyFile1 = GetPrivateKey("Data.Key.RSA.txt");
            var privateKeyFile2 = GetPrivateKey("Data.Key.RSA.txt");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile1, privateKeyFile2);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithPassPhraseAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("Data.Key.RSA.Encrypted.Aes.256.CBC.12345.txt", "12345");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithEmptyPassPhraseAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("Data.Key.RSA.Encrypted.Aes.256.CBC.12345.txt", null);
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey()
        {
            string unauthorizedKey = """
            -----BEGIN RSA PRIVATE KEY-----
            MIIEpAIBAAKCAQEAuK3OhcrEnQbbE1+WaE57tUCcTz1yqdE2AwvMfs3of1nyfGcS
            Rz9vzAFYU+3uEEApk0QOsIeWCyB2DIlPnlQHyjVWRYPqiTtQ7GmdzbF0ISa7dr23
            EHJKgtJxSm3O/sb5F9JyqlxFMhKpz5NVgnN7NFcej93opHZN6h9LaP8cHgJIepWV
            IkZqhcv8v6SpAgei0muoPHB+ZA6Rycnv+2//WUBzu+3AJu0PiHUkTTVC8M5svMRV
            Ah8CnLsCkAAx7ld4AH7McRlFjymmkwxTSewFJYkloI/OqDOjsmuW03Gmx+eytPWa
            HEPGeRhcz1kZ6eOmqrPMlTaLPV1MbFn86nauAQIDAQABAoIBAGEiWauZOMx2nKeV
            8SAvl3V/5DbxVOvotAXqIMbZOl4xSw8Pj1eWEBE26+RJEpvNg5CHjUpgJhT4H978
            Ibpe7DH418V8WtGPN0MBUhSsLy54lsUfh7fIxVQFp7zEAMmUkdNrxw+/tE1f75zU
            G3efkb+3ysVUrFZEOzrW9uzksT8+gm2Ll/IKuDy2r5k9mJr2cX5OYKxXjtNo5duO
            UK+M3jW9Sk1k23Jzpq2GwuJGTTjgtI41ND6CDkrY7COdRQdIx3eQ0uQSXosKNREe
            lv0VTlboVyh8JXt+G1tkfA6+Al77/mzycaZVX26C8Io7Y/S7JVG7TT1p1RsFGZM8
            kcqvpBkCgYEA7vD3S+6T+8Ql8U877nDi/Ttf16NEUUQllgjWgCP+DiWcqQGWaiaB
            JTYyM4Ydb4jy2jAcAdf3HfImE4QO3+u/wyuQrdlvWByHo2NqOxYMdyqKqwGh7qhU
            zZFbGfHRD/gV4hWXfzj65wA8uMBVc5J3/ug7nmkTWywiDH/SsPdbxmcCgYEAxd0c
            EbJ3dlIyK5Ul1Gw5dASyE91Nx/NHAvB+5QHH5rIe/IqbtxbXmEMKcxwEPN8hvpzs
            g487TQFkNPze6X8vZkiuaNLUq9vwRlQwr/LIdjLLKOA69wKfFDSkei8LEMgEz7Wg
            ZEm8ifJP75hGozx31bW4dYX2o2X75SbXneMVF1cCgYEAo4h8WJXC5o9KwKtQA1Nz
            p4lZgUaW3V/csaD+3djEan5HiEwz3BbaUNOU7DqgLtP2EmrW4FQlJ3Oxp628WHkL
            V9KbRMEKOa3dD3BdJm9ivLR7D6sgXy0KTV9skIc2ZM2QfJn2g/ZFkpBQ/sl0MpNO
            WUIse7DCtKWx8AgT9VZ2k4UCgYB1G8JSQyPrtwiUvQkP6iIzJdhUY4Z20ulztu4U
            EvLC+yfV5x/0xKNELmHP8YQclyA81loyH6NEl488wXIaFznxuxDnX+mZ8moK5ieO
            7A5zzuppvhWIP1fyOJok6xUMkKYwXdqZoP7jUrS3JZShZteyeIS9olVxLpphbZTu
            kQnZrwKBgQDhO2+iGXwNLS+OFKwEiyUgvi6jb5OrIsdwWgqaqQarm6h0QWtxrCs6
            CMFFEusswZEGRo83J6lQxtcXvhWzTkVPu69J8YvTQqcKlvUSA9TEG2iX9bwXSWzy
            LeGb5NjBZ3szfzp9l5Utnj5GuAGoDDDKpf7M6S95Lg6F58Mhd/tCFA==
            -----END RSA PRIVATE KEY-----
            """;

            using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(unauthorizedKey));
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, new PrivateKeyFile(memoryStream));
        }

        public PasswordAuthenticationMethod CreateRegularUserPasswordAuthenticationMethod()
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
