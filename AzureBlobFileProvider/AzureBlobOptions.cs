﻿using System;

namespace EZms.Core.AzureBlobFileProvider
{
    public class AzureBlobOptions
    {
        public Uri BaseUri { get; set; }
        public string Token { get; set; }
        public string DocumentContainer { get; set; }
        public string ConnectionString { get; set; }
    }
}
