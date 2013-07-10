using CookComputing.MetaWeblog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BlogMigrator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{       
      public static BlogSource sourceBlog = new BlogSource();
      public static BlogSource destBlog = new BlogSource();
      public static BlogSource rewriteBlog = new BlogSource();
      public static List<LogData> itemsToRewrite = new List<LogData>();
      public static bool rewritePosts;
      public static string rewriteMessage;
	}
}