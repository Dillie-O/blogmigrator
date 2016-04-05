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
        /// <param name="sender">ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void allPostsWorker_ProgressChanged
                     (object sender, ProgressChangedEventArgs e)
        {           
            UpdateStatusText(((WorkerArgs)e.UserState).status); 
        }

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">
        /// RunWorkerCompleted event.
        /// </param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void allPostsWorker_RunWorkerCompleted
                     (object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerArgs myArgs = (WorkerArgs)e.Result;                        
            StringBuilder ProgressText = new StringBuilder();
            
            ProgressText.AppendLine(txtStatus.Text);

            if (e.Cancelled)
            {
                UpdateStatusText("Process cancelled.");
                UpdateStatusBar("Process cancelled.");
            }
            else if (e.Error != null)
            {
                UpdateStatusText("Error with process. Process halted.");
                UpdateStatusBar("Process halted.");
            }
            else
            {
               // Use an observable collection to properly bind/update the
               // ListView
               PostCollection.Clear();

               if ((App.sourceBlog.blogPosts.Count) > 0 || (App.sourceBlog.blogData != null))
               {
                  if (App.sourceBlog.blogPosts.Count > 0)
                  {
                     foreach (Post postItem in App.sourceBlog.blogPosts)
                     {
                        PostData myPost = new PostData(postItem);
                        PostCollection.Add(myPost);
                     }

                     btnMigrate.IsEnabled = true;
                     btnSelectAllPosts.IsEnabled = true;
                  }
                  else
                  {
                    for (int i = 0; i <= App.sourceBlog.blogData.posts.Length - 1; i++)
                     {
                        PostData myPost = new PostData(App.sourceBlog.blogData.posts[i]);
                        PostCollection.Add(myPost);
                     }

                     btnMigrate.IsEnabled = true;
                     btnSelectAllPosts.IsEnabled = true;                    
                  }
               }               
                
               lsvAllPosts.ItemsSource = PostCollection;
               lblEntriesCount.Content = "[" + PostCollection.Count + " Total]"; 
                
               UpdateStatusText("Process complete.");
               UpdateStatusBar("Process complete.");
            }
        }

        /// <summary>
        /// Runs the get all posts process as a background thread.
        /// </summary>
        /// <param name="sender">DoWork event.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/1/2010   [Created]
        /// </history>
        private void allPostsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs myArgs = (WorkerArgs)e.Argument;
            
            Services myService = new Services();
            BlogSource myBlog = App.sourceBlog;

            myArgs.status = "Retrieving post from source...";
            allPostsWorker.ReportProgress(0, myArgs);

            try
            {
               // Clear out any old blog entries.
               App.sourceBlog.blogData = new BlogML.blogType();
               App.sourceBlog.blogPosts = new List<Post>();
               App.sourceBlog.postsToMigrate = new List<int>();
               
               // Retrieve data via XML-RPC unless a source file has been 
                // specified. 
                if (String.IsNullOrEmpty(myBlog.blogFile))
                {
                    myArgs.status = "Connecting to source...";
                    allPostsWorker.ReportProgress(10, myArgs);
                    myBlog.blogPosts = myService.GetAllPosts
                                                 (myBlog.serviceUrl, myBlog.blogId, 
                                                  myBlog.username, myBlog.password);
                }
                else
                {
                    myArgs.status = "Parsing file for posts...";
                    allPostsWorker.ReportProgress(10, myArgs);

                    XmlSerializer serializer = new XmlSerializer
                                                   (typeof(BlogML.blogType));
                    TextReader reader = new StreamReader(myBlog.blogFile);

                    myBlog.blogData = (BlogML.blogType)
                                      serializer.Deserialize(reader);
                    reader.Close();

                    myArgs.status = "Posts retrieved.";
                    allPostsWorker.ReportProgress(100, myArgs);
                }
            }
            catch (Exception ex)
            {
              MessageBox.Show("An error occurred retrieving blog posts:" + 
                              Environment.NewLine + Environment.NewLine + 
                              ex.ToString() + 
                              Environment.NewLine + Environment.NewLine + 
                              "Please verify your settings and try retrieving " + 
                              "posts again.", "Error Retrieving Posts", 
                              MessageBoxButton.OK, MessageBoxImage.Error);

              allPostsWorker.CancelAsync();
            }            
        }
    }
}
