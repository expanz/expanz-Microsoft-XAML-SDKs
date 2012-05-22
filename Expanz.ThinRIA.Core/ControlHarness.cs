using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA
{
    public partial class ControlHarness
    {
        #region Member Variables
        protected IActivityContainer _activityContainer;
        protected object _control;
        #endregion

        #region Constructor
        public ControlHarness(object control)
        {
            _control = control;

            if (control is DependencyObject)
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode((DependencyObject)control))
                {
                    IsDesignTime = true;
                }
                else
                {
                    _activityContainer = FindParentActivityContainer(control);
                }
            }

            RegisterControl(control);
        }
  
        internal static IActivityContainer FindParentActivityContainer(object control)
        {
            IActivityContainer activityContainer = null;
            DependencyObject parent = VisualTreeHelper.GetParent((DependencyObject)control);
            
            while (parent != null && activityContainer == null)
            {
                if (parent is IActivityContainer) 
                    activityContainer = (IActivityContainer)parent;
                else 
                    parent = VisualTreeHelper.GetParent(parent);
            }

            if (activityContainer == null && ApplicationEx.CreatingContainer != null)
                activityContainer = ApplicationEx.CreatingContainer;

            return activityContainer;
        }
        #endregion

        #region Public Properties
        public bool IsInitialsed { get { return _activityContainer != null; } }

        public IActivityContainer ParentActivity
        {
            get { return _activityContainer; }
        }

        public bool IsDesignTime { get; private set; }

        public XElement MethodElement { get { return new XElement(Common.Requests.MethodInvocation); } }
        public XElement DeltaElement { get { return new XElement(Common.Requests.FieldValueChange); } }
        public XElement ContextElement { get { return new XElement(Common.Requests.SetContext); } }
        public XElement DataPublicationElement { get { return new XElement(Common.DataPublication); } }
        public XElement CustomElement { get { return new XElement(Common.CustomContent); } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send xml for server execution
        /// </summary>
        /// <param name="delta"></param>
        public void SendXml(XElement delta)
        {
            _activityContainer.Exec(delta);
            //ActivityHarness.ContextMenuPublisher = null;
        }

        /// <summary>
        /// Send xml for server execution
        /// </summary>
        /// <param name="delta"></param>
        public void SendXml(XElement[] delta)
        {
            _activityContainer.Exec(delta);
           // ActivityHarness.ContextMenuPublisher = null;
        }

        /// <summary>
        /// Upload file to server
        /// </summary>
        /// <param name="Field"></param>
        /// <param name="filename"></param>
        public void UploadDiskFile(string Field, string filename)
        {
            if (!System.IO.File.Exists(filename)) 
                return;

            XElement[] sendArray = new XElement[2];

            //Create DELTA for filename
            XElement delta = new XElement(Common.Requests.FieldValueChange);
            delta.SetAttributeValue(Common.IDAttrib, Field + ".FileName");
            delta.SetAttributeValue(Common.PublishFieldValue, filename);
            sendArray[0] = delta;

            //Create DELTA for contents
            delta = new XElement(Common.Requests.FieldValueChange);
            delta.SetAttributeValue(Common.IDAttrib, Field + ".FileContents");

            try
            {
                // Convert the file into a string
                FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
                int fileLength = (int)file.Length;
                Byte[] fileData = new Byte[fileLength];
                file.Read(fileData, 0, fileLength);
                file.Close();

                string fileAsString = Convert.ToBase64String(fileData);
                delta.SetElementValue(delta.Name, fileAsString);
                delta.SetAttributeValue(Common.PublishFieldValue, Common.LongDataValueIndicator);
                delta.SetAttributeValue(Common.LongDataEncoding, "BASE64");
                sendArray[1] = delta;
            }
            catch (System.IO.IOException)
            {
                //MessageBox.Show(x.Message, "Error: Unable to read file", MessageBoxButton.OK);
                return;
            }
            catch (Exception)
            {
                //MessageBox.Show(x.ToString());
                return;
            }

            SendXml(sendArray);
        }
        #endregion

        #region Private Methods
        protected void RegisterControl(object control)
        {
            if (_activityContainer != null)
                _activityContainer.RegisterControl(control);
        } 
        #endregion
    }
}
