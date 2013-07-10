using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlogMigrator
{
   /// <summary>
   /// Interaction logic for RewriteSourcePosts.xaml
   /// </summary>
   public partial class RewriteSourcePosts : Window
   {
      public ObservableCollection<LogData> LogCollection =
                           new ObservableCollection<LogData>();

      BackgroundWorker rewriteWorker = new BackgroundWorker();
      
      public RewriteSourcePosts()
      {
         InitializeComponent();

         rewriteWorker.WorkerReportsProgress = true;
         rewriteWorker.WorkerSupportsCancellation = true;
         rewriteWorker.DoWork += rewriteWorker_DoWork;
         rewriteWorker.ProgressChanged += rewriteWorker_ProgressChanged;
         rewriteWorker.RunWorkerCompleted += rewriteWorker_RunWorkerCompleted;
      }

      private void btnTestRewrite_Click(object sender, RoutedEventArgs e)
      {
         string status;
         string serviceUrl = "";
         Services myService = new Services();

         if (cmbRewriteService.SelectedIndex > -1)
         {
            ComboBoxItem itemDest = (ComboBoxItem)cmbRewriteService.SelectedItem;

            switch (itemDest.Tag.ToString())
            {
               case "SS":
                  serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                  break;

               case "WP":
                  serviceUrl = txtRewriteBlogUrl.Text + "/xmlrpc.php";
                  break;

               case "ASPNet":
                  serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                  break;

               case "OTHER":
                  serviceUrl = txtRewriteServiceUrl.Text;
                  break;
            }

            status = myService.CheckServerStatus
                               (serviceUrl, txtRewriteBlogId.Text,
                                txtRewriteUser.Text, txtRewritePassword.Text);
         }
         else
         {
            status = "Connection failed. No service type specified.";
         }

         MessageBox.Show(status, "Test Destination Connection Result",
                         MessageBoxButton.OK, MessageBoxImage.Information);  
      }

      private void btnRewriteHelp_Click(object sender, RoutedEventArgs e)
      {
         ConnectionHelpWindow helpWindow = new ConnectionHelpWindow();
         helpWindow.Show();
      }

      private void btnRewriteFile_Click(object sender, RoutedEventArgs e)
      {
         Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
         dlg.DefaultExt = ".csv";
         dlg.Filter = "Import Log Files (.csv)|*.csv|All Files|*.*";

         Nullable<bool> result = dlg.ShowDialog();

         if (result == true)
         {
            txtRewriteFile.Text = dlg.FileName;
         }
      }

      /// <summary>
      /// Selects all entries in the ListView.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void btnSelectAllEntries_Click(object sender, RoutedEventArgs e)
      {
         lsvLogEntries.SelectAll();
      }

      private void btnRewrite_Click(object sender, RoutedEventArgs e)
      {
         WorkerArgs myArgs = new WorkerArgs();
         
         if (cmbRewriteService.SelectedIndex > -1)
         {
            ComboBoxItem itemDest = (ComboBoxItem)cmbRewriteService.SelectedItem;

            switch (itemDest.Tag.ToString())
            {
               case "SS":
                  App.rewriteBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                  break;

               case "WP":
                  App.rewriteBlog.serviceUrl = txtRewriteBlogUrl.Text + "/xmlrpc.php";
                  break;

               case "ASPNet":
                  App.rewriteBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                  break;

               case "OTHER":
                  App.rewriteBlog.serviceUrl = txtRewriteServiceUrl.Text;
                  break;
            }

            App.rewriteBlog.blogId = txtRewriteBlogId.Text;
            App.rewriteBlog.rootUrl = txtRewriteBlogUrl.Text;
            App.rewriteBlog.username = txtRewriteUser.Text;
            App.rewriteBlog.password = txtRewritePassword.Text;
            App.rewriteBlog.blogFile = txtRewriteFile.Text;

            App.itemsToRewrite.Clear();
            foreach (LogData logItem in lsvLogEntries.SelectedItems)
            {
               App.itemsToRewrite.Add(logItem);
            }

            App.rewriteMessage = txtUpdateSource.Text;

            myArgs.processToRun = "rewrite";
            myArgs.status = "Starting rewrite process...";
            rewriteWorker.RunWorkerAsync(myArgs);
         }
         else
         {
            MessageBox.Show("Please specify a service type.", 
                            "Rewrite Sources Error.",
                            MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      private void btnLoadLog_Click(object sender, RoutedEventArgs e)
      {
         LogData logItem;
         string[] logValues;
         bool firstLine = true;

         if(File.Exists(txtRewriteFile.Text) )
         {
            LogCollection.Clear();

            using (StreamReader sr = new StreamReader(txtRewriteFile.Text))
            {
               string line;
               // Read and display lines from the file until the end of 
               // the file is reached.
               while ((line = sr.ReadLine()) != null)
               {
                  if (!firstLine)
                  {
                     logValues = line.Split((Char.Parse(",")));
                     logItem = new LogData
                                   (Convert.ToInt32(logValues[0]), logValues[1],
                                    Convert.ToInt32(logValues[2]), logValues[3]);
                     LogCollection.Add(logItem);
                  }
                  else
                  {
                     firstLine = false;
                  }
               }
            }

            lblEntriesCount.Content = "[" + LogCollection.Count + " Total]";
            lsvLogEntries.ItemsSource = LogCollection;

            if (LogCollection.Count > 0)
            {
               btnRewrite.IsEnabled = true;
               btnSelectAllEntries.IsEnabled = true;
            }
         }
      }

      private void btnClose_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
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
   }
}
