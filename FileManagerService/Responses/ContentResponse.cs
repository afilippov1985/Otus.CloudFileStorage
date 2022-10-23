using Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace FileManagerService.Responses
{
    public class ContentResponse
    {
        public Result Result { get; set; }

        public IEnumerable<object> Directories { get; set; }

        public IEnumerable<object> Files { get; set; }
    }
}
