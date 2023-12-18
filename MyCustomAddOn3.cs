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

using PhatTrien;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	
public class TpoRange :  NinjaTrader.NinjaScript.AddOnBase
{
private int daysBack = 5;
private DateTime moc_cuoi , moc_dau;	
	
	
private bool barsRequestSubscribed = false;
private BarsRequest yeucau_nen; 
public double [] barinfo = new double[2];

//public PhatTrien.TPO call_tpo;
	
// hàm khởi tạo
	
	
	
private void  TinhToan(DateTime moc_dau, DateTime moc_cuoi)
{
  // Khai báo khởi tạo một request 
  yeucau_nen = new BarsRequest(Cbi.Instrument.GetInstrument("GC 02-24"), moc_dau, moc_cuoi);
 
  // cấu hình thời gian bắt đầu vầ kết thúc đoạn muốn lấy ,loại nến và giá trị loại nến
  yeucau_nen.BarsPeriod = new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 5 };
  yeucau_nen.TradingHours = TradingHours.Get("aaa"); // Tradinghours phiên giao dịch
 
  // Hàm này dành cho nếu có connect realtime -> sẽ gọi các sự kiện call tới hàm onbarupdate. 
  yeucau_nen.Update     += OnBarUpdate;
 
  // Thực hiện request nếu lỗi thông báo qua output1
  yeucau_nen.Request(new Action<BarsRequest, ErrorCode, string>(  
	  (bars, errorCode, errorMessage) =>
  {
    if (errorCode != ErrorCode.NoError)
    {
      // Thông báo lỗi ở đây
      NinjaTrader.Code.Output.Process(string.Format("lỗi request bar ko thành công: {0}, {1}",
                                      errorCode, errorMessage), PrintTo.OutputTab1);
      return;
    }
 
    // Kết quả rả về các bar sẽ nằm trong 1 mảng kết quả chứa tất cả các bar
	NinjaTrader.Code.Output.Process("ham request ",PrintTo.OutputTab1);
	
	// Gọi tính Tpo
	PhatTrien.TPO  new_tpo = new PhatTrien.TPO(bars.Bars.GetOpen(0));
	
    for (int i = 0; i < bars.Bars.Count; i++)
    {
      // vòng lặp các nến
      // NinjaTrader.Code.Output.Process(i.ToString(),PrintTo.OutputTab1);
		/*
		NinjaTrader.Code.Output.Process(string.Format("Time: {0} Open: {1} High: {2} Low: {3} Close: {4} Volume: {5}",
                                      bars.Bars.GetTime(i),
                                      bars.Bars.GetOpen(i),
                                      bars.Bars.GetHigh(i),
                                      bars.Bars.GetLow(i),
                                      bars.Bars.GetClose(i),
                                      bars.Bars.GetVolume(i)), PrintTo.OutputTab1);
                                      */
		
		new_tpo.TinhTpo( bars.Bars.GetLow(i), bars.Bars.GetHigh(i));
    }
 
    // Kiểm tra có đang kết nối thời gian thực tới data (rithmic Cqg...vv )
    lock (Connection.Connections)
      if (Connection.Connections.FirstOrDefault() == null)
        NinjaTrader.Code.Output.Process("Real-Time Bars: Not connected.", PrintTo.OutputTab1);
  }));
}
 

private void OnBarUpdate(object sender, BarsUpdateEventArgs e)
{

	NinjaTrader.Code.Output.Process("ham Update ",PrintTo.OutputTab1);
  for (int i = e.MinIndex; i <= e.MaxIndex; i++)
  {
    // Processing every single tick
                                       // e.BarsSeries.GetOpen(i);
                                 NinjaTrader.Code.Output.Process(i.ToString(),PrintTo.OutputTab1);
                                   barinfo[0]  =       e.BarsSeries.GetClose(i);
                                   barinfo[1]  =           e.BarsSeries.GetVolume(i);
	    NinjaTrader.Code.Output.Process(e.BarsSeries.GetAsk(i).ToString(), PrintTo.OutputTab1);
  }
}
 

public  void Cleanup()
{
  // Make sure to unsubscribe to the bars request subscription
  if (yeucau_nen != null)
    {
       yeucau_nen.Update -= OnBarUpdate;
       yeucau_nen.Dispose();
      yeucau_nen = null;
     }
}
 

}

}