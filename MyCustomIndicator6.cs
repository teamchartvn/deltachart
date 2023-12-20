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
	public class ZigZagSigle : Indicator
	{
		/// <summary>
		/// KHAI BAO 
		/// //Depth (độ sâu): số nến tổi thiểu trong trường hợp không có độ lệch (deviation) giữa 2 mốc cao và thấp của chỉ báo zigzag (đỉnh sóng và đáy sóng).
		///Depth1 = input.int(12, 'Depth', minval=1, step=1,group = "Zigzang 1")

        //Deviation (độ lệch): được tính bằng pips hoặc points. Đây là khoảng cách tối thiểu giữa 2 mốc cao và thấp của chỉ báo zig zag.
		///Deviation1 = input.int(5, 'Deviation', minval=1, step=1,group = "Zigzang 1")

        //Backstep: hiểu là biên độ nến thay đổi giữa 2 mốc cao và thấp của chỉ báo zigzag.
		///Backstep1 = input.int(2, 'Backstep', minval=2, step=1,group = "Zigzang 1")
		/// line_thick1 = input.int(2, 'Line Thickness', minval=1, maxval=4,group = "Zigzang 1") // độ dày đường liên quan vẽ
		///upcolor1 = input(color.lime, 'Bull Color',group = "Zigzang 1")        // Màu tăng UP
		///dncolor1 = input(color.red, 'Bear Color',group = "Zigzang 1")        // Màu giảm
		///showzz1 = input(true, 'Show Zigzang 1',group = "Zigzang 1")          // Bật tắt show line
		///repaint1 = input(true, 'Repaint Levels',group = "Zigzang 1")         // 
		///shiftlabel1 = input.float(0.3, 'Move label 1 up/doww', minval=-100,group = "Zigzang 1")
		///
		///Depth2 = input.int(12, 'Depth', minval=1, step=1,group = "Zigzang 2")              // số nến tối thiểu trong trường hợp nếu Deviation2 =0
		///Deviation2 = input.int(5, 'Deviation', minval=1, step=1,group = "Zigzang 2")      // khoảng cách tối thiểu giữa mốc cao và thấp ( mức cao mức thấp phải chênh nhau tối thiểu - deviation )
		///Backstep2 = input.int(2, 'Backstep', minval=2, step=1,group = "Zigzang 2")        //hiểu là biên độ nến thay đổi giữa 2 mốc cao và thấp của chỉ báo zig zag.
		///line_thick2 = input.int(2, 'Line Thickness', minval=1, maxval=4,group = "Zigzang 2")
		///upcolor2 = input(color.lime, 'Bull Color',group = "Zigzang 2")
		///dncolor2 = input(color.red, 'Bear Color',group = "Zigzang 2")
		///showzz2 = input(true, 'Show Zigzang 2',group = "Zigzang 2")
		///repaint2 = input(true, 'Repaint Levels',group = "Zigzang 2")
		///shiftlabel2 = input.float(-0.1, 'Move label 2 up/doww', minval=-100,group = "Zigzang 2")
		/// 
		/// </summary>
		private Series<double>		zigZagHighZigZags;
		private Series<double>		zigZagLowZigZags;
		private Series<double>		zigZagHighSeries;
		private Series<double>		zigZagLowSeries;

		
		private int depth_2 = 0; 
		private int deviation_2 =0;  
		private double backstep_2 =0.0;
		
		
		
		private int backstep , depth , deviation ;
		private double last_low, last_high;
		private bool trend =false ;   // false giảm , true tăng 
		/// <summary>
		/// 
		/// </summary>
		private int dept =12;
		//private bool  lowing ,highing;
		
		private List< bool > lowing  = new List<bool>();
		private List< bool > highing  = new List<bool>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyCustomIndicator6";
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
				
				// THam số mặc định của Indi nếu giá trị ko được set
				backstep = 3;
				depth = 12;
				deviation =5;
				last_low =0 ;
				last_high =0;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < dept-1) // Need at least 3 bars to calculate Low/High      12 giá trị đầu của mảng Higwing lowing mặc định = false:
			{
				zigZagHighSeries[0]		= 0;
				zigZagLowSeries[0] 		= 0;
				
                highing.Add(false);
				lowing.Add(false);
				return;
			}
			
			int prev_low = LowestBar(Low,dept);
			int prev_hig = HighestBar(High,dept);
			
			//lowing = (2 == prev_low || Low[0] - Low[prev_low] > deviation_2 * TickSize ) ? true:false;
			lowing.Add((2 == prev_low || Low[0] - Low[prev_low] > deviation_2 * TickSize ) ? true:false);
			//highing = (2 == prev_hig || High[0] - High[prev_low] > deviation_2 * TickSize ) ? true:false;
			highing.Add( (2 == prev_hig || High[0] - High[prev_low] > deviation_2 * TickSize ) ? true:false);
			
			int lh = MRO(() => !highing[(highing.Count -1)],1,dept-1);
			
			// Check last_low 
			
			
			
			
			 Print(1);
			if( BarsInProgress ==0 && IsFirstTickOfBar &&CurrentBars[0]!=0 ){
				 if (CurrentBars[0] >1){
				
					
				 last_high =	last_high > BarsArray[0].GetLow(CurrentBars[0]-1)?last_high:BarsArray[0].GetLow(CurrentBars[0]-1);
					 
					 
					 if (trend == false  ){ // đang trend giảm
					  if(last_low < BarsArray[0].GetLow(CurrentBars[0]-1)){
					 	trend = false; // Giảm
					        }else{
					 	        last_low = BarsArray[0].GetLow(CurrentBars[0]-1);
					            } 
					 }
					 if(last_high < BarsArray[0].GetLow(CurrentBars[0]-1)){
					 	
					 }
				 }
				 
				else if (CurrentBars[0] ==1){ // nến thứ 2 cập nhật giá trị đầu tiên cho hig,low
					last_low = BarsArray[0].GetLow(CurrentBars[0]-1);
					last_high = BarsArray[0].GetHigh (CurrentBars[0]-1);
				}
				
				
				
				
				
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZigZagSigle[] cacheZigZagSigle;
		public ZigZagSigle ZigZagSigle()
		{
			return ZigZagSigle(Input);
		}

		public ZigZagSigle ZigZagSigle(ISeries<double> input)
		{
			if (cacheZigZagSigle != null)
				for (int idx = 0; idx < cacheZigZagSigle.Length; idx++)
					if (cacheZigZagSigle[idx] != null &&  cacheZigZagSigle[idx].EqualsInput(input))
						return cacheZigZagSigle[idx];
			return CacheIndicator<ZigZagSigle>(new ZigZagSigle(), input, ref cacheZigZagSigle);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZigZagSigle ZigZagSigle()
		{
			return indicator.ZigZagSigle(Input);
		}

		public Indicators.ZigZagSigle ZigZagSigle(ISeries<double> input )
		{
			return indicator.ZigZagSigle(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZigZagSigle ZigZagSigle()
		{
			return indicator.ZigZagSigle(Input);
		}

		public Indicators.ZigZagSigle ZigZagSigle(ISeries<double> input )
		{
			return indicator.ZigZagSigle(input);
		}
	}
}

#endregion
