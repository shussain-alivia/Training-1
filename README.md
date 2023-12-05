// Creating a CfnDomainName
var apiDomainName = new CfnDomainName(this, "ApiDomainName", new CfnDomainNameProps
{
    DomainName = "api.example.com", // Your domain name
    CertificateArn = certificate.CertificateArn // ARN of the certificate
});

// Creating a DomainName from the CfnDomainName
var customDomain = DomainName.FromDomainNameAttributes(this, "CustomDomain", new DomainNameAttributes
{
    DomainName = apiDomainName.GetAtt("DomainName").ToString(),
    DomainNameAliasHostedZoneId = apiDomainName.GetAtt("RegionalHostedZoneId").ToString(),
    DomainNameAliasTarget = apiDomainName.GetAtt("RegionalDomainName").ToString(),
    Certificate = Certificate.FromCertificateArn(this, "Certificate", apiDomainName.CertificateArn)
});

// Using the customDomain in BasePathMapping
var basePathMapping = new BasePathMapping(this, "BasePathMapping", new BasePathMappingProps
{
    DomainName = customDomain,
    RestApi = yourLambdaRestApi, // Replace 'yourLambdaRestApi' with your API object
    BasePath = "/", // Specify the base path you want
    Stage = yourDeploymentStage // Replace 'yourDeploymentStage' with your Stage object
});
