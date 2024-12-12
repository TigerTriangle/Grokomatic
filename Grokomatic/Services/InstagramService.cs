using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Grokomatic.Services
{    
    public class InstagramService
    {
        private readonly IInstaApi _instaApi;

        public InstagramService(IInstaApi instaApi)
        {
            _instaApi = instaApi;
        }

        public async Task PostOnInstagram(string text, string imagePath)
        {
            var mediaImage = new InstaImageUpload
            {
                // leave zero if dimensions are unknown
                Height = 0,
                Width = 0,
                Uri = imagePath
            };

            var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, text);
            Console.WriteLine(result.Succeeded
                ? $"Media created: {result.Value.Pk}, {result.Value.Caption}"
                : $"Unable to upload photo: {result.Info.Message}");
        }
    }
}
