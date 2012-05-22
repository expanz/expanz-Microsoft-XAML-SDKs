using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Core
{
    public partial class TrafficDebuggingWindow : ChildWindow
    {
        public TrafficDebuggingWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened()
        {
            txtRequest.Text = ApplicationEx.Instance.ServerApplicationService.Request.ToString();
            txtResponse.Text = ApplicationEx.Instance.ServerApplicationService.Response.ToString();
            base.OnOpened();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

