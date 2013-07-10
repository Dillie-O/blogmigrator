using CookComputing.MetaWeblog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;

namespace BlogMigrator
{
   class Generator
   {
      /// <summary>
      /// Writes the contents of the BlogML document into a WXR formatted file.
      /// </summary>
      /// <param name="BlogData">The blog data.</param>
      /// <param name="PostIds">The posts to migrate.</param>
      /// <param name="BaseUrl">The base URL of the source blog.</param>
      /// <param name="FilePath">The filename.</param>
      public void WriteWXRDocument(BlogML.blogType BlogData, List<int> PostIds,
                                   string BaseUrl, string FilePath)
      {
         BlogML.categoryType currCategory;
         BlogML.categoryRefType currCatRef;
         string categoryName;
         BlogML.commentType currComment;
         BlogML.postType currPost;
         XmlTextWriter writer = new XmlTextWriter(FilePath, new UTF8Encoding(false));
         writer.Formatting = Formatting.Indented;

         writer.WriteStartDocument();
         writer.WriteStartElement("rss");
         writer.WriteAttributeString("version", "2.0");
         writer.WriteAttributeString("xmlns", "content", null, "http://purl.org/rss/1.0/modules/content/");
         writer.WriteAttributeString("xmlns", "wfw", null, "http://wellformedweb.org/CommentAPI/");
         writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
         writer.WriteAttributeString("xmlns", "wp", null, "http://wordpress.org/export/1.0/");

         // Write Blog Info.
         writer.WriteStartElement("channel");
         writer.WriteElementString("title", String.Join(" ", string.Join(" ", BlogData.title.Text)));
         writer.WriteElementString("link", BaseUrl + BlogData.rooturl);
         writer.WriteElementString("description", "Exported Blog");
         writer.WriteElementString("pubDate", BlogData.datecreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
         writer.WriteElementString("generator", "http://wordpress.org/?v=MU");
         writer.WriteElementString("language", "en");
         writer.WriteElementString("wp:wxr_version", "1.0");
         writer.WriteElementString("wp:base_site_url", BlogData.rooturl);
         writer.WriteElementString("wp:base_blog_url", BlogData.rooturl);
           
         // Create tags (currently not in use with BlogML document)
         //for(int i = 0; i <= tagCount - 1; i++)
         //{
         //    writer.WriteStartElement("wp:tag");
         //    writer.WriteElementString("wp:tag_slug", tags[0].ToString().Replace(' ', '-'));
         //    writer.WriteStartElement("wp:tag_name");
         //    writer.WriteCData(tags[0].ToString());
         //    writer.WriteEndElement(); // wp:tag_name
         //    writer.WriteEndElement(); // sp:tag
         //}

         // Create categories
         if (BlogData.categories != null)
         {
               for (int i = 0; i <= BlogData.categories.Length - 1; i++)
               {
                  currCategory = BlogData.categories[i];
                  writer.WriteStartElement("wp:category");
                  writer.WriteElementString("wp:category_nicename", string.Join(" ", currCategory.title.Text).ToLower().Replace(' ', '-'));
                  writer.WriteElementString("wp:category_parent", "");
                  writer.WriteStartElement("wp:cat_name");
                  writer.WriteCData(string.Join(" ", currCategory.title.Text));
                  writer.WriteEndElement(); // wp:cat_name
                  writer.WriteEndElement(); // wp:category
               }
         }

         // TODO: Swap code so that all posts are processed, not just first 5.
         for(int i = 0; i <= BlogData.posts.Length - 1; i++)
         {
            currPost = BlogData.posts[i];

            if(PostIds.Contains(Convert.ToInt32(currPost.id)))
            {
               writer.WriteStartElement("item");
               writer.WriteElementString("title", string.Join(" ", currPost.title.Text));
               writer.WriteElementString("link", BaseUrl + currPost.posturl);
               writer.WriteElementString("pubDate", currPost.datecreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
               writer.WriteStartElement("dc:creator");
               writer.WriteCData(String.Join(" ", BlogData.authors.author.title.Text));
               writer.WriteEndElement(); // dc:creator

               // Post Tags/Categories (currently only categories are implemented with BlogML
               if (currPost.categories != null)
               {
                  for (int j = 0; j <= currPost.categories.Length - 1; j++)
                  {
                     currCatRef = currPost.categories[j];
                     categoryName = GetCategoryById(BlogData, Convert.ToInt32(currCatRef.@ref));
                     writer.WriteStartElement("category");
                     writer.WriteCData(categoryName);
                     writer.WriteEndElement(); // category
                     writer.WriteStartElement("category");
                     writer.WriteAttributeString("domain", "category");
                     writer.WriteAttributeString("nicename", categoryName.ToLower().Replace(' ', '-'));
                     writer.WriteCData(categoryName);
                     writer.WriteEndElement(); // category domain=category
                  }
               }

               writer.WriteStartElement("guid");
               writer.WriteAttributeString("isPermaLink", "false");
               writer.WriteString(" ");
               writer.WriteEndElement(); // guid
               writer.WriteElementString("description", ".");
               writer.WriteStartElement("content:encoded");
               writer.WriteCData(currPost.content.Value);
               writer.WriteEndElement(); // content:encoded
               writer.WriteElementString("wp:post_id", currPost.id);
               writer.WriteElementString("wp:post_date", currPost.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
               writer.WriteElementString("wp:post_date_gmt", currPost.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
               writer.WriteElementString("wp:comment_status", "open");
               writer.WriteElementString("wp:ping_status", "open");
               writer.WriteElementString("wp:post_name", string.Join(" ", currPost.title.Text).ToLower().Replace(' ', '-'));
               writer.WriteElementString("wp:status", "publish");
               writer.WriteElementString("wp:post_parent", "0");
               writer.WriteElementString("wp:menu_order", "0");
               writer.WriteElementString("wp:post_type", "post");
               writer.WriteStartElement("wp:post_password");
               writer.WriteString(" ");
               writer.WriteEndElement(); // wp:post_password

               if (currPost.comments != null)
               {
                  for (int k = 0; k <= currPost.comments.Length - 1; k++)
                  {
                     currComment = currPost.comments[k];
                     writer.WriteStartElement("wp:comment");
                     writer.WriteElementString("wp:comment_date", currComment.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
                     writer.WriteElementString("wp:comment_date_gmt", currComment.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));                                          
                     writer.WriteStartElement("wp:comment_author");
                     if ((!String.IsNullOrEmpty(currComment.useremail)) || (currComment.useremail != "http://"))
                     {
                        writer.WriteCData(currComment.username);
                     }
                     else
                     {
                        writer.WriteCData("Nobody");
                     }
                     writer.WriteEndElement(); // wp:comment_author
                     writer.WriteElementString("wp:comment_author_email", currComment.useremail);
                     writer.WriteElementString("wp:comment_author_url", currComment.userurl);
                     writer.WriteElementString("wp:comment_type", " ");
                     writer.WriteStartElement("wp:comment_content");
                     writer.WriteCData(currComment.content.Value);
                     writer.WriteEndElement(); // wp:comment_content

                     if (currComment.approved)
                     {
                        writer.WriteElementString("wp:comment_approved", null, "1");
                     }
                     else
                     {
                        writer.WriteElementString("wp:comment_approved", null, "0");
                     }

                     writer.WriteElementString("wp", "comment_parent", null, "0");
                     writer.WriteEndElement(); // wp:comment
                  }
               }

               writer.WriteEndElement(); // item
            }
                
         }

         writer.WriteEndElement(); // channel
         writer.WriteEndElement(); // rss

         writer.Flush();
         writer.Close();
      }

      /// <summary>
      /// Writes the contents of the BlogML document into a WXR formatted file.
      /// </summary>
      /// <param name="BlogPosts">The blog data.</param>
      /// <param name="BlogIds">The blog entries to migrate.</param>
      /// <param name="BaseUrl">The base URL of the source blog.</param>
      /// <param name="FilePath">The filename.</param>
      public void WriteWXRDocument(List<Post> BlogPosts, List<int> BlogIds,
                                   string BaseUrl, string FilePath)
      {
         //BlogML.categoryType currCategory;
         //BlogML.categoryRefType currCatRef;
         //string categoryName;
         //BlogML.commentType currComment;
         //BlogML.postType currPost;
         //XmlTextWriter writer = new XmlTextWriter(FilePath, new UTF8Encoding(false));
         //writer.Formatting = Formatting.Indented;

         //writer.WriteStartDocument();
         //writer.WriteStartElement("rss");
         //writer.WriteAttributeString("version", "2.0");
         //writer.WriteAttributeString("xmlns", "content", null, "http://purl.org/rss/1.0/modules/content/");
         //writer.WriteAttributeString("xmlns", "wfw", null, "http://wellformedweb.org/CommentAPI/");
         //writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
         //writer.WriteAttributeString("xmlns", "wp", null, "http://wordpress.org/export/1.0/");

         //// Write Blog Info.
         //writer.WriteStartElement("channel");
         //writer.WriteElementString("title", String.Join(" ", string.Join(" ", BlogData.title.Text)));
         //writer.WriteElementString("link", BaseUrl + BlogData.rooturl);
         //writer.WriteElementString("description", "Exported Blog");
         //writer.WriteElementString("pubDate", BlogData.datecreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
         //writer.WriteElementString("generator", "http://wordpress.org/?v=MU");
         //writer.WriteElementString("language", "en");
         //writer.WriteElementString("wp:wxr_version", "1.0");
         //writer.WriteElementString("wp:base_site_url", BlogData.rooturl);
         //writer.WriteElementString("wp:base_blog_url", BlogData.rooturl);

         //// Create tags (currently not in use with BlogML document)
         ////for(int i = 0; i <= tagCount - 1; i++)
         ////{
         ////    writer.WriteStartElement("wp:tag");
         ////    writer.WriteElementString("wp:tag_slug", tags[0].ToString().Replace(' ', '-'));
         ////    writer.WriteStartElement("wp:tag_name");
         ////    writer.WriteCData(tags[0].ToString());
         ////    writer.WriteEndElement(); // wp:tag_name
         ////    writer.WriteEndElement(); // sp:tag
         ////}

         //// Create categories
         //if (BlogData.categories != null)
         //{
         //   for (int i = 0; i <= BlogData.categories.Length - 1; i++)
         //   {
         //      currCategory = BlogData.categories[i];
         //      writer.WriteStartElement("wp:category");
         //      writer.WriteElementString("wp:category_nicename", string.Join(" ", currCategory.title.Text).ToLower().Replace(' ', '-'));
         //      writer.WriteElementString("wp:category_parent", "");
         //      writer.WriteStartElement("wp:cat_name");
         //      writer.WriteCData(string.Join(" ", currCategory.title.Text));
         //      writer.WriteEndElement(); // wp:cat_name
         //      writer.WriteEndElement(); // wp:category
         //   }
         //}

         //// TODO: Swap code so that all posts are processed, not just first 5.
         //for (int i = 0; i <= BlogData.posts.Length - 1; i++)
         //{
         //   currPost = BlogData.posts[i];

         //   if (BlogIds.Contains(Convert.ToInt32(currPost.id)))
         //   {
         //      writer.WriteStartElement("item");
         //      writer.WriteElementString("title", string.Join(" ", currPost.title.Text));
         //      writer.WriteElementString("link", BaseUrl + currPost.posturl);
         //      writer.WriteElementString("pubDate", currPost.datecreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
         //      writer.WriteStartElement("dc:creator");
         //      writer.WriteCData(String.Join(" ", BlogData.authors.author.title.Text));
         //      writer.WriteEndElement(); // dc:creator

         //      // Post Tags/Categories (currently only categories are implemented with BlogML
         //      if (currPost.categories != null)
         //      {
         //         for (int j = 0; j <= currPost.categories.Length - 1; j++)
         //         {
         //            currCatRef = currPost.categories[j];
         //            categoryName = GetCategoryById(BlogData, Convert.ToInt32(currCatRef.@ref));
         //            writer.WriteStartElement("category");
         //            writer.WriteCData(categoryName);
         //            writer.WriteEndElement(); // category
         //            writer.WriteStartElement("category");
         //            writer.WriteAttributeString("domain", "category");
         //            writer.WriteAttributeString("nicename", categoryName.ToLower().Replace(' ', '-'));
         //            writer.WriteCData(categoryName);
         //            writer.WriteEndElement(); // category domain=category
         //         }
         //      }

         //      writer.WriteStartElement("guid");
         //      writer.WriteAttributeString("isPermaLink", "false");
         //      writer.WriteString(" ");
         //      writer.WriteEndElement(); // guid
         //      writer.WriteElementString("description", ".");
         //      writer.WriteStartElement("content:encoded");
         //      writer.WriteCData(currPost.content.Value);
         //      writer.WriteEndElement(); // content:encoded
         //      writer.WriteElementString("wp:post_id", currPost.id);
         //      writer.WriteElementString("wp:post_date", currPost.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
         //      writer.WriteElementString("wp:post_date_gmt", currPost.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
         //      writer.WriteElementString("wp:comment_status", "open");
         //      writer.WriteElementString("wp:ping_status", "open");
         //      writer.WriteElementString("wp:post_name", string.Join(" ", currPost.title.Text).ToLower().Replace(' ', '-'));
         //      writer.WriteElementString("wp:status", "publish");
         //      writer.WriteElementString("wp:post_parent", "0");
         //      writer.WriteElementString("wp:menu_order", "0");
         //      writer.WriteElementString("wp:post_type", "post");
         //      writer.WriteStartElement("wp:post_password");
         //      writer.WriteString(" ");
         //      writer.WriteEndElement(); // wp:post_password

         //      if (currPost.comments != null)
         //      {
         //         for (int k = 0; k <= currPost.comments.Length - 1; k++)
         //         {
         //            currComment = currPost.comments[k];
         //            writer.WriteStartElement("wp:comment");
         //            writer.WriteElementString("wp:comment_date", currComment.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
         //            writer.WriteElementString("wp:comment_date_gmt", currComment.datecreated.ToString("yyyy-MM-dd HH:mm:ss"));
         //            writer.WriteStartElement("wp:comment_author");
         //            if ((!String.IsNullOrEmpty(currComment.useremail)) || (currComment.useremail != "http://"))
         //            {
         //               writer.WriteCData(currComment.username);
         //            }
         //            else
         //            {
         //               writer.WriteCData("Nobody");
         //            }
         //            writer.WriteEndElement(); // wp:comment_author
         //            writer.WriteElementString("wp:comment_author_email", currComment.useremail);
         //            writer.WriteElementString("wp:comment_author_url", currComment.userurl);
         //            writer.WriteElementString("wp:comment_type", " ");
         //            writer.WriteStartElement("wp:comment_content");
         //            writer.WriteCData(currComment.content.Value);
         //            writer.WriteEndElement(); // wp:comment_content

         //            if (currComment.approved)
         //            {
         //               writer.WriteElementString("wp:comment_approved", null, "1");
         //            }
         //            else
         //            {
         //               writer.WriteElementString("wp:comment_approved", null, "0");
         //            }

         //            writer.WriteElementString("wp", "comment_parent", null, "0");
         //            writer.WriteEndElement(); // wp:comment
         //         }
         //      }

         //      writer.WriteEndElement(); // item
         //   }

         //}

         //writer.WriteEndElement(); // channel
         //writer.WriteEndElement(); // rss

         //writer.Flush();
         //writer.Close();
      }      

      public string GetCategoryById(BlogML.blogType BlogData, int CategoryId)
      {
         string results = "none";

         for(int i = 0; i <= BlogData.categories.Length - 1; i++)
         {
               if (BlogData.categories[i].id == CategoryId.ToString())
               {
                  results = String.Join(" ", BlogData.categories[i].title.Text);
                  break;
               }
         }

         return results;
      }
   }
}
