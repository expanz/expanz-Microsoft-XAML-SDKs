using System;
using System.Xml.Linq;

namespace Expanz.Extensions.BCL
{
    public static class XElementExtensions
    {
        /// <summary>
        /// get the attribute value (null if no attribute exists)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this XElement element, XName attributeName)
        {
            string value = null;

            if (element.Attribute(attributeName) != null)
                value = element.Attribute(attributeName).Value;

            return value;
        }
        /// <summary>
        /// same as XmlELement.GetAttribute() - returns empty string if no attribute exists
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAttribute(this XElement element, XName attributeName)
        {
            string value = string.Empty;

            if (element.Attribute(attributeName) != null)
                value = element.Attribute(attributeName).Value;

            return value;
        }

        public static T GetAttributeValue<T>(this XElement element, XName attributeName)
        {
            return GetAttributeValue<T>(element, attributeName, default(T));
        }

        public static T GetAttributeValue<T>(this XElement element, XName attributeName, T defaultValue)
        {
            T value = defaultValue;

            try
            {
                if (element.Attribute(attributeName) != null)
                {
                    string stringValue = element.Attribute(attributeName).Value;

                    if (value.GetType() == typeof(bool))
                        stringValue = Common.boolValue(stringValue).ToString();
                    
                    value = (T)Convert.ChangeType(stringValue, typeof(T), null);
                }
            }
            catch { }

            return value;
        }

        /// <summary>
        /// return a bool whether the passed element has the specified attribute
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool HasAttribute(this XElement element, XName attribute)
        {
            return element.Attribute(attribute) != null;
        }
        public static void SetAttribute(this XElement element, XName attribute, string value)
        {
            element.SetAttributeValue(attribute, value);
        }
    }
}
