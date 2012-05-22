using System.Windows.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expanz.ThinRIA.Controls
{
    [TemplatePart(Name = "UserNameField", Type = typeof(TextBox))]
    [TemplatePart(Name = "PasswordField", Type = typeof(PasswordBox))]
    [TemplatePart(Name = "LoginButton", Type = typeof(Button))]
    public partial class LoginForm : Control, INotifyPropertyChanged
    {
        #region Constants
        private const string _busyStatusText = "Please wait...";
        #endregion

        #region Member Variables
        private ApplicationEx _context = null;
        private Button LoginButton = null;
        private TextBox UserNameField = null;
        private PasswordBox PasswordField = null;
        private bool _canLogin = true;
        #endregion

        #region Events
        public event EventHandler LoginSuccessful;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public LoginForm()
        {
            BusyStatusText = _busyStatusText;
            this.DefaultStyleKey = typeof(LoginForm);

            _context = ApplicationEx.Instance;
        }
        #endregion

        #region Event Handlers
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UserNameField = GetTemplateChild("UserNameField") as TextBox;
            PasswordField = GetTemplateChild("PasswordField") as PasswordBox;

            if (UserNameField != null)
            {
                UserNameField.KeyDown += Field_KeyDown;
                UserNameField.GotFocus += UserNameField_GotFocus;

                UserNameField.Focus();
            }

            if (PasswordField != null)
            {
                PasswordField.KeyDown += Field_KeyDown;
                PasswordField.GotFocus += PasswordField_GotFocus;

#if WPF
                PasswordField.Password = Password;
#endif
            }

            LoginButton = GetTemplateChild("LoginButton") as Button;

#if WINDOWS_PHONE
            if (LoginButton != null)
                LoginButton.Click += new RoutedEventHandler(LoginButton_Click);
#endif
        }

        private void Field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login.Execute(null);
            }
        }

        private void UserNameField_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void PasswordField_GotFocus(object sender, RoutedEventArgs e)
        {
            ((PasswordBox)sender).SelectAll();
        }
        #endregion

        #region Dependency Properties
        public String UserName
        {
            get { return (String)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(String), typeof(LoginForm), null);

        public String Password
        {
            get { return (String)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(String), typeof(LoginForm), new PropertyMetadata(OnPasswordPropertyChanged));

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if WPF
            PasswordBox passwordField = ((LoginForm)d).PasswordField;

            if (passwordField != null)
                passwordField.Password = e.NewValue.ToString();
#endif
        }

        public String BusyStatusText
        {
            get { return (String)GetValue(BusyStatusTextProperty); }
            set { SetValue(BusyStatusTextProperty, value); }
        }

        public static readonly DependencyProperty BusyStatusTextProperty =
            DependencyProperty.Register("BusyStatusText", typeof(String), typeof(LoginForm), null);

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(LoginForm), null);
        #endregion

        #region Commands (plus supporting methods)
        public ICommand Login
        {
            get
            {
                return new DelegateCommand(BeginLogin, CanLogin);
            }
        }

        private bool CanLogin(object param)
        {
            return _canLogin;
        }
        #endregion

        #region Service Calls and Completed Event Handlers
        public void BeginLogin(object param)
        {
            BeginLogin();
        }

        public void BeginLogin()
        {
#if WPF
            //not needed, handled in XAML
            if (PasswordField != null)
                Password = PasswordField.Password;
#else
            
            // Needed so if ENTER was pressed in one of the fields, it's binding will be updated.
            BindingExpression userNamedBindingExpression = UserNameField.GetBindingExpression(TextBox.TextProperty);
            userNamedBindingExpression.UpdateSource();
            BindingExpression passwordBindingExpression = PasswordField.GetBindingExpression(PasswordBox.PasswordProperty);
            passwordBindingExpression.UpdateSource();
#endif

            BusyStatusText = "Logging in...";
            IsBusy = true;
            _canLogin = false;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Login"));

            if (LoginButton != null)
                LoginButton.IsEnabled = false;

            _context.CreateSession(UserName, Password, _context.PreferredSite, "", AppContext_AuthenticationCompleted);
        }

        private void AppContext_AuthenticationCompleted(object sender, AuthenticationCompletedEventArgs e)
        {
            IsBusy = false;
            Login.CanExecute(true);
            _canLogin = true;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Login"));

            if (LoginButton != null)
                LoginButton.IsEnabled = true;

            if (e.IsAuthenticated)
            {
                if (LoginSuccessful != null)
                    LoginSuccessful(this, new EventArgs());
            }
            else
            {
                ApplicationEx.Instance.DisplayMessageBox(e.Reference, "Login Invalid");
            }
        }
        #endregion
    }
}
