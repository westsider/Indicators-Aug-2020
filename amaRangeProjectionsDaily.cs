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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion


// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Daily Range Projections indicator displays support and resistance levels based on the volatility of the prior N days. The noise bands are showing how far noise traders typically drive prices.
	/// A breakout from the noise bands needs the participation of higher timeframe traders. Expansion bands or average daily range projections may be used as daily targets. The indicator further allows 
	///	for displaying the opening gap, which may be optionally calculated from the daily close.
	/// </summary>
	///
	[Gui.CategoryOrder("Algorithmic Options", 1000100)]
	[Gui.CategoryOrder("Input Parameters", 1000200)]
	[Gui.CategoryOrder("Display Options", 1000300)]
	[Gui.CategoryOrder("Plot Colors", 8000100)]
	[Gui.CategoryOrder("Plot Parameters", 8000200)]
	[Gui.CategoryOrder("Shading", 8000300)]
	[Gui.CategoryOrder("Data Box", 8000400)]
	[Gui.CategoryOrder("Version", 8000500)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaRangeProjectionsDailyTypeConverter")]
	public class amaRangeProjectionsDaily : Indicator
	{
		private DateTime							sessionDateTmp0					= Globals.MinDate;
		private DateTime							cacheSessionDate0				= Globals.MinDate;
		private DateTime							sessionDateTmp1					= Globals.MinDate;
		private DateTime							dailyBarDate					= Globals.MinDate;
		private DateTime							priorDailyBarDate				= Globals.MinDate;
		private double								percentageADN					= 100.0;
		private double								percentageADE					= 100.0;
		private double								percentageADR					= 100.0;
		private double								multiplierADN					= 0.0;
		private double								multiplierADE					= 0.0;
		private double								multiplierADR					= 0.0;
		private double								currentDailyOpen				= 0.0;
		private double								currentDailyHigh				= 0.0;
		private double								currentDailyLow					= 0.0;
		private double								currentDailyClose				= 0.0;
		private double								priorDailyOpen					= 0.0;
		private double								priorDailyHigh					= 0.0;
		private double								priorDailyLow					= 0.0;
		private double								priorDailyClose					= 0.0;
		private double								priorOpen						= 0.0;
		private double								priorHigh						= 0.0;
		private double								priorLow						= 0.0;
		private double								priorClose						= 0.0;
		private double								currentOpen						= 0.0;
		private double								currentHigh						= 0.0;
		private double								currentLow						= 0.0;
		private double								currentClose					= 0.0;
		private double								currentRange					= 0.0;
		private double								priorBarMedian					= 0.0;
		private double								averageDailyNoise				= 0.0;
		private double								averageDailyExpansion			= 0.0;
		private double								averageRange					= 0.0;
		private double								expansionWidth					= 0.0;
		private double								maxRange						= 0.0;
		private double								displaySize						= 0.0;
		private int									referencePeriod					= 20;
		private int									aggregatePeriod					= 0;
		private int									displacement					= 0;
		private int									count							= 0;
		private int									cacheLastBarPainted				= 0;
		private bool								showPriorHighLow				= true;
		private bool								showPriorClose					= true;
		private bool								showCurrentOpen					= true;
		private bool								showGap							= true;
		private bool								showNoiseLevels					= true;
		private bool								showExpansionLevels				= true;
		private bool								showADRLevels					= false;
		private bool								showNoiseBands					= true;
		private bool								showExpansionBands				= true;
		private bool								showADRBands					= false;
		private bool								showLabels						= true;
		private bool								showDataBox						= true;
		private bool								isIntraday0						= true;
		private bool								calcOpen						= false;
		private bool								continuationTick				= false;
		private bool								timeBased0						= false;
		private bool								breakAtEOD						= true;
		private bool								errorMessage					= true;
		private bool								basicError						= false;
		private bool								sundaySessionError				= false;
		private bool								isCryptoCurrency				= false;
		private bool								dailyBarsError					= false;
		private bool								initBarSeries0					= false;
		private bool								calculateFromPriceData			= true;
		private bool								calculateFromIntradayData		= false;
		private bool								isCurrencyFuture1				= false;
		private bool								isCurrencyFuture2				= false;
		private bool								isForex							= false;
		private bool								lastBarStartsNewPeriod			= false;
		private amaSessionTypeRPRD					sessionType						= amaSessionTypeRPRD.Daily_Bars;
		private amaCalcModeRPRD						calcMode						= amaCalcModeRPRD.Daily_Data;
		private amaDataBoxLocationRPRD				dataBoxLocation					= amaDataBoxLocationRPRD.Left;
		private List<double>						openArray						= new List<double>();
		private List<double>						highArray						= new List<double>();
		private List<double>						lowArray						= new List<double>();
		private readonly List<int>					newSessionBarIdxArr				= new List<int>();
		private SessionIterator						sessionIterator0				= null;
		private System.Windows.Media.Brush			priorHighBrush					= Brushes.DarkGreen;
		private System.Windows.Media.Brush			priorLowBrush 					= Brushes.Firebrick;
		private System.Windows.Media.Brush			priorCloseBrush 				= Brushes.DarkOrange;
		private System.Windows.Media.Brush			currentOpenBrush 				= Brushes.SaddleBrown;
		private System.Windows.Media.Brush			noiseLevelBrush					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush			expansionLevelBrush 			= Brushes.Navy;
		private System.Windows.Media.Brush			targetLevelBrush 				= Brushes.Purple;
		private System.Windows.Media.Brush			gapBrushS						= Brushes.Goldenrod;
		private System.Windows.Media.Brush			gapBrush						= null;
		private System.Windows.Media.Brush			noiseBandBrush					= null;
		private System.Windows.Media.Brush			expansionBandBrush				= null;
		private System.Windows.Media.Brush			targetBandBrush					= null;
		private System.Windows.Media.Brush			dataBoxTextBrush				= Brushes.White;
		private System.Windows.Media.Brush			dataBoxBackBrushS				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush			dataBoxBackBrush				= null;
		private System.Windows.Media.Brush			errorBrush						= Brushes.Black;
		private	SharpDX.Direct2D1.Brush 			gapBrushDX						= null;
		private	SharpDX.Direct2D1.Brush 			noiseBandBrushDX				= null;
		private	SharpDX.Direct2D1.Brush 			expansionBandBrushDX			= null;
		private	SharpDX.Direct2D1.Brush 			targetBandBrushDX				= null;
		private	SharpDX.Direct2D1.Brush 			dataBoxTextBrushDX				= null;
		private	SharpDX.Direct2D1.Brush 			dataBoxBackBrushDX				= null;
		private	SharpDX.Direct2D1.Brush 			adr0BrushDX						= null;
		private	SharpDX.Direct2D1.Brush 			adr1BrushDX						= null;
		private	SharpDX.Direct2D1.Brush 			adr3BrushDX						= null;
		private	SharpDX.Direct2D1.Brush 			adr5BrushDX						= null;
		private	SharpDX.Direct2D1.Brush 			adr10BrushDX					= null;
		private	SharpDX.Direct2D1.Brush 			adr20BrushDX					= null;
		private	SharpDX.Direct2D1.SolidColorBrush 	transparentBrushDX				= null;
		private	SharpDX.Direct2D1.Brush[] 			brushesDX;
		private SimpleFont							errorFont						= null;
		private string[]							symbolArray1					= new string[]{"6A","6B","6M","6N","6S"};
		private string[]							symbolArray2					= new string[]{"6C","6E","6J", "6L"};
		private string								errorText1						= "The amaRangeProjectionsDaily indicator only works on price data.";
		private string								errorText2						= "The amaRangeProjectionsDaily indicator cannot be used on weekly or monthly charts.";
		private string								errorText3						= "The amaRangeProjectionsDaily indicator cannot be used with a displacement.";
		private string								errorText4						= "The amaRangeProjectionsDaily indicator cannot be used when the 'Break at EOD' data series property is unselected.";
		private string								errorText5						= "The amaRangeProjectionsDaily indicator requires the setting 'DailyBars' when it is used on daily charts.";
		private string								errorText6						= "amaRangeProjectionsDaily: Projections may not be calculated from fractional Sunday sessions. Please change your trading hours template.";
		private string								errorText7						= "amaRangeProjectionsDaily: Insufficient daily data! Please reload daily data or calculate pivots from intraday data.";
		private string								errorText8						= "amaRangeProjectionsDaily: Insufficient historical data. Please increase the chart look back period.";
		private int									noiseBandOpacity				= 80;
		private int									expansionBandOpacity			= 60;
		private int									targetBandOpacity				= 60;
		private int									gapOpacity						= 40;
		private int									dataBoxOpacity					= 100;
		private int									plot0Width						= 3;
		private int									plot1Width						= 3;
		private int									plot2Width						= 2;
		private int									plot3Width						= 2;
		private int									plot4Width						= 2;
		private int									plot5Width						= 2;
		private int									labelFontSize					= 14;
		private int									shiftLabelOffset				= 10;
		private int									dataBoxFontSize					= 14;
		private int									dataBoxOffsetRight				= 40;
		private int									dataBoxOffsetUpperRight			= 10;
		private int									dataBoxOffsetLeft				= 10;
		private int									dataBoxOffsetUpperLeft			= 50;
		private PlotStyle							plot0Style						= PlotStyle.Line;
		private DashStyleHelper						dash0Style						= DashStyleHelper.Dot;
		private PlotStyle							plot1Style						= PlotStyle.Line;
		private DashStyleHelper						dash1Style						= DashStyleHelper.Dot;
		private PlotStyle							plot2Style						= PlotStyle.Line;
		private DashStyleHelper						dash2Style						= DashStyleHelper.Dot;
		private PlotStyle							plot3Style						= PlotStyle.Line;
		private DashStyleHelper						dash3Style						= DashStyleHelper.Solid;
		private PlotStyle							plot4Style						= PlotStyle.Line;
		private DashStyleHelper						dash4Style						= DashStyleHelper.Solid;
		private PlotStyle							plot5Style						= PlotStyle.Line;
		private DashStyleHelper						dash5Style						= DashStyleHelper.Solid;
		private string								versionString					= "v 3.2  -  March 9, 2020";
		private Series<DateTime>					tradingDate0;
		private Series<DateTime>					tradingDate1;
		private Series<bool>						dailyProjectionReached;
		private Series<int>							countDown;
		private Series<double>						currentHistoricalOpen;
		private Series<double>						currentHistoricalHigh;
		private Series<double>						currentHistoricalLow;
		private Series<double>						currentHistoricalClose;
		private Series<double>						currentPrimaryBarsOpen;
		private Series<double>						currentPrimaryBarsHigh;
		private Series<double>						currentPrimaryBarsLow;
		private Series<double>						currentPrimaryBarsClose;
		private Series<double>						averageDailyRange;
		private Series<double>						bandWidth;
		private Series<double>						adr0;
		private Series<double>						adr1;
		private Series<double>						adr3;
		private Series<double>						adr5;
		private Series<double>						adr10;
		private Series<double>						adr20;
		private Series<double>						adrMax;
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Daily Range Projections indicator displays support and resistance levels based on the volatility of the prior N days. The noise bands are showing how far"
												+ " noise traders typically drive prices. A breakout from the noise bands needs the participation of higher timeframe traders. Expansion bands or average daily range"
												+ "  projections may be used as daily targets. The indicator further allows for displaying the opening gap, which may be optionally calculated from the daily close.";
				Name						= "amaRangeProjectionsDaily";
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				IsAutoScale					= false;
				ArePlotsConfigurable		= false;
				if(Calculate == Calculate.OnEachTick)
					Calculate = Calculate.OnPriceChange;
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-Low");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-Close");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"D-Open");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADN-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADN-Low");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADE-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADE-Low");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADR-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"ADR-Low");
				SetZOrder(-2);
			}
			else if (State == State.Configure)
			{
				if(sessionType == amaSessionTypeRPRD.Daily_Bars || calcMode == amaCalcModeRPRD.Daily_Data)
					AddDataSeries(BarsPeriodType.Day, 1);
				BarsRequiredToPlot = 3;
				displacement = Displacement;
				noiseBandBrush = noiseLevelBrush.Clone();
				noiseBandBrush.Opacity = (float) noiseBandOpacity/100.0;
				noiseBandBrush.Freeze();
				expansionBandBrush = expansionLevelBrush.Clone();
				expansionBandBrush.Opacity = (float) expansionBandOpacity/100.0;
				expansionBandBrush.Freeze();
				targetBandBrush = targetLevelBrush.Clone();
				targetBandBrush.Opacity = (float) targetBandOpacity/100.0;
				targetBandBrush.Freeze();
				gapBrush = gapBrushS.Clone();
				gapBrush.Opacity = (float) gapOpacity/100.0;
				gapBrush.Freeze();
				dataBoxBackBrush = dataBoxBackBrushS.Clone();
				dataBoxBackBrush.Opacity = (float) dataBoxOpacity/100.0;
				dataBoxBackBrush.Freeze();
				if(showPriorHighLow)
				{	
					Plots[0].Brush = priorHighBrush;
					Plots[1].Brush = priorLowBrush;
				}	
				else
				{	
					Plots[0].Brush = Brushes.Transparent;
					Plots[1].Brush = Brushes.Transparent;
				}
				if(showPriorClose)
					Plots[2].Brush = priorCloseBrush;
				else
					Plots[2].Brush = Brushes.Transparent;
				if(showCurrentOpen)
					Plots[3].Brush = currentOpenBrush;
				else
					Plots[3].Brush = Brushes.Transparent;
				if(showNoiseLevels)
				{	
					Plots[4].Brush = noiseLevelBrush;
					Plots[5].Brush = noiseLevelBrush;
				}
				else
				{	
					Plots[4].Brush = Brushes.Transparent;
					Plots[5].Brush = Brushes.Transparent;
				}
				if(showExpansionLevels)
				{	
					Plots[6].Brush = expansionLevelBrush;
					Plots[7].Brush = expansionLevelBrush;
				}
				else
				{	
					Plots[6].Brush = Brushes.Transparent;
					Plots[7].Brush = Brushes.Transparent;
				}
				if(showADRLevels)
				{	
					Plots[8].Brush = targetLevelBrush;
					Plots[9].Brush = targetLevelBrush;
				}
				else
				{	
					Plots[8].Brush = Brushes.Transparent;
					Plots[9].Brush = Brushes.Transparent;
				}
				for (int i = 0; i < 2; i++)
				{
					Plots[i].Width = plot0Width;
					Plots[i].DashStyleHelper = dash0Style;
				}
				Plots[2].Width = plot1Width;
				Plots[2].DashStyleHelper = dash1Style;
				Plots[3].Width = plot2Width;
				Plots[3].DashStyleHelper = dash2Style;
				for (int i = 4; i < 6; i++)
				{
					Plots[i].Width = plot3Width;
					Plots[i].DashStyleHelper = dash3Style;
				}
				for (int i = 6; i < 8; i++)
				{
					Plots[i].Width = plot4Width;
					Plots[i].DashStyleHelper = dash4Style;
				}
				for (int i = 8; i < 10; i++)
				{
					Plots[i].Width = plot5Width;
					Plots[i].DashStyleHelper = dash5Style;
				}
				Plots[0].Name = "Y-High";
				Plots[1].Name = "Y-Low";
				Plots[2].Name = "Y-Close";
				if(showNoiseLevels)
				{	
					Plots[4].Name = "ADN" + referencePeriod + "-High";
					Plots[5].Name = "ADN" + referencePeriod + "-Low";
				}	
				if(showExpansionLevels)
				{	
					Plots[6].Name = "ADE" + referencePeriod + "-High";
					Plots[7].Name = "ADE" + referencePeriod + "-Low";
				}	
				if(showADRLevels)
				{	
					Plots[8].Name = "ADR" + referencePeriod + "-High";
					Plots[9].Name = "ADR" + referencePeriod + "-Low";
				}	
				brushesDX = new SharpDX.Direct2D1.Brush[Values.Length];
			}
		  	else if (State == State.DataLoaded)
		 	{
				if(BarsArray[0].BarsType.IsIntraday)
					isIntraday0 = true;
				else
					isIntraday0 = false;
				tradingDate0 = new Series<DateTime>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				dailyProjectionReached = new Series<bool>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				countDown = new Series<int>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				if(isIntraday0 && (sessionType == amaSessionTypeRPRD.Daily_Bars || calcMode == amaCalcModeRPRD.Daily_Data))
				{
					tradingDate1 = new Series<DateTime>(BarsArray[1], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalOpen = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalHigh = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalLow = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalClose = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				}
				currentPrimaryBarsOpen = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				currentPrimaryBarsHigh = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				currentPrimaryBarsLow = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				currentPrimaryBarsClose = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				averageDailyRange = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				bandWidth = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr0 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr1 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr3 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr5 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr10 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adr20 = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				adrMax = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex && (TickSize == 0.00001 || TickSize == 0.001))
					displaySize = 5 * TickSize;
				else
					displaySize = TickSize;
				if (BarsArray[0].BarsType.IsTimeBased) 
					timeBased0 = true;
				else
					timeBased0 = false;
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
				if(calcMode	!= amaCalcModeRPRD.Daily_Data && sessionType != amaSessionTypeRPRD.Daily_Bars)
					calculateFromIntradayData = true;
				else
					calculateFromIntradayData = false;
		    	sessionIterator0 = new SessionIterator(BarsArray[0]);
		  	}
			else if (State == State.Historical)
			{
				aggregatePeriod = Math.Max(20, referencePeriod);
				for (int i=0; i < aggregatePeriod; i++)
				{
					openArray.Add(0.0);
					highArray.Add(double.MinValue);
					lowArray.Add(double.MaxValue);
				}
				multiplierADN = percentageADN / 100.0;
				multiplierADE = percentageADE / 100.0;
				multiplierADR = percentageADR / 100.0;
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex)
					isForex = true;
				else
					isForex = false;
				isCurrencyFuture1 = false;
				for(int i = 0; i < 5; i++)
				{	
					if(symbolArray1[i] == Instrument.MasterInstrument.Name)	
						isCurrencyFuture1 = true; 
				}
				isCurrencyFuture2 = false;
				for(int i = 0; i < 4; i++)
				{	
					if(symbolArray2[i] == Instrument.MasterInstrument.Name)	
						isCurrencyFuture2 = true; 
				}
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency)
					isCryptoCurrency = true;
				else
					isCryptoCurrency = false;
				if(ChartBars != null)
				{	
					breakAtEOD = ChartBars.Bars.IsResetOnNewTradingDay;
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
				}
				errorMessage = false;
				if(!calculateFromPriceData)
				{
					Draw.TextFixed(this, "error text 1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}	
				else if(BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Week || BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Month)
				{
					Draw.TextFixed(this, "error text 2", errorText2, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if(displacement != 0)
				{
					Draw.TextFixed(this, "error text 3", errorText3, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if(!breakAtEOD)
				{
					Draw.TextFixed(this, "error text 4", errorText4, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if (!isIntraday0 && sessionType != amaSessionTypeRPRD.Daily_Bars)
				{
					Draw.TextFixed(this, "error text 5", errorText5, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
			}	
		}

		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 0)
			{
				if(IsFirstTickOfBar)
				{	
					if(errorMessage)
					{	
						if(basicError)
							return;
						else if(sundaySessionError)
						{	
							Draw.TextFixed(this, "error text 6", errorText6, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
							return;
						}	
						else if(dailyBarsError)
						{	
							Draw.TextFixed(this, "error text 7", errorText7, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
							return;
						}
					}	
				}	
					
				if (!initBarSeries0 || CurrentBars[0] == 0)
				{	
					if(!initBarSeries0)
					{
						tradingDate0[0] = GetLastBarSessionDate0(Times[0][0]);
						dailyProjectionReached[0] = false;
						countDown[0] = referencePeriod + 1;
						if(isIntraday0 && (sessionType == amaSessionTypeRPRD.Daily_Bars || calcMode == amaCalcModeRPRD.Daily_Data))
						{	
							currentHistoricalOpen.Reset();
							currentHistoricalHigh.Reset();
							currentHistoricalLow.Reset();
							currentHistoricalClose.Reset();
						}	
						currentPrimaryBarsOpen.Reset();
						currentPrimaryBarsHigh.Reset();
						currentPrimaryBarsLow.Reset();
						currentPrimaryBarsClose.Reset();
						PriorHigh.Reset();
						PriorLow.Reset();
						PriorClose.Reset();
						CurrentOpen.Reset();
						ADN_High.Reset();
						ADN_Low.Reset();
						ADE_High.Reset();
						ADE_Low.Reset();
						ADR_High.Reset();
						ADR_Low.Reset();
						averageDailyRange.Reset();
						bandWidth.Reset();
						ADR0.Reset();
						ADR1.Reset();
						ADR3.Reset();
						ADR5.Reset();
						ADR10.Reset();
						ADR20.Reset();
						adrMax.Reset();
						bandWidth.Reset();
						initBarSeries0 = true;
					}	
					return;
				}
				
				if(IsFirstTickOfBar)
				{	
					if(BarsArray[0].IsFirstBarOfSession || !isIntraday0)
					{	
						tradingDate0[0] = GetLastBarSessionDate0(Times[0][0]);
						if(tradingDate0[0].DayOfWeek == DayOfWeek.Sunday)
						{
							if(!isCryptoCurrency)
							{	
								sundaySessionError = true; 
								errorMessage = true;
								return;
							}	
						}
						if(tradingDate0[0] != tradingDate0[1])
						{
							if((sessionType == amaSessionTypeRPRD.Daily_Bars || calcMode == amaCalcModeRPRD.Daily_Data) && CurrentBars[1] < 0)
								dailyBarsError = true;
							else
								dailyBarsError = false;
							dailyProjectionReached[0] = false;
							countDown[0] = countDown[1] - 1; 
							if(countDown[0] < referencePeriod) // calculations are only performed for the third day with intraday data (first day maybe incomplete, second day is needed for collecting data)
							{	
								if (dailyBarsError)
								{
									errorMessage = true;
									return;
								}	
								if(!isIntraday0) // daily projections displayed on a daily chart
								{
									priorOpen = currentPrimaryBarsOpen[1];
									priorHigh = currentPrimaryBarsHigh[1];
									priorLow = currentPrimaryBarsLow[1];
									priorClose = currentPrimaryBarsClose[1];
								}
								else if(sessionType == amaSessionTypeRPRD.Daily_Bars) // daily projections calculated from daily data  
								{	
									if(IsConnected())
									{
										if(tradingDate0[0] > tradingDate1[0]) 
										{ 
											priorOpen = currentDailyOpen;
											priorHigh = currentDailyHigh;	
											priorLow = currentDailyLow;
											priorClose = currentDailyClose;
										}
										else
										{
											priorOpen = priorDailyOpen;
											priorHigh = priorDailyHigh;	
											priorLow = priorDailyLow;
											priorClose = priorDailyClose;
										}	
									}
									else // workaround needed because of NinjaTrader bar processing bug
									{
										priorOpen = currentHistoricalOpen[1];
										priorHigh = currentHistoricalHigh[1];
										priorLow = currentHistoricalLow[1];
										priorClose = currentHistoricalClose[1];
										count = 0;
										priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1]));
										dailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][0]));
										if(priorDailyBarDate == dailyBarDate && BarsArray[1].Count - 2 > CurrentBars[1])
										{	
											while (priorDailyBarDate >= dailyBarDate)
											{	
												count = count - 1;
												priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
												if (count == -5)
													break;
											}	
											priorOpen = BarsArray[1].GetOpen(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
											priorHigh = BarsArray[1].GetHigh(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
											priorLow = BarsArray[1].GetLow(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
											priorClose = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
										}	
									}	
								}
								else if(calcMode == amaCalcModeRPRD.Daily_Data) // daily projections calculated with highs and low taken from intraday data, but daily close taken from daily data
								{
									priorOpen = currentPrimaryBarsOpen[1];
									priorHigh = currentPrimaryBarsHigh[1];
									priorLow = currentPrimaryBarsLow[1];
									if(IsConnected())
									{	
										if(tradingDate0[0] > tradingDate1[0]) 
											priorClose = currentDailyClose;
										else
											priorClose = priorDailyClose;
									}	
									else // workaround needed because of NinjaTrader bar processing bug
									{
										priorClose = currentHistoricalClose[1];
										count = 0;
										priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1]));
										dailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][0]));
										if(priorDailyBarDate == dailyBarDate && BarsArray[1].Count - 2 > CurrentBars[1])
										{	
											while (priorDailyBarDate >= dailyBarDate)
											{	
												count = count - 1;
												priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
												if (count == -5)
													break;
											}	
											priorClose = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
										}	
									}	
								}	
								else // daily projections calculated from intraday data
								{
									priorOpen = currentPrimaryBarsOpen[1];	
									priorHigh = currentPrimaryBarsHigh[1];	
									priorLow = currentPrimaryBarsLow[1];
									priorClose = currentPrimaryBarsClose[1];
								}
								for(int i = aggregatePeriod - 1; i > 0; i--)
								{	
									openArray[i] = openArray[i-1];
									highArray[i] = highArray[i-1];
									lowArray[i] = lowArray[i-1];
								}
								openArray[0] = priorOpen;
								highArray[0] = priorHigh;
								lowArray[0] = priorLow;
								averageDailyNoise = 0.0;
								averageDailyExpansion = 0.0;
								averageRange = 0.0;
								for (int i = 0; i < referencePeriod; i++)
								{
									averageDailyNoise = averageDailyNoise + Math.Min(highArray[i] - openArray[i], openArray[i] - lowArray[i]);
									averageDailyExpansion = averageDailyExpansion + Math.Max(highArray[i] - openArray[i], openArray[i] - lowArray[i]);
									averageRange = averageRange + highArray[i] - lowArray[i];
								}
								averageDailyNoise = multiplierADN * averageDailyNoise / referencePeriod;
								averageDailyExpansion = multiplierADE * averageDailyExpansion / referencePeriod;	
								expansionWidth = 0.1 * averageDailyNoise;
								
								//Calculating projection levels
								currentOpen = Opens[0][0];
								
								if(countDown[0] <= 0)
								{	
									PriorHigh[0] = priorHigh;
									PriorLow[0] = priorLow;
									PriorClose[0] = priorClose;
									CurrentOpen[0] = currentOpen;
									ADN_High[0] = currentOpen + averageDailyNoise;
									ADN_Low[0] = currentOpen - averageDailyNoise;
									ADE_High[0] = currentOpen + averageDailyExpansion;
									ADE_Low[0] = currentOpen - averageDailyExpansion;
									averageDailyRange[0] = multiplierADR * averageRange / referencePeriod;
									bandWidth[0] = expansionWidth;
								}	
								if(countDown[0] <= referencePeriod - 20)
								{
									averageRange = 0;
									for (int i = 0; i < 20; i++)
									{
										averageRange = averageRange + highArray[i] - lowArray[i];
										if(i == 0)
											ADR1[0] = averageRange;
										else if (i == 2)
										{	
											ADR3[0] = averageRange / (i+1);
											maxRange = Math.Max(ADR1[0], ADR3[0]);
										}	
										else if (i == 4)
										{	
											ADR5[0] = averageRange / (i+1);
											maxRange = Math.Max(maxRange, ADR5[0]);
										}	
										else if (i == 9)
										{	
											ADR10[0] = averageRange / (i+1);
											maxRange = Math.Max(maxRange, ADR10[0]);
										}	
										else if (i == 19)
										{	
											ADR20[0] = averageRange / (i+1);
											adrMax[0] = Math.Max(maxRange, ADR20[0]);
										}	
									}	
								}	
							}
						}	
						else // case where Bars.IsFirstBarOfSession is true inside the trading day
						{	
							tradingDate0[0] = tradingDate0[1];
							dailyProjectionReached[0] = dailyProjectionReached[1];
							countDown[0] = countDown[1];
							if (countDown[0] <= 0)
							{
								PriorHigh[0] = PriorHigh[1];
								PriorLow[0] = PriorLow[1];
								PriorClose[0] = PriorClose[1];
								CurrentOpen[0] = CurrentOpen[1];
								ADN_High[0] = ADN_High[1];
								ADN_Low[0] = ADN_Low[1];
								ADE_High[0] = ADE_High[1];
								ADE_Low[0] = ADE_Low[1];
								averageDailyRange[0] = averageDailyRange[1];
								bandWidth[0] = bandWidth[1];
							}	
							if(countDown[0] <= referencePeriod - 20)
							{
								ADR1[0] = ADR1[1];
								ADR3[0] = ADR3[1];
								ADR5[0] = ADR5[1];
								ADR10[0] = ADR10[1];
								ADR20[0] = ADR20[1];
								adrMax[0] = adrMax[1];
							}	
						}	
					}
					else // case where Bars.IsFirstBarOfSession is false
					{	
						tradingDate0[0] = tradingDate0[1];
						dailyProjectionReached[0] = dailyProjectionReached[1];
						countDown[0] = countDown[1];
						if (countDown[0] <= 0)
						{
							PriorHigh[0] = PriorHigh[1];
							PriorLow[0] = PriorLow[1];
							PriorClose[0] = PriorClose[1];
							CurrentOpen[0] = CurrentOpen[1];
							ADN_High[0] = ADN_High[1];
							ADN_Low[0] = ADN_Low[1];
							ADE_High[0] = ADE_High[1];
							ADE_Low[0] = ADE_Low[1];
							averageDailyRange[0] = averageDailyRange[1];
							bandWidth[0] = bandWidth[1];
						}	
						if(countDown[0] <= referencePeriod - 20)
						{
							ADR1[0] = ADR1[1];
							ADR3[0] = ADR3[1];
							ADR5[0] = ADR5[1];
							ADR10[0] = ADR10[1];
							ADR20[0] = ADR20[1];
							adrMax[0] = adrMax[1];
						}	
					}
					
					if(countDown[0] > referencePeriod)
						ADR0.Reset();
					if(countDown[0] > 0)
					{
						PriorHigh.Reset();
						PriorLow.Reset();
						PriorClose.Reset();
						CurrentOpen.Reset();
						ADN_High.Reset();
						ADN_Low.Reset();
						ADE_High.Reset();
						ADE_Low.Reset();
						ADR_High.Reset();
						ADR_Low.Reset();
						averageDailyRange.Reset();
						bandWidth.Reset();
					}
					if(countDown[0] > referencePeriod - 20)
					{
						ADR1.Reset();
						ADR3.Reset();
						ADR5.Reset();
						ADR10.Reset();
						ADR20.Reset();
						adrMax.Reset();
					}	
					
					// needed as a workaround for NinjaTrader bar processing bug
					if(isIntraday0 && (sessionType == amaSessionTypeRPRD.Daily_Bars || calcMode == amaCalcModeRPRD.Daily_Data) && CurrentBars[1] >= 0)  
					{	
						if(CurrentBars[1] >= 0)
						{	
							currentHistoricalOpen[0] = BarsArray[1].GetOpen(BarsArray[1].GetBar(Times[0][0]));
							currentHistoricalHigh[0] = BarsArray[1].GetHigh(BarsArray[1].GetBar(Times[0][0]));
							currentHistoricalLow[0] = BarsArray[1].GetLow(BarsArray[1].GetBar(Times[0][0]));
							currentHistoricalClose[0] = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][0]));
						}
						else
						{
							currentHistoricalOpen.Reset();
							currentHistoricalHigh.Reset();
							currentHistoricalLow.Reset();
							currentHistoricalClose.Reset();
						}	
					}
				}	
				
				if (tradingDate0[0] != tradingDate0[1])
				{	
					currentOpen 	= Opens[0][0];
					currentHigh		= Highs[0][0];
					currentLow 		= Lows[0][0];
					currentClose	= Closes[0][0];
					currentRange	= currentHigh - currentLow;
				}
				else 
				{	
					currentOpen		= currentPrimaryBarsOpen[1];
					currentHigh		= Math.Max(currentPrimaryBarsHigh[1], Highs[0][0]);
					currentLow 		= Math.Min(currentPrimaryBarsLow[1], Lows[0][0]);
					currentClose	= Closes[0][0];
					currentRange	= currentHigh - currentLow;
				}
				currentPrimaryBarsOpen[0] = currentOpen; 
				currentPrimaryBarsHigh[0] = currentHigh;
				currentPrimaryBarsLow[0] = currentLow;
				currentPrimaryBarsClose[0] = currentClose;
				
				if(countDown[0] <= referencePeriod)
					ADR0[0] = currentRange;
				if(countDown[0] <= 0)
				{	
					averageRange = averageDailyRange[0];
					if(currentRange <= averageRange)
					{	
						ADR_High[0] = currentPrimaryBarsLow[0] + averageRange;
						ADR_Low[0] = currentPrimaryBarsHigh[0] - averageRange;
					}
					else if(tradingDate0[0] != tradingDate0[1])
					{	
						if(currentHigh > currentOpen + 0.5 * averageRange && currentLow < currentOpen - 0.5 * averageRange)
						{	
							ADR_High[0] = currentOpen + 0.5 * averageRange;
							ADR_Low[0] = currentOpen - 0.5 * averageRange;
						}	
						else if (currentHigh > currentOpen + 0.5 *averageRange)
						{
							ADR_High[0] = currentLow + averageRange;
							ADR_Low[0] = currentLow;
						}	
						else if(currentLow < currentOpen - 0.5 * averageRange)  
						{
							ADR_High[0] = currentHigh;
							ADR_Low[0] = currentHigh - averageRange;
						}
						dailyProjectionReached[0] = true;
					}
					else if (!dailyProjectionReached[1])
					{	
						if(currentHigh > currentPrimaryBarsHigh[1] && currentLow < currentPrimaryBarsLow[1])
						{
							priorBarMedian = 0.5 * (currentPrimaryBarsHigh[1]  + currentPrimaryBarsLow[1]);
							if(currentHigh > priorBarMedian + 0.5 * averageRange && currentLow < priorBarMedian - 0.5 * averageRange)
							{	
								ADR_High[0] = priorBarMedian + 0.5 * averageRange;
								ADR_Low[0] = priorBarMedian - 0.5 * averageRange;
							}	
							else if (currentHigh > priorBarMedian + 0.5 *averageRange)
							{
								ADR_High[0] = currentLow + averageRange;
								ADR_Low[0] = currentLow;
							}	
							else if(currentLow < priorBarMedian - 0.5 * averageRange)  
							{
								ADR_High[0] = currentHigh;
								ADR_Low[0] = currentHigh - averageRange;
							}
						}
						else if (currentHigh > currentPrimaryBarsHigh[1])
						{
							ADR_High[0] = currentPrimaryBarsLow[1] + averageRange;
							ADR_Low[0] = currentPrimaryBarsLow[1];
						}	
						else if (currentLow < currentPrimaryBarsLow[1])
						{
							ADR_High[0] = currentPrimaryBarsHigh[1];
							ADR_Low[0] = currentPrimaryBarsHigh[1] - averageRange;
						}	
						dailyProjectionReached[0] = true;
					}
					else if (dailyProjectionReached[1])	
					{	
						ADR_High[0] = ADR_High[1];
						ADR_Low[0] = ADR_Low[1];
					}
				}	
			}	
				
			if(BarsInProgress == 1 && isIntraday0)
			{
				if(CurrentBars[1] == 0)
				{	
					if(IsFirstTickOfBar)
						tradingDate1[0] = GetLastBarSessionDate1(Times[1][0]);
					currentDailyOpen	= Opens[1][0];
					currentDailyHigh	= Highs[1][0];
					currentDailyLow	 	= Lows[1][0];
					currentDailyClose	= Closes[1][0];
					return;
				}	
				
				if(IsFirstTickOfBar)
				{	
					tradingDate1[0] = GetLastBarSessionDate1(Times[1][0]);
					if(tradingDate1[0] != tradingDate1[1])
					{	
						priorDailyOpen 		= currentDailyOpen;
						priorDailyHigh 		= currentDailyHigh;
						priorDailyLow  		= currentDailyLow;
						priorDailyClose 	= currentDailyClose;
						currentDailyOpen	= Opens[1][0];
						currentDailyHigh	= Highs[1][0];
						currentDailyLow	 	= Lows[1][0];
						currentDailyClose	= Closes[1][0];
					}
					else
					{
						currentDailyHigh	= Math.Max(currentDailyHigh, Highs[1][0]);
						currentDailyLow	 	= Math.Min(currentDailyLow, Lows[1][0]);
						currentDailyClose	= Closes[1][0];
					}
				}	
				else
				{
					currentDailyHigh	= Math.Max(currentDailyHigh, Highs[1][0]);
					currentDailyLow	 	= Math.Min(currentDailyLow, Lows[1][0]);
					currentDailyClose	= Closes[1][0];
				}
			}
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorHigh
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorLow
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorClose
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CurrentOpen
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADN_High
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADN_Low
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADE_High
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADE_Low
		{
			get { return Values[7]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR_High
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR_Low
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR0
		{
			get { return adr0; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR1
		{
			get { return adr1; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR3
		{
			get { return adr3; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR5
		{
			get { return adr5; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR10
		{
			get { return adr10; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ADR20
		{
			get { return adr20; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BandWidth
		{
			get { return bandWidth; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public int CacheLastBarPainted
		{
			get { return cacheLastBarPainted; }
			set { cacheLastBarPainted = value; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public bool LastBarStartsNewPeriod
		{
			get { return lastBarStartsNewPeriod; }
			set { lastBarStartsNewPeriod = value; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Input data", Description = "Select between taking high, low and close from intraday data or from daily data", GroupName = "Algorithmic Options", Order = 0)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaSessionTypeRPRD SessionType
		{	
            get { return sessionType; }
            set { sessionType = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Input data for close", Description = "Select between taking close from intraday data or from daily data", GroupName = "Algorithmic Options", Order = 1)]
		public amaCalcModeRPRD CalcMode
		{	
            get { return calcMode; }
            set { calcMode = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Reference period", Description = "Select number of days for calculating the projection levels", GroupName = "Input Parameters", Order = 0)]
		public int ReferencePeriod
		{	
            get { return referencePeriod; }
            set { referencePeriod = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "% noise levels", Description = "Sets the percentage value used to calculate noise levels", GroupName = "Input Parameters", Order = 1)]
		public double PercentageADN
		{	
            get { return percentageADN; }
            set { percentageADN = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "% expansion levels", Description = "Sets the percentage value used to calculat expansion levels", GroupName = "Input Parameters", Order = 2)]
		public double PercentageADE
		{	
            get { return percentageADE; }
            set { percentageADE = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "% ADR target levels", Description = "Sets the percentage value used to calculate the ADR target levels", GroupName = "Input Parameters", Order = 3)]
		public double PercentageADR
		{	
            get { return percentageADR; }
            set { percentageADR = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show prior high & low", Description = "Shows prior day high and low", GroupName = "Display Options", Order = 0)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowPriorHighLow
        {
            get { return showPriorHighLow; }
            set { showPriorHighLow = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show prior close", Description = "Shows prior close (settlement)", GroupName = "Display Options", Order = 1)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowPriorClose
        {
            get { return showPriorClose; }
            set { showPriorClose = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show current open", Description = "Shows the current day's opening price", GroupName = "Display Options", Order = 2)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowCurrentOpen
        {
            get { return showCurrentOpen; }
            set { showCurrentOpen = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show opening gap", Description = "Shows the current day's opening gap", GroupName = "Display Options", Order = 3)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowGap
        {
            get { return showGap; }
            set { showGap = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show noise levels", Description = "Shows upper and lower noise levels", GroupName = "Display Options", Order = 4)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowNoiseLevels
        {
            get { return showNoiseLevels; }
            set { showNoiseLevels = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show noise bands", Description = "Shows upper and lower noise bands", GroupName = "Display Options", Order = 5)]
      	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowNoiseBands
        {
            get { return showNoiseBands; }
            set { showNoiseBands = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show expansion levels", Description = "Shows upper and lower expansions levels", GroupName = "Display Options", Order = 6)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowExpansionLevels
        {
            get { return showExpansionLevels; }
            set { showExpansionLevels = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show expansion bands", Description = "Shows upper and lower expansion bands", GroupName = "Display Options", Order = 7)]
      	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowExpansionBands
        {
            get { return showExpansionBands; }
            set { showExpansionBands = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show ADR target levels", Description = "Shows upper and lower ADR target levels", GroupName = "Display Options", Order = 8)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowADRLevels
        {
            get { return showADRLevels; }
            set { showADRLevels = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show ADR target bands", Description = "Shows upper and lower ADR target bands", GroupName = "Display Options", Order = 9)]
      	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowADRBands
        {
            get { return showADRBands; }
            set { showADRBands = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show labels", Description = "Shows labels for all pivot lines", GroupName = "Display Options", Order = 10)]
  		[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowLabels
        {
            get { return showLabels; }
            set { showLabels = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show ADR data box", Description = "Shows ADR data for the last 20 days ", GroupName = "Display Options", Order = 11)]
  		[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowDataBox
        {
            get { return showDataBox; }
            set { showDataBox = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day high", Description = "Sets the color for the prior day high", GroupName = "Plot Colors", Order = 0)]
		public System.Windows.Media.Brush PriorHighBrush
		{ 
			get {return priorHighBrush;}
			set {priorHighBrush = value;}
		}

		[Browsable(false)]
		public string PriorHighBrushSerializable
		{
			get { return Serialize.BrushToString(priorHighBrush); }
			set { priorHighBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day low", Description = "Sets the color for the prior day low", GroupName = "Plot Colors", Order = 1)]
		public System.Windows.Media.Brush PriorLowBrush
		{ 
			get {return priorLowBrush;}
			set {priorLowBrush = value;}
		}

		[Browsable(false)]
		public string PriorLowBrushSerializable
		{
			get { return Serialize.BrushToString(priorLowBrush); }
			set { priorLowBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day close", Description = "Sets the color for the prior day close", GroupName = "Plot Colors", Order = 2)]
		public System.Windows.Media.Brush PriorCloseBrush
		{ 
			get {return priorCloseBrush;}
			set {priorCloseBrush = value;}
		}

		[Browsable(false)]
		public string PriorCloseBrushSerializable
		{
			get { return Serialize.BrushToString(priorCloseBrush); }
			set { priorCloseBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current day open", Description = "Sets the color for the current day open", GroupName = "Plot Colors", Order = 3)]
		public System.Windows.Media.Brush CurrentOpenBrush
		{ 
			get {return currentOpenBrush;}
			set {currentOpenBrush = value;}
		}

		[Browsable(false)]
		public string CurrentOpenBrushSerializable
		{
			get { return Serialize.BrushToString(currentOpenBrush); }
			set { currentOpenBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Opening gap", Description = "Sets the color for the opening gap", GroupName = "Plot Colors", Order = 4)]
		public System.Windows.Media.Brush GapBrushS
		{ 
			get {return gapBrushS;}
			set {gapBrushS = value;}
		}

		[Browsable(false)]
		public string GapBrushSSerializable
		{
			get { return Serialize.BrushToString(gapBrushS); }
			set { gapBrushS = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Noise levels", Description = "Sets the color for the noise levels", GroupName = "Plot Colors", Order = 5)]
		public System.Windows.Media.Brush NoiseLevelBrush
		{ 
			get {return noiseLevelBrush;}
			set {noiseLevelBrush = value;}
		}

		[Browsable(false)]
		public string NoiseLevelBrushSerializable
		{
			get { return Serialize.BrushToString(noiseLevelBrush); }
			set { noiseLevelBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Expansion levels", Description = "Sets the color for the expansion levels", GroupName = "Plot Colors", Order = 6)]
		public System.Windows.Media.Brush ExpansionLevelBrush
		{ 
			get {return expansionLevelBrush;}
			set {expansionLevelBrush = value;}
		}

		[Browsable(false)]
		public string ExpansionLevelBrushSerializable
		{
			get { return Serialize.BrushToString(expansionLevelBrush); }
			set { expansionLevelBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ADR target levels", Description = "Sets the color for the ADR target levels", GroupName = "Plot Colors", Order = 7)]
		public System.Windows.Media.Brush TargetLevelBrush
		{ 
			get {return targetLevelBrush;}
			set {targetLevelBrush = value;}
		}

		[Browsable(false)]
		public string TargetLevelBrushSerializable
		{
			get { return Serialize.BrushToString(targetLevelBrush); }
			set { targetLevelBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style prior high/low", Description = "Sets the dash style for the prior day high and low", GroupName = "Plot Parameters", Order = 0)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width prior high/low", Description = "Sets the plot width for the prior day high and low", GroupName = "Plot Parameters", Order = 1)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style prior close", Description = "Sets the dash style for the prior day close", GroupName = "Plot Parameters", Order = 2)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width prior close", Description = "Sets the plot width for the prior day close", GroupName = "Plot Parameters", Order = 3)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style current open", Description = "Sets the dash style for the current open", GroupName = "Plot Parameters", Order = 4)]
		public DashStyleHelper Dash2Style
		{
			get { return dash2Style; }
			set { dash2Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width current open", Description = "Sets the plot width for the current open", GroupName = "Plot Parameters", Order = 5)]
		public int Plot2Width
		{	
            get { return plot2Width; }
            set { plot2Width = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style noise levels", Description = "Sets the dash style for the noise levels", GroupName = "Plot Parameters", Order = 6)]
		public DashStyleHelper Dash3Style
		{
			get { return dash3Style; }
			set { dash3Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width noise levels", Description = "Sets the plot width for the noise levels", GroupName = "Plot Parameters", Order = 7)]
		public int Plot3Width
		{	
            get { return plot3Width; }
            set { plot3Width = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style expansion levels", Description = "Sets the dash style for the expansion levels", GroupName = "Plot Parameters", Order = 8)]
		public DashStyleHelper Dash4Style
		{
			get { return dash4Style; }
			set { dash4Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width expansion levels", Description = "Sets the plot width for the expansion levels", GroupName = "Plot Parameters", Order = 9)]
		public int Plot4Width
		{	
            get { return plot4Width; }
            set { plot4Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style ADR target levels", Description = "Sets the dash style for the ADR target levels", GroupName = "Plot Parameters", Order = 10)]
		public DashStyleHelper Dash5Style
		{
			get { return dash5Style; }
			set { dash5Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width ADR target levels", Description = "Sets the plot width for the ADR target levels", GroupName = "Plot Parameters", Order = 11)]
		public int Plot5Width
		{	
            get { return plot5Width; }
            set { plot5Width = value; }
		}
			
		[Range(10, 40)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label font size", Description = "Sets the size for all pivot labels", GroupName = "Plot Parameters", Order = 12)]
		public int LabelFontSize
		{	
            get { return labelFontSize; }
            set { labelFontSize = value; }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label offset", Description = "Allows for shifting the labels further to the right", GroupName = "Plot Parameters", Order = 13)]
		public int ShiftLabelOffset
		{	
            get { return shiftLabelOffset; }
            set { shiftLabelOffset = value; }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Gap opacity", Description = "Sets the opacity for the opening gap", GroupName = "Shading", Order = 0)]
        public int GapOpacity
        {
            get { return gapOpacity; }
            set { gapOpacity = value; }
        }
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Noise band opacity", Description = "Sets the opacity for the noise bands", GroupName = "Shading", Order = 1)]
        public int NoiseBandOpacity
        {
            get { return noiseBandOpacity; }
            set { noiseBandOpacity = value; }
        }
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Expansion band opacity", Description = "Sets the opacity for the expansion bands", GroupName = "Shading", Order = 2)]
        public int ExpansionBandOpacity
        {
            get { return expansionBandOpacity; }
            set { expansionBandOpacity = value; }
        }
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ADR target band opacity", Description = "Sets the opacity for the ADR target bands", GroupName = "Shading", Order = 3)]
        public int TargetBandOpacity
        {
            get { return targetBandOpacity; }
            set { targetBandOpacity = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text color", Description = "Sets the text color for the data box", GroupName = "Data Box", Order = 0)]
		public System.Windows.Media.Brush DataBoxTextBrush
		{ 
			get {return dataBoxTextBrush;}
			set {dataBoxTextBrush = value;}
		}

		[Browsable(false)]
		public string DataBoxTextBrushSerializable
		{
			get { return Serialize.BrushToString(dataBoxTextBrush); }
			set { dataBoxTextBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Back color", Description = "Sets the back color for the data box", GroupName = "Data Box", Order = 1)]
		public System.Windows.Media.Brush DataBoxBackBrushS
		{ 
			get {return dataBoxBackBrushS;}
			set {dataBoxBackBrushS = value;}
		}

		[Browsable(false)]
		public string DataBoxBackBrushSSerializable
		{
			get { return Serialize.BrushToString(dataBoxBackBrushS); }
			set { dataBoxBackBrushS = Serialize.StringToBrush(value); }
		}
		
		[Range(8, 24)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text font size", Description = "Sets the size for the text in the data box", GroupName = "Data Box", Order = 2)]
		public int DataBoxFontSize
		{	
            get { return dataBoxFontSize; }
            set { dataBoxFontSize = value; }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Back color opacity", Description = "Sets the opacity of the back color for the data box", GroupName = "Data Box", Order = 3)]
		public int DataBoxOpacity
		{	
            get { return dataBoxOpacity; }
            set { dataBoxOpacity = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Box display", Description = "Select between displaying the box in the upper left or upper right corner of the chart", GroupName = "Data Box", Order = 4)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaDataBoxLocationRPRD DataBoxLocation
		{	
            get { return dataBoxLocation; }
            set { dataBoxLocation = value; }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Left margin", Description = "Sets the left side margin for the data box", GroupName = "Data Box", Order = 5)]
        public int DataBoxOffsetLeft
        {
            get { return dataBoxOffsetLeft; }
            set { dataBoxOffsetLeft = value; }
        }
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper margin", Description = "Sets the upper margin for the data box when located left", GroupName = "Data Box", Order = 6)]
        public int DataBoxOffsetUpperLeft
        {
            get { return dataBoxOffsetUpperLeft; }
            set { dataBoxOffsetUpperLeft = value; }
        }
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Right margin", Description = "Sets the right side margin for the data box", GroupName = "Data Box", Order = 7)]
        public int DataBoxOffsetRight
        {
            get { return dataBoxOffsetRight; }
            set { dataBoxOffsetRight = value; }
        }
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper margin", Description = "Sets the upper margin for the data box when located right", GroupName = "Data Box", Order = 8)]
        public int DataBoxOffsetUpperRight
        {
            get { return dataBoxOffsetUpperRight; }
            set { dataBoxOffsetUpperRight = value; }
        }

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous
		
		public override string FormatPriceMarker(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
		}			

		public string FormatDataOutput(double price)
		{
			string outputString = Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			if (isForex)
			{	
				char[] charsToTrim = {'0', ',', '.'};
				outputString = outputString.TrimStart(charsToTrim);
				int index = outputString.IndexOf('.');
				if (index > -1)
					outputString = outputString.Remove(index, 1);
				index = outputString.IndexOf(',');
				if (index > -1)
					outputString = outputString.Remove(index, 1);
				index = outputString.IndexOf('\'');
				if (index > -1)
					outputString = outputString.Remove(index, 2);
			}
			else if (isCurrencyFuture1)
			{	
				char[] charsToTrim = {'0', ',', '.'};
				outputString = outputString.TrimStart(charsToTrim);
			}
			else if(isCurrencyFuture2)
			{	
				char[] charsToTrim = {'0', ',', '.'};
				outputString = outputString.TrimStart(charsToTrim);
				outputString = outputString.Remove(outputString.Length - 1);
			}	
			return outputString;
		}
		
		private bool IsConnected()
        {
			try
			{
				if (BarsArray[0] != null && BarsArray[0].Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected)
					return true;
				else
					return false;
			}
			catch
			{
				return false;
			}	
		}
		
		private DateTime GetLastBarSessionDate0(DateTime time)
		{
			sessionIterator0.CalculateTradingDay(time, timeBased0);
			sessionDateTmp0 = sessionIterator0.ActualTradingDayExchange;
			if(cacheSessionDate0 != sessionDateTmp0) 
			{
				cacheSessionDate0 = sessionDateTmp0;
				if (newSessionBarIdxArr.Count == 0 || (newSessionBarIdxArr.Count > 0 && CurrentBars[0] > (int) newSessionBarIdxArr[newSessionBarIdxArr.Count - 1]))
						newSessionBarIdxArr.Add(CurrentBars[0]);
			}
			return sessionDateTmp0;			
		}
		
		private DateTime GetLastBarSessionDate1(DateTime time)
		{
			sessionIterator0.CalculateTradingDay(time, true);
			sessionDateTmp1 = sessionIterator0.ActualTradingDayExchange;
			return sessionDateTmp1;			
		}
		
		public override void OnRenderTargetChanged()
		{
			if (gapBrushDX != null)
				gapBrushDX.Dispose();
			if (noiseBandBrushDX != null)
				noiseBandBrushDX.Dispose();
			if (expansionBandBrushDX != null)
				expansionBandBrushDX.Dispose();
			if (targetBandBrushDX != null)
				targetBandBrushDX.Dispose();
			if(dataBoxTextBrushDX != null)
				dataBoxTextBrushDX.Dispose();
			if(dataBoxBackBrushDX != null)
				dataBoxBackBrushDX.Dispose();
			if(adr0BrushDX != null)
				adr0BrushDX.Dispose();
			if(adr1BrushDX != null)
				adr1BrushDX.Dispose();
			if(adr3BrushDX != null)
				adr3BrushDX.Dispose();
			if(adr5BrushDX != null)
				adr5BrushDX.Dispose();
			if(adr10BrushDX != null)
				adr10BrushDX.Dispose();
			if(adr20BrushDX != null)
				adr20BrushDX.Dispose();
			if(transparentBrushDX != null)
				transparentBrushDX.Dispose();
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				if(brushesDX[seriesCount] != null)
					brushesDX[seriesCount].Dispose();

			if (RenderTarget != null)
			{
				try
				{
					gapBrushDX 	= gapBrush.ToDxBrush(RenderTarget);
					noiseBandBrushDX = noiseBandBrush.ToDxBrush(RenderTarget);
					expansionBandBrushDX = expansionBandBrush.ToDxBrush(RenderTarget);
					targetBandBrushDX = targetBandBrush.ToDxBrush(RenderTarget);
					dataBoxTextBrushDX = dataBoxTextBrush.ToDxBrush(RenderTarget);
					dataBoxBackBrushDX = dataBoxBackBrush.ToDxBrush(RenderTarget);
					adr0BrushDX = Brushes.Magenta.ToDxBrush(RenderTarget);
					adr1BrushDX = Brushes.DeepSkyBlue.ToDxBrush(RenderTarget);
					adr3BrushDX = Brushes.LimeGreen.ToDxBrush(RenderTarget);
					adr5BrushDX = Brushes.Yellow.ToDxBrush(RenderTarget);
					adr10BrushDX = Brushes.DarkOrange.ToDxBrush(RenderTarget);
					adr20BrushDX = Brushes.Red.ToDxBrush(RenderTarget);
					transparentBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Transparent);
					for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
						brushesDX[seriesCount] 	= Plots[seriesCount].BrushDX;
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (ChartBars == null || errorMessage || !IsVisible) return;
			int	lastBarPainted = ChartBars.ToIndex;
			if(lastBarPainted  < 0 || BarsArray[0].Count < lastBarPainted) return;

			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
			TextFormat textFormat1 = new TextFormat(Globals.DirectWriteFactory, "Arial", (float)this.LabelFontSize);		
			TextFormat textFormat2 = new TextFormat(Globals.DirectWriteFactory, "Arial", (float)this.DataBoxFontSize);		

			int lastBarCounted				= Inputs[0].Count - 1;
			int	lastBarOnUpdate				= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex				= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 			= ChartBars.FromIndex;
			int firstBarIndex  	 			= Math.Max(BarsRequiredToPlot, firstBarPainted);
			int firstBarIdxToPaint  		= 0;
			int lastPlotIndex				= lastBarPainted;
			int lastPlotCalcIndex			= lastBarIndex;
			int firstPlotIndex				= 0;
			int	resolution					= 1 + this.LabelFontSize/2;
			double barWidth					= chartControl.GetBarPaintWidth(ChartBars);
			double smallGap					= 2.0;
			double labelOffset				= (this.LabelFontSize + barWidth)/2.0 + shiftLabelOffset;
			double firstX					= 0.0;
			double lastX					= 0.0;
			double val						= 0.0;
			double valB						= 0.0;
			double valC						= 0.0;
			double valO						= 0.0;
			double valH						= 0.0;
			double valL						= 0.0;
			double y						= 0.0;
			double yC						= 0.0;
			double yO						= 0.0;
			double yH						= 0.0;
			double yL						= 0.0;
			double[] yArr					= new double[Values.Length];
			float width						= 0.0f;
			float height					= 0.0f;
			string[] plotLabels				= new string[Values.Length];
			bool firstLoop					= true;
			DateTime lastBarPaintedTime		= ChartBars.Bars.GetTime(lastBarPainted);
			DateTime lastBarIndexTime		= ChartBars.Bars.GetTime(lastBarIndex);
			DateTime tradingDayEndPainted;
			DateTime tradingDayEndByIndex;
			SharpDX.Vector2 startPointDX;
			SharpDX.Vector2 endPointDX;
			SharpDX.Vector2 textPointDX;
			SharpDX.RectangleF rect;
			
			if(lastBarPainted != CacheLastBarPainted)
			{
				sessionIterator0.GetNextSession(lastBarPaintedTime, timeBased0);
				tradingDayEndPainted = sessionIterator0.ActualTradingDayEndLocal;
				sessionIterator0.GetNextSession(lastBarIndexTime, timeBased0);
				tradingDayEndByIndex = sessionIterator0.ActualTradingDayEndLocal;
				CacheLastBarPainted = lastBarPainted;
				if(tradingDayEndByIndex < tradingDayEndPainted)
					LastBarStartsNewPeriod = true;
				else
					LastBarStartsNewPeriod = false;
			}
			if(LastBarStartsNewPeriod)
				lastPlotIndex = lastBarIndex;
			
			if(!Values[0].IsValidDataPointAt(lastPlotCalcIndex))
			{
				if(!errorMessage)
				{	
					SharpDX.Direct2D1.Brush errorBrushDX = errorBrush.ToDxBrush(RenderTarget);
					TextFormat textFormat3 = new TextFormat(Globals.DirectWriteFactory, "Arial", 20.0f);		
					SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, errorText8, textFormat3, 1000, 20.0f);
					SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout.Metrics.Width - 5, ChartPanel.Y + (ChartPanel.H - textLayout.Metrics.Height));
					RenderTarget.DrawTextLayout(lowerTextPoint, textLayout, errorBrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					errorBrushDX.Dispose();
					textFormat3.Dispose();
					textLayout.Dispose();
				}	
				RenderTarget.AntialiasMode = oldAntialiasMode;
				textFormat1.Dispose();
				textFormat2.Dispose();
				return;
			}
			
			do
			{
				//check whether lastBarIndex contains a pivot value
				for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
				{
					int prevSessionBreakIdx = newSessionBarIdxArr[i];
					if (prevSessionBreakIdx <= lastPlotIndex)
					{
						firstBarIdxToPaint = prevSessionBreakIdx;
						break;
					}
				}
				firstPlotIndex = Math.Max(firstBarIndex, firstBarIdxToPaint);
				if(!Values[0].IsValidDataPointAt(lastPlotCalcIndex))
					break;
				if(firstPlotIndex > firstBarPainted)
					firstX	= chartControl.GetXByBarIndex(ChartBars, firstPlotIndex - 1) + smallGap;
				else
					firstX	= smallGap;
				lastX	= chartControl.GetXByBarIndex(ChartBars, lastPlotIndex);
				
				for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				{
					val						= Values[seriesCount].GetValueAt(lastPlotCalcIndex);
					yArr[seriesCount] 		= chartScale.GetYByValue(val);
					plotLabels[seriesCount] = Plots[seriesCount].Name;
					brushesDX[seriesCount] 	= Plots[seriesCount].BrushDX;
				}
				valB = BandWidth.GetValueAt(lastPlotCalcIndex); 

				// adjust labels in case that two pivots have near-identical prices
				
				if(ShowCurrentOpen)
				{
					if (ShowPriorClose && Math.Abs(yArr[3] - yArr[2]) < resolution)
					{
						plotLabels[2] = plotLabels[2] + "/" + plotLabels[3]; 
						plotLabels[3] = "";
						brushesDX[3] = transparentBrushDX;
					}
					else if (ShowPriorHighLow && Math.Abs(yArr[3] - yArr[0]) < resolution)
					{
						plotLabels[0] = plotLabels[0] + "/" + plotLabels[3]; 
						plotLabels[3] = "";
						brushesDX[3] = transparentBrushDX;
					}
					else if (ShowPriorHighLow && Math.Abs(yArr[3] - yArr[1]) < resolution)
					{
						plotLabels[1] = plotLabels[1] + "/" + plotLabels[3]; 
						plotLabels[3] = "";
						brushesDX[3] = transparentBrushDX;
					}
				}
				if(ShowPriorClose)
				{	
					if (ShowPriorHighLow && Math.Abs(yArr[2] - yArr[0]) < resolution)
					{
						plotLabels[0] = plotLabels[0] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowPriorHighLow && Math.Abs(yArr[2] - yArr[1]) < resolution)
					{
						plotLabels[1] = plotLabels[1] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowADRLevels && Math.Abs(yArr[2] - yArr[8]) < resolution)
					{
						plotLabels[8] = plotLabels[8] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowADRLevels && Math.Abs(yArr[2] - yArr[9]) < resolution)
					{
						plotLabels[9] = plotLabels[9] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowExpansionLevels && Math.Abs(yArr[2] - yArr[6]) < resolution)
					{
						plotLabels[6] = plotLabels[6] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowExpansionLevels && Math.Abs(yArr[2] - yArr[7]) < resolution)
					{
						plotLabels[7] = plotLabels[7] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowNoiseLevels && Math.Abs(yArr[2] - yArr[4]) < resolution)
					{
						plotLabels[4] = plotLabels[4] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
					else if (ShowNoiseLevels && Math.Abs(yArr[2] - yArr[5]) < resolution)
					{
						plotLabels[5] = plotLabels[5] + "/" + plotLabels[2]; 
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
				}
				if(ShowPriorHighLow)
				{
					if (ShowADRLevels && Math.Abs(yArr[0] - yArr[8]) < resolution)
					{
						plotLabels[8] = plotLabels[8] + "/" + plotLabels[0]; 
						plotLabels[0] = "";
						brushesDX[0] = transparentBrushDX;
					}
					else if (ShowExpansionLevels && Math.Abs(yArr[0] - yArr[6]) < resolution)
					{
						plotLabels[6] = plotLabels[6] + "/" + plotLabels[0]; 
						plotLabels[0] = "";
						brushesDX[0] = transparentBrushDX;
					}
					else if (ShowNoiseLevels && Math.Abs(yArr[0] - yArr[4]) < resolution)
					{
						plotLabels[4] = plotLabels[4] + "/" + plotLabels[0]; 
						plotLabels[0] = "";
						brushesDX[0] = transparentBrushDX;
					}
					if (ShowADRLevels && Math.Abs(yArr[1] - yArr[9]) < resolution)
					{
						plotLabels[9] = plotLabels[9] + "/" + plotLabels[1]; 
						plotLabels[1] = "";
						brushesDX[1] = transparentBrushDX;
					}
					else if (ShowExpansionLevels && Math.Abs(yArr[1] - yArr[7]) < resolution)
					{
						plotLabels[7] = plotLabels[7] + "/" + plotLabels[1]; 
						plotLabels[1] = "";
						brushesDX[1] = transparentBrushDX;
					}
					else if (ShowNoiseLevels && Math.Abs(yArr[1] - yArr[5]) < resolution)
					{
						plotLabels[5] = plotLabels[5] + "/" + plotLabels[1]; 
						plotLabels[1] = "";
						brushesDX[1] = transparentBrushDX;
					}
				}
				if(ShowADRLevels)
				{
					if(ShowExpansionLevels && Math.Abs(yArr[8] - yArr[6]) < resolution)
					{
						plotLabels[6] = plotLabels[6] + "/" + plotLabels[8]; 
						plotLabels[8] = "";
						brushesDX[8] = transparentBrushDX;
					}
					else if(ShowNoiseLevels && Math.Abs(yArr[8] - yArr[4]) < resolution)
					{
						plotLabels[4] = plotLabels[4] + "/" + plotLabels[8]; 
						plotLabels[8] = "";
						brushesDX[8] = transparentBrushDX;
					}
					if(ShowExpansionLevels && Math.Abs(yArr[9] - yArr[7]) < resolution)
					{
						plotLabels[7] = plotLabels[7] + "/" + plotLabels[9]; 
						plotLabels[9] = "";
						brushesDX[9] = transparentBrushDX;
					}
					else if(ShowNoiseLevels && Math.Abs(yArr[9] - yArr[5]) < resolution)
					{
						plotLabels[5] = plotLabels[5] + "/" + plotLabels[9]; 
						plotLabels[9] = "";
						brushesDX[9] = transparentBrushDX;
					}
				}
				
				//Draw gap
				if(ShowGap)
				{	
					valC = Values[2].GetValueAt(lastPlotCalcIndex); 
					valO = Values[3].GetValueAt(lastPlotCalcIndex);
					yC = chartScale.GetYByValue(valC);
					yO = chartScale.GetYByValue(valO);
					
					if(yO < yC)
					{
						width = (float)(lastX - firstX);
						height = (float)(yC - yO);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yO);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, gapBrushDX);
					}
					else if (yO > yC)
					{
						width = (float)(lastX - firstX);
						height = (float)(yO - yC);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yC);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, gapBrushDX);
					}
				}
				
				//Draw noise bands
				if(ShowNoiseBands)
				{	
					val = Values[4].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, noiseBandBrushDX);
					
					val = Values[5].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, noiseBandBrushDX);
				}
				
				//Draw expansion bands
				if(ShowExpansionBands)
				{	
					val = Values[6].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, expansionBandBrushDX);
					
					val = Values[7].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, expansionBandBrushDX);
				}
				
				//Draw ADR projection bands
				if(ShowADRBands)
				{	
					val = Values[8].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, targetBandBrushDX);
					
					val = Values[9].GetValueAt(lastPlotCalcIndex); 
					valH = val + valB; 
					valL = val - valB;
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					width = (float)(lastX - firstX);
					height = (float)(yH - yL);
					startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
					rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
					RenderTarget.FillRectangle(rect, targetBandBrushDX);
				}
				
				// Loop through all plot values on the chart
				for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				{
					Plot plot = Plots[seriesCount];
					y = yArr[seriesCount];
					
					// Draw pivot lines
					startPointDX = new SharpDX.Vector2((float)firstX, (float)y);
					endPointDX = new SharpDX.Vector2((float)lastX, (float)y);
					RenderTarget.DrawLine(startPointDX, endPointDX, plot.BrushDX, plot.Width, plot.StrokeStyle);
					
					// Draw pivot text
					if(ShowLabels && firstLoop)
					{	
							textPointDX = new SharpDX.Vector2((float)(lastX + labelOffset), (float)y - (float)LabelFontSize/2.0f);
							TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plotLabels[seriesCount], textFormat1, Math.Max(100, ChartControl.Properties.BarMarginRight - (float)labelOffset - 20), (float)LabelFontSize);
							RenderTarget.DrawTextLayout(textPointDX, textLayout, brushesDX[seriesCount]);
							textLayout.Dispose();
					}	
				}
				
				if(lastPlotIndex < firstPlotIndex)
					lastPlotIndex = 0;
				else
					lastPlotIndex = firstPlotIndex - 1;
				lastPlotCalcIndex = lastPlotIndex;
				firstLoop = false;
			}
			while (lastPlotIndex >= firstBarIndex);
				
			if(ShowDataBox && adrMax.IsValidDataPointAt(lastBarIndex))
			{
				double range0 = ADR0.GetValueAt(lastBarIndex);
				double range1 = ADR1.GetValueAt(lastBarIndex);
				double range3 = ADR3.GetValueAt(lastBarIndex);
				double range5 = ADR5.GetValueAt(lastBarIndex);
				double range10 = ADR10.GetValueAt(lastBarIndex);
				double range20 = ADR20.GetValueAt(lastBarIndex);
				double rangeMax = Math.Max(range0, adrMax.GetValueAt(lastBarIndex));
				double percentage0 = 0.0;
				double percentage1 = 0.0;
				double percentage3 = 0.0;
				double percentage5 = 0.0;
				double percentage10 = 0.0;
				double percentage20 = 0.0;
				if(rangeMax.ApproxCompare(0) == 1)
				{	
					percentage0 = range0 / rangeMax; 
					percentage1 = range1 / rangeMax; 
					percentage3 = range3 / rangeMax; 
					percentage5 = range5 / rangeMax; 
					percentage10 = range10 / rangeMax; 
					percentage20 = range20 / rangeMax; 
				}
				TextLayout textLayoutAR20 = new TextLayout(Globals.DirectWriteFactory, "20 day ADR ", textFormat2, 500, (float)DataBoxFontSize);
				TextLayout textLayoutAR10 = new TextLayout(Globals.DirectWriteFactory, "10 day ADR ", textFormat2, 500, (float)DataBoxFontSize);
				TextLayout textLayoutAR5 = new TextLayout(Globals.DirectWriteFactory, "5 day ADR ", textFormat2, 500, (float)DataBoxFontSize);
				TextLayout textLayoutAR3 = new TextLayout(Globals.DirectWriteFactory, "3 day ADR ", textFormat2, 500, (float)DataBoxFontSize);
				TextLayout textLayoutAR1 = new TextLayout(Globals.DirectWriteFactory, "Yesterday ", textFormat2, 500, (float)DataBoxFontSize);
				TextLayout textLayoutAR0 = new TextLayout(Globals.DirectWriteFactory, "Today ", textFormat2, 500, (float)DataBoxFontSize);
				float dataWidth = textLayoutAR20.Metrics.Width;
				float dataHeight = textLayoutAR20.Metrics.Height;
				float histogramWidth = 1.5f*dataWidth;
				float rectangleWidth = 3.7f*dataWidth; 
				float rectangleHeight = 7.8f*dataHeight;
				float xWidth0 = histogramWidth * (float) percentage0;
				float xWidth1 = histogramWidth * (float) percentage1;
				float xWidth3 = histogramWidth * (float) percentage3;
				float xWidth5 = histogramWidth * (float) percentage5;
				float xWidth10 = histogramWidth * (float) percentage10;
				float xWidth20 = histogramWidth * (float) percentage20;
				float leftOffset = (float) dataBoxOffsetLeft;
				float rightOffset = (float) dataBoxOffsetRight;
				float xStart0 = 0.0f;
				float xStart1 = 0.0f;
				float yStart0 = 0.0f;
				float upperOffset = 0.0f;
				
				if(DataBoxLocation == amaDataBoxLocationRPRD.Left)
				{	
					upperOffset = (float) dataBoxOffsetUpperLeft;
					xStart0 = ChartPanel.X + 0.15f*dataWidth + leftOffset;
				}	
				else
				{	
					upperOffset = (float) dataBoxOffsetUpperRight;
					xStart0 = ChartPanel.X + ChartPanel.W - 3.55f*dataWidth - rightOffset;
				}
				yStart0 = ChartPanel.Y + upperOffset; 
				startPointDX = new SharpDX.Vector2(xStart0 - 0.15f*dataWidth, yStart0);
				xStart1 = xStart0 + 1.2f*dataWidth;
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, rectangleWidth, rectangleHeight);
				RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
				RenderTarget.FillRectangle(rect, dataBoxBackBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 0.4f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR20, dataBoxTextBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 1.6f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR10, dataBoxTextBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 2.8f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR5, dataBoxTextBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 4.0f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR3, dataBoxTextBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 5.2f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR1, dataBoxTextBrushDX);
				textPointDX = new SharpDX.Vector2(xStart0, yStart0 + 6.4f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR0, dataBoxTextBrushDX);
				
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 0.65f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth20, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr20BrushDX);
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 1.85f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth10, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr10BrushDX);
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 3.05f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth5, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr5BrushDX);
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 4.25f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth3, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr3BrushDX);
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 5.45f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth1, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr1BrushDX);
				startPointDX = new SharpDX.Vector2(xStart1, yStart0 + 6.65f*dataHeight);
				rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, xWidth0, 0.6f * dataHeight);
				RenderTarget.FillRectangle(rect, adr0BrushDX);

				textLayoutAR20 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range20), textFormat2, 500, (float)DataBoxFontSize);
				textLayoutAR10 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range10), textFormat2, 500, (float)DataBoxFontSize);
				textLayoutAR5 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range5), textFormat2, 500, (float)DataBoxFontSize);
				textLayoutAR3 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range3), textFormat2, 500, (float)DataBoxFontSize);
				textLayoutAR1 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range1), textFormat2, 500, (float)DataBoxFontSize);
				textLayoutAR0 = new TextLayout(Globals.DirectWriteFactory, FormatDataOutput(range0), textFormat2, 500, (float)DataBoxFontSize);
				xStart0 = xStart1 + 0.1f*dataWidth;;
				xStart1 = xStart0 + xWidth20;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 0.4f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR20, dataBoxTextBrushDX);
				xStart1 = xStart0 + xWidth10;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 1.6f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR10, dataBoxTextBrushDX);
				xStart1 = xStart0 + xWidth5;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 2.8f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR5, dataBoxTextBrushDX);
				xStart1 = xStart0 + xWidth3;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 4.0f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR3, dataBoxTextBrushDX);
				xStart1 = xStart0 + xWidth1;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 5.2f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR1, dataBoxTextBrushDX);
				xStart1 = xStart0 + xWidth0;
				textPointDX = new SharpDX.Vector2(xStart1, yStart0 + 6.4f*dataHeight);
				RenderTarget.DrawTextLayout(textPointDX, textLayoutAR0, dataBoxTextBrushDX);
				textLayoutAR20.Dispose();
				textLayoutAR10.Dispose();
				textLayoutAR5.Dispose();
				textLayoutAR3.Dispose();
				textLayoutAR1.Dispose();
				textLayoutAR0.Dispose();
			}
			RenderTarget.AntialiasMode = oldAntialiasMode;
			textFormat1.Dispose();
			textFormat2.Dispose();
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{
	public class amaRangeProjectionsDailyTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaRangeProjectionsDaily	thisRangeProjectionsInstance		= (amaRangeProjectionsDaily) value;
			amaSessionTypeRPRD			sessionTypeFromInstance				= thisRangeProjectionsInstance.SessionType;
			bool						showPriorHighLowFromInstance		= thisRangeProjectionsInstance.ShowPriorHighLow;
			bool						showPriorCloseFromInstance			= thisRangeProjectionsInstance.ShowPriorClose;
			bool						showCurrentOpenFromInstance			= thisRangeProjectionsInstance.ShowCurrentOpen;
			bool						showNoiseLevelsFromInstance			= thisRangeProjectionsInstance.ShowNoiseLevels;
			bool						showExpansionLevelsFromInstance		= thisRangeProjectionsInstance.ShowExpansionLevels;
			bool						showADRLevelsFromInstance			= thisRangeProjectionsInstance.ShowADRLevels;
			bool						showGapFromInstance					= thisRangeProjectionsInstance.ShowGap;
			bool						showNoiseBandsFromInstance			= thisRangeProjectionsInstance.ShowNoiseBands;
			bool						showExpansionBandsFromInstance		= thisRangeProjectionsInstance.ShowExpansionBands;
			bool						showADRBandsFromInstance			= thisRangeProjectionsInstance.ShowADRBands;
			bool						showLabelsFromInstance				= thisRangeProjectionsInstance.ShowLabels;
			bool						showDataBoxFromInstance				= thisRangeProjectionsInstance.ShowDataBox;
			amaDataBoxLocationRPRD		dataBoxLocationFromInstance			= thisRangeProjectionsInstance.DataBoxLocation;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if (sessionTypeFromInstance == amaSessionTypeRPRD.Daily_Bars && thisDescriptor.Name == "CalcMode")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPriorHighLowFromInstance && (thisDescriptor.Name == "PriorHighBrush" || thisDescriptor.Name == "PriorLowBrush" 
					|| thisDescriptor.Name == "Dash0Style" || thisDescriptor.Name == "Plot0Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPriorCloseFromInstance && (thisDescriptor.Name == "PriorCloseBrush" || thisDescriptor.Name == "Dash1Style" || thisDescriptor.Name == "Plot1Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showCurrentOpenFromInstance && (thisDescriptor.Name == "CurrentOpenBrush" || thisDescriptor.Name == "Dash2Style" || thisDescriptor.Name == "Plot2Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showNoiseLevelsFromInstance && (thisDescriptor.Name == "Dash3Style" || thisDescriptor.Name == "Plot3Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showExpansionLevelsFromInstance && (thisDescriptor.Name == "Dash4Style" || thisDescriptor.Name == "Plot4Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showADRLevelsFromInstance && (thisDescriptor.Name == "Dash5Style" || thisDescriptor.Name == "Plot5Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showGapFromInstance && (thisDescriptor.Name == "GapBrushS" || thisDescriptor.Name == "GapOpacity"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showNoiseBandsFromInstance && thisDescriptor.Name == "NoiseBandOpacity")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showExpansionBandsFromInstance && thisDescriptor.Name == "ExpansionBandOpacity")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showADRBandsFromInstance && thisDescriptor.Name == "TargetBandOpacity")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showNoiseLevelsFromInstance && !showNoiseBandsFromInstance && thisDescriptor.Name == "NoiseLevelBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showExpansionLevelsFromInstance && !showExpansionBandsFromInstance && thisDescriptor.Name == "ExpansionLevelBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showADRLevelsFromInstance && !showADRBandsFromInstance && thisDescriptor.Name == "TargetLevelBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showLabelsFromInstance && (thisDescriptor.Name == "LabelFontSize" || thisDescriptor.Name == "ShiftLabelOffset"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showDataBoxFromInstance && (thisDescriptor.Name == "DataBoxTextBrush" || thisDescriptor.Name == "DataBoxFontSize" || thisDescriptor.Name == "DataBoxBackBrushS" 
					|| thisDescriptor.Name == "DataBoxOpacity" || thisDescriptor.Name == "DataBoxLocation" || thisDescriptor.Name == "DataBoxOffsetLeft" || thisDescriptor.Name == "DataBoxOffsetUpperLeft" 
					|| thisDescriptor.Name == "DataBoxOffsetRight" || thisDescriptor.Name == "DataBoxOffsetUpperRight")) 
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (dataBoxLocationFromInstance == amaDataBoxLocationRPRD.Left && (thisDescriptor.Name == "DataBoxOffsetRight" || thisDescriptor.Name == "DataBoxOffsetUpperRight")) 
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (dataBoxLocationFromInstance == amaDataBoxLocationRPRD.Right && (thisDescriptor.Name == "DataBoxOffsetLeft" || thisDescriptor.Name == "DataBoxOffsetUpperLeft")) 
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else	
					adjusted.Add(thisDescriptor);
			}
			return adjusted;		
		}
	}
}

#region Public Enums
public enum amaSessionTypeRPRD 
{
	Trading_Hours, 
	Daily_Bars
}

public enum amaCalcModeRPRD 
{
	Intraday_Data,
	Daily_Data
}

public enum amaDataBoxLocationRPRD
{
	Left,
	Right
}	
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaRangeProjectionsDaily[] cacheamaRangeProjectionsDaily;
		public LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			return amaRangeProjectionsDaily(Input, sessionType, calcMode, referencePeriod, percentageADN, percentageADE, percentageADR);
		}

		public LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(ISeries<double> input, amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			if (cacheamaRangeProjectionsDaily != null)
				for (int idx = 0; idx < cacheamaRangeProjectionsDaily.Length; idx++)
					if (cacheamaRangeProjectionsDaily[idx] != null && cacheamaRangeProjectionsDaily[idx].SessionType == sessionType && cacheamaRangeProjectionsDaily[idx].CalcMode == calcMode && cacheamaRangeProjectionsDaily[idx].ReferencePeriod == referencePeriod && cacheamaRangeProjectionsDaily[idx].PercentageADN == percentageADN && cacheamaRangeProjectionsDaily[idx].PercentageADE == percentageADE && cacheamaRangeProjectionsDaily[idx].PercentageADR == percentageADR && cacheamaRangeProjectionsDaily[idx].EqualsInput(input))
						return cacheamaRangeProjectionsDaily[idx];
			return CacheIndicator<LizardIndicators.amaRangeProjectionsDaily>(new LizardIndicators.amaRangeProjectionsDaily(){ SessionType = sessionType, CalcMode = calcMode, ReferencePeriod = referencePeriod, PercentageADN = percentageADN, PercentageADE = percentageADE, PercentageADR = percentageADR }, input, ref cacheamaRangeProjectionsDaily);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			return indicator.amaRangeProjectionsDaily(Input, sessionType, calcMode, referencePeriod, percentageADN, percentageADE, percentageADR);
		}

		public Indicators.LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(ISeries<double> input , amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			return indicator.amaRangeProjectionsDaily(input, sessionType, calcMode, referencePeriod, percentageADN, percentageADE, percentageADR);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			return indicator.amaRangeProjectionsDaily(Input, sessionType, calcMode, referencePeriod, percentageADN, percentageADE, percentageADR);
		}

		public Indicators.LizardIndicators.amaRangeProjectionsDaily amaRangeProjectionsDaily(ISeries<double> input , amaSessionTypeRPRD sessionType, amaCalcModeRPRD calcMode, int referencePeriod, double percentageADN, double percentageADE, double percentageADR)
		{
			return indicator.amaRangeProjectionsDaily(input, sessionType, calcMode, referencePeriod, percentageADN, percentageADE, percentageADR);
		}
	}
}

#endregion
