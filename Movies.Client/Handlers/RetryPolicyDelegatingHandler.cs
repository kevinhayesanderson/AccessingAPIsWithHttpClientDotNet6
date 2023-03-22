namespace Movies.Client.Handlers
{
    public class RetryPolicyDelegatingHandler : DelegatingHandler
    {
        private readonly int _maximumAmountOfRetries;

        public RetryPolicyDelegatingHandler(int maximumAmountOfRetries) : base()
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        public RetryPolicyDelegatingHandler(HttpMessageHandler innerHandler, int maximumAmountOfRetries) : base(innerHandler)
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _maximumAmountOfRetries; i++)
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}