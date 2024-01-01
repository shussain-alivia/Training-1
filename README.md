{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "EventBridgeInvokePermission",
      "Effect": "Allow",
      "Action": [
        "lambda:InvokeFunction"
      ],
      "Resource": "arn:aws:lambda:REGION:ACCOUNT_ID:function:YOUR_LAMBDA_FUNCTION_NAME"
    }
  ]
}
