using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Expanz.ThinRIA
{
    public partial class ClientMessageWindow : ChildWindow
    {
        private Dictionary<string, string> clientOptions;
        private Dictionary<string, RadioButton> clientRadioButtons;
        private string optionType;

        public XDocument Result { get; private set; }
        public ClientMessageWindow(XElement myXml)
        {
            InitializeComponent();
            clientOptions = new Dictionary<string,string>();
            clientRadioButtons = new Dictionary<string, RadioButton>();
            Result = new XDocument();
            PublishXml(myXml);
        }
        public ClientMessageWindow()
        {
            InitializeComponent();
        }

        private void PublishXml(XElement theElt)
        {
            Title = theElt.Attribute(Common.UIMessage.Title).Value;
            txtMessage.Text = theElt.Attribute(Common.UIMessage.text).Value;

            XElement theChildElt = (XElement)theElt.FirstNode;
            while (theChildElt != null) 
            {
                // ACTIONS - create a button for each one
                if (theChildElt.Name == Common.UIMessage.Actions)
                {
                    CreateClientActionButtons(theChildElt);
                }
                else if (theChildElt.Name == Common.UIMessage.Options)
                {
                    if (theChildElt.Attribute(Common.UIMessage.type).Value == Common.UIMessage.typeList)
                    {
                        optionType = Common.UIMessage.typeList;
                        CreateListOptions(theChildElt);
                    }
                    else
                    {
                        optionType = Common.UIMessage.typeRadioButton;
                        CreateRadioButtonOptions(theChildElt);
                    }
                }
                theChildElt = (XElement)theChildElt.NextNode;
            }
        }
        protected override void OnOpened()
        {
            foreach (Control control in ListOptionsContainer.Children)
            {
                control.Width = ListOptionsContainer.ActualWidth - 20;
            }
            base.OnOpened();
        }
        private void CreateClientActionButtons(XElement xactions)
        {
            IEnumerable<XNode> childNodes = xactions.Nodes();
            int totalNodes = childNodes.Count();
            double theSize = (80 * totalNodes) + 12;
            if (theSize < this.Width)
            {
                theSize = this.Width;
            }

            this.Width = theSize;

            XElement buttonElt = (XElement)xactions.FirstNode;

            while (buttonElt != null)
            {
                string label = buttonElt.Attribute(Common.UIMessage.label).Value;
                string request = "";
                string response = "";
                if (buttonElt.Nodes().Count() > 0)
                {
                    if (((XElement)buttonElt.FirstNode).Name == Common.UIMessage.Request)
                    {
                        request = ((XElement)buttonElt.FirstNode).FirstNode.ToString();//.InnerXml;
                    }
                    else if (((XElement)buttonElt.FirstNode).Name == Common.UIMessage.Response)
                    {
                        response = ((XElement)buttonElt.FirstNode).FirstNode.ToString();
                    }
                }

                ClientMessageButton theButton = CreateAndInitClientMessageButton();
                theButton.ButtonRequest = request;
                theButton.ButtonResponse = response;

                theButton.TabIndex = 0;
                theButton.Content = label;

                XAttribute selOption = buttonElt.Attribute(Common.UIMessage.selectedOption);
                if (selOption != null && Common.boolValue(selOption.Value))
                {
                    theButton.Tag = Common.UIMessage.selectedOption;
                }
                //XAttribute defaultAttr = buttonElt.Attribute("default");
                //if (label.ToUpper() == "OK" || (defaultAttr != null && Common.boolValue(defaultAttr.Value)))
                //{

                //}
                //XAttribute cancelAttr = buttonElt.Attribute("cancel");
                //if (label.ToUpper() == "CANCEL" || (cancelAttr != null && Common.boolValue(cancelAttr.Value)))
                //{

                //}

                ActionButtonsContainer.Children.Add(theButton);

                buttonElt = (XElement)buttonElt.NextNode;
            }

        }
        private void CreateListOptions(XElement xoptions)
        {
            ListBox listOptions = CreateAndInitListBox();
            ListOptionsContainer.Children.Add(listOptions);

            XElement xoption = (XElement)xoptions.FirstNode;
            while (xoption != null)
            {
                string label = xoption.Attribute(Common.UIMessage.label).Value;
                string theXml = xoption.ToString();
                clientOptions.Add(label, theXml);

                listOptions.Items.Add(label);

                xoption = (XElement)xoption.NextNode;
            }

            if (listOptions.Items.Count > 0)
            {
                listOptions.SelectedIndex = 0;
            }
        }
        private void CreateRadioButtonOptions(XElement xoptions)
        {
            XElement xoption = (XElement)xoptions.FirstNode;
            int j = 0;
            while (xoption != null)
            {
                string label = xoption.Attribute(Common.UIMessage.label).Value;
                string theXml = xoption.ToString();

                // Create a radio button
                RadioButton button = new RadioButton();
                button.Name = "RadioButton" + j.ToString();
                button.TabIndex = j;
                button.Content = label;
                clientOptions.Add(button.Name, theXml);
                clientRadioButtons.Add(button.Name, button);
                RadioButtonsContainer.Children.Add(button);
                
                xoption = (XElement)xoption.NextNode;
                j++;
            }

            if (this.clientRadioButtons.Count > 0)
            {
                clientRadioButtons["RadioButton0"].IsChecked = true;
            }
        }

        void theButton_Click(object s, RoutedEventArgs e)
        {
            var sender = s as ClientMessageButton;
            bool dlgResult = false;
            try
            {
                string innerXml = "";
                if (sender.ButtonResponse.Length > 0)
                {
                    innerXml = sender.ButtonResponse;
                    dlgResult = false;
                }
                else if (sender.ButtonRequest.Length > 0)
                {
                    innerXml = sender.ButtonRequest;
                    dlgResult = true;
                }
                else if (sender.Tag != null
                    && sender.Tag.ToString() == Common.UIMessage.selectedOption)
                {
                    //Determine which option is selected
                    if (optionType == Common.UIMessage.typeList)
                    {
                        ListBox lst = ListOptionsContainer.Children[0] as ListBox;
                        string selectedOption = lst.SelectedItem.ToString();
                        if (clientOptions.ContainsKey(selectedOption))
                        {
                            innerXml = clientOptions[selectedOption].ToString();
                        }
                    }
                    else if (optionType == Common.UIMessage.typeRadioButton)
                    {
                        for (int i = 0; i < clientRadioButtons.Count; i++)
                        {
                            if (clientRadioButtons["RadioButton" + i.ToString()].IsChecked == true)
                            {
                                innerXml = clientOptions["RadioButton" + i.ToString()].ToString();
                                break;
                            }
                        }
                    }
                    dlgResult = true;
                }
                else
                {
                    dlgResult = false;
                }
                if (dlgResult)
                {
                    Result = XDocument.Parse(innerXml);
                }
            }
            catch (Exception ex)
            {
                Logging.DebugException(ex, "Error");
            }
            finally
            {
                DialogResult = dlgResult;
            }

        }

        private ClientMessageButton CreateAndInitClientMessageButton()
        {
            ClientMessageButton theButton = new ClientMessageButton(this);
            theButton.HorizontalAlignment = HorizontalAlignment.Right;
            theButton.Margin = new Thickness(0, 0, 10, 5);
            theButton.Width = 75;
            theButton.Click += new RoutedEventHandler(theButton_Click);
            //Grid.SetRow(theButton, 1);

            return theButton;
        }
        private ListBox CreateAndInitListBox()
        {
            ListBox lst = new ListBox();

            lst.Margin = new Thickness(10, 10, 10, 10);
            return lst;
        }
        private class ClientMessageButton : Button
        {
            #region Constructor functions

            public ClientMessageButton(ClientMessageWindow window)
            {
                InitializeComponent();
                ClientMessageWindow = window;
            }
            public ClientMessageButton()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.Click += this.ClientMessageButton_Click;
                ButtonRequest = "";
                ButtonResponse = "";
            }

            #endregion

            #region Properties and Variables

            private string buttonRequest;
            public string ButtonRequest
            {
                get { return buttonRequest; }
                set { buttonRequest = value; }
            }

            private string buttonResponse;
            public string ButtonResponse
            {
                get { return buttonResponse; }
                set { buttonResponse = value; }
            }

            public ClientMessageWindow ClientMessageWindow
            {
                get;
                private set;
            }

            #endregion

            #region Event functions

            public void ClientMessageButton_Click(object sender, System.EventArgs e)
            {
                if (ClientMessageWindow != null)
                {
                    //ClientMessageWindow.MessageButtonClick(this, e);
                }
            }

            #endregion
        }
    }
}

