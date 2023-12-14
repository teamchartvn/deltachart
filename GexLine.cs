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
	public class GexLine : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "1-GexLine";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Brush mausac = Brushes.Green;
				
				PaintPriceMarkers = true;  // hiển thị indi theo trục y
				
				//AddPlot(Brushes.Green,"Gexline");
			}
			else if (State == State.Configure)
			{
			}
		}



       private void DrawLine( double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
        {
            SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();

            if (dashStyle == DashStyleHelper.Dash)
            {
                ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash;
            }
            if (dashStyle == DashStyleHelper.DashDot)
            {
                ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot;
            }
            if (dashStyle == DashStyleHelper.DashDotDot)
            {
                ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;
            }
            if (dashStyle == DashStyleHelper.Dot)
            {
                ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;
            }
            if (dashStyle == DashStyleHelper.Solid)
            {
                ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
            }

            SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);

            SharpDX.Vector2 startPoint = new System.Windows.Point(x1, y1).ToVector2();
            SharpDX.Vector2 endPoint   = new System.Windows.Point(x2, y2).ToVector2();

           // RenderTarget.DrawLine(startPoint, endPoint,brush.DxBrush, width, strokeStyle);
			using (SharpDX.Direct2D1.SolidColorBrush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Blue))
                       {
                               RenderTarget.DrawLine(startPoint, endPoint,dxBrush, width, strokeStyle);
                       }

            strokeStyle.Dispose();
            strokeStyle = null;
        }
		
protected override void OnBarUpdate()
		{
			
		}
		
		
protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
{
  // implicitly recreate and dispose of brush on each render pass
  //DrawLine(200,1950,100,1950,2,DashStyleHelper.Dash);
	 SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
	ssProps.DashStyle =  SharpDX.Direct2D1.DashStyle.Dash;      // kiểu loại đường chấm liền đứt ...
	SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);

	
	        SharpDX.Vector2 startPoint = new System.Windows.Point(500, 600).ToVector2();
            SharpDX.Vector2 endPoint   = new System.Windows.Point(800, 200).ToVector2();
             //SolidColorBrush brush        = new SolidColorBrush(renderTarget, color.ToRawColor4(1f));
           // RenderTarget.DrawLine(startPoint, endPoint,brush.DxBrush, width, strokeStyle);
		
                            //   RenderTarget.DrawLine(startPoint, endPoint,Brushes.GreenYellow, 5,strokeStyle);
               

}
		
#region Properties
		private bool show_gex = true;
       [Display(GroupName = "1-Delta Tnround", Description="Numbers of bars used for calculations")]
		public bool Show
		{
			get { return show_gex; }
			set { show_gex = value; }
		}
		
		
		
		public Brush mausac 
		{ 
			get; set; 
		}
		
		[Browsable(false)]
		public string Fgetcolor
		{
		  get { return Serialize.BrushToString(mausac); }
		  set { mausac = Serialize.StringToBrush(value); }
		}
#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GexLine[] cacheGexLine;
		public GexLine GexLine()
		{
			return GexLine(Input);
		}

		public GexLine GexLine(ISeries<double> input)
		{
			if (cacheGexLine != null)
				for (int idx = 0; idx < cacheGexLine.Length; idx++)
					if (cacheGexLine[idx] != null &&  cacheGexLine[idx].EqualsInput(input))
						return cacheGexLine[idx];
			return CacheIndicator<GexLine>(new GexLine(), input, ref cacheGexLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GexLine GexLine()
		{
			return indicator.GexLine(Input);
		}

		public Indicators.GexLine GexLine(ISeries<double> input )
		{
			return indicator.GexLine(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GexLine GexLine()
		{
			return indicator.GexLine(Input);
		}

		public Indicators.GexLine GexLine(ISeries<double> input )
		{
			return indicator.GexLine(input);
		}
	}
}

#endregion
