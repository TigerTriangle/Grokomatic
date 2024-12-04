using System.Text;
using Tweetinvi.Core.Web;
using Tweetinvi.Models;
using Tweetinvi;
using Grokomatic.Models;

namespace Grokomatic.Services
{
    public class XService
    {
        // ----------------- Fields ----------------

        private readonly ITwitterClient client;

        // ----------------- Constructor ----------------

        public XService(ITwitterClient client)
        {
            this.client = client;
        }

        public async Task<ITwitterResult> PostX(XPostRequest xRequest)
        {

            var jsonBody = client.Json.Serialize(xRequest);
            var result = await this.client.Execute.AdvanceRequestAsync((ITwitterRequest request) =>
            {
                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            });

            return result;

        }
    }
}
