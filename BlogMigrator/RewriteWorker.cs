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
   public partial class RewriteSourcePosts : Window
   {
      /// <summary>
        /// Updates log text with progress message.
        /// </summary>
        /// <param name="sender">Rewrite ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/16/2010   [Created]
        /// </history>
        private void rewriteWorker_ProgressChanged
                     (object sender, ProgressChangedEventArgs e)
        {
            UpdateStatusText(((WorkerArgs)e.UserState).status);
        }

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">
        /// Rewrite worker RunWorkerCompleted event.
        /// </param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>
        /// Sean Patterson    11/16/2010   [Created]
        /// </history>
        private void rewriteWorker_RunWorkerCompleted
                     (object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerArgs myArgs = (WorkerArgs)e.Result;
            StringBuilder ProgressText = new StringBuilder();

            ProgressText.AppendLine(txtStatus.Text);

            if (e.Cancelled)
            {
                UpdateStatusText("Rewrite cancelled.");
            }
            else if (e.Error != null)
            {
                UpdateStatusText("Error with rewrite. Process halted.");
            }
            else
            {
                UpdateStatusText("Rewrite complete.");
            }
        }

        /// <summary>
        /// Runs the rewrite process as a background thread.
        /// </summary>
        /// <param name="sender">Rewrite DoWork dvent.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>
        /// Sean Patterson    11/16/2010   [Created]
        /// </history>
        private void rewriteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
           WorkerArgs myArgs = (WorkerArgs)e.Argument;
           Services myService = new Services();
           Post origPost;
           Post updatePost;
                      
            myArgs.status = "Starting rewrite process...";
            rewriteWorker.ReportProgress(0, myArgs);

            foreach(LogData logItem in App.itemsToRewrite)
            {
               origPost = myService.GetPost
                                    (App.rewriteBlog.serviceUrl, logItem.sourceId, 
                                     App.rewriteBlog.username, App.rewriteBlog.password);

               myArgs.status = "Rewriting post: " + origPost.title;
               rewriteWorker.ReportProgress(15, myArgs);
  
               updatePost = origPost;
               string newUrl = "<a href='" + logItem.destinationUrl + "'>" + logItem.destinationUrl + "</a>";
               string newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
               updatePost.description = newMessage;

               myService.UpdatePost(App.rewriteBlog.serviceUrl,
                                    App.rewriteBlog.username,
                                    App.rewriteBlog.password,
                                    updatePost);
            }
        }                      
   }
}
