using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Expanz.ThinRIA.Core
{
    internal class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo, IEditableObject //, IActivityContainer, IServerBoundControl, IEditableControl, IDataControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private bool _isBusy;
        protected ActivityHarness harness;

        public ViewModelBase()
        {
            //ParseViewModelDecorations();

            //ApplicationEx.CreatingContainer = this;
            //harness = new ActivityHarness(this);
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            return null;
        }

        public bool HasErrors
        {
            get { return false; }
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Raise the PropertyChanged event for the 
        /// specified property.
        /// </summary>
        /// <param name="propertyName">
        /// A string representing the name of 
        /// the property that changed.</param>
        /// <remarks>
        /// Only raise the event if the value of the property 
        /// has changed from its previous value</remarks>
        protected void OnPropertyChanged(string propertyName)
        {
            // Validate the property name in debug builds
            VerifyProperty(propertyName);

            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Verifies whether the current class provides a property with a given
        /// name. This method is only invoked in debug builds, and results in
        /// a runtime exception if the <see cref="OnPropertyChanged"/> method
        /// is being invoked with an invalid property name. This may happen if
        /// a property's name was changed but not the parameter of the property's
        /// invocation of <see cref="OnPropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();

            // Look for a *public* property with the specified name
            System.Reflection.PropertyInfo pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer
                string msg = "OnPropertyChanged was invoked with invalid " +
                                "property name {0}. {0} is not a public " +
                                "property of {1}.";
                msg = String.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Assert(1 != 1, msg);
            }
        }
        #endregion

        public virtual bool IsValid
        {
            get
            {
                return !HasErrors; // _errors.Count == 0;
            }
        }

        private string GetPropertyNameFromExpression<T>(Expression<Func<T>> property)
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            return memberExpression.Member.Name;
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(GetPropertyNameFromExpression(property));
        }

        protected bool SetValue<T>(ref T property, T value, Expression<Func<T>> propertyDelegate)
        {
            if (Object.Equals(property, value))
            {
                return false;
            }
            property = value;

            OnPropertyChanged(propertyDelegate);

            return true;
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetValue(ref _isBusy, value, () => IsBusy);
            }
        }

        public bool IsInDesignTool
        {
            get { return false; } // DesignerProperties.GetIsInDesignMode(new DependencyObject()); }
        } 

        #region IEditableObject Members
        public virtual void BeginEdit()
        {

        }

        public virtual void CancelEdit()
        {

        }

        public virtual void EndEdit()
        {

        }
        #endregion

        /// <summary>
        /// Classes inheriting this base class can add additional data to the
        /// validation context by overriding this method.
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationContext CreateValidationContext()
        {
            return new ValidationContext(this, null, null);

            // Example override:

            //    // Make the parent available to the validation attributes
            //    var contextItems = new Dictionary<object, object>
            //    {
            //        { "Parent", _parent }
            //    };

            //    return new ValidationContext(this, null, contextItems);
        }

        protected bool ValidateProperty<T>(Expression<Func<T>> property, object value)
        {
            return ValidateProperty(GetPropertyNameFromExpression(property), value);
        }

        protected virtual bool ValidateProperty(string propertyName)
        {
            PropertyInfo propInfo = this.GetType().GetProperty(propertyName);
            object value = propInfo.GetValue(this, null);

            return ValidateProperty(propertyName, value);
        }

        protected virtual bool ValidateProperty(string propertyName, object value)
        {
            // Validate a property based upon its validation attributes
            List<ValidationResult> validationResults = new List<ValidationResult>();

            ValidationContext validationContext = CreateValidationContext();
            validationContext.MemberName = propertyName;

            bool isValid = Validator.TryValidateProperty(value, validationContext, validationResults);

            //ClearError(propertyName);

            //foreach (ValidationResult result in validationResults)
            //    AddError(propertyName, result.ErrorMessage);

            return isValid;
        }

        public virtual bool Validate()
        {
            // Validate this object based upon its validation attributes
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(this, CreateValidationContext(), validationResults, true);

            //_errors.Clear();

            //foreach (ValidationResult result in validationResults)
            //{
            //    foreach (string propertyName in result.MemberNames)
            //    {
            //        AddError(propertyName, result.ErrorMessage);
            //        OnPropertyChanged(propertyName);
            //    }

            //    // Handle when member names is empty (ie. object level errors)?
            //}

            return IsValid;
        }

        //private Dictionary<string, PropertyInfo> _dataPublicationProperties = null;

        //private void ParseViewModelDecorations()
        //{
        //    _dataPublicationProperties = new Dictionary<string, PropertyInfo>();

        //    PropertyInfo[] properties = GetType().GetProperties();

        //    foreach (PropertyInfo property in properties)
        //    {
        //        object[] attributes = property.GetCustomAttributes(typeof(DataPublicationAttribute), true);

        //        if (attributes.Length != 0)
        //        {
        //            var publication = attributes[0] as DataPublicationAttribute;
        //            _dataPublicationProperties.Add(publication.ID, property);
        //        }
        //    }
        //}

        //public virtual string ActivityName
        //{
        //    get
        //    {
        //        object[] attributes = GetType().GetCustomAttributes(typeof(ThinRIAViewModelAttribute), true);

        //        if (attributes.Length != 0)
        //            return ((ThinRIAViewModelAttribute)attributes[0]).ActivityName;
        //        else
        //            return null;
        //    }
        //    private set
        //    {
        //    }
        //}

        //public virtual void Load()
        //{
        //    harness.ActivityName = ActivityName;
        //    harness.Initialise(null);
        //}

        //#region IActivityContainer
        //public ApplicationEx AppHost
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public void Initialise(System.Xml.Linq.XElement xml)
        //{
        //    throw new NotImplementedException();
        //}

        //public void InitialiseCopy(ActivityHarness activityHarness)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IsInitialised { get { return harness.IsInitialised; } }
        //public string ActivityStamp { get { return harness.ActivityStamp; } }
        //public string ActivityStampEx { get { return harness.ActivityStampEx; } }
        //public int DuplicateIndex { get { return harness.duplicateIndex; } }
        //public string FixedContext { get { return harness.FixedContext; } }
        //public int PersistentKey { get { return harness.PersistentId; } }

        //public void RegisterControl(IServerBoundControl control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterControlContainer(IServerBoundControlContainer control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterDataControl(IDataControl control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterGraphControl(IGraphControl control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterCustomControl(ICustomContentPublisher control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterCustomSchemaPublisher(ICustomSchemaPublisher control)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterServerBoundButton(IMethodCaller button)
        //{
        //    throw new NotImplementedException();
        //}

        //public void RegisterDirtyButton(Controls.ButtonEx button)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Exec(System.Xml.Linq.XElement DeltaXml)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Exec(System.Xml.Linq.XElement[] DeltaXml)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AppendDataPublicationsToActivityRequest(XElement xml)
        //{
        //    //harness.appendDataPublicationsToActivityRequest(xml);

        //    foreach (PropertyInfo property in _dataPublicationProperties.Values)
        //    {
        //        object[] attributes = property.GetCustomAttributes(typeof(DataPublicationAttribute), true);

        //        if (attributes.Length != 0)
        //        {
        //            var publication = attributes[0] as DataPublicationAttribute;

        //            XElement dp = new XElement(Common.DataPublication);
        //            dp.SetAttributeValue(Common.IDAttrib, publication.ID);
        //            dp.SetAttributeValue(Common.Data.PublicationMethod, publication.Method);
        //            //e.Current.FillServerRegistrationXml(dp);
        //            xml.Add(dp);
        //        }
        //    }
        //}

        //public void PublishResponse(XElement xml)
        //{
        //    harness.PublishResponse(xml);
        //}

        //public bool Publishing { get { return harness.Publishing; } }

        //public void PublishDirtyChange(string modelObject, bool dirty)
        //{
        //    throw new NotImplementedException();
        //}

        //public void ResetWindowTitle(ActivityHarness harness)
        //{
        //    throw new NotImplementedException();
        //}

        //public void closeOnLogout()
        //{
        //    throw new NotImplementedException();
        //}

        //public void focus()
        //{
        //    throw new NotImplementedException();
        //}

        //public OptimiseTiming Optimsation
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}


        //public string ImplementationName { get { return this.GetType().Name; } }

        //public bool IsOKToClose
        //{
        //    get { return true; }
        //}


        //public string ActivityStyle
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string IconImageName
        //{
        //    get { throw new NotImplementedException(); }
        //} 
        //#endregion

        //#region IServerBoundControl
        //public string FieldId
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public void SetVisible(bool visible)
        //{
        //    throw new NotImplementedException();
        //} 
        //#endregion

        //#region IEditableControl
        //public System.Xml.Linq.XElement DeltaXml
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public void setEditable(bool editable)
        //{
        //    throw new NotImplementedException();
        //}

        //public void setNull()
        //{
        //    throw new NotImplementedException();
        //}

        //public void setValue(string text)
        //{
        //    throw new NotImplementedException();
        //}

        //public void setLabel(string label)
        //{
        //    throw new NotImplementedException();
        //}

        //public void setHint(string hint)
        //{
        //    throw new NotImplementedException();
        //}

        //public void publishXml(System.Xml.Linq.XElement xml)
        //{
        //    throw new NotImplementedException();
        //} 
        //#endregion

        //#region IDataControl
        //public void PublishData(XElement xml)
        //{
        //    string id = string.Empty;

        //    if (xml.Attribute(Common.IDAttrib) != null)
        //        id = xml.Attribute(Common.IDAttrib).Value;

        //    PropertyInfo propertyInfo = _dataPublicationProperties[id];
            
        //}

        //public string DataId
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string QueryID
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string PopulateMethod
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string ModelObject
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string AutoPopulate
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public void FillServerRegistrationXml(System.Xml.Linq.XElement xml)
        //{
        //    throw new NotImplementedException();
        //}

        //public ControlHarness BindingHarness
        //{
        //    get { throw new NotImplementedException(); }
        //} 
        //#endregion
    }
}
