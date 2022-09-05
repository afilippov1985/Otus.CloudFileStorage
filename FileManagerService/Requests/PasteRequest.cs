using System.Collections.Generic;

namespace FileManagerService.Requests
{
    public class PasteRequest
    {
        public string Disk { get; set; }

        public string? Path { get; set; }

        public ClipboardObject Clipboard { get; set; }

        public class ClipboardObject
        {
            public ClipboardType Type { get; set; }

            public string Disk { get; set; }

            // список папок для архивирования
            public IList<string> Directories { get; set; }

            // списко файлов для архивирования
            public IList<string> Files { get; set; }

            public enum ClipboardType
            {
                Copy,
                Cut,
            }
        }
    }
}
