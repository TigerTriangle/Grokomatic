using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grokomatic.Models
{
    /// <summary>
    /// There are a lot more fields according to:
    /// https://developer.twitter.com/en/docs/twitter-api/tweets/manage-tweets/api-reference/post-tweets
    /// but these are the ones we care about for our use case.
    /// </summary>
    public class XPostRequest
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
        public mediaIDS media { get; set; }


    }

    public class mediaIDS
    {
        public string[] media_ids { get; set; }
    }
}
