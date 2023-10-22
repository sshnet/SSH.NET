namespace Renci.SshNet.IntegrationTests
{
    public interface IAuthenticationMethodFactory
    {
        PasswordAuthenticationMethod CreatePowerUserPasswordAuthenticationMethod();

        PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethod();

        PrivateKeyAuthenticationMethod CreateRegularUserMultiplePrivateKeyAuthenticationMethod();

        PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithPassPhraseAuthenticationMethod();

        PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyWithEmptyPassPhraseAuthenticationMethod();

        PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey();

        PasswordAuthenticationMethod CreateRegulatUserPasswordAuthenticationMethod();

        PasswordAuthenticationMethod CreateRegularUserPasswordAuthenticationMethodWithBadPassword();

        KeyboardInteractiveAuthenticationMethod CreateRegularUserKeyboardInteractiveAuthenticationMethod();
    }
}
