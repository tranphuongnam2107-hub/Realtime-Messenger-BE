using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

namespace Services.Config.CloudinaryConfig
{
    public class CloudinaryConnection
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryConnection(IOptions<CloudinarySetting> config)
        {
            var account = new Account
            (
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public Cloudinary GetClient() => _cloudinary;
    }
}
