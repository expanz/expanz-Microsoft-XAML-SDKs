using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA;
using Expanz.ThinRIA.Controls;

namespace Expanz.ThinRIA.Forms
{
	public partial class Export : Window
	{
		private ListViewEx myListview;
		public Export()
		{
			InitializeComponent();
		}
		public Export(ListViewEx lv)
		{
			myListview = lv;
			InitializeComponent();
		}

		private void Export_Click(object sender, RoutedEventArgs e)
		{
			string fileName = "";
			if ((bool)ExportDelimited.IsChecked)
			{
				exportDelimited(out fileName);
			}
			else//xml
			{
			}
			if ((bool)AutoLaunch.IsChecked && fileName.Length > 0)
			{
				try
				{
					System.Diagnostics.Process.Start(fileName);
				}
				catch (Exception ex) { ApplicationEx.DebugException(ex); }
			}
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private string myDelimiter, myTextDelimiter;
		private bool exportDelimited(out string fileName)
		{
			bool returnVal = false;
			this.Cursor = Cursors.Wait;
			fileName = "";

			string message = "";
			StreamWriter s = null;

			// Validate
			if ((bool)rbOther.IsChecked && this.CustomDelimiter.Text.Length == 0)
				message += "\nPlease specify the Delimiter.";
			else
			{
				if ((bool)rbComma.IsChecked)
					myDelimiter = ",";
				else if ((bool)rbPipe.IsChecked)
					myDelimiter = "|";
				else if ((bool)rbTab.IsChecked)
					myDelimiter = (Convert.ToChar(9)).ToString();
				else
					myDelimiter = this.CustomDelimiter.Text;

				if ((bool)QuoteText.IsChecked)
					myTextDelimiter = "\"";
				else
					myTextDelimiter = "";

				try
				{
					if ((bool)cbTempFile.IsChecked)
					{
						string sFileName = System.IO.Path.GetTempFileName();
						ApplicationEx.Instance.TempFiles.Add(sFileName);
						sFileName = sFileName.Substring(0, sFileName.LastIndexOf(".") + 1) + "csv";
                        ApplicationEx.Instance.TempFiles.Add(sFileName);
						fileName = sFileName;
					}
					s = new StreamWriter(fileName);
				}
				catch (Exception ex)
				{
					message += "\nThe file could not be created.  Please check the File Name. "+ex.Message;
				}
			}

			if (message.Length > 0)
			{
				this.Cursor = Cursors.Arrow;
				MessageBox.Show(message);
			}
			else
			{
				StringBuilder temp = null;

				if ((bool)IncludeMetadata.IsChecked && myListview.HasMetaData)
				{
					Dictionary<string,string> md = myListview.Metadata;
					IDictionaryEnumerator de = md.GetEnumerator();
					while (de.MoveNext())
					{
						string t = null;
						if ((bool)WithHeadings.IsChecked)
						{
							t = de.Key.ToString() + "=" + de.Value.ToString();
						}
						else
						{
							t = de.Value.ToString();
						}
						s.WriteLine(t + new string(',', myListview.Columns.Count - 1));
					}
				}

				if ((bool)IncludeHeadings.IsChecked)
				{
					temp = new StringBuilder(200);
					for (int i = 1; i < myListview.Columns.Count; i++)
					{
						if (i > 1)
							temp.Append(myDelimiter);
						GridViewColumn col = myListview.Columns[i];
						if(col.Header!=null)
						{
							temp.Append(myTextDelimiter + col.Header.ToString() + myTextDelimiter);
						}
					}
					s.WriteLine(temp.ToString());
				}

				IEnumerator myEnum = this.myListview.Items.SourceCollection.GetEnumerator();
				while (myEnum.MoveNext())
				{
					if (myEnum.Current is XmlElement)
					{
						temp = new StringBuilder(200);
						XmlElement cell = (XmlElement)((XmlElement)myEnum.Current).FirstChild;
						while (cell != null)
						{
							if (temp.Length > 0) temp.Append(myDelimiter);
							temp.Append(myTextDelimiter + cell.InnerText + myTextDelimiter);
							cell = (XmlElement)cell.NextSibling;
						}
						s.WriteLine(temp.ToString());
					}
				}
				Status.Text = "Export complete";
				returnVal = true;
			}

			if (s != null)
				s.Close();
			this.Cursor = Cursors.Arrow;
			return returnVal;
		}
/*
		private bool ExportXml(out string fileName)
		{
			bool returnVal = false;
			this.Cursor = Cursors.WaitCursor;
			fileName = "";

			XmlDocument XML = new XmlDocument();
			XML.LoadXml("<Choice1/>");

			try
			{
				ListViewItemEx currentItem;

				IEnumerator myEnum = this.myList.Items.GetEnumerator();
				myEnum.Reset();
				while (myEnum.MoveNext())
				{
					currentItem = (ListViewItemEx)myEnum.Current;
					XmlElement ROW = XML.CreateElement(currentItem.ItemType);
					XML.DocumentElement.AppendChild(ROW);
					ROW.SetAttribute("key", currentItem.Id);
					for (int i = 0; i < currentItem.SubItems.Count; i++)
					{
						XmlElement DATA = XML.CreateElement(myList.Columns[i].Text);
						ROW.AppendChild(DATA);
						DATA.InnerText = currentItem.SubItems[i].Text;
					}
				}
				if (cbTempFile.Checked)
				{
					string sFileName = System.IO.Path.GetTempFileName();
					Host.TempFiles.Add(sFileName);
					sFileName = sFileName.Substring(0, sFileName.LastIndexOf(".") + 1) + "xml";
					Host.TempFiles.Add(sFileName);
					fileName = sFileName;
				}
				else
					fileName = this.FileName.Text;
				XML.Save(fileName);
				this.lblMessage.Text = "Export complete";
				returnVal = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
			return returnVal;
		}
		*/
	}
}
