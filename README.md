var targetInput = new RuleTargetInput();
            targetInput.Add("detail", "{ \"eventSource\": [\"aws:s3\"], \"eventName\": [\"ObjectCreated:*\"], \"eventTime\": \"$input{Time}\", \"bucketName\": \"$input{Bucket}\", \"objectKey\": \"$input{Key}\" }");
            targetInput.Add("detailType", "S3ObjectCreated"); // DetailType for the event (Modify as needed)
