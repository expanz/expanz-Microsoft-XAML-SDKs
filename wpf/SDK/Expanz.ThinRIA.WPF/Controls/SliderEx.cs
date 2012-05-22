using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using System;
using Expanz.ThinRIA.Core;
using Expanz.Extensions.BCL;
using System.Windows.Media;

namespace Expanz.ThinRIA.Controls
{
	public delegate void SliderIntValueChanged(object sender, EventArgs e);

    public class SliderEx : Slider, IServerBoundControl, IEditableControl, INotifyPropertyChanged
    {
        private string _fieldId;
        private bool _isValid = true;
		private bool publishRealTime;
		public bool PublishRealTime
		{
			set { publishRealTime = value; }
			get { return publishRealTime; }
		}
		private bool publishPending;
		private bool manualRego;
		public bool ManualRegistration
		{
			set { manualRego = value; }
			get { return manualRego; }
		}
		public ControlHarness myHarness;
		private int? myValueOverride=null;
		public int IntValue { get { if (myValueOverride != null) return (int)myValueOverride; else return toInt(Value); } }
		public SliderEx()
			: base()
		{
			this.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderEx_ValueChanged);
			this.IntValueChanged += new SliderIntValueChanged(SliderEx_IntValueChanged);
			this.LostFocus += new RoutedEventHandler(SliderEx_LostFocus);
		}

		void SliderEx_LostFocus(object sender, RoutedEventArgs e)
		{
			if (!PublishRealTime && publishPending)
			{
				publishPending = false;
				myHarness.SendXml(DeltaXml);
			}
		}
		public void intitialise()
		{
			myHarness = new ControlHarness(this);
		}
		public void SliderEx_IntValueChanged(object sender, EventArgs e)
		{
			if (myHarness != null && myHarness.IsInitialsed)
			{
				if (sender != this && e==null) myValueOverride = 0;
				if (PublishRealTime)
				{
					myHarness.SendXml(DeltaXml);
				}
				else
				{
					publishPending = true;
				}
				myValueOverride = null;
			}
		}

		void SliderEx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//only send integer value, and only if the difference of the value I last sent changes
			if (!publishing && (toInt(e.OldValue) != toInt(e.NewValue)))
			{
				EventArgs args = new EventArgs();
				IntValueChanged(this, args);
			}
		}
		public event SliderIntValueChanged IntValueChanged;

		private int toInt(double p)
		{
			return System.Convert.ToInt32(p);
		}
		public override void EndInit()
		{
			base.EndInit();
			if (!manualRego)
			{
                if (_fieldId == null && Name.Length > 0) _fieldId = Name;
				intitialise();
			}
		}
        #region Interface imlementations
        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this controls input values to.")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
            set { _fieldId = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Default values are set in MetaData on the server")]
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(SliderEx), new PropertyMetadata(string.Empty));
        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            private set
            {
                _isValid = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsValid"));
            }
        }
		public void SetVisible(bool v)
		{
			if (v)
			{
				Visibility = Visibility.Visible;
			}
			else
			{
				Visibility = Visibility.Hidden;
			}
		}
		public void SetEditable(bool e)
		{
			if (e)
			{
				Focusable = true;
			}
			else
			{
				Focusable = false;
			}
		}
		public bool publishing;
		public bool dontPublishIllegalValues;
		public void SetNull()
		{
			Value = 0;
		}
		public void SetValue(string text)
		{
			double d = 0;
			double.TryParse(text,out d);
			if ((d >= Minimum && d <= Maximum) || !dontPublishIllegalValues)
			{
				publishing = true;
				this.Value = d;
				publishing = false;
			}
		}
		public void SetLabel(string label)
		{
			//if (LabelPublisher != null) LabelPublisher.setLabel(label);
		}
		public void SetHint(string hint)
		{
			this.ToolTip = hint;
		}
		public void PublishXml(XElement xml)
		{
			if (xml.HasAttribute("minValue"))
			{
				double min = 0;
				double.TryParse(xml.GetAttribute("minValue"), out min);
				this.Minimum = min;
			}
			if (xml.HasAttribute("maxValue"))
			{
				double max = 0;
				double.TryParse(xml.GetAttribute("maxValue"), out max);
				this.Maximum = max;
				DoubleCollection dc = new DoubleCollection();
				double d=0;
				while(d < max)
				{
					dc.Add(d);
					d++;
				}
				this.Ticks = dc;
			}
			if (xml.HasAttribute("step"))
			{
				double step = 1;
				double.TryParse(xml.GetAttribute("step"), out step);
				this.TickFrequency = step;
			}
		}
		public XElement DeltaXml
		{
			get
			{
				XElement delta = myHarness.DeltaElement;
				delta.SetAttribute(Common.IDAttrib, FieldId);
				setDeltaText(delta);
				return delta;
			}
		}
		protected virtual void setDeltaText(XElement xml)
		{
            xml.SetAttribute(Common.PublishFieldValue, IntValue.ToString());
		}
        public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}
}
