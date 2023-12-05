internal FfsRebatesApiStack(Construct scope, string id, FfsRebatesApiProps props = null) : base(scope, id, props)
{
    // Lookup Pod VPC by ID
    IVpc vpc = Vpc.FromLookup(this, "pod_vpc", new VpcLookupOptions { VpcId = props.VpcId });

    // Setup Subnet Group from Subnet IDs
    ISubnetSelection subnetSelection = new SubnetSelection
    {
        Subnets = new ISubnet[]
        {
            Subnet.FromSubnetId(this, $"{props.FriendlyName} SubnetAz1", props.SubnetAz1["id"]),
            Subnet.FromSubnetId(this, $"{props.FriendlyName} SubnetAz2", props.SubnetAz2["id"]),
            Subnet.FromSubnetId(this, $"{props.FriendlyName} SubnetAz3", props.SubnetAz3["id"])
        }
    };

    Console.WriteLine("Creating SNS topic");
    var snsTopic = new FfsSnsTopic(this, $"{props.SnsTopic}-{props.RuntimeEnvironment}", new FfsSnsTopicProps
    {
        FriendlyName = props.SnsTopic,
        VpcId = props.VpcId,
        Env = props.Env,
        SubnetAz1 = props.SubnetAz1,
        SubnetAz2 = props.SubnetAz2,
        SubnetAz3 = props.SubnetAz3
    });
    Console.WriteLine("Completed SNS topic");

    List<IFunction> serviceLambdas = new List<IFunction>();

    foreach (var serviceName in props.Services)
    {
        if (serviceName == "Alerts")
        {
            // Logic for Alerts service
        }
        else
        {
            var serviceLambda = CreateServiceLambda(vpc, subnetSelection, serviceName, props.FriendlyName, props.RuntimeEnvironment, snsTopic);
            serviceLambdas.Add(serviceLambda);
        }
    }

    // Create Custom Domain and Persistent API setup after all Lambda functions are created
    SetupCustomDomainAndPersistentAPI(vpc, subnetSelection, serviceLambdas);
}

internal IFunction CreateServiceLambda(IVpc vpc, ISubnetSelection subnetSelection, string serviceName, string friendlyName, string runtimeEnvironment, FfsSnsTopic snsTopic = null)
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

    return serviceLambda; // Return the created Lambda function
}

private void SetupCustomDomainAndPersistentAPI(IVpc vpc, ISubnetSelection subnetSelection, List<IFunction> serviceLambdas)
{
    string domainName = $"api.{runtimeEnvironment}.orbisrx.com"; // Replace with your desired domain

    // Create the domain name for the API Gateway
    var domain = new DomainName(this, "CustomDomain", new DomainNameProps
    {
        DomainName = domainName,
        Certificate = Certificate.FromCertificateArn(this, "Certificate", "arn:aws:acm:us-east-1:232063328188:certificate/54cdb0e6-cb89-4395-a095-112564a6ef55"), // Replace with your certificate ARN 
        EndpointType = EndpointType.EDGE,
        SecurityPolicy = SecurityPolicy.TLS_1_2
    });

    // Iterate through the list of Lambda functions to set up Custom Domain and Persistent API
    foreach (var serviceLambda in serviceLambdas)
    {
        var restApiVar = $"{serviceLambda.FunctionName}-Lambda-Endpoint-CDK";
        
        // Add the API Gateway for this lambda function
        var api = new LambdaRestApi(this, restApiVar, new LambdaRestApiProps
        {
            Description = $"API Gateway for {serviceLambda.FunctionName}, created from the CDK",
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
            BasePath = $"/{serviceLambda.FunctionName.ToLower()}", // Adjust the base path for each Lambda function
            Stage = api.DeploymentStage
        });

        // Policy statement for RDS DB instance access
        PolicyStatement rdsPolicyStatement = new PolicyStatement();
        rdsPolicyStatement.AddActions("rds-db:connect", "s3:Put*", "s3:Get*", "s3:Delete*", "secretsmanager:DescribeSecret", "secretsmanager:GetSecretValue");
        rdsPolicyStatement.AddResources("*");
        rdsPolicyStatement.Effect = Effect.ALLOW;
        serviceLambda.AddToRolePolicy(rdsPolicyStatement);
    }
}
