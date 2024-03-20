private void AddCorsOptions(Amazon.CDK.AWS.APIGateway.IResource apiResource, string url)
{
    apiResource.AddMethod("OPTIONS", new MockIntegration(new IntegrationOptions
    {
        IntegrationResponses = new[]
        {
            new IntegrationResponse
            {
                StatusCode = "200",
                ResponseParameters = new Dictionary<string, string>
                {
                    { "method.response.header.Access-Control-Allow-Headers", "Content-Type, X-Amz-Date, Authorization, X-Api-Key, X-Amz-Security-Token, X-Anz-User-Agent" },
                    { "method.response.header.Access-Control-Allow-Origin", url },
                    { "method.response.header.Access-Control-Allow-Credentials", "false" },
                    { "method.response.header.Access-Control-Allow-Methods", "OPTIONS, GET, PUT, POST, DELETE" }
                }
            }
        },
        PassthroughBehavior = PassthroughBehavior.NEVER,
        RequestTemplates = new Dictionary<string, string>
        {
            { "application/json", "{\"statusCode\": 200}" }
        }
    }), new MethodOptions
    {
        MethodResponses = new[]
        {
            new MethodResponse
            {
                StatusCode = "200",
                ResponseParameters = new Dictionary<string, bool>
                {
                    { "method.response.header.Access-Control-Allow-Headers", true },
                    { "method.response.header.Access-Control-Allow-Methods", true },
                    { "method.response.header.Access-Control-Allow-Credentials", true },
                    { "method.response.header.Access-Control-Allow-Origin", true }
                }
            }
        }
    });
}
