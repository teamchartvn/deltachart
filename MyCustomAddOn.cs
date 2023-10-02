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
public static class DeltarTurnaroud{
		public static Dictionary<int,int> info_bar;
		public static int test;
		public static Dictionary<int ,int> getvalue{
			 get{return info_bar; }
			 set {info_bar = value;}
		}
		public static void Input(int index, int value_delta){ // Nhập dữ liệu từ detlta_bar
			info_bar.Add(index,value_delta);
		}
		public static int Get_Turnaround(int index_bar){ // Sử dụng để tính toán so sánh delta 2 nến liền kề 
			int rt=0;
			info_bar.TryGetValue(index_bar, out rt);
			return rt;
		}
		// hàm khởi tạo tĩnh 
		static DeltarTurnaroud(){
			test =888;
			info_bar = new Dictionary<int,int>(5);
		}
	   
	}