using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;

namespace PCLDeliveryAPI
{
    public class SecretsManager
    {
        public static DatabaseSecrets GetSecret(string secretName, string region)
        {
            DatabaseSecrets dbsecrets = new DatabaseSecrets();
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
                dbsecrets = JsonConvert.DeserializeObject<DatabaseSecrets>(response.SecretString);
            }

            return dbsecrets;
        }
    }

    public class DatabaseSecrets
    {
        public string? dbClusterIdentifier { get; set; }
        public string? password { get; set; }
        public string? dbname { get; set; }
        public string? engine { get; set; }
        public string? port { get; set; }
        public string? host { get; set; }
        public string? username { get; set; }

    }

    public class MasterDataSecrets
    {
        public string? base_address { get; set; }
        public string? url { get; set; }
        public string? client_id { get; set; }
        public string? secret_id { get; set; }
    }

    public class AccessToken
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public int? expires_in { get; set; }
    }

    public class Attributes
    {
        #region User Info Attributes
        public string username { get; set; }
        public object gender { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public object academicTitle { get; set; }
        public string mail { get; set; }
        public object jobTitle { get; set; }
        public object department { get; set; }
        public List<string> languages { get; set; }
        public string accountStatus { get; set; }
        public object locked { get; set; }
        public Contact contact { get; set; }

        #endregion

        #region Organization Attributes
        public string businessType { get; set; }
        public string companyName { get; set; }
        public string organizationStatus { get; set; }
        public string facilityType { get; set; }
        public string displayName { get; set; }
        public string porschePartnerNo { get; set; }
        public object supplierDuns { get; set; }
        public object investorType { get; set; }

        #endregion
    }

    public class Contact
    {
        public object mobile { get; set; }
        public object phone { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string type { get; set; }
        public Attributes attributes { get; set; }
        public Relationships relationships { get; set; }
    }

    public class Importer
    {
        public Data data { get; set; }
    }

    public class Organization
    {
        public Data data { get; set; }
    }

    public class Relationships
    {
        public Importer importer { get; set; }
        public Organization organization { get; set; }
        public SuperiorMarket superiorMarket { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }

    public class SuperiorMarket
    {
        public Data data { get; set; }
    }
}
