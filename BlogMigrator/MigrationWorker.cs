using CookComputing.MetaWeblog;
using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BlogMigrator
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Updates log text with progress message.
        /// </summary>
        /// <param name="sender">Conversion ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void migrationWorker_ProgressChanged
                     (object sender, ProgressChangedEventArgs e)
        {
            UpdateStatusText(((WorkerArgs)e.UserState).status);
        }

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">
        /// ConversionWorker RunWorkerCompleted event.
        /// </param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void migrationWorker_RunWorkerCompleted
                     (object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerArgs myArgs = (WorkerArgs)e.Result;
            StringBuilder ProgressText = new StringBuilder();

            ProgressText.AppendLine(txtStatus.Text);

            if (e.Cancelled)
            {
                UpdateStatusText("Migration cancelled.");
                UpdateStatusBar("Migration cancelled.");
            }
            else if (e.Error != null)
            {
                UpdateStatusText("Error with migration. Process halted.");
                UpdateStatusBar("Migration halted.");
            }
            else
            {
               // Use an observable collection to properly bind/update the
               // ListView
               PostCollection.Clear();

                if (App.sourceBlog.blogPosts.Count > 0)
                {
                    foreach (Post postItem in App.sourceBlog.blogPosts)
                    {
                        PostData myPost = new PostData(postItem);
                        PostCollection.Add(myPost);
                    }
                }
                else
                {
                    for (int i = 0; i <= App.sourceBlog.blogData.posts.Length - 1; i++)
                    {
                        PostData myPost = new PostData(App.sourceBlog.blogData.posts[i]);
                        PostCollection.Add(myPost);
                    }
                }

                lsvAllPosts.ItemsSource = PostCollection;
                lblEntriesCount.Content = "[" + PostCollection.Count + " Total]";

                UpdateStatusText("Process complete.");
                UpdateStatusBar("Process complete.");
            }
        }

        /// <summary>
        /// Runs the conversion process as a background thread.
        /// </summary>
        /// <param name="sender">Conversion DoWork dvent.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void migrationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs myArgs = (WorkerArgs)e.Argument;

            Generator myGenerator = new Generator();
            Services myService = new Services();

            // Load document.
            myArgs.status = "Starting migration process...";
            migrationWorker.ReportProgress(0, myArgs);

            switch (DetermineMigrationMethod())
            {
               case "XMLtoXML":
                  WriteXMLPosts();
                  break;

               case "FILEtoXML":
                  ImportXMLPosts();
                  break;

               case "XMLtoWXR":
                  myGenerator.WriteWXRDocument(App.sourceBlog.blogPosts,
                                               App.sourceBlog.postsToMigrate,
                                               App.sourceBlog.rootUrl,
                                               App.destBlog.blogFile);
                  break;

               case "FILEtoWXR":
                  myGenerator.WriteWXRDocument(App.sourceBlog.blogData,
                                               App.sourceBlog.postsToMigrate,
                                               App.sourceBlog.rootUrl,
                                               App.destBlog.blogFile);
                  break;

               default:
                  throw new Exception("No migration method found.");                                               
            }

            myArgs.status = "Migration process complete.";
            migrationWorker.ReportProgress(100, myArgs);
        }

        /// <summary>
        /// Publishes posts to the destination blog.
        /// </summary>
        /// <history>
        /// Sean Patterson   11/15/2010   [Created]
        /// </history>
        public void WriteXMLPosts()
        {
           Services blogService = new Services();
           Post resultPost;
           StreamWriter swLog;
           WorkerArgs args = new WorkerArgs();

           string LogFile = App.sourceBlog.serviceType + "_" + 
                            App.destBlog.serviceType + "_Migration-" +
                            DateTime.Now.ToString("yyyy_MM_dd_hhMMss") + ".csv";

           // Load document.
           swLog = new StreamWriter(LogFile);
           swLog.WriteLine("Source Id, Source Link, Destination Id, " +
                           "Destination Link");

           swLog.Flush();

           try
           {
              args.status = "Migrating posts from " + App.sourceBlog.serviceType +
                              " to " + App.destBlog.serviceType;
              migrationWorker.ReportProgress(15, args);

              if (App.rewritePosts)
              {
                 args.status = "Rewriting original posts ENABLED.";
                 migrationWorker.ReportProgress(15, args);
              }
              else
              {
                 args.status = "Rewriting original posts DISABLED.";
                 migrationWorker.ReportProgress(15, args);
              }
              
              foreach (Post blogPost in App.sourceBlog.blogPosts)
              {
                
                 if (App.sourceBlog.postsToMigrate.
                         Contains(Convert.ToInt32(blogPost.postid)))
                 {
                    args.status = "Writing Post: " + blogPost.title;
                    migrationWorker.ReportProgress(20, args);

                    resultPost = blogService.InsertPost
                                             (App.destBlog.serviceUrl, App.destBlog.blogId,
                                              App.destBlog.username, App.destBlog.password,
                                              blogPost);

                    swLog.WriteLine(blogPost.postid.ToString() + "," + blogPost.link + "," +
                                    resultPost.postid.ToString() + "," + resultPost.link);

                    swLog.Flush();

                    if (App.rewritePosts)
                    {
                       Post updatePost = blogPost;
                       string newUrl = "<a href='" + resultPost.link + "'>" + resultPost.link + "</a>";
                       string newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
                       updatePost.description = newMessage;

                       blogService.UpdatePost(App.sourceBlog.serviceUrl, 
                                              App.sourceBlog.username, 
                                              App.sourceBlog.password, 
                                              updatePost);
                    }
                 }
              }
              swLog.Close();              
           }
           catch (Exception ex)
           {
              swLog.Flush();
              swLog.Close();
              MessageBox.Show("An error occurred migrating blog posts:" +
                              Environment.NewLine + Environment.NewLine +
                              ex.ToString() +
                              Environment.NewLine + Environment.NewLine +
                              "Please verify your settings and try migrating " +
                              "posts again.", "Error Migrating Posts",
                              MessageBoxButton.OK, MessageBoxImage.Error);

              migrationWorker.CancelAsync();
           }           
        }

        /// <summary>
        /// Publishes posts to the destination blog from a BlogML object.
        /// </summary>
        /// <history>
        /// Sean Patterson   11/15/2010   [Created]
        /// </history>
        public void ImportXMLPosts()
        {
           Services blogService = new Services();
           Post resultPost;
           Post newPost;
           StreamWriter swLog;
           Generator myGenerator = new Generator();
           BlogML.categoryRefType currCatRef;
           string categoryName;
           BlogML.postType currPost;
           List<string> categoryList;
           WorkerArgs args = new WorkerArgs();

           string LogFile = App.sourceBlog.serviceType + "_" +
                            App.destBlog.serviceType + "_Migration-" +
                            DateTime.Now.ToString("yyyy_MM_dd_hhMMss") + ".csv";

           // Load document.
           swLog = new StreamWriter(LogFile);
           swLog.WriteLine("Source Id, Source Link, Destination Id, " +
                           "Destination Link");

           swLog.Flush();

           try
           {
              args.status = "Migrating posts from " + App.sourceBlog.serviceType +
                            " to " + App.destBlog.serviceType;
              migrationWorker.ReportProgress(15, args);

              for (int i = 0; i <= App.sourceBlog.blogData.posts.Length - 1; i++)
              {
                 currPost = App.sourceBlog.blogData.posts[i];

                 if (App.sourceBlog.postsToMigrate.Contains(Convert.ToInt32(currPost.id)))
                 {
                    args.status = "Writing Post: " + string.Join(" ", currPost.title.Text);
                    migrationWorker.ReportProgress(20, args);

                    newPost = new Post();
                    newPost.title = string.Join(" ", currPost.title.Text);
                    newPost.dateCreated = currPost.datecreated;
                    newPost.userid = App.destBlog.username;
                    newPost.postid = currPost.id;
                    newPost.description = currPost.content.Value;
                    newPost.link = App.sourceBlog.rootUrl + currPost.posturl;

                    // Post Tags/Categories (currently only categories are implemented with BlogML
                    if (currPost.categories != null)
                    {
                       categoryList = new List<string>();

                       for (int j = 0; j <= currPost.categories.Length - 1; j++)
                       {
                          currCatRef = currPost.categories[j];
                          categoryName = myGenerator.GetCategoryById
                                                     (App.sourceBlog.blogData,
                                                      Convert.ToInt32(currCatRef.@ref));
                          categoryList.Add(categoryName);
                       }

                       newPost.categories = categoryList.ToArray();
                    }

                    resultPost = blogService.InsertPost
                                               (App.destBlog.serviceUrl, App.destBlog.blogId,
                                                App.destBlog.username, App.destBlog.password,
                                                newPost);

                    swLog.WriteLine(newPost.postid.ToString() + "," + newPost.link + "," +
                                    resultPost.postid.ToString() + "," + resultPost.link);

                    swLog.Flush();

                    // Rewrite posts can still be done "live" even if a BlogML 
                    // file is being imported provided the serviceUrl details 
                    // are provided.
                    if (App.rewritePosts)
                    {
                       Post updatePost = newPost;
                       string newUrl = "<a href='" + resultPost.link + "'>" + resultPost.link + "</a>";
                       string newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
                       updatePost.description = newMessage;

                       blogService.UpdatePost(App.sourceBlog.serviceUrl,
                                              App.sourceBlog.username,
                                              App.sourceBlog.password,
                                              updatePost);
                    }
                 }
              }
              
              swLog.Close();
           }
           catch (Exception ex)
           {
              swLog.Flush();
              swLog.Close();
              MessageBox.Show("An error occurred migrating blog posts:" +
                              Environment.NewLine + Environment.NewLine +
                              ex.ToString() +
                              Environment.NewLine + Environment.NewLine +
                              "Please verify your settings and try migrating " +
                              "posts again.", "Error Migrating Posts",
                              MessageBoxButton.OK, MessageBoxImage.Error);

              migrationWorker.CancelAsync();
           }
        }

       /// <summary>
       /// Determines which method of migration to use based on the source and
       /// destination variables specified.
       /// </summary>
       /// <returns>Migration method code.</returns>
       /// <history>
       /// Sean Patterson   11/10/2010   [Created]
       /// </history>
        private string DetermineMigrationMethod()
        {
           string results = "";

           if (App.sourceBlog.serviceType != "FILE" && App.destBlog.serviceType != "FILE")
           {
              results = "XMLtoXML";
           }

           if (App.sourceBlog.serviceType == "FILE" && App.destBlog.serviceType != "FILE")
           {
              results = "FILEtoXML";
           }

           if (App.sourceBlog.serviceType != "FILE" && App.destBlog.serviceType == "FILE")
           {
              results = "XMLtoWXR";
           }

           if (App.sourceBlog.serviceType == "FILE" && App.destBlog.serviceType == "FILE")
           {
              results = "FILEtoWXR";
           }

           return results;
        }
    }
}
