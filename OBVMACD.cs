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
	public class OBVMACD : Indicator
	{
		
		private int windowLen;
		private int vlen;
		private int obvLength;
		
		private int obvEmaStart;
		private int fastMaStart;
		private int slowMaStart;
		
		private List<double> hlDelta; // high-low
		private List<double> obv;
		private List<double> obvSmooth;
		private List<double> shadowOut;
		private List<double> obvEma;
		private List<double> fastMa;
		private List<double> slowMa;
		private List<double> macd;
		//private List<double> processedMacd;
		private List<double> mcadSum;
		
		private Series<int> signal;
		private Series<double> processedMacd;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "01-OBVMACD";
				Calculate									= Calculate.OnEachTick;
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
				AddLine(Brushes.DarkGray,					0,				Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddPlot(Brushes.SkyBlue, "MACD");
				Plots[0].Width 		= 2;
				
				FastLength					= 9;
				SlowLength					= 26;
			}
			else if (State == State.Configure)
			{
				windowLen = 28;
				vlen = 14;
				obvLength = 1;
				
				obvEmaStart = vlen + windowLen + obvLength - 2;
				fastMaStart = vlen + windowLen + obvLength + FastLength - 3;
				slowMaStart = SlowLength - 1;
				
				hlDelta = new List<double>();
				obv = new List<double>();
				obvSmooth = new List<double>();
				shadowOut = new List<double>();
				obvEma = new List<double>();
				fastMa = new List<double>();
				slowMa = new List<double>();
				macd = new List<double>();
				mcadSum = new List<double>();
				signal = new Series<int>(this);
				
				processedMacd = new Series<double>(this);
				
				//macdTest = new Series<double>(this);
			}
			else if(State == State.DataLoaded)
			{
				//hlDelta = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//if(CurrentBars[0] <= fastMaStart + 1)
			//{
				if( BarsInProgress == 0 && IsFirstTickOfBar && CurrentBars[0] > 0)
				{
					//hlDelta[1] = BarsArray[0].GetHigh(CurrentBars[0]-1) - BarsArray[0].GetLow(CurrentBars[0]-1);
					
					//calculate high - low 
					double delta = BarsArray[0].GetHigh(CurrentBars[0]-1) - BarsArray[0].GetLow(CurrentBars[0]-1);
					hlDelta.Add(delta);

					
					//calculate obv
					CalculateOBV(ref BarsArray[0], ref obv, CurrentBars[0]-1);

					//calculate smooth (sma of obv), exclude obv index 0 
					double currentSmooth =  CurrentBars[0] > vlen ? CalculateSMA(ref obv, vlen, CurrentBars[0]-1) : 0;
				//	Print("smooth: " + currentSmooth);
					
					//add (current obv - smooth) to array
					double currentOBVSmooth = CurrentBars[0] > vlen ? obv[CurrentBars[0] - 1] - currentSmooth : 0.0;
					obvSmooth.Add(currentOBVSmooth);


					double shadow = 0.0, priceSpread = 0.0, volumeSpread = 0.0, currentOut = 0.0;
					
					if(CurrentBars[0] >= vlen + windowLen)
					{
						//calculate stdev of  (high-low)
						priceSpread = StdDev(ref hlDelta, windowLen, CurrentBars[0] - 1);
						
						//calculate stdev of (obv - smooth) 
						volumeSpread = StdDev(ref obvSmooth, windowLen, CurrentBars[0] - 1);
						
						//calculate shadow = (obv - smooth)/ stdev obvsmooth * stdev highlow
						shadow = currentOBVSmooth / volumeSpread * priceSpread;
						
						//calculate out = shadow > 0 ? high + shadow : low + shadow 
						currentOut = shadow > 0 ? BarsArray[0].GetHigh(CurrentBars[0]-1) + shadow : BarsArray[0].GetLow(CurrentBars[0]-1) + shadow;
						shadowOut.Add(currentOut);
					}
					else
						shadowOut.Add(currentOut);

					
					//calculate ema of out 
					double currentObvEma = CurrentBars[0] > obvEmaStart ? CalculateEMA(ref shadowOut, ref obvEma, obvLength, obvEmaStart, CurrentBars[0]-1) : 0.0;
					obvEma.Add(currentObvEma);
					//Print("obvema: " + currentObvEma);
					
					//calculate slow ma (ema(close, slowLength))
					double currentSlowMa = CurrentBars[0] > slowMaStart ? CalculateEMA(ref BarsArray[0], ref slowMa, SlowLength, slowMaStart, CurrentBars[0]-1) : 0.0;
					slowMa.Add(currentSlowMa);
					//Print("slow ma: " + currentSlowMa);
					
					double currentFastMa = 0.0 , currentMACD = 0.0, currentMacdSum = 0.0;
					if(CurrentBars[0] > fastMaStart)
					{
						//calculate fast ma (ema(obvema, fastLength))
						currentFastMa = CalculateEMA(ref obvEma, ref fastMa, FastLength, fastMaStart, CurrentBars[0]-1);
						fastMa.Add(currentFastMa);
						
						
						//calculate macd
						currentMACD = currentFastMa - slowMa[CurrentBars[0]-1];
						macd.Add(currentMACD);
						
						//calculate processed macd
						if(CurrentBars[0] == fastMaStart + 1)
						{
							mcadSum.Add(currentMacdSum);
							processedMacd[1] = currentMACD;
							Values[0][1] = processedMacd[1];
						}
						else
						{
							currentMacdSum = Math.Abs(currentMACD - processedMacd[2]) + mcadSum[CurrentBars[0] - 2];
							mcadSum.Add(currentMacdSum);
							double average = currentMacdSum/(CurrentBars[0] - 1);
							
							processedMacd[1] = (currentMACD > processedMacd[2]+average) || (currentMACD<processedMacd[2]-average) ? currentMACD : processedMacd[2];
							
							//Add plot
							Values[0][1] =processedMacd[1];
							
							signal[1] = processedMacd[1] - processedMacd[2] > 0 ? 1 : processedMacd[1] - processedMacd[2] < 0 ? -1 : signal[2];
							
							if(signal[1] == 1)
								PlotBrushes[0][1] = Brushes.SkyBlue;
							else
								PlotBrushes[0][1] = Brushes.Red;
							
							if(signal[1] - signal[2] > 0)
							{
								if(!show_muiten )
									return;
								Draw.ArrowUp(this, (CurrentBars[0]-2).ToString()+ "up", true, 2, High[2] + 5*TickSize, Brushes.Blue);
								//Print("signal up ");
								//Draw.Text(this, (CurrentBars[0]-1).ToString()+ "index",(CurrentBars[0]-2).ToString(),2 , High[2] + TickSize);
							}
							if(signal[1] - signal[2] < 0)
							{
								if(!show_muiten )
									return;
								Draw.ArrowDown(this, (CurrentBars[0]-2).ToString()+ "down", true, 2, Low[2] - 5*TickSize, Brushes.Red);
								//Print("signal down ");
								
							}
							
						}
					}
					else
					{
						fastMa.Add(currentFastMa);
						macd.Add(currentMACD);
						mcadSum.Add(currentMacdSum);
						processedMacd[1] = 0.0; 
					}
					//Print("fast ma: " + currentFastMa);
					//Print("macd: " + currentMACD);
					//Print("sum macd " + currentMacdSum);
					//Print("macd processed: " + processedMacd[1]);

					
					//
				}
		
			
			
		}
		//calculate standared deviation
		protected double StdDev(ref List<double> array, int period, int index)
		{
			double sum=0.0;
			for(int i=index;i>index-period;i--)
				sum+=array[i];
			
			double mean = sum/period, sumSquaredDiff = 0.0;
			for(int i=index;i>index-period;i--)
				sumSquaredDiff += (array[i]-mean) * (array[i]-mean);
			return (double)Math.Sqrt(sumSquaredDiff / period);
		}
		
		//calculate obv
		protected void CalculateOBV(ref Bars barArray, ref List<double> obvArray,int index)
		{
			double currentOBV = 0.0;
			if(index == 0)
			{
				currentOBV = 0.0;
				obvArray.Add(currentOBV);
		//		Print("obv: " + currentOBV);
				return;
			}
			double vol = barArray.GetVolume(index);
			double prevClose = barArray.GetClose(index-1);
			double currentClose = barArray.GetClose(index);
			
			if(currentClose < prevClose)
				currentOBV = obvArray[index - 1] - vol;
			else if(currentClose > prevClose)
				currentOBV = obvArray[index - 1] + vol;
			else
				currentOBV = obvArray[index - 1];
				
			obvArray.Add(currentOBV);
			Print("obv: " + currentOBV);
		}
		
		//calculate sma
		protected double CalculateSMA(ref List<double> array, int period, int index)
		{
			double sum=0.0;
			if(index < period - 1)
				return 0.0;
			for(int i=index; i>index-period;i--)
				sum += array[i];
			return (double)(sum / period);
		}
		
		//calculate ema
		protected double CalculateEMA(ref List<double> array, ref List<double> emaArray, int period, int start, int index)
		{
			double ema = 0.0;
			if(index == start)
			{
				double sum = 0.0, average=0.0;
				for(int i = index; i > index-period; i--)
					sum += array[i];
				ema = (double)(sum/period);
				return ema;
			}
			
			double alpha = 2.0 / (period + 1.0);
			ema = alpha * array[index] + (1.0 - alpha) * emaArray[index - 1];
			return ema;
		}
		
		//overloaded method CalculateEMA
		protected double CalculateEMA(ref Bars barArray, ref List<double> emaArray, int period, int start, int index)
		{
			double ema = 0.0;
			if(index == start)
			{
				double sum = 0.0, average=0.0;
				for(int i = index; i > index-period; i--)
					sum += barArray.GetClose(i);
				ema = (double)(sum/period);
				return ema;
			}
			
			double alpha = 2.0 / (period + 1.0);
			ema = alpha * barArray.GetClose(index) + (1.0 - alpha) * emaArray[index - 1];
			return ema;
		}
		#region Properties
		private bool show_muiten= false;
		 [Display(GroupName = "Con Cac", Description="Numbers of bars used for calculations")]
		public bool Show
		{
			get { return show_muiten; }
			set { show_muiten = value; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast Length", Order=1, GroupName="Parameters")]
		public int FastLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow Length", Order=2, GroupName="Parameters")]
		public int SlowLength
		{ get; set; }
		#endregion
		
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OBVMACD[] cacheOBVMACD;
		public OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return OBVMACD(Input, fastLength, slowLength);
		}

		public OBVMACD OBVMACD(ISeries<double> input, int fastLength, int slowLength)
		{
			if (cacheOBVMACD != null)
				for (int idx = 0; idx < cacheOBVMACD.Length; idx++)
					if (cacheOBVMACD[idx] != null && cacheOBVMACD[idx].FastLength == fastLength && cacheOBVMACD[idx].SlowLength == slowLength && cacheOBVMACD[idx].EqualsInput(input))
						return cacheOBVMACD[idx];
			return CacheIndicator<OBVMACD>(new OBVMACD(){ FastLength = fastLength, SlowLength = slowLength }, input, ref cacheOBVMACD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return indicator.OBVMACD(Input, fastLength, slowLength);
		}

		public Indicators.OBVMACD OBVMACD(ISeries<double> input , int fastLength, int slowLength)
		{
			return indicator.OBVMACD(input, fastLength, slowLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return indicator.OBVMACD(Input, fastLength, slowLength);
		}

		public Indicators.OBVMACD OBVMACD(ISeries<double> input , int fastLength, int slowLength)
		{
			return indicator.OBVMACD(input, fastLength, slowLength);
		}
	}
}

#endregion
