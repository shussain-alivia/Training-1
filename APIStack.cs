foreach (var svc in serviceLambdas)
{
    var api = svc.Api;
    
    // Add OPTIONS method to each resource
    foreach (var resource in api.Root.Resources.Values)
    {
        resource.AddMethod("OPTIONS", new MockIntegration(new IntegrationOptions()), new MethodOptions
        {
            MethodResponses = new MethodResponse[]
            {
                new MethodResponse
                {
                    StatusCode = "200",
                    ResponseParameters = new Dictionary<string, bool>
                    {
                        { "method.response.header.Access-Control-Allow-Headers", true },
                        { "method.response.header.Access-Control-Allow-Methods", true },
                        { "method.response.header.Access-Control-Allow-Origin", true }
                    }
                }
            },
            IntegrationResponses = new[]
            {
                new IntegrationResponse
                {
                    StatusCode = "200",
                    ResponseParameters = new Dictionary<string, string>
                    {
                        { "method.response.header.Access-Control-Allow-Headers", "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token,X-Anz-User-Agent'" },
                        { "method.response.header.Access-Control-Allow-Origin", "'*'" }, // Adjust as needed
                        { "method.response.header.Access-Control-Allow-Methods", "'OPTIONS,GET,PUT,POST,DELETE'" } // Adjust as needed
                    },
                    PassthroughBehavior = PassthroughBehavior.NEVER,
                    RequestTemplates = new Dictionary<string, string>
                    {
                        { "application/json", "{\"statusCode\": 200}" }
                    }
                }
            }
        });
    }

    // Add behavior options for CloudFront
    behaviors.Add($"/{svc.ApiPath}/*", new BehaviorOptions
    {
        Origin = new RestApiOrigin(api, new RestApiOriginProps
        {
            OriginId = $"{svc.ApiPath}-api-origin",
            OriginPath = "", // Ensure this is empty
        }),
        ViewerProtocolPolicy = ViewerProtocolPolicy.ALLOW_ALL,
        AllowedMethods = AllowedMethods.ALLOW_ALL,
        CachePolicy = CachePolicy.CACHING_DISABLED,
        OriginRequestPolicy = OriginRequestPolicy.ALL_VIEWER_EXCEPT_HOST_HEADER
    });
}
