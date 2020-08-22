//+----------------------------------------------------------------------------------------------+
//| Copyright © <2020>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// Bill William's Awesome Oscillator is a variation of the MACD. It is calculated as the difference from a 5 period and a 34 period simple moving average,
	/// where the moving average calculation uses the midpoints of the price bars instead of the closes. The Awesome Oscillator is used to measure momentum.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Display Options", 1000200)]
	[Gui.CategoryOrder("Oscillator Plot", 8000100)]
	[Gui.CategoryOrder("Signal Line Plot", 8000200)]
	[Gui.CategoryOrder("Zeroline", 8000300)]
	[Gui.CategoryOrder("Version", 8000400)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaAwesomeOscillatorTypeConverter")]
	public class amaAwesomeOscillator : Indicator
	{
		private int					fastPeriod					= 5;
		private int					slowPeriod 					= 34;
		private int					signalPeriod				= 5;
		private double				oscillatorValue				= 0;
		private double				priorOscillatorValue		= 0;
		private bool				showSignal					= true;
		private bool				autoBarWidth				= true;
		private bool				calculateFromPriceData		= true;
		private Brush				upRisingBrush				= Brushes.LimeGreen;
		private Brush				upFallingBrush				= Brushes.DarkGreen;
		private Brush				downRisingBrush				= Brushes.DarkRed;
		private Brush				downFallingBrush			= Brushes.Red;
		private Brush				signalBrush					= Brushes.DarkSlateGray;
		private Brush				zerolineBrush				= Brushes.DarkSlateGray;
		private int 				plot0Width 					= 5;
		private PlotStyle 			plot1Style					= PlotStyle.Line;
		private DashStyleHelper		dash1Style					= DashStyleHelper.Solid;
		private int 				plot1Width 					= 2;
		private int					lineWidth					= 1;
		private DashStyleHelper		lineStyle					= DashStyleHelper.Solid;
		private string				versionString				= "v 1.2  -  March 7, 2020";
		private SMA					fastSMA;
		private SMA					slowSMA;
		private SMA					signal;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nBill William's Awesome Oscillator is a variation of the MACD. It is calculated as the difference from a 5 period and a 34 period simple moving average, "
												+ "where the moving average calculation uses the midpoints of the price bars instead of the closes. The Awesome Oscillator is used to measure momentum.";
				Name						= "amaAwesomeOscillator";
				ArePlotsConfigurable		= false;
				AreLinesConfigurable		= false;
				IsSuspendedWhileInactive 	= true;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Bar, "Oscillator");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Signal");
				AddLine(new Stroke(Brushes.Purple, 1), 0, "Zero Line");
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = Math.Max(fastPeriod, slowPeriod);
				if(autoBarWidth)
					Plots[0].AutoWidth = true;
				else
				{	
					Plots[0].AutoWidth = false;
					Plots[0].Width = plot0Width;
				}	
				if(showSignal)
				{	
					Plots[1].DashStyleHelper = dash1Style;
					Plots[1].Width = plot1Width;
					Plots[1].Brush = signalBrush;
				}
				else
					Plots[1].Brush = Brushes.Transparent;
				Lines[0].Width = lineWidth;
				Lines[0].DashStyleHelper = lineStyle;
				Lines[0].Brush = zerolineBrush;
			}	
			else if (State == State.DataLoaded)
			{
				if(Input is PriceSeries)
				{
					calculateFromPriceData = true;
					fastSMA = SMA(Median, fastPeriod);
					slowSMA = SMA(Median, slowPeriod);
					signal = SMA(Oscillator, signalPeriod);
				}
				else
				{
					calculateFromPriceData = false;
					fastSMA = SMA(Input, fastPeriod);
					slowSMA = SMA(Input, slowPeriod);
					signal = SMA(Oscillator, signalPeriod);
				}	
			}	
		}
		
		protected override void OnBarUpdate()
		{
			oscillatorValue = fastSMA[0] - slowSMA[0];
			Oscillator[0] = oscillatorValue;
			Signal [0] = signal[0];
			
			if(CurrentBar == 0)
				return;
			
			if(IsFirstTickOfBar)
				priorOscillatorValue = Oscillator[1];
			if(oscillatorValue > 0 && oscillatorValue >= priorOscillatorValue)
				PlotBrushes[0][0] = upRisingBrush;
			else if(oscillatorValue > 0)
				PlotBrushes[0][0] = upFallingBrush;
			else if(oscillatorValue < 0 && oscillatorValue <= priorOscillatorValue)
				PlotBrushes[0][0] = downFallingBrush;
			else if(oscillatorValue < 0)
				PlotBrushes[0][0] = downRisingBrush;
			else
				PlotBrushes[0][0] = PlotBrushes[0][1];
		}

		#region Properties
        [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> Oscillator
        {
            get { return Values[0]; }
        }
		
       	[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> Signal
        {
            get { return Values[1]; }
        }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast SMA period",  Description = "Sets period for the fast simple moving average", GroupName = "Input Parameters", Order = 0)]
		public int FastPeriod
		{	
            get { return fastPeriod; }
            set { fastPeriod = value; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow SMA period",  Description = "Sets period for the fast simple moving average", GroupName = "Input Parameters", Order = 1)]
		public int SlowPeriod
		{	
            get { return slowPeriod; }
            set { slowPeriod = value; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal period",  Description = "Sets period for the signal line", GroupName = "Input Parameters", Order = 2)]
		public int SignalPeriod
		{	
            get { return signalPeriod; }
            set { signalPeriod = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show signal line", Description = "Shows signal line", GroupName = "Display Options", Order = 1)]
     	[RefreshProperties(RefreshProperties.All)] 
		public bool ShowSignal
		{
			get { return showSignal; }
			set { showSignal = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish acceleration", Description = "Sets the color for an accelerating bull trend", GroupName = "Oscillator Plot", Order = 0)]
		public System.Windows.Media.Brush UpRisingBrush
		{ 
			get {return upRisingBrush;}
			set {upRisingBrush = value;}
		}

		[Browsable(false)]
		public string UpRisingBrushSerializable
		{
			get { return Serialize.BrushToString(upRisingBrush); }
			set { upRisingBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish retracement", Description = "Sets the color for a retracement during a bull trend", GroupName = "Oscillator Plot", Order = 1)]
		public System.Windows.Media.Brush UpFallingBrush
		{ 
			get {return upFallingBrush;}
			set {upFallingBrush = value;}
		}

		[Browsable(false)]
		public string UpFallingBrushSerializable
		{
			get { return Serialize.BrushToString(upFallingBrush); }
			set { upFallingBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish acceleration", Description = "Sets the color for an accelerating bear trend", GroupName = "Oscillator Plot", Order = 2)]
		public System.Windows.Media.Brush DownFallingBrush
		{ 
			get {return downFallingBrush;}
			set {downFallingBrush = value;}
		}

		[Browsable(false)]
		public string DownFallingBrushSerializable
		{
			get { return Serialize.BrushToString(downFallingBrush); }
			set { downFallingBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish retracement", Description = "Sets the color for a retracement during a bear trend", GroupName = "Oscillator Plot", Order = 3)]
		public System.Windows.Media.Brush DownRisingBrush
		{ 
			get {return downRisingBrush;}
			set {downRisingBrush = value;}
		}

		[Browsable(false)]
		public string DownRisingBrushSerializable
		{
			get { return Serialize.BrushToString(downRisingBrush); }
			set { downRisingBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Auto-adjust bar width", Description = "Auto-adjusts bar width of oscillator to match price bars", GroupName = "Oscillator Plot", Order = 4)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool AutoBarWidth
        {
            get { return autoBarWidth; }
            set { autoBarWidth = value; }
        }
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bar width oscillator", Description = "Sets the bar width for the Awesome Oscillator", GroupName = "Oscillator Plot", Order = 5)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line", Description = "Sets the color for the signal line", GroupName = "Signal Line Plot", Order = 0)]
		public Brush SignalBrush
		{ 
			get {return signalBrush;}
			set {signalBrush = value;}
		}

		[Browsable(false)]
		public string SignalBrushSerializable
		{
			get { return Serialize.BrushToString(signalBrush); }
			set { signalBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style signal", Description = "Sets the plot style for the signal line", GroupName = "Signal Line Plot", Order = 1)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style signal", Description = "Sets the dash style for the signal line", GroupName = "Signal Line Plot", Order = 2)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width signal", Description = "Sets the plot width for the signal line", GroupName = "Signal Line Plot", Order = 3)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Zeroline", Description = "Sets the color for the zeroline", GroupName = "Zeroline", Order = 0)]
		public System.Windows.Media.Brush ZerolineBrush
		{
			get { return zerolineBrush; }
			set { zerolineBrush = value; }
		}

		[Browsable(false)]
		public string ZerolineBrushSerialize
		{
			get { return Serialize.BrushToString(zerolineBrush); }
			set { zerolineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style zeroline", Description = "Sets the dash style for the zeroline", GroupName = "Zeroline", Order = 1)]
		public DashStyleHelper LineStyle
		{
			get { return lineStyle; }
			set { lineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line width zeroline", Description = "Sets the line width for the zeroline", GroupName = "Zeroline", Order = 2)]
		public int LineWidth
		{	
            get { return lineWidth; }
            set { lineWidth = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{		
	public class amaAwesomeOscillatorTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaAwesomeOscillator		thisAwesomeOscillatorInstance		= (amaAwesomeOscillator) value;
			bool						autoBarWidthFromInstance			= thisAwesomeOscillatorInstance.AutoBarWidth;
			bool						showSignalFromInstance				= thisAwesomeOscillatorInstance.ShowSignal;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if (autoBarWidthFromInstance && thisDescriptor.Name == "Plot0Width") 
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showSignalFromInstance && (thisDescriptor.Name == "SignalBrush" || thisDescriptor.Name == "Plot1Style" || thisDescriptor.Name == "Dash1Style" || thisDescriptor.Name == "Plot1Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else	
					adjusted.Add(thisDescriptor);
			}
			return adjusted;
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaAwesomeOscillator[] cacheamaAwesomeOscillator;
		public LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(int fastPeriod, int slowPeriod, int signalPeriod)
		{
			return amaAwesomeOscillator(Input, fastPeriod, slowPeriod, signalPeriod);
		}

		public LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(ISeries<double> input, int fastPeriod, int slowPeriod, int signalPeriod)
		{
			if (cacheamaAwesomeOscillator != null)
				for (int idx = 0; idx < cacheamaAwesomeOscillator.Length; idx++)
					if (cacheamaAwesomeOscillator[idx] != null && cacheamaAwesomeOscillator[idx].FastPeriod == fastPeriod && cacheamaAwesomeOscillator[idx].SlowPeriod == slowPeriod && cacheamaAwesomeOscillator[idx].SignalPeriod == signalPeriod && cacheamaAwesomeOscillator[idx].EqualsInput(input))
						return cacheamaAwesomeOscillator[idx];
			return CacheIndicator<LizardIndicators.amaAwesomeOscillator>(new LizardIndicators.amaAwesomeOscillator(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod, SignalPeriod = signalPeriod }, input, ref cacheamaAwesomeOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(int fastPeriod, int slowPeriod, int signalPeriod)
		{
			return indicator.amaAwesomeOscillator(Input, fastPeriod, slowPeriod, signalPeriod);
		}

		public Indicators.LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(ISeries<double> input , int fastPeriod, int slowPeriod, int signalPeriod)
		{
			return indicator.amaAwesomeOscillator(input, fastPeriod, slowPeriod, signalPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(int fastPeriod, int slowPeriod, int signalPeriod)
		{
			return indicator.amaAwesomeOscillator(Input, fastPeriod, slowPeriod, signalPeriod);
		}

		public Indicators.LizardIndicators.amaAwesomeOscillator amaAwesomeOscillator(ISeries<double> input , int fastPeriod, int slowPeriod, int signalPeriod)
		{
			return indicator.amaAwesomeOscillator(input, fastPeriod, slowPeriod, signalPeriod);
		}
	}
}

#endregion
