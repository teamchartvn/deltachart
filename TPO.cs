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
	public class TPO : Indicator
	{
		private bool replay = false;
		private double moc_ban_dau = 0.0;
		private double phantunhonhat_tpo; // Kích thước nhỏ nhất của một ô tpo
		private double phantunhonhat_vpo ; // Số tick size nhỏ nhất để tính vol
		
		private double poc_vol,poc_tpo , poc_delta , bien_tren, bien_duoi;
		private int [,] hang_tpo;
		//private List<int [,]> tpo = new List<int [,]>();   //khởi tạo
		//private  List<int[,]> tpo_list = new List<int[,]>();
		
		private List<int> tpo_tren = new List<int>();
		private List<int> tpo_duoi = new List<int>();
		// 2 List chia đôi tpo ban đầu , lấy mốc giá ban đầu làm mốc gốc.
		//private List<int,int> list_tren = new List<int,int>();
		//private List<int,int> list_duoi = new List<int,int>();
		private int hang_hientai=0;
		DateTime moc_thoigian ,time_bar;  // Đặt lại sau mỗi 30' ( or một mức được cấu hính )
		
		private int thoigian_mau =30; // thời gian lấy mẫu tpo 
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Tính TPO cho các loại dữ liệu thị trường";
				Name										= "1.TPO";
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
				
				poc_tpo = poc_delta =poc_vol = bien_duoi =bien_duoi =0;
				
				//phantunhonhat_tpo = 3* TickSize;
				//phantunhonhat_vpo =   5* TickSize;
			}
			else if (State == State.Configure)
			{
				if(!Bars.IsTickReplay){
					AddDataSeries(Data.BarsPeriodType.Minute,5);
				}
				else{
					replay = true;
				}
				
			}
		}

		/*
		public TPO(){
			poc_tpo = poc_delta =poc_vol = bien_duoi =bien_duoi =0;
		}
		*/
		private void KhoiTaoTPO(){ // lấy giá mở cửa của nến đầu tiên trong phiên , nếu là range fix, lấy giá mở cửa của nến tại con trỏ chuột.
			poc_tpo = poc_delta =poc_vol = bien_duoi =bien_duoi =0;
			
		}

       
		// giá lớn hơn or nhỏ  hơn biên trên biên dưới đạt đủ  mức tối thiểu min* tickSize = thêm một ô
		// Cấp nhật giá trị bientren bienduoi
		private void TinhTPO_TheoKhoang(ref double high, ref double low){ // Chạy cho chế độ tĩnh khi ko còn nến đang chạy( chưa hoàn thành )
			
		}
		
		private void TinhTPO(){
	
			var gia_ask = GetCurrentAsk();
			var gia_bid = GetCurrentBid();
			double gia_hientai = GetCurrentBid();
			
			while(gia_ask > bien_tren){ // bien trên + tăng dần giatrinhonhat
				// 
				bien_tren += phantunhonhat_tpo;  // Cập nhật giá trị biên trên
				hang_hientai +=1; // Tăng hàng hiện tại của tpo lên 1 hàng
				if(hang_hientai >= 1){                                               // hang_hientai =1 ở list_tren
					if (tpo_tren.Count >= hang_hientai-1 ){
						tpo_tren[hang_hientai-1] +=1;  // tăng thêm một ô của hàng đã tồn tại
					}
					else{
						tpo_tren.Add(1);  // THêm một hàng mới và có 1 ô;
					}
				}
				else{ // hang_hientai ở list dưới hang_hientai bắt đầu =0 trùng với index của tpo_duoi- lấy trị tuyệt đối , do lúc này hang_hientai <=0:
					var index_duoi = Math.Abs(hang_hientai);
					// vì giá lớn hơn biên trên vì thế index luôn nhỏ < số hàng hiện có của tpo_duoi
					tpo_duoi[index_duoi] +=1; // thêm một ô vào hàng
				}
				
			}
			while(gia_bid < bien_duoi){
				bien_duoi -= phantunhonhat_tpo;  // Cập nhật giá trị biên dưới
				hang_hientai -=1; // 
				// Giá < bien_duoi , nếu nằm ở tpo_tren , bien_tren <=max tpo. => hang_hientai <= tpo_tren.count
				 if(hang_hientai >= 1){   
					 tpo_tren[hang_hientai-1] +=1;
				 }
				 else{
					 var index_duoi = Math.Abs(hang_hientai);
				 	if(tpo_duoi.Count >= index_duoi){
						tpo_duoi[index_duoi] +=1; // tăng thêm một ô của hàng
					}
					else{tpo_duoi.Add(1); }        // thêm một hàng mới có 1 ô
				 }
				
			}
			
				// reset bien_tren, bien_duoi
			  //	DateTime goc = DateTime.Now;
				DateTime new_time = moc_thoigian.AddMinutes(thoigian_mau);
				if( new_time<= DateTime.Now)
					moc_thoigian = new_time;
		
			 
			
			
		}
		
		
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if(Calculate == Calculate.OnBarClose){
				time_bar = Bars.GetTime(CurrentBars[0]);
				//TinhTPO();
			}
			if(Calculate == Calculate.OnEachTick &&  Calculate == Calculate.OnPriceChange){
				if(IsFirstTickOfBar){
					time_bar = Bars.GetTime(CurrentBars[0]-1);
					//TinhTPO();
				}
			}
			
			if(replay){
				Print("Replay is enable");
			}
		}
		
		
		 protected override void OnMarketData(MarketDataEventArgs e){
		 	
			 if(e.MarketDataType == MarketDataType.Last){
			 //	Print(e.Time);
			 }
		 }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TPO[] cacheTPO;
		public TPO TPO()
		{
			return TPO(Input);
		}

		public TPO TPO(ISeries<double> input)
		{
			if (cacheTPO != null)
				for (int idx = 0; idx < cacheTPO.Length; idx++)
					if (cacheTPO[idx] != null &&  cacheTPO[idx].EqualsInput(input))
						return cacheTPO[idx];
			return CacheIndicator<TPO>(new TPO(), input, ref cacheTPO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TPO TPO()
		{
			return indicator.TPO(Input);
		}

		public Indicators.TPO TPO(ISeries<double> input )
		{
			return indicator.TPO(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TPO TPO()
		{
			return indicator.TPO(Input);
		}

		public Indicators.TPO TPO(ISeries<double> input )
		{
			return indicator.TPO(input);
		}
	}
}

#endregion
