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
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace PhatTrien
{
	public class TPO //: NinjaTrader.NinjaScript.AddOnBase
	{
		private double moc_ban_dau = 0.0;
		private double phantunhonhat_tpo; // Kích thước nhỏ nhất của một ô tpo
		//private double phantunhonhat_vpo ; // Số tick size nhỏ nhất để tính vol
		
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
		DateTime moc_thoigian,time_bar ,thoigian_tick;  // Đặt lại sau mỗi 30' ( or một mức được cấu hính )
		public int thoigian_mau =30;  // mặc định 30
	
	
	public TPO(double mocbandau){
		moc_ban_dau = poc_tpo =bien_duoi=bien_duoi=mocbandau;
		phantunhonhat_tpo = 3.0;
		hang_hientai =1 ; // Khởi tạo hàng ban đầu ở list_trên 
	}
		
	public void TinhTpo( double gia_ask ,  double gia_bid ){
	
		//	var gia_ask = GetCurrentAsk();
		//	var gia_bid = GetCurrentBid();
	    //  double gia_hientai = GetCurrentBid();
			
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
			
			// reset bien_tren, bien_duoi sau khi thời gian đạt đủ thoigian_mau ( 30' chẳng hạn ).
			// mối mới = mốc cũ + thoigian_mau
			// nếu thời gian của 1 tick or nến ( thới gian hiện tại lúc phát sinh sự kiện ) >= thời gian mốc mới =>
			// => cập nhật lại mốc hiện tại, nếu ko thì thôi.
		    //	DateTime goc = DateTime.Now;
				DateTime new_time = moc_thoigian.AddMinutes(thoigian_mau);
				if( new_time<= thoigian_tick)
					moc_thoigian = new_time;
		
			 
			
			
		}
	}
}
