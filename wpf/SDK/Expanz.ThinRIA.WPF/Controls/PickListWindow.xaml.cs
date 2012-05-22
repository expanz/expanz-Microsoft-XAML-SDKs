using System.Windows;
using System.Xml.Linq;
using System.Windows.Input;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Controls
{
    public partial class PickListWindow : Window
    {
        ActivityHarness _activityHarness = null;

        public PickListWindow(ActivityHarness activityHarness, XElement data)
        {
            InitializeComponent();

            _activityHarness = activityHarness;
            PickList.ContextChanging += PickList_ContextChanging;
            if (data.Attribute("contextObject") != null)
            {
                PickList.ModelObject = data.GetAttribute("contextObject");
            }
            else
            {
                PickList.ModelObject = string.Empty;
            }
            PickList.PublishData(data);
        }

        private void PickList_ContextChanging(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
