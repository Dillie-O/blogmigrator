using BlogML;
using CookComputing.XmlRpc;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlogMigrator
{
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
      public ObservableCollection<PostData> PostCollection =
                           new ObservableCollection<PostData>();

      BackgroundWorker allPostsWorker = new BackgroundWorker();
      BackgroundWorker migrationWorker = new BackgroundWorker();
        
      public MainWindow()
	{
		this.InitializeComponent();

         allPostsWorker.WorkerReportsProgress = true;
         allPostsWorker.WorkerSupportsCancellation = true;
         allPostsWorker.DoWork += allPostsWorker_DoWork;
         allPostsWorker.ProgressChanged += allPostsWorker_ProgressChanged;
         allPostsWorker.RunWorkerCompleted += allPostsWorker_RunWorkerCompleted;
            
         migrationWorker.WorkerReportsProgress = true;
         migrationWorker.WorkerSupportsCancellation = true;
         migrationWorker.DoWork += migrationWorker_DoWork;
         migrationWorker.ProgressChanged += migrationWorker_ProgressChanged;
         migrationWorker.RunWorkerCompleted += migrationWorker_RunWorkerCompleted;
	}

      /// <summary>
      /// Exits the application.
      /// </summary>
      /// <param name="sender">Quit menu click event.</param>
      /// <param name="e">Click event arguments.</param>
      /// <history>
      /// Sean Patterson    11/4/2010   [Created]
      /// </history>
      private void mnuFileQuit_Click(object sender, RoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      /// <summary>
      /// Displays the about window.
      /// </summary>
      /// <param name="sender">About menu click event.</param>
      /// <param name="e">Click event arguments.</param>
      /// <history>
      /// Sean Patterson    11/4/2010   [Created]
      /// </history>
      private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
      {
         AboutWindow about = new AboutWindow();
         about.ShowDialog();
      }

      /// <summary>
      /// Handles btnGetAllPosts click event.
      /// </summary>
      /// <param name="sender">Get All Posts button click event.</param>
      /// <param name="e">Click event arguments.</param>
      /// <history>
      /// Sean Patterson    11/4/2010   [Created]
      /// </history>
      private void btnGetAllPosts_Click(object sender, RoutedEventArgs e)
      {
         WorkerArgs myArgs = new WorkerArgs();
         myArgs.processToRun = "getallposts";
         myArgs.status = "Get all posts action selected.";
                
         try
         {
               allPostsWorker.RunWorkerAsync(myArgs);                                
         }
         catch (XmlRpcFaultException fex)
         {
               // Flush out old records to prevent accidental writes.
               if (App.sourceBlog.blogPosts.Count > 0)
               {
                  App.sourceBlog.blogPosts.Clear();
               }

               App.sourceBlog.blogData = null;

               if (PostCollection.Count > 0)
               {
                  PostCollection.Clear();
               }
                                
               lblEntriesCount.Content = "[0 Total]";

               MessageBox.Show("XML-RPC error migrating posts: " +
                              Environment.NewLine + Environment.NewLine +
                              fex.ToString() + "Please check your settings and " +
                              "try again.", "Migration Result",
                              MessageBoxButton.OK, MessageBoxImage.Error);
         }
         catch (Exception ex)
         {
               // Flush out old records to prevent accidental writes.
               if (App.sourceBlog.blogPosts.Count > 0)
               {
                  App.sourceBlog.blogPosts.Clear();
               }

               App.sourceBlog.blogData = null;

               if (PostCollection.Count > 0)
               {
                  PostCollection.Clear();
               }

               lblEntriesCount.Content = "[0 Total]";

               MessageBox.Show("General error migrating posts: " +
                              Environment.NewLine + Environment.NewLine +
                              ex.ToString() + "Please check your settings and " +
                              "try again.", "Migration Result",
                              MessageBoxButton.OK, MessageBoxImage.Information);
         }                                    
      }               

      /// <summary>
      /// Migrates the posts from the source to destination server.
      /// </summary>
      /// <param name="sender">Migrate button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson    11/4/2010   [Created]
      /// </history>
      private void btnMigrate_Click(object sender, RoutedEventArgs e)
      {
         WorkerArgs myArgs = new WorkerArgs();
         myArgs.processToRun = "migrateposts";
         myArgs.status = "Migrate posts action selected.";

         try
         {
            App.sourceBlog.postsToMigrate.Clear();
            
            foreach (PostData item in lsvAllPosts.SelectedItems)
            {
               App.sourceBlog.postsToMigrate.Add(item.postid);
            }

            if (chkUpdateSource.IsChecked == true)
            {
               App.rewritePosts = true;
               App.rewriteMessage = txtUpdateSource.Text;
            }
            else
            {
               App.rewritePosts = false;
               App.rewriteMessage = null;
            }

            if (App.sourceBlog.postsToMigrate.Count > 0)
            {
               migrationWorker.RunWorkerAsync(myArgs);
            }
            else
            {
               MessageBox.Show("Please specify at least one post to migrate.", 
                               "No Posts Specified.", MessageBoxButton.OK, 
                               MessageBoxImage.Exclamation);
            }
         }
         catch (XmlRpcFaultException fex)
         {
               MessageBox.Show("XML-RPC error migrating posts: " +
                               Environment.NewLine + Environment.NewLine + 
                               fex.ToString() + "Please check your settings and " +
                               "try again.", "Migration Result", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
         }
         catch (Exception ex)
         {
            MessageBox.Show("General error migrating posts: " +
                            Environment.NewLine + Environment.NewLine +
                            ex.ToString() + "Please check your settings and " +
                            "try again.", "Migration Result",
                            MessageBoxButton.OK, MessageBoxImage.Information);
         }
      }

      /// <summary>
      /// Updates the status TextBox.
      /// </summary>
      /// <param name="Message">The message to add.</param>
      /// /// <history>
      /// Sean Patterson    11/6/2010   [Created]
      /// </history>
      public void UpdateStatusText(string message)
      {
         StringBuilder ProgressText = new StringBuilder();
         ProgressText.AppendLine(txtStatus.Text);
         ProgressText.AppendLine(message);
         txtStatus.Text = ProgressText.ToString();
         txtStatus.ScrollToLine(txtStatus.LineCount - 1);
      }

      /// <summary>
      /// Updates the status bar.
      /// </summary>
      /// <param name="Message">The message to add.</param>
      /// /// <history>
      /// Sean Patterson    11/6/2010   [Created]
      /// </history>
      public void UpdateStatusBar(string message)
      {
         StatusBarMessage.Content = message;
      }

      /// <summary>
      /// Displays the configure source window.
      /// </summary>
      /// <param name="sender">Configure source button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnConfigureSource_Click(object sender, RoutedEventArgs e)
      {
         ConfigureSource configWindow = new ConfigureSource();
         configWindow.Owner = this;
         configWindow.ShowDialog();
      }

      /// <summary>
      /// Displays the configure destination window.
      /// </summary>
      /// <param name="sender">Configure source button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnConfigureDestination_Click(object sender, RoutedEventArgs e)
      {
         ConfigureDestination configWindow = new ConfigureDestination();
         configWindow.Owner = this;
         configWindow.ShowDialog();
      }

   /// <summary>
   /// Selects all posts in the ListView
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private void btnSelectAllPosts_Click(object sender, RoutedEventArgs e)
   {
      lsvAllPosts.SelectAll();
   }

   private void mnuToolsRewrite_Click(object sender, RoutedEventArgs e)
   {
      RewriteSourcePosts rewriteWindow = new RewriteSourcePosts();
      rewriteWindow.Owner = this;
      rewriteWindow.ShowDialog();
   }        
}
}