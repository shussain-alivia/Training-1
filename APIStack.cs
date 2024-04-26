var response = await Polly.Policy
    .Handle<KeyNotFoundException>()
    .RetryAsync(RETRY_TIMES)
    .ExecuteAsync(async () =>
    {
        var httpResponse = await _httpClient.GetAsync(_httpClient.BaseAddress).ConfigureAwait(false);
        var responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return (httpResponse, responseBody);
    })
    .ConfigureAwait(false);

var httpResponseMessage = response.Item1;
var responseBody = response.Item2;
