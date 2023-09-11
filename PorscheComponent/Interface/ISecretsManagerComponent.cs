using PorscheComponent.Component;

namespace PorscheComponent.Interface
{
    public interface ISecretsManagerComponent
    {
        AWSSecrets GetSecret(string secretName, string region);
    }
}
