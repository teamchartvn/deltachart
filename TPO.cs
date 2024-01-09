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
	
	public class TPO_Hang{
		public double gia_cua_hang;
		public int hang_hien_tai;
		private double buoc_nhay;
		
		public  TPO_Hang(double tpo_nhonhat,double open){
			buoc_nhay  =tpo_nhonhat;
			hang_hien_tai =0; // hàng ban đầu =0
			gia_cua_hang = open ; // giá tại hàng đầu tiên
		}
		 public void tang_them(){
		 	gia_cua_hang += buoc_nhay;
			 hang_hien_tai +=1;
		 }
		 public void giam_bot(){
		 	gia_cua_hang -= buoc_nhay;
			 hang_hien_tai -=1;
		 }
		 
		
	}
	
	
	public class TPO : Indicator
	{
		private bool replay = false;
		private double moc_ban_dau = 0.0;
		private double phantunhonhat_tpo; // Kích thước nhỏ nhất của một ô tpo
		private double phantunhonhat_vpo ; // Số tick size nhỏ nhất để tính vol
		
		private double poc_vol,poc_tpo , poc_delta ;
		//private int [,] hang_tpo;
		//private List<int [,]> tpo = new List<int [,]>();   //khởi tạo
		//private  List<int[,]> tpo_list = new List<int[,]>();
		
		// taoj truoc 2 mang gia tri tpo_tren tpo_duoi san moi thu 1000 phan tu
		// kichs thuoc nay co the tuy chinh ban dau theo TPO thang , nam , tuan ...
		// khi het size co the khoi tao copy + tao new x2 phan tu mang moi
		private    int[] tpo_tren = new int[1000];
		private	   int[] tpo_duoi = new int[1000];
		//private List<int> tpo_tren = new List<int>(100);
		//private List<int> tpo_duoi = new List<int>(100);
		private TPO_Hang  hang_tpo ;
		private int giatri_nen;     // giá trị định nghĩa của nến 5' 15' 30' = thời gian 1 cây nến

		//private int hang_hientai=0,hanghientai_tpo=0;
		private int bien_tren ,bien_duoi;      // hàng lớn nhất bên trên hàng lớn nhất bên dưới , sẽ đặt lại = moc_datlai sau khi reset time tpo
		
		DateTime mocthoigian_dautien ,thoigian_tieptheo;  // Đặt lại sau mỗi 30' ( or một mức được cấu hính )
		
		private int thoigian_mau =30; // thời gian lấy mẫu tpo 
		
		// gốc tpo = giá trị đầu tiên của tpo ( hàng số 0 ở giữa không đổi ) , goc_datlai = đặt lại sau mỗi khoảng thời gian tpo nhỏ nhất 
	    private int goc_tpo , goc_datlai;   
		
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
				//
				//
				
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


		// giá lớn hơn or nhỏ  hơn biên trên biên dưới đạt đủ  mức tối thiểu min* tickSize = thêm một ô
		// Cấp nhật giá trị bientren bienduoi
		private void TinhTPO_TheoKhoang(ref double high, ref double low, DateTime time_close){ // Chạy cho chế độ tĩnh khi ko còn nến đang chạy( chưa hoàn thành )
			
		}
		
		
		public void Hienthi(){
		foreach (int mau in tpo_tren )
		{
			NinjaTrader.Code.Output.Process(mau.ToString(),PrintTo.OutputTab1);
		}
		
		NinjaTrader.Code.Output.Process("------------------",PrintTo.OutputTab1);
		foreach (int mau in tpo_duoi )
		{
				NinjaTrader.Code.Output.Process(mau.ToString(),PrintTo.OutputTab1);
		}
	    }
		
		
		

		
		private void KhoiTaoTPO(double open , DateTime goc_time ){
            phantunhonhat_tpo =2;
			giatri_nen = Bars.BarsPeriod.Value; // số phút của thanh nến ( 5 15 20 30 ...)
			
			mocthoigian_dautien = goc_time; // tiem open nến đầu tiên của session
			thoigian_tieptheo = goc_time.AddMinutes(thoigian_mau);  // mốc thời gian tiếp theo lần đầu tiên
			goc_tpo = 0;
			bien_tren =0;
			bien_duoi =0;
			// khởi tạo hàng tpo ( chứa thông tin hàng hiện tại và mốc giá tương ứng )
			hang_tpo = new TPO_Hang(phantunhonhat_tpo,open );
			giatri_nen = Bars.BarsPeriod.Value;
			
			// taoj truoc 2 mang gia tri tpo_tren tpo_duoi san moi thu 100 phan tu
		}
		
		
		private void Dichuyen_tpo(){
			
			
		}
		private void  CapNhatPhanTuTpo(){
			
		}
		
		
		public void TpoOnClose( double high,  double low ,double open ,double close, DateTime time_open){
			Print(CurrentBars[0].ToString() +"=> Higt: "+high.ToString() +" low : "+ low.ToString()+ " open: "+open.ToString()+" close :" +close.ToString());
			int yy =0;
			
			
			//
			if(thoigian_tieptheo  <= time_open)
				{
					
					// thiết lập lại thời gian cho trường hợp đặc biệt
					thoigian_tieptheo = time_open.AddMinutes(thoigian_mau);  // nếu nến mới cách xa nến đóng cửa trước đó vd: eod ngày hôm trước tới ngày hôm sau  hay có một phiên nghỉ giao dịch. nến tiếp t heo sẽ cách > thoigian_mau
					
					// reset  bien trên biên dưới , di chuyển hàng hiện tại về vị trí open
					while(hang_tpo.gia_cua_hang <= open -phantunhonhat_tpo){
			           	hang_tpo.tang_them();
			           	// không thêm vào list TPO
			           	//zz++;
			           	Print("  Open đặc biệt cao =============================================" );
			           	
			           }
					   while(hang_tpo.gia_cua_hang > open){
					   	hang_tpo.giam_bot();
						   	Print(" Open  đặc biệt -thấp =============================================" );
					   }
				}
			
			
			while(high > hang_tpo.gia_cua_hang + phantunhonhat_tpo){  // hàng 0 ứng với ô đầu tiên Draw.Dot Draw.Dot Draw.Dot
				hang_tpo.tang_them();
				yy++;
				if( hang_tpo.hang_hien_tai > bien_tren) // Chỉ cập nhật khi hàng hiện tại nhỏ hơn biên trên
				{
					Print("update phantu tren " + hang_tpo.hang_hien_tai.ToString());
					
						bien_tren = hang_tpo.hang_hien_tai;
					
					Print("bien_tren : " + bien_tren.ToString() + " Bien duoi   : " +  bien_duoi.ToString() );
					if(hang_tpo.hang_hien_tai >=0){  // capaj nhat vao tpo_tren
						// update gia trij hang tpo
						//Print("high + " );
						tpo_tren[hang_tpo.hang_hien_tai] +=1;
						Print("high + " + tpo_tren[hang_tpo.hang_hien_tai].ToString() );
					}else{ // <0 tpo_duoi
						 tpo_duoi[Math.Abs(hang_tpo.hang_hien_tai)-1] +=1; 
						Print("high - " + tpo_tren[hang_tpo.hang_hien_tai].ToString() );
					} 
					
				}
				
			       
				// nhập vào list ở đây
				//Print(" Tăng " + yy .ToString() + " giá = " + hang_tpo.gia_cua_hang.ToString() + " hàng = " + hang_tpo.hang_hien_tai.ToString());
			}
			
			int xx =0;
			while(low < hang_tpo.gia_cua_hang){
				hang_tpo.giam_bot();
				// nhập vào list ở đây
				if(hang_tpo.hang_hien_tai < bien_duoi)              // Chỉ cập nhật khi hàng hiện tại nhỏ hơn biên dưới
				{
					Print("update phantu duoi " + hang_tpo.hang_hien_tai.ToString());
					
					bien_duoi = hang_tpo.hang_hien_tai;
					Print("bien_tren : " + bien_tren.ToString() + " Bien duoi   : " +  bien_duoi.ToString() );
					if(hang_tpo.hang_hien_tai >=0){  // capaj nhat vao tpo_tren
						// update gia trij hang tpo
						tpo_tren[hang_tpo.hang_hien_tai]++;
						Print("low + " + tpo_tren[hang_tpo.hang_hien_tai].ToString() );
					}else{ // <0 tpo_duoi
						 tpo_duoi[Math.Abs(hang_tpo.hang_hien_tai)-1] ++; 
						Print("low -" + ( Math.Abs(hang_tpo.hang_hien_tai)-1).ToString() );
					} 
				}
					
				
				xx++;
				//Print("giảm " + xx.ToString() + " giá = " + hang_tpo.gia_cua_hang.ToString() + " hàng = " + hang_tpo.hang_hien_tai.ToString());
			}
			
			// khi dong nen: giá low luôn <= giá close
			//Print("bd = " + bien_duoi.ToString() + " : bt =" + bien_tren.ToString());
			
			// Chỉ khi nào mốc thời gian kích hoạt reset tpo thì mới cần move hàng về giá trị close
			if(thoigian_tieptheo <= time_open.AddMinutes(giatri_nen)){  // time close nến 
				
				thoigian_tieptheo = thoigian_tieptheo.AddMinutes(thoigian_mau);
				// Move về vị trí đóng nến 
				int zz =0;
			           while(hang_tpo.gia_cua_hang <= close -phantunhonhat_tpo){
			           	hang_tpo.tang_them();
			           	// không thêm vào list TPO
			           	zz++;
			          // 	Print(" đóng " + zz.ToString());
			           	
			           }
					   while(hang_tpo.gia_cua_hang > close){
					   	hang_tpo.giam_bot();
						//  	Print(" đóng " + zz.ToString());
					   }
					   Draw.Dot(this,"xanh" +CurrentBar ,true,0,BarsArray[0].GetHigh(CurrentBars[0]) + 2* TickSize,Brushes.GreenYellow,true);
					    //Draw.Dot(this,);
					  // Print("Biên trên ="+ bien_tren.ToString() + " Biên dưới = " + bien_duoi.ToString());
			           Print("---------------- " + hang_tpo.hang_hien_tai + "---------------- " + hang_tpo.hang_hien_tai.ToString());
					   
				// Đặt lại các mốc ở đây
				goc_datlai = hang_tpo.hang_hien_tai; // giá trị của close
				bien_tren=bien_duoi= hang_tpo.hang_hien_tai;
				Print("mốc mới tpo " + goc_datlai.ToString() +" thời gian = "+ time_open.ToString());
					   Print("END TPO <<<<<<  ");
			}
			
			
		}
		
		protected override void OnBarUpdate()
		{
			if(BarsInProgress ==0){
			//	Print("new Bar : time "+Bars.GetTime(CurrentBar).ToString());
				//Add your custom indicator logic here.
			       if(Calculate == Calculate.OnBarClose){
			       	
			       	if (BarsArray[0].IsFirstBarOfSession){
                           Print( "Số phút của nến " + Bars.BarsPeriod.Value.ToString());
			       		Print(" OnBarUpdate >> open /n/n " +BarsArray[0].GetOpen(0).ToString());
			       						KhoiTaoTPO( BarsArray[0].GetOpen(0),BarsArray[0].GetTime(0)); // Giá open của nến đầu tiên = mốc ban đầu , set thời gian ban đầu
					 TpoOnClose(BarsArray[0].GetHigh(CurrentBars[0]),BarsArray[0].GetLow(CurrentBars[0]),BarsArray[0].GetOpen(CurrentBars[0]),BarsArray[0].GetClose(CurrentBars[0]),BarsArray[0].GetTime(CurrentBars[0]));
						
						
						// In thư Debug 
						
						Print("++++++++++++++++++++++++++++++++++");
						Print("++++++++++++++++++++++++++++++++++");
						Print("top tren");
						int  i =0;
						foreach( int phantu_tren in tpo_tren){
							i++;
							
							if(phantu_tren==0)
								break;
							Print(i.ToString() +phantu_tren.ToString());
						}
						Print("So phan tu tpo_tren" +i.ToString());
						int z =0;
						Print("tpo duoi");
						foreach( int phantu_duoi in tpo_duoi){
							z++;
							
							if(phantu_duoi==0)
								break;
							Print(z.ToString() + phantu_duoi.ToString());
						}
							Print("So phan tu tpo_duoi" +z.ToString());
						
						
						// end debug 
			       	}
			       	else {
			       	      TpoOnClose(BarsArray[0].GetHigh(CurrentBars[0]),BarsArray[0].GetLow(CurrentBars[0]),BarsArray[0].GetOpen(CurrentBars[0]),BarsArray[0].GetClose(CurrentBars[0]),BarsArray[0].GetTime(CurrentBars[0]));
			       	}
			       }
			       
			       if(State != State.Historical){
			       	Print(State.ToString());
			       }
			       
			       
			    
			       
			       if(replay){
			       	Print("Replay is enable");
			       }
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
