using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;

namespace EventBridgeLambdaIntegration
{
    public class EventBridgeLambdaStack : Stack
    {
        public EventBridgeLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create a Lambda function
            var myLambda = new Function(this, "MyLambda", new FunctionProps
            {
                // Your Lambda function configuration
            });

            // Stream Lambda logs to EventBridge Logs
            var logGroup = new LogGroup(this, "MyLambdaLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/lambda/{myLambda.FunctionName}" // Automatically creates log group for Lambda
            });

            // Subscribe Lambda logs to EventBridge bus
            logGroup.AddSubscriptionFilter("LambdaSubscriptionFilter", new SubscriptionFilterOptions
            {
                FilterPattern = FilterPattern.AllEvents(), // Adjust the filter pattern as needed
                Destination = new EventBridgeDestination(new EventBridgeDestinationProps
                {
                    EventBus = EventBus.FromEventBusName(this, "MyEventBus", "YourEventBusName") // Replace with your EventBridge bus name
                })
            });
        }
    }
}





myLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "events:PutEvents" },
                Resources = new[] { "*" } // Adjust the resource to a specific EventBridge bus ARN if possible
            }));

            // Check or modify the EventBridge rule configuration
            var eventRule = new Rule(this, "MyEventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.lambda" },
                    DetailType = new[] { "Lambda Function Invocation Result" },
                    Detail = new Dictionary<string, object>
                    {
                        { "requestParameters.functionName", new[] { myLambda.FunctionName } }
                    }
                }
            });

            // Add the Lambda function as a target for the EventBridge rule
            eventRule.AddTarget(new LambdaFunction(myLambda));

            // Create CloudWatch Logs subscription filter for EventBridge
            var logGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
            {
                LogGroupName = "YourLogGroupName" // Replace with your log group name
            });

            var eventRuleForLogs = new Rule(this, "MyEventRuleForLogs", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.logs" },
                    DetailType = new[] { "Log Group" },
                    Detail = new Dictionary<string, object>
                    {
                        { "logGroupName", "YourLogGroupName" } // Replace with your log group name
                    }
                }
            });

            eventRuleForLogs.AddTarget(new LambdaFunction(myLambda));
        }
    }
}
