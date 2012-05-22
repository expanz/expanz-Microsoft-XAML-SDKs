namespace Expanz
{
    using System.Diagnostics;
    using System.Globalization;

    public static class Culture
    {
        public static CultureInfo Current
        {
            [DebuggerStepThrough]
            get
            {
                return CultureInfo.CurrentUICulture;
            }
        }

        public static CultureInfo Invariant
        {
            [DebuggerStepThrough]
            get
            {
                return CultureInfo.InvariantCulture;
            }
        }
    }
}