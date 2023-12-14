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
	public class deltabar : Indicator
	{
		private double	buys,thannen_sma=0;
		private double	sells;
		private int activeBar = 0,periodsma_thannen=1;
        private List<double  > Delta_list = new List<double >();
		private double  last_delta =0,giatritb_thannen=0,heso_thannen=1;
		private bool check_vol, check_osb, check_delta,checkvol_osb,checkvol_nc,nhanchim;
		private bool delta_tails;
		
		// Khai báo biến check thân nến
		private List<double > Thannen_list = new List<double>();
		private int period_thannen =3; 
		private double sma_thannen=0;
		// màu mũi tên tín hiệu thỏa mãn vol;
		private Brush volup_color = Brushes.Green;
        private Brush voldow_color = Brushes.Red;
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
				osb_dow = Brushes.OrangeRed;
				
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
				return 	Value[0] = (EMA(Volume,Period_vol)[1]);
					break;
				}
				// If the maType is defined as a HMA then...
				case CustomEnumNamespace.UniversalMovingAverage.HMA:
				{
					// Sets the plot to be equal to the HMA's plot
				return 	Value[0] = (HMA(Volume,Period_vol)[1]);
					break;
				}
				// If the maType is defined as a SMA then...
				case CustomEnumNamespace.UniversalMovingAverage.SMA:
				{
					// Sets the plot to be equal to the SMA's plot
				return 	Value[0] = (SMA(Volume,Period_vol)[1]);
					break;
				}
				// If the maType is defined as a WMA then...
				case CustomEnumNamespace.UniversalMovingAverage.WMA:
				{
					// Sets the plot to be equal to the WMA's plot
				return 	Value[0] = (WMA(Volume,Period_vol)[1]);
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
			if( Delta_list[CurrentBars[0]-1] >0  ){
                    if(Delta_list[CurrentBars[0]-2] >0)
					  return; 
					if( BarsArray[0].GetLow(CurrentBars[0]-1) <= BarsArray[0].GetLow(CurrentBars[0]-2) && BarsArray[0].GetClose(CurrentBars[0]-1) > BarsArray[0].GetOpen(CurrentBars[0]-1)){
						Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Green,true);
							Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,volup_color,true);
						return; // Delta Tunaround ok => done
					}
					
			}
			else { // đỏ
				if( Delta_list[CurrentBars[0]-2] <0)
					return;
					if(BarsArray[0].GetHigh(CurrentBars[0]-1) >= BarsArray[0].GetHigh(CurrentBars[0]-2) &&BarsArray[0].GetClose(CurrentBars[0]-1) < BarsArray[0].GetOpen(CurrentBars[0]-1)  ){
							Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,voldow_color,true);
						return; // delta tunaround ok done !
					}
				
			}
			
		}

		private void Delta_phanky(){
			if(Delta_list[CurrentBars[0]-1] > 0 && (BarsArray[0].GetOpen(CurrentBars[0]-1) > BarsArray[0].GetClose(CurrentBars[0]-1) )){
				Draw.Dot(this,"do" +CurrentBar ,true,1,BarsArray[0].GetLow(CurrentBars[0]-1) - 2* TickSize,Brushes.RosyBrown,true);
			}
			if(Delta_list[CurrentBars[0]-1] < 0 && (BarsArray[0].GetOpen(CurrentBars[0]-1) < BarsArray[0].GetClose(CurrentBars[0]-1) )){
				Draw.Dot(this,"xanh" +CurrentBar ,true,1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,Brushes.GreenYellow,true);
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
			// H1 -h2 > 0 && l1-l2 <0 Dieu kien check Osb
			if(BarsArray[0].GetHigh(CurrentBars[0]-1)  - BarsArray[0].GetHigh(CurrentBars[0]-2)>0 &&  BarsArray[0].GetLow(CurrentBars[0]-2)- BarsArray[0].GetLow(CurrentBars[0]-1) >0){
				// OSB XANH
			
				bool vol_ok =false ;
				if(checkvol_osb ==true){// CO CHECK VOl
					vol_ok = ( BarsArray[0].GetVolume(CurrentBars[0]-1) > BarsArray[0].GetVolume(CurrentBars[0]-2))?true:false;
				
				if(vol_ok){ 
					    if(BarsArray[0].GetClose(CurrentBars[0]-1) <BarsArray[0].GetOpen(CurrentBars[0]-1) ){
					// Mặc định ko check vol
						BarBrushes[1]  =osb_dow;
						CandleOutlineBrushes[1]  = osb_dow;
					
				}else{ // OSB Xanh
						BarBrushes[1]  =osb_up;
						CandleOutlineBrushes[1]  = osb_up;
				}
				}
				}else{     // Khong check vol
					     if(BarsArray[0].GetClose(CurrentBars[0]-1) <BarsArray[0].GetOpen(CurrentBars[0]-1) ){
					// Mặc định ko check vol
						BarBrushes[1]  =osb_dow;
						CandleOutlineBrushes[1]  = osb_dow;
					
				  }else{ // OSB đỏ
						BarBrushes[1]  =osb_up;
						CandleOutlineBrushes[1]  = osb_up;
				  }
				}
			//	Print("OSB = "+ (CurrentBars[0]-1).ToString());
			}
		}
		
		private bool CheckThanNen(){
			
			 double new_thannen = Math.Abs(BarsArray[0].GetOpen(CurrentBars[0]-1) -BarsArray[0].GetClose(CurrentBars[0]-1));
			         Thannen_list.Add(new_thannen); // Mỗi khi chạy qua nến mới, thêm độ dài nến cũ vào list
			/*
			        try {
				          if(CurrentBars[0]-1 > period_thannen) {
					           //sma_thannen += sma_thannen + Math.Abs( new_thannen - Thannen_list[CurrentBars[0]- period_thannen])/period_thannen;
							 sma_thannen +=    (new_thannen - Thannen_list[CurrentBars[0]- period_thannen]);
							// Print( "gia tri "+ sma_thannen.ToString());
							  Print(new_thannen.ToString() + "  vs " + sma_thannen.ToString());
							  if(new_thannen >   sma_thannen){
								  
								  
							  	 return true;
							  }else{return false;}
							
				           }
						   else{  // Nếu số nến chưa đủ số period tính toán ( số nến tính tổng trung bình )
						   	sma_thannen += new_thannen;
							   if(CurrentBars[0]== period_thannen)
								   sma_thannen = ( sma_thannen)/period_thannen;
							   //Print(sma_thannen);
							   return false;
						   }
				  //Print(sma_thannen);
			}
			catch(Exception e){
				Print(" Lỗi CheckThanNen"+e.ToString() );
			}
			return false;
			*/
		   if(CurrentBars[0]<3)
				return false;
			if( new_thannen > ( Thannen_list[CurrentBars[0]-1] + Thannen_list[CurrentBars[0]-2]+ Thannen_list[CurrentBars[0]-3])/3)
			{return true;}
			else{return false;}
			
			
		}
		// Check neens Doj
		private bool CheckDojBar(bool Delay){ //Delay=true check 1 nen phia truoc
			int index_bar =0;
			
			if(Delay ==true){
				index_bar =1;
			}
		//iput_bar.Count
			 // thaan nen < = 25%
				try {if ( Math.Abs(BarsArray[0].GetClose(CurrentBars[0]-index_bar) - BarsArray[0].GetOpen(CurrentBars[0]-index_bar) )  <=0.25*  (BarsArray[0].GetHigh(CurrentBars[0]-index_bar))-BarsArray[0].GetLow(CurrentBars[0]-index_bar)  ){
					return true;
				}
				else{return false;}
				}
				catch(Exception  e){
					return false;
				}
			
			//return false;
		}
		
		private bool CheckWithBar(){
			
			return false;
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
		
		
		/// <summary>
		///  ONBARUPDATE
		/// </summary>
		protected override void OnBarUpdate()
		{
			
			
			// Draw.Text(this, "Bardltchgb"+ CurrentBar, (CurrentBar).ToString(),	0, High[0] + (1 * TickSize + 1), Brushes.White);//      Debug index bar
		  if( BarsInProgress ==0 && IsFirstTickOfBar &&CurrentBars[0]!=0 ){
			  Delta_list.Add(last_delta);
			 last_delta = 0;
			
			   // Loại trừ trường hợp tick đàu tiên của nến đầu tiên (bắt đầu phiên delta lúc này chưa có )
			  // Neu dieu kien check do dai than nen duoc kick hoat

			 
			   // Print((CurrentBars[0]).ToString() + " " + Delta_list[CurrentBars[0]] );
			   // Print((CurrentBar).ToString() + " " + Delta_list[CurrentBar] );
				if(Calculate != Calculate.OnBarClose && (State == State.Realtime || BarsArray[0].IsTickReplay)){
				//	Print(" Che do phat thuc + tick đâu tiên của nến");
					
				if(check_thannen ==true){
				if (CheckThanNen()==false)
				{
					//Print("sai");
					return;
				}
					
			   }
				
			   
				
			  
			  
					if(CurrentBars[0] >=2 ){ // N
						//Print(MA_VOL().ToString());
						Delta_phanky();
						
						if(check_vol == true )
						{
							if(BarsArray[0].GetVolume(CurrentBars[0]-1) >  BarsArray[0].GetVolume(CurrentBars[0]-2)){
								volup_color = Brushes.Blue;
							    voldow_color = Brushes.Orange;
							}
							else { // Dat lai mau cu neu ko phai vol ok
							      volup_color = Brushes.Green;
							      voldow_color = Brushes.Red;
						         } 
						}
						Tunaround();
						if(check_osb==true)
						{
		  	             OSBCheck();
		                     }
					}
				}
			}
	       //END tnround
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
		
		public bool DeltaTails
		{
			get { return delta_tails; }
			set { delta_tails = value; }
		}
	    [Display(GroupName = "1-Delta Tnround", Description="Numbers of bars used for calculations")]
		public bool CheckVol
		{
			get { return check_vol; }
			set { check_vol = value; }
		}
		
		[Display(GroupName = "1-Delta Tnround", Description="Period SMA thana nen")]
		public int PeriodSMA_thannen
		{
			get { return periodsma_thannen; }
			set { periodsma_thannen = Math.Max(1, value); }
		}
		private bool check_thannen =true;
        [Display(GroupName = "1-Delta Tnround", Description="nhap gia tri Period SMA than nen")]
		public bool CheckThannen{
			get{return check_thannen;}
			set{check_thannen =value;}
		}
		[Display(GroupName = "1-Delta Tnround", Description="nhap gia tri Period SMA than nen")]
		public double Nhapheso_thanen{
			set{heso_thannen = value;}
			get{return heso_thannen;}
		}
		
		private double heso_volume ;
		[Display(GroupName = "1-Delta Tnround", Description="nhap gia tri Period SMA vol")]
		public double Get_hesovol {
			get{return heso_volume;}
			set{heso_volume =Math.Max(1,value);}
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
			set { Period_vol = Math.Max(14, value); }
		}
		
		/// <summary>
		///  Dau vao SMA than nen
		/// </summary>
		/// 
		
		
		// Heej so sanh vs SMA than nen ( lon hon bao nhieu lan SMA than nen )
		
		
		
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
		private deltabar[] cachedeltabar;
		public deltabar deltabar()
		{
			return deltabar(Input);
		}

		public deltabar deltabar(ISeries<double> input)
		{
			if (cachedeltabar != null)
				for (int idx = 0; idx < cachedeltabar.Length; idx++)
					if (cachedeltabar[idx] != null &&  cachedeltabar[idx].EqualsInput(input))
						return cachedeltabar[idx];
			return CacheIndicator<deltabar>(new deltabar(), input, ref cachedeltabar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.deltabar deltabar()
		{
			return indicator.deltabar(Input);
		}

		public Indicators.deltabar deltabar(ISeries<double> input )
		{
			return indicator.deltabar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.deltabar deltabar()
		{
			return indicator.deltabar(Input);
		}

		public Indicators.deltabar deltabar(ISeries<double> input )
		{
			return indicator.deltabar(input);
		}
	}
}

#endregion
