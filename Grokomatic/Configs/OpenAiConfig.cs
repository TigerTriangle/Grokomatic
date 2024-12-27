using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grokomatic.Configs
{
    public class OpenAiConfig : IAiConfig
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public string? ImageModel { get; set; }
        public string? Endpoint { get; set; }
    }
}
