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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class DCTBF : Indicator
	{
        //Vars
        private int N = 0;

        double alpha1;

        Series<double> HP;
        Series<double> SmoothHP;
        Series<double> delta;
        Series<double> gamma;
        Series<double> alpha;
        Series<double> beta;
        Series<double> Period;
        Series<double> MaxAmpl;
        Series<double> Num;
        Series<double> Denom;
        Series<double> DC;
        Series<double> DomCyc;

        //Arrays
        double[] I = { 0 };
        double[] OldI = { 0 };
        double[] OlderI = { 0 };
        double[] Q = { 0 };
        double[] OldQ = { 0 };
        double[] OlderQ = { 0 };
        double[] Real = { 0 };
        double[] OldReal = { 0 };
        double[] OlderReal = { 0 };
        double[] Imag = { 0 };
        double[] OldImag = { 0 };
        double[] OlderImag = { 0 };
        double[] Ampl = { 0 };
        double[] OldAmpl = { 0 };
        double[] DB = { 0 };

        private double DegToRads(double x)
        {
            return x * (Math.PI / 180);
        }

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Dominant cycle tuned bypass filter";
				Name										= "DCTBF";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
                AddPlot(Brushes.SeaGreen, "SinPlot");
                AddPlot(Brushes.Coral, "CosPlot");

            }
			else if (State == State.Configure)
			{
			}
            else if (State == State.DataLoaded)
            {
                HP = new Series<double>(this, MaximumBarsLookBack.Infinite);
                SmoothHP = new Series<double>(this, MaximumBarsLookBack.Infinite);
                delta = new Series<double>(this, MaximumBarsLookBack.Infinite);
                gamma = new Series<double>(this, MaximumBarsLookBack.Infinite);
                alpha = new Series<double>(this, MaximumBarsLookBack.Infinite);
                beta = new Series<double>(this, MaximumBarsLookBack.Infinite);
                Period = new Series<double>(this, MaximumBarsLookBack.Infinite);
                MaxAmpl = new Series<double>(this, MaximumBarsLookBack.Infinite);
                Num = new Series<double>(this, MaximumBarsLookBack.Infinite);
                Denom = new Series<double>(this, MaximumBarsLookBack.Infinite);
                DC = new Series<double>(this, MaximumBarsLookBack.Infinite);
                DomCyc = new Series<double>(this, MaximumBarsLookBack.Infinite);

                I = new double[51];
                OldI = new double[51];
                OlderI = new double[51];
                Q = new double[51];
                OldQ = new double[51];
                OlderQ = new double[51];
                Real = new double[51];
                OldReal = new double[51];
                OlderReal = new double[51];
                Imag = new double[51];
                OldImag = new double[51];
                OlderImag = new double[51];
                Ampl = new double[51];
                OldAmpl = new double[51];
                DB = new double[51];
            }
        }

		protected override void OnBarUpdate()
		{
            alpha1 = (1 - Math.Sin(DegToRads(360 / 40))) / Math.Cos(DegToRads(360 / 40));

            if (CurrentBar < 1)
            {
                HP[0] = 0;
                return;
            }

            HP[0] = .5 * (1 + alpha1) * (Median[0] - Median[1]) + alpha1 * HP[1];

            if (CurrentBar == 1)
            {
                SmoothHP[0] = 0;
                return;
            }


            if (CurrentBar < 7)
            {
                SmoothHP[0] = Median[0] - Median[1];
            }
            else
            {
                SmoothHP[0] = (HP[0] + 2 * HP[1] + 3 * HP[2] + 3 * HP[3] + 2 * HP[4] + HP[5]) / 12;
            }

            if (CurrentBar < 6)
                return;

            delta[0] = -.015 * CurrentBar + .5;

            if (delta[0] < .15)
                delta[0] = .15;

            for (N = 8; N <= 50; ++N)
            {
                beta[0] = Math.Cos(DegToRads((360 / N)));
                gamma[0] = 1 / Math.Cos(DegToRads(720 * delta[0] / N));
                alpha[0] = gamma[0] - Math.Sqrt(gamma[0] * gamma[0] - 1);
                Q[N] = (N / 6.283185) * (SmoothHP[0] - SmoothHP[1]);
                I[N] = SmoothHP[0];
                Real[N] = .5 * (1 - alpha[0]) * (I[N] - OlderI[N]) + beta[0] * (1 + alpha[0]) * OldReal[N] - alpha[0] * OlderReal[N];
                Imag[N] = .5 * (1 - alpha[0]) * (Q[N] - OlderQ[N]) + beta[0] * (1 + alpha[0]) * OldImag[N] - alpha[0] * OlderImag[N];
                Ampl[N] = (Real[N] * Real[N] + Imag[N] * Imag[N]);
            }

            for (N = 8; N <= 50; ++N)
            {
                OlderI[N] = OldI[N];
                OldI[N] = I[N];
                OlderQ[N] = OldQ[N];
                OldQ[N] = Q[N];
                OlderReal[N] = OldReal[N];
                OldReal[N] = Real[N];
                OlderImag[N] = OldImag[N];
                OldImag[N] = Imag[N];
                OldAmpl[N] = Ampl[N];
            }

            MaxAmpl[0] = Ampl[10];

            for (N = 8; N <= 50; ++N)
            {
                if (Ampl[N] > MaxAmpl[0])
                {
                    MaxAmpl[0] = Ampl[N];
                }
            }

            for (N = 8; N <= 50; ++N)
            {
                if (MaxAmpl[0] != 0 && (Ampl[N] / MaxAmpl[0]) > 0)
                {
                    DB[N] = -10 * Math.Log(.01 / (1 - .99 * Ampl[N] / MaxAmpl[0])) / Math.Log(10.0);
                }

                if (DB[N] > 20)
                {
                    DB[N] = 20;
                }
            }

            Num[0] = 0;
            Denom[0] = 0;

            for (N = 8; N <= 50; ++N)
            {
                if (DB[N] <= 3)
                {
                    Num[0] = Num[0] + N * (20 - DB[N]);
                    Denom[0] = Denom[0] + (20 - DB[N]);
                }

                if (Denom[0] != 0)
                {
                    DC[0] = Num[0] / Denom[0];
                }
            }

            if (CurrentBar < 10)
                return;

            DomCyc[0] = GetMedian(DC, 10);

            if(DomCyc[0] < 8)
            {
                DomCyc[0] = 20;
            }

            beta[0] = Math.Cos(DegToRads(360 / DomCyc[0]));

            gamma[0] = 1 / Math.Cos(DegToRads(720 * delta[0] / DomCyc[0]));

            alpha[0] = gamma[0] - Math.Sqrt(gamma[0] * gamma[0] - 1);

            Values[0][0] = .5 * (1 - alpha[0]) * (SmoothHP[0] - SmoothHP[1]) + beta[0] * (1 +alpha[0]) * Values[0][1] - alpha[0] * Values[0][2];
            Values[1][0] = (DomCyc[0] / 6.28) * (Values[0][0] - Values[0][1]);
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DCTBF[] cacheDCTBF;
		public DCTBF DCTBF()
		{
			return DCTBF(Input);
		}

		public DCTBF DCTBF(ISeries<double> input)
		{
			if (cacheDCTBF != null)
				for (int idx = 0; idx < cacheDCTBF.Length; idx++)
					if (cacheDCTBF[idx] != null &&  cacheDCTBF[idx].EqualsInput(input))
						return cacheDCTBF[idx];
			return CacheIndicator<DCTBF>(new DCTBF(), input, ref cacheDCTBF);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DCTBF DCTBF()
		{
			return indicator.DCTBF(Input);
		}

		public Indicators.DCTBF DCTBF(ISeries<double> input )
		{
			return indicator.DCTBF(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DCTBF DCTBF()
		{
			return indicator.DCTBF(Input);
		}

		public Indicators.DCTBF DCTBF(ISeries<double> input )
		{
			return indicator.DCTBF(input);
		}
	}
}

#endregion
