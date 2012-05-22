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

namespace Expanz.ThinRIA
{
    /// <summary>
    /// Interaction logic for TrafficDebug.xaml
    /// </summary>
    public partial class TrafficDebug : Window
    {
        public TrafficDebug()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            txtRequest.Text = ApplicationEx.Instance.ServerApplicationService.Request.ToString();
            txtResponse.Text = ApplicationEx.Instance.ServerApplicationService.Response.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
