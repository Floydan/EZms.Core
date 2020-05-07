using System;
using System.Collections.Generic;
using System.Text;
using EZms.Core.Middleware.Models;

namespace EZms.Core.Models
{
    public class MiddlewareSeeds
    {
        public MiddlewareSeeds()
        {
            Contents = new List<MiddlewareSeed>();
        }

        public IList<MiddlewareSeed> Contents { get; set; }
    }
}
