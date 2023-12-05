internal void CreateServiceLambda(IVpc vpc, ISubnetSelection subnetSelection, string serviceName, string friendlyName, string runtimeEnvironment, FfsSnsTopic snsTopic = null)
{
    string serviceFullName = $"FeeForService.{serviceName}";
    string serviceFriendlyName = $"{serviceName}{friendlyName}";

    IFunction serviceLambda = new Function(this, $"{serviceFriendlyName} Lambda", new FunctionProps()
    {
        FunctionName = $"{serviceFriendlyName}Lambda",
        Code = Amazon.CDK.AWS.Lambda.Code.FromAsset($"../dist/{serviceFullName}/release/publish"),
        Handler = serviceFullName,
        Runtime = Runtime.DOTNET_6,
        Description = $"Lambda function created for \"{friendlyName}\"",
        Vpc = vpc,
        MemorySize = 2848,
        VpcSubnets = subnetSelection,
        Timeout = Duration.Seconds(30),
        Environment = new Dictionary<string, string>
        {
            { "ASPNETCORE_ENVIRONMENT", runtimeEnvironment }
        }
    });

    string domainName = $"api.{runtimeEnvironment}.orbisrx.com"; // Replace with your desired domain

    // Create the domain name for the API Gateway
    var domain = new DomainName(this, $"{serviceFriendlyName}CustomDomain", new DomainNameProps
    {
        DomainName = domainName,
        Certificate = Certificate.FromCertificateArn(this, "Certificate", "arn:aws:acm:us-east-1:232063328188:certificate/54cdb0e6-cb89-4395-a095-112564a6ef55"), // Replace with your certificate ARN 
        EndpointType = EndpointType.EDGE,
        SecurityPolicy = SecurityPolicy.TLS_1_2
    });

    var restApiVar = $"{serviceFriendlyName}-Lambda-Endpoint-CDK";
    
    // Add the API Gateway for this lambda function
    var api = new LambdaRestApi(this, restApiVar, new LambdaRestApiProps
    {
        Description = $"API Gateway for {serviceFriendlyName} Lambda, created from the CDK",
        RestApiName = restApiVar,
        Handler = serviceLambda,
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
        }
    });

    // Map Lambda functions to specific paths on the custom domain
    domain.AddBasePathMapping(api, new BasePathMappingOptions
    {
        BasePath = "/alerts", // Replace with your specific base paths
        Stage = api.DeploymentStage
        // Lambda function associations based on the acceptance criteria URLs
        // Example: alertsLambdaUrl goes here
    });

    domain.AddBasePathMapping(api, new BasePathMappingOptions
    {
        BasePath = "/authorization", // Replace with your specific base paths
        Stage = api.DeploymentStage
        // Lambda function associations based on the acceptance criteria URLs
        // Example: authorizationLambdaUrl goes here
    });

    // Add more base path mappings for other Lambda functions as per acceptance criteria

    // Policy statement for RDS DB instance access
    PolicyStatement rdsPolicyStatement = new PolicyStatement();
    rdsPolicyStatement.AddActions("rds-db:connect", "s3:Put*", "s3:Get*", "s3:Delete*", "secretsmanager:DescribeSecret", "secretsmanager:GetSecretValue");
    rdsPolicyStatement.AddResources("*");
    rdsPolicyStatement.Effect = Effect.ALLOW;
    serviceLambda.AddToRolePolicy(rdsPolicyStatement);
}
