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
    /// Interaction logic for ConfigureSource.xaml
    /// </summary>
    public partial class ConfigureSource : Window
    {
        public ConfigureSource()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves the source connection details into the application variable.
        /// </summary>
        /// <param name="sender">Save button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>
        /// Sean Patterson   [11/11/2010]   Created
        /// </history>
        private void btnSaveSource_Click(object sender, RoutedEventArgs e)
        {
            Services myService = new Services();

            if (cmbSourceService.SelectedIndex > -1)
            {
               ComboBoxItem itemSource = (ComboBoxItem)cmbSourceService.SelectedItem;

               App.sourceBlog = new BlogSource();
               App.sourceBlog.serviceType = itemSource.Tag.ToString();

               switch (itemSource.Tag.ToString())
               {
                  case "SS":
                     App.sourceBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                     break;

                  case "WP":
                     App.sourceBlog.serviceUrl = txtSourceBlogUrl.Text + "/xmlrpc.php";
                     break;

                  case "ASPNet":
                     App.sourceBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                     break;

                  case "OTHER":
                     App.sourceBlog.serviceUrl = txtSourceServiceUrl.Text;
                     break;

                  case "FILE":
                     App.sourceBlog.serviceUrl = txtSourceServiceUrl.Text;
                     break;
               }

               App.sourceBlog.rootUrl = txtSourceBlogUrl.Text;
               App.sourceBlog.blogId = txtSourceBlogId.Text;
               App.sourceBlog.username = txtSourceUser.Text;
               App.sourceBlog.password = txtSourcePassword.Text;
               App.sourceBlog.blogFile = txtSourceFile.Text;

               if (itemSource.Tag.ToString() != "FILE")
               {
                  ((MainWindow)this.Owner).btnConfigureSource.Content =
                               "Configure Source Blog" +
                               Environment.NewLine + Environment.NewLine +
                               itemSource.Tag.ToString() + " - " + txtSourceBlogUrl.Text;
               }
               else
               {
                  ((MainWindow)this.Owner).btnConfigureSource.Content =
                     "Configure Source Blog" +
                     Environment.NewLine + Environment.NewLine +
                     itemSource.Tag.ToString() + " - " + txtSourceFile.Text;

               }

               MessageBox.Show("Source Configuration Saved.", "Configuration Saved.",
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
        /// <param name="sender">Source Help button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>
        /// Sean Patterson   [11/11/2010]   Created
        /// </history>
        private void btnSourceHelp_Click(object sender, RoutedEventArgs e)
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
        /// <param name="sender">Test Source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>
        /// Sean Patterson   [11/11/2010]   Created
        /// </history>
        private void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "BlogML Files (.xml)|*.xml|All Files|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                txtSourceFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Tests the source connection by attempting to retrieve a post.
        /// </summary>
        /// <param name="sender">Test Source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>
        /// Sean Patterson   [11/11/2010]   Created
        /// </history>
        private void btnTestSource_Click(object sender, RoutedEventArgs e)
        {
            string status;
            string serviceUrl = "";
            Services myService = new Services();

            if (cmbSourceService.SelectedIndex > -1)
            {
               ComboBoxItem itemSource = (ComboBoxItem)cmbSourceService.SelectedItem;

               switch (itemSource.Tag.ToString())
               {
                  case "SS":
                     serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                     break;

                  case "WP":
                     serviceUrl = txtSourceBlogUrl.Text + "/xmlrpc.php";
                     break;

                  case "ASPNet":
                     serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                     break;

                  case "OTHER":
                     serviceUrl = txtSourceServiceUrl.Text;
                     break;
               }

               if (itemSource.Tag.ToString() != "FILE")
               {
                  status = myService.CheckServerStatus
                                     (serviceUrl, txtSourceBlogId.Text,
                                      txtSourceUser.Text, txtSourcePassword.Text);
               }
               else
               {
                  if (System.IO.File.Exists(txtSourceFile.Text))
                  {
                     status = "Connection successful.";
                  }
                  else
                  {
                     status = "Connection failed. File not found.";
                  }
               }               
            }
            else
            {
               status = "Connection failed. No service type specified.";
            }
            MessageBox.Show(status, "Test Source Connection Result",
                                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
