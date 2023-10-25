// 
// Copyright (C) 2017, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Deltabar : Indicator
	{
		private double	buys;
		private double	sells;
		private int activeBar = 0;
        private List<double  > Delta_list = new List<double >();
		private double  last_delta =0;
		private bool check_vol, check_osb, check_delta,checkvol_osb,checkvol_nc,nhanchim;
	//	public async Task<void> thongbao(){}
		//private double 
		private CustomEnumNamespace.UniversalMovingAverage	maType_vol	= CustomEnumNamespace.UniversalMovingAverage.SMA;
		private int											Period_vol	= 14;
		
		protected override void OnStateChange()
		{
		
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBuySellVolume;
				Name					= "1-Deltabar";
				BarsRequiredToPlot		= 1;
				Calculate				= Calculate.OnEachTick;
				DrawOnPricePanel		= true;
				IsOverlay				= false;
				DisplayInDataBox		= false;

				// Plots will overlap each other no matter which one of these comes first
				// in NT8, we would add the Sells first in code and then Buys, and the "Sells" was always in front of the buys.
				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Bar, "BuySellDiff");
				AddLine(Brushes.Gray, 0, "zeroLine");
				
				// Phấn mặc định màu nến
				osb_up = Brushes.LightGreen;
				osb_dow = Brushes.DarkOrange;
				
				nhan_chim = Brushes.SpringGreen;
				OSB = Brushes.Red;
				DojiBrush = Brushes.DodgerBlue;
				
				
			}
			else if (State == State.Historical)
			{
				if (Calculate != Calculate.OnEachTick)
				{
					//Draw.TextFixed(this, "NinjaScriptInfo", string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnBarCloseError, Name), TextPosition.BottomRight);
					Log(string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnBarCloseError, Name), LogLevel.Error);
				}
			}
		}
		
		private double MA_VOL(){             // tính toán giá trị MA của vol để so sánh
			
			switch (maType_vol)
			{
				// If the maType is defined as an EMA then...
				case CustomEnumNamespace.UniversalMovingAverage.EMA:
				{
					// Sets the plot to be equal to the EMA's plot
				return 	Value[0] = (EMA(Period_vol)[0]);
					break;
				}
				
				// If the maType is defined as a HMA then...
				case CustomEnumNamespace.UniversalMovingAverage.HMA:
				{
					// Sets the plot to be equal to the HMA's plot
				return 	Value[0] = (HMA(Period_vol)[0]);
					break;
				}
				
				// If the maType is defined as a SMA then...
				case CustomEnumNamespace.UniversalMovingAverage.SMA:
				{
					// Sets the plot to be equal to the SMA's plot
				return 	Value[0] = (SMA(Period_vol)[0]);
					break;
				}
				
				// If the maType is defined as a WMA then...
				case CustomEnumNamespace.UniversalMovingAverage.WMA:
				{
					// Sets the plot to be equal to the WMA's plot
				return 	Value[0] = (WMA(Period_vol)[0]);
					break;
				}
				default:
					return 0;
					break;
			}
			
		}

		private void Tunaround(){
			//  Chọn kiểu tính toán sma volum
		
			// xanh 
			if( Delta_list[CurrentBars[0]-1] >0 && Delta_list[CurrentBars[0]-2] <0){
				
				// Cần làm thêm delta tails
					// tính toán ở đây
					Print(Delta_list[CurrentBars[0]-1] + "  " + (CurrentBars[0]-1));
					if( BarsArray[0].GetLow(CurrentBars[0]-1) <= BarsArray[0].GetLow(CurrentBars[0]-2) && BarsArray[0].GetClose(CurrentBars[0]-1) > BarsArray[0].GetOpen(CurrentBars[0]-1)){
						Print("up");
						Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Green,true);
						if(check_vol == true  )
						{
							if( BarsArray[0].GetVolume(CurrentBars[0]-1) >  BarsArray[0].GetVolume(CurrentBars[0]-2))
								//Draw.Text(this, "volUP"+ CurrentBar, "vol",	0, BarsArray[0].GetLow(CurrentBars[0]-1) - (18 * TickSize + 0.25), Brushes.Green);
								Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Blue,true);
						}
						else{ // Nếu ko chọn lọc vol
						//	Draw.Text(this, "volUP"+ CurrentBar, "vol",	0, BarsArray[0].GetLow(CurrentBars[0]-1) - (18 * TickSize + 0.25), Brushes.Green);
							Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Green,true);
						}
						
					}
					return;
				
			}
			if(Delta_list[CurrentBars[0]-1] <0 && Delta_list[CurrentBars[0]-2] >0) { // đỏ
				
					// tính toán ở đây
					Print(Delta_list[CurrentBars[0]-1] + "  " + (CurrentBars[0]-1));
					if(BarsArray[0].GetHigh(CurrentBars[0]-1) >= BarsArray[0].GetHigh(CurrentBars[0]-2) &&BarsArray[0].GetClose(CurrentBars[0]-1) < BarsArray[0].GetOpen(CurrentBars[0]-1)  ){
						Print("dow");
						 Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,Brushes.Red,true);
						if(check_vol == true)
						{      
							if( BarsArray[0].GetVolume(CurrentBars[0]-1) >  BarsArray[0].GetVolume(CurrentBars[0]-2))
								Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,Brushes.Pink,true);
							//	Draw.Text(this, "voldow"+ CurrentBar, "vol",	0, BarsArray[0].GetHigh(CurrentBars[0]-1) + (18 * TickSize + 0.25), Brushes.Red);
						}
						else{
							Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,Brushes.Red,true);
						}
					}
				
				return;
			}
		}
		
		private void DrawDelta(){
			if (CurrentBar < activeBar || CurrentBar <= BarsRequiredToPlot)
				return; 
			if (CurrentBar != activeBar)
			{
				BuySellDiff[1] = buys - sells;
				
				if (BuySellDiff[1] > 0)
				{
					PlotBrushes[0][1] = Brushes.Green;
				}
				else
				{
					PlotBrushes[0][1] = Brushes.Red;
				}	
				
				buys = 0;
				sells = 0;
				activeBar = CurrentBar;
				
			}
			
			// Dynamically update current bar
			
			BuySellDiff[0] =  buys - sells;	   //store intrabar diff		
			
			if (BuySellDiff[0] > 0)  // update intrabar
			{
				PlotBrushes[0][0] = Brushes.Green;
			}
			else
			{
				PlotBrushes[0][0] = Brushes.Red;
			}
		}
		
		private void OSBCheck(){
			if(check_osb==false)
				return;
			// H1 -h2 > 0 && l1-l2 <0
			if(BarsArray[0].GetHigh(CurrentBars[0]-1)  - BarsArray[0].GetHigh(CurrentBars[0]-2)>0 &&  BarsArray[0].GetLow(CurrentBars[0]-2)- BarsArray[0].GetLow(CurrentBars[0]-1) >0){
				// OSB XANH
				bool vol_ok =false ;
				if(checkvol_osb ==true)
					 vol_ok = ( BarsArray[0].GetVolume(CurrentBars[0]-1) > BarsArray[0].GetVolume(CurrentBars[0]-2))?true:false;
				
				if(BarsArray[0].GetClose(CurrentBars[0]-1) <BarsArray[0].GetOpen(CurrentBars[0]-1) ){
					if(checkvol_osb==true){
						CandleOutlineBrush = osb_up;
					}
					else if (check_osb ==false){ // Mặc định ko check vol
						CandleOutlineBrush = osb_up;
					}
				}else{ // OSB đỏ
					if(checkvol_osb==true){
						CandleOutlineBrush = osb_up;
					}
					else if(check_osb ==false) { // Mặc định ko check vol
						CandleOutlineBrush = osb_up;
					}
				}
			}
			
		}
		
		
		protected override void OnMarketData(MarketDataEventArgs e)
		{			
			if(e.MarketDataType == MarketDataType.Last)
			{				
				if(e.Price >= e.Ask)
				{
					buys += e.Volume;
					last_delta  += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
					sells += e.Volume;
					last_delta -= e.Volume;
				}
			}	
		}
		
		protected override void OnBarUpdate()
		{
			 //Draw.Text(this, "Bardltchgb"+ CurrentBar, (CurrentBar).ToString(),	0, High[0] + (1 * TickSize + 1), Brushes.White);//      Debug index bar
		  if( BarsInProgress ==0 && IsFirstTickOfBar &&CurrentBars[0]!=0 ){
			   // Loại trừ trường hợp tick đàu tiên của nến đầu tiên (bắt đầu phiên delta lúc này chưa có )
			  // thêm last_delta tới list 
			 Delta_list.Add(last_delta);
			 last_delta = 0;
			   // Print((CurrentBars[0]).ToString() + " " + Delta_list[CurrentBars[0]] );
			   // Print((CurrentBar).ToString() + " " + Delta_list[CurrentBar] );
				if(Calculate != Calculate.OnBarClose && (State == State.Realtime || BarsArray[0].IsTickReplay)){
				//	Print(" Che do phat thuc + tick đâu tiên của nến");
					if(CurrentBars[0] >=2){
                     Tunaround();
					}
				}
			}
	       //END tnround

		  
		  if(check_osb==true){
		  	
		  }
		  
		  // Vẽ Delta
		  DrawDelta();
		}
		
		
		
		

		#region Properties


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySellDiff
		{
			get { return Values[0]; }
		}
		[Display(GroupName = "1-Delta Tnround", Description="Numbers of bars used for calculations")]
		public bool CheckDelta
		{
			get { return check_vol; }
			set { check_vol = value; }
		}
	    [Display(GroupName = "1-Delta Tnround", Description="Numbers of bars used for calculations")]
		public bool CheckVol
		{
			get { return check_vol; }
			set { check_vol = value; }
		}

		/// <summary>
		///  CHeck OSB
		/// </summary>
		/// 
		[Display(GroupName = "2. Check OSB", Description="Check nến osb")]
		public bool CheckOSB
		{
			get { return check_osb; }
			set { check_osb = value; }
		}
		[Display(GroupName = "2. Check OSB", Description="Check nến osb")]
		public bool CheckVolOSB
		{
			get { return checkvol_osb; }
			set { checkvol_osb = value; }
		}
		[Display(GroupName = "2. Check OSB", Description="Check nến osb")]
		[XmlIgnore]
		public Brush osb_up 
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string F_OsbUp
		{
		  get { return Serialize.BrushToString(osb_up); }
		  set { osb_up = Serialize.StringToBrush(value); }
		}
		
		[Display(GroupName = "2. Check OSB", Description="Check nến osb")]
		[XmlIgnore]
		public Brush osb_dow
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string F_OsbDow
		{
		  get { return Serialize.BrushToString(osb_dow); }
		  set { osb_dow = Serialize.StringToBrush(value); }
		}
		///
		/// 
		/// Check Nhấn chìm
		/// 
		[Display(GroupName = "3. Check nhấn chìm", Description="Nến nhấm chim")]
		public bool NhanChim
		{
			get { return nhanchim; }
			set { nhanchim = value; }
		}
		[Display(GroupName = "3. Check nhấn chìm", Description="Kiếm tra vol nhấn chìm")]
		public bool NhanChim_vol
		{
			get { return checkvol_nc; }
			set { checkvol_nc = value; }
		}
		/// 
		///
		///    Cấu hình EMA SMA volume
		/// 
		/// 
		[Display(GroupName = "Cấu hình period lọc Vol", Description="Chọn Kiểu lọc tính trung bình vol")]
		public CustomEnumNamespace.UniversalMovingAverage MAType
		{
			get { return maType_vol; }
			set { maType_vol = value; }
		}
		
		[Display(GroupName = "Cấu hình period lọc Vol", Description="Số thanh nến tính toán")]
		public int fPeriod_vol
		{
			get { return Period_vol; }
			set { Period_vol = Math.Max(1, value); }
		}
		
		/// <summary>
		/// /////////////////////////////////////
		/// </summary>
		[Display(GroupName = "Màu nến", Description="Tùy chỉnh màu nến")]
		[XmlIgnore]
		public Brush nhan_chim 
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string nhanchimcolor
		{
		  get { return Serialize.BrushToString(nhan_chim); }
		  set { nhan_chim = Serialize.StringToBrush(value); }
		}
			
		[Display(GroupName = "Màu nến", Description="Tùy chỉnh màu nến")]
		[XmlIgnore]
		public Brush OSB 
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string OsbBarcolo
		{
		  get { return Serialize.BrushToString(OSB); }
		  set { OSB = Serialize.StringToBrush(value); }
		}
		
		
		[Display(GroupName = "Màu nến", Description="Tùy chỉnh màu nến")]
		[XmlIgnore]
		public Brush DojiBrush 
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string DojiBrushSerialize
		{
		  get { return Serialize.BrushToString(DojiBrush); }
		  set { DojiBrush = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Deltabar[] cacheDeltabar;
		public Deltabar Deltabar()
		{
			return Deltabar(Input);
		}

		public Deltabar Deltabar(ISeries<double> input)
		{
			if (cacheDeltabar != null)
				for (int idx = 0; idx < cacheDeltabar.Length; idx++)
					if (cacheDeltabar[idx] != null &&  cacheDeltabar[idx].EqualsInput(input))
						return cacheDeltabar[idx];
			return CacheIndicator<Deltabar>(new Deltabar(), input, ref cacheDeltabar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Deltabar Deltabar()
		{
			return indicator.Deltabar(Input);
		}

		public Indicators.Deltabar Deltabar(ISeries<double> input )
		{
			return indicator.Deltabar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Deltabar Deltabar()
		{
			return indicator.Deltabar(Input);
		}

		public Indicators.Deltabar Deltabar(ISeries<double> input )
		{
			return indicator.Deltabar(input);
		}
	}
}

#endregion
