using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CookComputing.MetaWeblog;

namespace BlogMigrator
{
    /// <summary>
    /// The PostData class is a sortable version of the Post class returned by
    /// the XML-RPC service. An additional boolean field is added to indicate 
    /// if the post has been selected to migration.
    /// </summary>
    public class PostData
    {
        public DateTime dateCreated { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string[] categories { get; set; }
        public Enclosure enclosure { get; set; }
        public string link { get; set; }
        public string permalink { get; set; }
        public int postid { get; set; }
        public Source source { get; set; }
        public string userid { get; set; }
        public object mt_allow_comments { get; set; }
        public object mt_allow_pings { get; set; }
        public object mt_convert_breaks { get; set; }
        public string mt_text_more { get; set; }
        public string mt_excerpt { get; set; }
        public bool isSelected { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PostData()
        {
        }

        /// <summary>
        /// Constructor that copies the members from MetaWeblog post object.
        /// </summary>
        /// <param name="post"></param>
        public PostData(CookComputing.MetaWeblog.Post post)
        {
            dateCreated = post.dateCreated;
            description = post.description;
            title = post.title;
            categories = post.categories;
            enclosure = post.enclosure;
            link = post.link;
            permalink = post.permalink;
            postid = Convert.ToInt32(post.postid);
            source = post.source;
            userid = post.userid;
            mt_allow_comments = post.mt_allow_comments;
            mt_allow_pings = post.mt_allow_pings;
            mt_convert_breaks = post.mt_convert_breaks;
            mt_text_more = post.mt_text_more;
            mt_excerpt = post.mt_excerpt; 
        }

        /// <summary>
        /// Constructor that copies the members from BlogML post object.
        /// </summary>
        /// <param name="post"></param>
        public PostData(BlogML.postType post)
        {
            //dateCreated = post.datecreated;
            //description = post.content.Value;
            title = string.Join(" ", post.title.Text);
            //categories = post.categories;
            //enclosure = post.enclosure;
            //link = post.posturl;
            //permalink = "";
            postid = Convert.ToInt32(post.id);
            //source = new Source();
            //userid = post.authors.author.ToString();
            //mt_allow_comments = 1;
            //mt_allow_pings = 1;
            //mt_convert_breaks = 0;
            //mt_text_more = "";
            //if (post.excerpt.Value.Length > 0)
            //{
            //    mt_excerpt = String.Join(" ", post.excerpt.Value);
            //}
        }
    }
}
