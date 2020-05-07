using System.Collections.Generic;
using EZms.Core.Models;

namespace EZms.Core.Middleware.Models
{
    public class MiddlewareSeed : Content
    {
        public IList<MiddlewareSeed> Children { get; set; }
    }
}
