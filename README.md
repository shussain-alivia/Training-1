// ... (existing code)

// Define your custom domain name
string domainName = "example.com"; // Replace with your desired domain name

// Define the base path for your API
string basePath = "/v1"; // Replace with your desired base path

// Create the domain name for the API Gateway
var domain = new DomainName(this, $"{serviceFriendlyName}CustomDomain", new DomainNameProps
{
    DomainName = domainName,
    Certificate = Certificate.FromCertificateArn(this, "Certificate", "YOUR_CERTIFICATE_ARN"), // Replace with your certificate ARN
    EndpointType = EndpointType.EDGE,
    SecurityPolicy = SecurityPolicy.TLS_1_2
});

// Create the API Gateway using the custom domain name
var api = new LambdaRestApi(this, restApiVar, new LambdaRestApiProps
{
    Description = $"API Gateway for {serviceFriendlyName} Lambda, created from the CDR",
    RestApiName = restApiVar,
    Handler = serviceLanbda,
    Proxy = true,
    DefaultCorsPreflightOptions = new CorsOptions
    {
        AllowCredentials = true,
        AllowHeaders = new string[] { "*" },
        AllowMethods = new string[] { "*" },
        AllowOrigins = new string[] { "*" }
    },
    DomainName = new DomainNameOptions
    {
        DomainName = domain.DomainNameAliasDomainName,
        Certificate = domain.Certificate,
        BasePath = basePath
    }
});

// Map the custom domain name to the API Gateway
domain.AddBasePathMapping(api, new BasePathMappingOptions
{
    BasePath = basePath,
    Stage = api.DeploymentStage
});

// ...
