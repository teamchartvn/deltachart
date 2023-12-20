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
	public class LinRegSlopeSFX : Indicator
	{
		// Exponential moving regression slope variables
		private int nn = 0;
		private double n, w, sumX, sumX2, sumY1, sumXY1, correction;
		// super fast linear regression slope variables
		private double m1, m2, sumP, sumIP;
		// public results, updated each bar
		public double slope, yvalue, err;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Linear regression slope - super fast calc with Exponential weighting option";
				Name										= "LinRegSlopeSFX";
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
				
				Period										= 8;
				Exponential									= false;
				Returnval									= 1;
				
				AddPlot(Brushes.MediumBlue, "SlopeSFX");
				AddLine(Brushes.DarkOliveGreen, 0, "ZeroLine");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double rtn;
			int ix;
			if (Exponential) {
/*
 Exponential linear regression slope
 Copyright (c) 2004 by Alex Matulich / Unicorn Research Corporation.
 Ported from TradeStation to NinjaTrader August 2008 by Alex Matulich.
 Tradestation version is published at http://unicorn.us.com/trading/el.html

 This function calculates a linear regression slope using Exponential
 moving averages as the summation terms in the slope formula.  A
 simple average multiplied by the number of elements equals the sum,
 so we can calcuate the regression slope using simple averages.
 However, if we replace the simple averages in the computation by
 Exponential averages, the resulting slope magnitude turns out larger
 by a factor of 3N/(N+1), where N is the number of elements, therefore
 we must divide the result by this correction factor (this simple
 correction was actually a lot of work to figure out).

 The correction factor will give perfect results for regions of
 perfectly constant slopes, but for noisy data like markets, the
 response time of the Exponential average will make the SWINGS in
 slope magnitude appear smaller than the actual regression slope,
 although the slope will be smoother in areas of fairly constant slope.
*/
				double denom, sumY = 0.0, sumXY = 0.0;
				if (Period != nn || CurrentBar < 1) { // re-initialize if Period changes
					nn = Period;                // save new Period
					n = (nn-1.0)*0.75 + 1.0;    // lag correction
					sumX = 0.5 * n*(n-1.0);     // sum of x values from 0 to n-1
					sumX2 = (n-1.0)*n*(2.0*n-1.0)/6.0;  //sum of x^2 from 0 to n-1
					sumY = Input[0] * n;        // initialize sum of Y values
					sumXY = sumX*Input[0];      // initialize sum of X*Y values
					w = 2.0 / (n + 1.0);        // Exponential weighting factor
					correction = (n+1.0) / (3.0*n); // amplitude correction factor
					if (CurrentBar < 1) {
						sumY1 = sumY;			// would be sumY[1] in a series
						sumXY1 = sumXY;			// would be sumXY[1] in a series
					}
				} else { // calculate sum of Y values and sum of X*Y values
					sumY = w*Input[0]*n + (1.0-w)*sumY1;
					sumXY = Input[0]*(n-1.0) + sumXY1*(n-2.0)/n;
					sumY1 = sumY;               // save for next bar
					sumXY1 = sumXY;             // save for next bar
				}
				denom = n*sumX2 - sumX*sumX;    // denominator of slope formula
				if (denom < 0.00001) denom = 0.00001;
				slope = correction * n*(n*sumXY - sumX*sumY) / denom;
				yvalue = sumY/n + slope * 0.5*n; // regression value, public variable
			}
			else {
/*
 Linear Regression Slope Super Fast Calc
 Derived from an algorithm developed by Bob Fulks and separately by
 Mark A. Simms in 2002.
 Elimination of calculation loop by Alex Matulich, April 2004.
 Ported from TradeStation to NinjaTrader by Alex Matulich, August 2008.
 Tradestation version is published at http://unicorn.us.com/trading/el.html
 
 This is a super-efficient version of the Fulks/Simms Linear
 Regression Slope Fast Calc algorithm.  In this version, a loop gets
 executed only once during initialization, rather than at every bar.
 The result matches exactly the traditional linear regression slope.

 This function assumes that the Y-axis (where X=0) always coincides
 with the current bar.  Therefore, the Y-intercept is the same as
 the value of the regression line at the current bar, and is given
 by the formula:

 YIntercept = yvalue = Average(Price, Length) + slope * Length/2;
*/
				int ix1;
				// Re-initialize 0th bar, every 10000 bars, or when Period changes
				if (CurrentBar < Period || nn != Period || CurrentBar % 10000 == 0) {
					nn = Period;
 					m1 = 6.0 / ((double)Period * ((double)Period + 1.0));
					m2 = 2.0 / ((double)Period - 1.0);
					sumP = 0.0; sumIP = 0.0;
					// this loop is executed only during initialization
					for (ix = 0; ix < Period; ++ix) {
						ix1 = (ix < CurrentBar) ? ix : CurrentBar;
						sumP += Input[ix1];
						sumIP += (ix1 * Input[ix1]);
					}
				} else {
					// Linear regression slope super fast calculation
					sumIP += (sumP - Period * Input[Period]);
					sumP += (Input[0] - Input[Period]);
				}
				slope = m1 * (sumP - m2 * sumIP);
				yvalue = sumP/Period + slope * Period * 0.5;
			}
			switch (Returnval) {
				case 3:
					rtn = 0.0;
					if (CurrentBar > 1) {
						for (ix = 0; ix < ((CurrentBar <= Period) ? CurrentBar+1 : Period); ++ix) {
							rtn = (Input[ix] - (-slope * ix + yvalue));
							err += rtn*rtn;
						}
						rtn = err = Math.Sqrt(err/ix);
					}
					break;
				case 2: rtn = yvalue; break;
				default: rtn = slope; break;
			}
			SlopeSFX[0] = rtn;         // result
        }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Lookback interval", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Exponential", Description="true=Exponential moving slope, false=normal weighting", Order=2, GroupName="Parameters")]
		public bool Exponential
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Returnval", Description="Return value 1=slope, 2=LinReg value, 3=average error", Order=3, GroupName="Parameters")]
		public int Returnval
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SlopeSFX
		{
			get { return Values[0]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegSlopeSFX[] cacheLinRegSlopeSFX;
		public LinRegSlopeSFX LinRegSlopeSFX(int period, bool exponential, int returnval)
		{
			return LinRegSlopeSFX(Input, period, exponential, returnval);
		}

		public LinRegSlopeSFX LinRegSlopeSFX(ISeries<double> input, int period, bool exponential, int returnval)
		{
			if (cacheLinRegSlopeSFX != null)
				for (int idx = 0; idx < cacheLinRegSlopeSFX.Length; idx++)
					if (cacheLinRegSlopeSFX[idx] != null && cacheLinRegSlopeSFX[idx].Period == period && cacheLinRegSlopeSFX[idx].Exponential == exponential && cacheLinRegSlopeSFX[idx].Returnval == returnval && cacheLinRegSlopeSFX[idx].EqualsInput(input))
						return cacheLinRegSlopeSFX[idx];
			return CacheIndicator<LinRegSlopeSFX>(new LinRegSlopeSFX(){ Period = period, Exponential = exponential, Returnval = returnval }, input, ref cacheLinRegSlopeSFX);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegSlopeSFX LinRegSlopeSFX(int period, bool exponential, int returnval)
		{
			return indicator.LinRegSlopeSFX(Input, period, exponential, returnval);
		}

		public Indicators.LinRegSlopeSFX LinRegSlopeSFX(ISeries<double> input , int period, bool exponential, int returnval)
		{
			return indicator.LinRegSlopeSFX(input, period, exponential, returnval);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegSlopeSFX LinRegSlopeSFX(int period, bool exponential, int returnval)
		{
			return indicator.LinRegSlopeSFX(Input, period, exponential, returnval);
		}

		public Indicators.LinRegSlopeSFX LinRegSlopeSFX(ISeries<double> input , int period, bool exponential, int returnval)
		{
			return indicator.LinRegSlopeSFX(input, period, exponential, returnval);
		}
	}
}

#endregion
