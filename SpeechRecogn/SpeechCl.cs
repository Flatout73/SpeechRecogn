using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechRecogn
{
    class SpeechCl
    {
        protected string api_key;
        public static readonly string api_uri = "https://api.cognitive.microsoft.com/sts/v1.0";
        private string token;
        private Timer accessTokenRenewer;

        private const int RefreshTokenDuration = 9;

        public SpeechCl(string subscriptionKey)
        {
            this.api_key = subscriptionKey;
            this.token = FetchToken(api_uri, api_key).Result;

            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback),
                                            this,
                                            TimeSpan.FromMinutes(RefreshTokenDuration),
                                            TimeSpan.FromMilliseconds(-1));
        }
        
        public string GetAccessToken()
        {
            return this.token;
        }

        private void RenewAccessToken()
        {
            this.token = FetchToken(api_uri, api_key).Result;
            Console.WriteLine("Renewed token.");
        }

        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }

        private async Task<string> FetchToken(string fetchUri, string subscriptionKey)
        {
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder(fetchUri);
                uriBuilder.Path += "/issueToken";

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                return await result.Content.ReadAsStringAsync();
            }
        } 

    }
}
