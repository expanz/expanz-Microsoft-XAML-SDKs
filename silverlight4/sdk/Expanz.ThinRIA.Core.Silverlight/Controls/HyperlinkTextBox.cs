using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    [TemplatePart(Name = UrlFieldControlName, Type = typeof(TextBox))]
    public class HyperlinkTextBox : Control
    {
        private const string UrlFieldControlName = "UrlField";
        private TextBox UrlField = null;

        public HyperlinkTextBox()
        {
            DefaultStyleKey = typeof(HyperlinkTextBox);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UrlField = this.GetTemplateChild(UrlFieldControlName) as TextBox;
        }

        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this control's input values to.")]
        public string FieldId
        {
            get { return (string)GetValue(FieldIdProperty); }
            set { SetValue(FieldIdProperty, value); }
        }

        public static readonly DependencyProperty FieldIdProperty =
            DependencyProperty.Register("FieldId", typeof(string), typeof(HyperlinkTextBox), null);

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            // Default the focus to the URlField text box whenever this control receives the focus
            if (UrlField != null)
                UrlField.Focus();
        }
    }
}
