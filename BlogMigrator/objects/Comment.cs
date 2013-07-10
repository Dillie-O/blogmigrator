using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogMigrator
{
    /// <summary>
    /// This class represents a post comment.
    /// </summary>
    class Comment
    {
        public String author { get; set; }
        public String email { get; set; }
        public String url { get; set; }
        public String text { get; set; }
        public String publishDate { get; set; }
    }
}
