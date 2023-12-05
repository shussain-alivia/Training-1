using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;

// Inside your construct or stack
// Assuming you have a certificate ARN available as 'certificateArn'

// Create a certificate using the certificate ARN
ICertificate certificate = Certificate.FromCertificateArn(this, "Certificate", certificateArn);

// Create the API Gateway domain name
var apiDomainName = new CfnDomainName(this, "ApiDomainName", new CfnDomainNameProps
{
    DomainName = "api.example.com", // Your domain name
    CertificateArn = certificate.CertificateArn // ARN of the certificate
});

// Creating the BasePathMapping to associate the domain with the API
var basePathMapping = new BasePathMapping(this, "BasePathMapping", new BasePathMappingProps
{
    DomainName = apiDomainName,
    RestApi = yourLambdaRestApi, // Replace 'yourLambdaRestApi' with your API object
    BasePath = "/", // Specify the base path you want
    Stage = yourDeploymentStage // Replace 'yourDeploymentStage' with your Stage object
});
