using CookComputing.MetaWeblog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogMigrator
{
   public class BlogSource
   {
      public string serviceType { get; set; }
      public string serviceUrl { get; set; }
      public string rootUrl { get; set; }
      public string blogId { get; set; }
      public string username { get; set; }
      public string password { get; set; }
      public string blogFile { get; set; }
      public BlogML.blogType blogData { get; set; }
      public List<Post> blogPosts { get; set; }
      public List<int> postsToMigrate { get; set; }

      public BlogSource()
      {
         blogPosts = new List<Post>();
         postsToMigrate = new List<int>();
      }
   }
}
