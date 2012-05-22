using Expanz.ThinRIA.Core.Common.VisualStudio.Design;
using Expanz.ThinRIA.Core.Silverlight.VisualStudio.Design;
using Microsoft.Windows.Design.Metadata;

namespace Expanz.ThinRIA.Core.Silverlight.Design
{
    internal class RegisterMetadata : IProvideAttributeTable
    {
        AttributeTable IProvideAttributeTable.AttributeTable
        {
            get
            {
                //System.IO.File.AppendAllText(@"C:\Temp\DesignDebug.txt", "Entering RegisterMetadata");

                ThinRIAAttributeTableBuilder builder = new ThinRIAAttributeTableBuilder(new SilverlightTypeResolver());
                return builder.CreateTable();
            }
        }
    }
}
