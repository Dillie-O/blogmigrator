using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
   /// Interaction logic for ConfigureDestination.xaml
   /// </summary>
   public partial class ConfigureDestination : Window
   {
      public ConfigureDestination()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Saves the destination connection details into the application 
      /// variable.
      /// </summary>
      /// <param name="sender">Save button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnSaveSource_Click(object sender, RoutedEventArgs e)
      {
         Services myService = new Services();

         if (cmbDestinationService.SelectedIndex > -1)
         {
            ComboBoxItem itemDest = (ComboBoxItem)cmbDestinationService.SelectedItem;

            App.destBlog = new BlogSource();
            App.destBlog.serviceType = itemDest.Tag.ToString();

            switch (itemDest.Tag.ToString())
            {
               case "SS":
                  App.destBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                  break;

               case "WP":
                  App.destBlog.serviceUrl = txtDestinationBlogUrl.Text + "/xmlrpc.php";
                  break;

               case "ASPNet":
                  App.destBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                  break;

               case "OTHER":
                  App.destBlog.serviceUrl = txtDestinationServiceUrl.Text;
                  break;

               case "FILE":
                  App.destBlog.serviceUrl = txtDestinationServiceUrl.Text;
                  break;
            }

            App.destBlog.rootUrl = txtDestinationBlogUrl.Text;
            App.destBlog.blogId = txtDestinationBlogId.Text;
            App.destBlog.username = txtDestinationUser.Text;
            App.destBlog.password = txtDestinationPassword.Text;
            App.destBlog.blogFile = txtDestinationFile.Text;

            if (itemDest.Tag.ToString() != "FILE")
            {
               ((MainWindow)this.Owner).btnConfigureSource.Content =
                            "Configure Source Blog" +
                            Environment.NewLine + Environment.NewLine +
                            itemDest.Tag.ToString() + " - " + txtDestinationBlogUrl.Text;
            }
            else
            {
               ((MainWindow)this.Owner).btnConfigureSource.Content =
                  "Configure Source Blog" +
                  Environment.NewLine + Environment.NewLine +
                  itemDest.Tag.ToString() + " - " + txtDestinationFile.Text;

            }
            MessageBox.Show("Destination Configuration Saved.", "Configuration Saved.",
                           MessageBoxButton.OK, MessageBoxImage.Information);
         }
         else
         {
            MessageBox.Show("Please specify a service type.", "Configuration Not Saved.",
                             MessageBoxButton.OK, MessageBoxImage.Error);
         }         
      }

      /// <summary>
      /// Displays the connection help window.
      /// </summary>
      /// <param name="sender">Destination Help button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnDestHelp_Click(object sender, RoutedEventArgs e)
      {
         ConnectionHelpWindow helpWindow = new ConnectionHelpWindow();
         helpWindow.Show();
      }

      /// <summary>
      /// Closes the window.
      /// </summary>
      /// <param name="sender">Close button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnClose_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }

      /// <summary>
      /// Displays a file dialog window to input source file.
      /// </summary>
      /// <param name="sender">Test Destination button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnDestinationFile_Click(object sender, RoutedEventArgs e)
      {
         Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
         dlg.DefaultExt = ".xml";
         dlg.Filter = "WXR Files (.xml)|*.xml|All Files|*.*";

         Nullable<bool> result = dlg.ShowDialog();

         if (result == true)
         {
            txtDestinationFile.Text = dlg.FileName;
         }
      }

      /// <summary>
      /// Tests the destination connection by attempting to retrieve a post.
      /// </summary>
      /// <param name="sender">Test Source button click event.</param>
      /// <param name="e">Button click event arguments.</param>
      /// <history>
      /// Sean Patterson   [11/11/2010]   Created
      /// </history>
      private void btnTestDestination_Click(object sender, RoutedEventArgs e)
      {            
         string status;
         string serviceUrl = "";
         Services myService = new Services();

         if (cmbDestinationService.SelectedIndex > -1)
         {
            ComboBoxItem itemDest = (ComboBoxItem)cmbDestinationService.SelectedItem;

            switch (itemDest.Tag.ToString())
            {
               case "SS":
                  serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                  break;

               case "WP":
                  serviceUrl = txtDestinationBlogUrl.Text + "/xmlrpc.php";
                  break;

               case "ASPNet":
                  serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                  break;

               case "OTHER":
                  serviceUrl = txtDestinationServiceUrl.Text;
                  break;
            }

            if (itemDest.Tag.ToString() != "FILE")
            {
               status = myService.CheckServerStatus
                                  (serviceUrl, txtDestinationBlogId.Text,
                                   txtDestinationUser.Text, txtDestinationPassword.Text);
            }
            else
            {
               // There is no need to test if the destination file exists, only 
               // that a location has been specified.
               if (!string.IsNullOrEmpty(txtDestinationFile.Text))
               {
                  status = "Connection successful.";
               }
               else
               {
                  status = "Connection failed. No file specified.";
               }
            }
         }
         else
         {
            status = "Connection failed. No service type specified.";
         }

         MessageBox.Show(status, "Test Destination Connection Result",
                           MessageBoxButton.OK, MessageBoxImage.Information);         
      }

      /// <summary>
      /// Inserts a sample post in the destination server.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void btnInsertSample_Click(object sender, RoutedEventArgs e)
      {
         Services myService = new Services();
         string serviceUrl = "";
         string status;

         if (cmbDestinationService.SelectedIndex > -1)
         {
            ComboBoxItem itemDest = (ComboBoxItem)cmbDestinationService.SelectedItem;

            try
            {
               switch (itemDest.Tag.ToString())
               {
                  case "SS":
                     serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                     break;

                  case "WP":
                     serviceUrl = txtDestinationBlogUrl.Text + "/xmlrpc.php";
                     break;

                  case "ASPNet":
                     serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                     break;

                  case "OTHER":
                     serviceUrl = txtDestinationServiceUrl.Text;
                     break;
               }

               if (itemDest.Tag.ToString() != "FILE")
               {
                  status = myService.InsertSamplePost
                                     (serviceUrl, txtDestinationBlogId.Text,
                                      txtDestinationUser.Text, txtDestinationPassword.Text);
               }
               else
               {
                  status = "No test post. Destination is a file.";
               }

               MessageBox.Show(status, "Add Sample Post Result",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
               MessageBox.Show("Error inserting test post:" +
                                 Environment.NewLine + Environment.NewLine +
                                 ex.ToString() +
                                 Environment.NewLine + Environment.NewLine +
                                 "Please check your settings and try again.",
                                 "Error adding sample post.",
                                 MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
         else
         {
            MessageBox.Show("Error inserting test post. " + 
                            "No source type specified.", 
                            "Error adding sample post.",
                            MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }        
   }
}
