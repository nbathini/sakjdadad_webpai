using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using PorscheComponent.Interface;

namespace PorscheComponent.Component
{
    public class SecretsManagerComponent : ISecretsManagerComponent
    {
        #region Get Secret By Secret Name and Region From AW Secret Manager
        public AWSSecrets GetSecret(string secretName, string region)
        {
            AWSSecrets awsSecrets = new AWSSecrets();

            string secret = string.Empty;

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                //VersionStage = "AWSCURRENT"
            };

            GetSecretValueResponse response = client.GetSecretValueAsync(request).Result;

            if (response != null && response.SecretString != null)
            {
                awsSecrets = JsonConvert.DeserializeObject<AWSSecrets>(response.SecretString);
            }

            return awsSecrets;
        }

        #endregion
    }

    public class AWSSecrets
    {
        public string? dbClusterIdentifier { get; set; }
        public string? password { get; set; }
        public string? dbname { get; set; }
        public string? engine { get; set; }
        public string? port { get; set; }
        public string? host { get; set; }
        public string? username { get; set; }

    }
}
