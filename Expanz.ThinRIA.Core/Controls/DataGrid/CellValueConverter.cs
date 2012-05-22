using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml.Linq;

namespace Expanz.ThinRIA.Controls
{
    public class CellValueConverter : IValueConverter
    {
        /// <summary>
        /// Function to convert the data, while binding
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string data = string.Empty;

            if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
            {
                XDocument d = XDocument.Parse(value.ToString());
                //XElement xe = d.Root.Element(parameter.ToString());
                XElement xe = d.Root.Elements("Cell").Where(x => x.Attribute("id").Value == parameter.ToString()).FirstOrDefault();

                if (xe != null)
                    data = xe.Value;
            }

            return data;
        }

        /// <summary>
        /// Function to convert a value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}