#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.Utilities
{
    /// <summary>
    /// Convert source file from NT7 to NT8
    /// </summary>
    [Description("Convert source file from NT7 to NT8")]
    public class Convert7to8 : Indicator
    {
	private bool AfterPropertiesDefined = false;
	string path = "";//@"C:\Users\Ben\Documents\NinjaTrader 8\bin\Custom\Indicators\Utilities";
	string OnStateChangedDeclaration = null;
	string NameOfGraphicObject = string.Empty;//typically 'graphic', will be replaced with 'RenderTarget'
	string NL = Environment.NewLine;
	List<string>   UsingStatementsToAdd = new List<string>();
	List<string>   UsingStatementsToCommentOut = new List<string>();
	private bool   UsingStatementsHandled = false;
	private string PropertyDisplayName = string.Empty;
	private string CategoryName=string.Empty;
	private int    PropertyDisplayNameLine = -1;
	private int    CategoryNameLine = -1;
	private Regex reg1;
		bool AddSoundFolderCode = false;
		bool HasAddedSoundFolderCode = false;
		
        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
		protected override void OnStateChange()
		{
			//Print("Convert 7to8 State: "+State.ToString());
			if (State == State.SetDefaults){
    	        IsOverlay				= false;
				ClearOutputWindow();
	        }
			if (State == State.Configure)
	        {
				OnStateChangedDeclaration = @"protected override void OnStateChange()"+NL+"	{"+NL+"		if (State == State.SetDefaults)";

				path = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),"NinjaTrader 8","bin","Custom","Indicators","Utilities");
				Print("Path: "+path);
				string search = "*.cs7";
				DirectoryInfo di = new DirectoryInfo(path);
				var fi = di.GetFiles(search);

				string[] list = new string[fi.Length];
				foreach (System.IO.FileInfo f in fi)
				{
					string newfilename = f.FullName.Replace(".cs7",".cs");
					if(File.Exists(newfilename)) {
						Log(newfilename+" exists...please delete or move - conversion aborted",LogLevel.Alert);
						continue;
					}
					UsingStatementsToAdd.Add("System");
					UsingStatementsToAdd.Add("System.ComponentModel");
					UsingStatementsToAdd.Add("System.ComponentModel.DataAnnotations");
					UsingStatementsToAdd.Add("System.Threading.Tasks");
					UsingStatementsToAdd.Add("System.Windows");
					UsingStatementsToAdd.Add("System.Windows.Input");
					UsingStatementsToAdd.Add("System.Windows.Media");
					UsingStatementsToAdd.Add("System.Xml.Serialization");
					UsingStatementsToAdd.Add("NinjaTrader.Cbi");
					UsingStatementsToAdd.Add("NinjaTrader.Gui");
					UsingStatementsToAdd.Add("NinjaTrader.Gui.Tools");
					UsingStatementsToAdd.Add("NinjaTrader.Gui.Chart");
					UsingStatementsToAdd.Add("NinjaTrader.Data");
					UsingStatementsToAdd.Add("NinjaTrader.NinjaScript");
					UsingStatementsToAdd.Add("NinjaTrader.Core.FloatingPoint");
					UsingStatementsToAdd.Add("SharpDX.DirectWrite");
					UsingStatementsToAdd.Add("NinjaTrader.NinjaScript.DrawingTools");
					UsingStatementsToCommentOut.Add("System.Drawing");
					UsingStatementsToCommentOut.Add("System.Drawing.Drawing2D");


	//ignore any file whose name starts with "@"
					if(f.Name.Contains("@")) continue;
					var lines = new List<string>(File.ReadAllLines(f.FullName));
					for(int i = 0; i<lines.Count; i++){
//						if(lines[i].Contains("#region") || lines[i].Contains("#endregion")) continue;
						string TrimmedLine = lines[i].Trim();
//-----------------------------------------------------------------------------------------------------
						if(!UsingStatementsHandled && TrimmedLine.Contains("namespace NinjaTrader.Indicator")){
							UsingStatementsHandled = true;
	//change the Namespace declaration
							lines[i] = "namespace NinjaTrader.NinjaScript.Indicators";
							string tempstr = string.Empty;
							for(int k = 0; k<i; k++){
								if(TrimmedLine.StartsWith(@"//")) continue;
	//add missing using statements
								for(int ptr=0;ptr<UsingStatementsToAdd.Count;ptr++){
									reg1 = new Regex(@"using\s+"+UsingStatementsToAdd[ptr]+@";");
	//on the odd chance that the file already has a the necessary using, you don't need add another instance of it
									if(reg1.IsMatch(lines[k])) UsingStatementsToAdd[ptr] = string.Empty;
								}
	//if any of the statements starts with an unsupported using statement, comment them out
								for(int ptr=0;ptr<UsingStatementsToCommentOut.Count;ptr++){
									reg1 = new Regex(@"using\s+"+UsingStatementsToCommentOut[ptr]+@"\s*;");
									if(reg1.IsMatch(lines[k])) {
										lines[k] = string.Concat(@"//",lines[k]);
										Print("Commented out using statement:   '"+lines[k]+"'");
									}
								}
							}
							var lineslist = new List<string>(lines);
							foreach(string ustmt in UsingStatementsToAdd){
								if(ustmt.Length>0){
									lineslist.Insert(i-1,"using "+ustmt+";");
									Print("Added using statement: "+ustmt);
									i++;
								}
							}
//							lines = new String[lineslist.Count];
//							lineslist.CopyTo(lines,0);
							lines = lineslist;
						}
//-----------------------------------------------------------------------------------------------------
						if(lines[i].Contains("Color.FromKnownColor")){
							lines[i] = lines[i].Replace("Color.FromKnownColor(Known","(");
						}
//-----------------------------------------------------------------------------------------------------
	//Change the "Add(new Plot(" statements to "AddPlot(new Stroke(..." statement
						if(lines[i].Contains("Add(new Plot(")){
							var elements = TrimmedLine.Split(new Char[]{','});
							elements[0] = elements[0].Replace("Add(new Plot(","AddPlot(");
							if(lines[i].Contains("new Pen")){
//				Add(new Plot(new Pen(Color.                     Orange, 1), "SMMA"));
//				AddPlot     (new NinjaTrader.Gui.Stroke(Brushes.Yellow, 1), PlotStyle.Hash, "MarkerLineBottom");
								elements[0] = elements[0].Replace("new Pen(Color","new Stroke(Brushes");
							}else{
//				Add(new Plot(Color.                             Orange, "SMMA"));
//				AddPlot     (new NinjaTrader.Gui.Stroke(Brushes.Yellow, 1), PlotStyle.Hash, "MarkerLineBottom");
								elements[0] = elements[0].Replace("Color.","new Stroke(Brushes.")+",1)";
							}
							lines[i] = ConvertListToString(elements,',');
							lines[i] = lines[i].Replace("));",");");
						}
//-----------------------------------------------------------------------------------------------------
	//Change the "Add(new Line(" statements to "AddLine(new Stroke(..." statement
						else if(lines[i].Contains("Add(new Line(")){
	//changes Add(new Line(new Pen(Color.DarkGray,2) 0.0, "Zero line"));   to  				AddLine(new Stroke(Brushes.DarkGray,DashStyleHelper.Dot,2), 0.0, "Zero line");
							reg1 = new Regex(@"Add\s*\(\s*new\s+Line\s*\(\s*new\s+Pen\s*\(\s*Color.([a-zA-Z]+)\s*([\s\S]+)\s*\)\s*\)\s*;");
							lines[i] = reg1.Replace(lines[i],@"AddLine(new Stroke(Brushes.$1, DashStyleHelper.Solid $2);");

	//changes Add(new Line(Color.Red, 0, "Zero line"));   to  				AddLine(new Stroke(Brushes.Red,DashStyleHelper.Dot,1), 0, "Zero line");
							reg1 = new Regex(@"Add\s*\(\s*new\s+Line\s*\(\s*Color.([a-zA-Z]+)\s*,\s*([\s\S]+)\s*\)\s*\)\s*;");
							lines[i] = reg1.Replace(lines[i],@"AddLine(new Stroke(Brushes.$1, DashStyleHelper.Solid, 1), $2);");
						}
//-----------------------------------------------------------------------------------------------------
	//Change the Add (background dataseries)
						if(lines[i].Trim().StartsWith("Add(")){
//				Add(BaseInstrument, PeriodType.Minute, pMinutesPerBar);
//				Add(PeriodType.Minute, pMinutesPerBar);
							lines[i] = lines[i].Replace("Add(","AddDataSeries(");
						}
//-----------------------------------------------------------------------------------------------------
//						string stripped = KeepOnlyTheseCharacters(lines[i],33,9999,' ');
//-----------------------------------------------------------------------------------------------------
		//converts: private Font txtFont;   to   private NinjaTrader.Gui.Tools.SimpleFont txtFont;
						reg1 = new Regex(@"\s+Font\s+");
						lines[i] = reg1.Replace(lines[i],@" NinjaTrader.Gui.Tools.SimpleFont ");
		//converts: new Font("Arial"...   to   new SimpleFont("Arial"...
						reg1 = new Regex(@"\s*new\s+Font\s*\(");
						lines[i] = reg1.Replace(lines[i],@" new NinjaTrader.Gui.Tools.SimpleFont(");
//-----------------------------------------------------------------------------------------------------
						reg1 = new Regex(@"private\s*StringAlignment");
						lines[i] = reg1.Replace(lines[i],@"private System.Windows.TextAlignment");
						reg1 = new Regex(@"new\s*StringAlignment");
						lines[i] = reg1.Replace(lines[i],@"new System.Windows.TextAlignment");
						lines[i] = lines[i].Replace("StringAlignment","TextAlignment");
//-----------------------------------------------------------------------------------------------------
	//replace "Initialize()" method with OnStateChanged() method
						lines[i] = lines[i].Replace("protected override void Initialize()",OnStateChangedDeclaration);
//-----------------------------------------------------------------------------------------------------
		//comment out PriceTypeSupported
						lines[i] = lines[i].Replace("PriceTypeSupported",@"//PriceTypeSupported");
//-----------------------------------------------------------------------------------------------------
						if(    lines[i].Contains("Overlay") 
							|| lines[i].Contains("ChartOnly")
							|| lines[i].Contains("AutoScale")
							|| lines[i].Contains("PlotsConfigurable")
							|| lines[i].Contains("LinesConfigurable")
							|| lines[i].Contains("CalculateOnBarClose")
						){
							lines[i] = lines[i].Replace("this.",string.Empty);
							List<string> phrases = new List<string>();
							reg1 = new Regex(@"(""+[\s\S]*""+)");
							string[] noquotedphrases = reg1.Split(lines[i]);//gets all of the quoted phrases from the line
							foreach(string sss in noquotedphrases){
								if(sss.StartsWith(@"""")) {
									lines[i] = lines[i].Replace(sss,@"@$"+phrases.Count.ToString()+@"@$");
									//each placeholder will be:  @$0@$ or @$1@$   These placeholders to be replaced with the quoted phrase after the pruning is completed.
									if(!phrases.Contains(sss))phrases.Add(sss);
								}
							}
//   Skips the [Description()] attribute...just in case there's a "CalculateOnBarClose" phrase in there
//							reg1 = new Regex(@"\[Description\(");
//        converts:  Description("must have CalculateOnBarClose=true")];   to  Description()]  ...basically removing any instance of where CalculateOnBarClose is in a phrase
//							reg1 = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");
							string stripped = KeepOnlyTheseCharacters(lines[i],33,9999," ");
							stripped = stripped.Replace("="," = ");
							var elements = stripped.Split(new Char[]{' '},StringSplitOptions.RemoveEmptyEntries);
							for(int k=0; k<elements.Length; k++){
								try{
									if(elements[k]=="Overlay") elements[k]="IsOverlay";
									else if(elements[k]=="ChartOnly") elements[k]="IsChartOnly";
									else if(elements[k]=="AutoScale") elements[k]="IsAutoScale";
									else if(elements[k]=="PlotsConfigurable") elements[k]="ArePlotsConfigurable";
									else if(elements[k]=="LinesConfigurable") elements[k]="AreLinesConfigurable";
									else if(elements[k].Contains("CalculateOnBarClose")){
											if(elements[k]=="CalculateOnBarClose=") {
												elements[k]="Calculate =";
												if(elements[k+1].StartsWith("true")) elements[k+1]="Calculate.OnBarClose;";
												else if(elements[k+1].StartsWith("false")) elements[k+1]="Calculate.OnPriceChange;";
											}
											else if(elements[k]=="CalculateOnBarClose") {
												elements[k]="Calculate";
												if(elements[k+2].StartsWith("true")) elements[k+2]="Calculate.OnBarClose;";
												else if(elements[k+2].StartsWith("false")) elements[k+2]="Calculate.OnPriceChange;";
											}
//										}
									}
								}catch{lines[i]=lines[i]+"//Unable to convert this line";}
							}
							lines[i] = ConvertListToString(elements);
							for(int phraseid = 0; phraseid<phrases.Count; phraseid++){
								lines[i]=lines[i].Replace(@"@$"+phraseid.ToString()+@"@$",phrases[phraseid]);
							}
						}
//-----------------------------------------------------------------------------------------------------
	//replace "OnStartUp()" method with 
						lines[i] = lines[i].Replace("protected override void OnStartUp()","if (State == State.Configure)");
//-----------------------------------------------------------------------------------------------------
	//replace "new SolidBrush" with "new SolidColorBrush"
						lines[i] = lines[i].Replace("new SolidBrush","new SolidColorBrush");
						lines[i] = lines[i].Replace("new  SolidBrush","new SolidColorBrush");
//-----------------------------------------------------------------------------------------------------
	//replace "SolidBrush" with "Brush", for variable declarations such as:   SolidBrush b;  which becomes Brush b;
						lines[i] = lines[i].Replace("SolidBrush","Brush");
//-----------------------------------------------------------------------------------------------------
	//any line that contains a "new SolidColorBrush" declaration, you must add ".Freeze();" to the end of that variable.
						if(lines[i].Contains("new SolidColorBrush")){
							var elements = lines[i].Split(new Char[]{'='});
							if(elements.Length>1){
								var e1 = elements[0].Trim().Split(new Char[]{' '});
								lines[i] = string.Concat(lines[i]," ",e1[e1.Length-1].Trim(),".Freeze();");
							}
						}
//-----------------------------------------------------------------------------------------------------
						if(lines[i].Contains("public override void Plot(")){
							if(lines[i+1].Trim()=="{") lines[i+1]=string.Empty;
	//  0         1       2     3         4            5            6        7      8        9      10     11      12
	//public    override void Plot     Graphics     graphics     Rectangle  bounds double minPrice double maxPrice  {
	//protected override void OnRender ChartControl chartControl ChartScale chartScale)
							var elements = lines[i].Split(new Char[]{' ',',','(',')'},StringSplitOptions.RemoveEmptyEntries);
							var NewElements = new List<string>();
							NameOfGraphicObject = elements[5].Trim()+".";
							NewElements.Add("protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {");
							NewElements.Add(NL+"if (!IsVisible) return;");
							NewElements.Add("double "+elements[9]+" = chartScale.MinValue; double "+elements[11]+" = chartScale.MaxValue;");
							NewElements.Add(NL+"base.OnRender(chartControl, chartScale);");
							NewElements.Add(NL+"Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);");
							NewElements.Add(NL+"Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);");
							NewElements.Add(NL+"int firstBarPainted = ChartBars.FromIndex;");
							NewElements.Add(NL+"int lastBarPainted = ChartBars.ToIndex;");
							NewElements.Add(NL);
							lines[i] = ConvertListToString(NewElements);
						}
//-----------------------------------------------------------------------------------------------------
		//converts:  base.Plot(graphics, bounds, minPrice, maxPrice);   and comments it out
						reg1 = new Regex(@"base.Plot\([\s\S]+,[\s\S]+,[\s\S]+,[\s\S]+\);");
						lines[i] = reg1.Replace(lines[i],@"//$_");
						lines[i] = lines[i].Replace(".Pen.DashStyle",".DashStyleHelper");
						lines[i] = lines[i].Replace(".DashStyle",".DashStyleHelper");
						lines[i] = lines[i].Replace("DashStyle.","DashStyleHelper.");
						lines[i] = lines[i].Replace("ChartControl.LastBarPainted","lastBarPainted");
						lines[i] = lines[i].Replace("ChartControl.FirstBarPainted","firstBarPainted");
						lines[i] = lines[i].Replace("ChartControl.BarMargin","ChartControl.Properties.BarMarginRight");
						lines[i] = lines[i].Replace("this.LastBarIndexPainted","ChartBars.ToIndex");
						lines[i] = lines[i].Replace("LastBarIndexPainted","ChartBars.ToIndex");
						lines[i] = lines[i].Replace("this.FirstBarIndexPainted","ChartBars.FromIndex");
						lines[i] = lines[i].Replace("FirstBarIndexPainted","ChartBars.FromIndex");
						lines[i] = lines[i].Replace("ChartStyle.GetBarPaintWidth(ChartControl.BarWidth)","chartControl.GetBarPaintWidth(ChartBars);");
//						lines[i] = lines[i].Replace("","");
//						lines[i] = lines[i].Replace("","");
//						lines[i] = lines[i].Replace("","");

						lines[i] = lines[i].Replace("Data.BarsType.GetInstance(Bars.BarsPeriod.BarsPeriodType).IsIntraday","BarsArray[0].BarsType.IsIntraday");
						lines[i] = lines[i].Replace("bounds.Width","ChartPanel.W");
						lines[i] = lines[i].Replace("bounds.X","ChartPanel.X");
						lines[i] = lines[i].Replace("bounds.Location.","ChartPanel.");
						lines[i] = lines[i].Replace("bounds.Left","ChartPanel.X");
						lines[i] = lines[i].Replace("bounds.Y","ChartPanel.Y");
						lines[i] = lines[i].Replace("bounds.Height","ChartPanel.H");
		//converts:  chartControl.GetXByBarIndex(BarsArray[0],   into chartControl.GetXByBarIndex(ChartBars,
						lines[i] = lines[i].Replace("GetXByBarIndex(BarsArray[0],","GetXByBarIndex(ChartBars,");
						lines[i] = lines[i].Replace("GetXByBarIndex(Bars,","GetXByBarIndex(ChartBars,");
						lines[i] = lines[i].Replace("ChartControl.GetYByValue(","chartScale.GetYByValue(");
						reg1 = new Regex(@"chartScale.GetYByValue\s*\([\s\S]+,([\s\S]+)\s*\)");
						lines[i] = reg1.Replace(lines[i],@"chartScale.GetYByValue($1)");
//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("BarsArray[0].Period","BarsArray[0].BarsPeriod");
						lines[i] = lines[i].Replace("BarsArray[1].Period","BarsArray[1].BarsPeriod");
						lines[i] = lines[i].Replace("BarsArray[2].Period","BarsArray[2].BarsPeriod");
						lines[i] = lines[i].Replace("BarsArray[3].Period","BarsArray[3].BarsPeriod");
						lines[i] = lines[i].Replace("BarsArray[4].Period","BarsArray[4].BarsPeriod");
//-----------------------------------------------------------------------------------------------------
						if(lines[i].Contains("Alert(")){
							lines[i] = lines[i].Replace("Color.","Brushes.");
							lines[i] = lines[i].Replace("NinjaTrader.Cbi.Priority.","NinjaTrader.NinjaScript.Priority.");
						}
//-----------------------------------------------------------------------------------------------------
						if(lines[i].Contains("Plots[")){
//							Print("TrimmedLine: '"+TrimmedLine+"'");
							string[] elements = lines[i].Split(new Char[]{';'},StringSplitOptions.RemoveEmptyEntries);
							lines[i]=string.Empty;
							for(int idx=0;idx<elements.Length;idx++){
								elements[idx]=elements[idx]+";";
								reg1 = new Regex(@"((Plots\[[^\n\r]+\])\.Pen)\.Color\s*=\s*Color\.([A-Z][a-z]+);\s*");//converts: Plots[2].Pen.Color = Color.Yellow;  to  Plots[2].Pen = new Pen(Brushes.Yellow,Plots[2].Width);
								elements[idx] = reg1.Replace(elements[idx],@"$1 = new Pen(Brushes.$3,$2.Width);");
								lines[i]=string.Concat(lines[i],elements[idx]);
							}
//							string[] elements = lines[i].Split(new Char[]{',','=',' '},StringSplitOptions.RemoveEmptyEntries);
//							if(elements[1].Trim().EndsWith(".Pen.Color")) {
////v7:					Plots[2].Pen.Color = Color.Yellow;
////v8:					Plots[2].Pen = new Pen(Brushes.Yellow,Plots[2].Width);
//								string pen = elements[0].Replace(".Color",string.Empty);
//								string color = elements[1].Replace("Color.",string.Empty).Replace(";",string.Empty);
//								lines[i] = string.Concat(pen," = new Pen(Brushes.",color,", ",elements[0].Replace(".Pen.Color",string.Empty),".Width);");
//							}
						}
//-----------------------------------------------------------------------------------------------------
//						if(lines[i].Contains("Color.FromArgb")){
////v7:  BuyTimeBlockColor = Color.FromArgb(25*pTimeBlockOpacity, Plots[0].Pen.Color);
////v8:  BuyTimeBlockColor = Color.FromArgb((byte)(25*pTimeBlockOpacity), ((SolidColorBrush)Plots[0].Pen.Brush).Color.R, ((SolidColorBrush)Plots[0].Pen.Brush).Color.G, ((SolidColorBrush)Plots[0].Pen.Brush).Color.B);
//							try{
//								string[] elements = lines[i].Split(new Char[]{','},StringSplitOptions.RemoveEmptyEntries);
//								string[] OpacityElement = elements[0].Split(new char[]{'('},StringSplitOptions.RemoveEmptyEntries);
//								OpacityElement[1]="((byte)("+OpacityElement[1]+")";
//								elements[0] = OpacityElement[0]+OpacityElement[1];
//								string[] ColorElement = elements[1].Split(new Char[]{',',')',';'},StringSplitOptions.RemoveEmptyEntries);
//								string colorR="";	string colorG="";	string colorB="";
//								if(ColorElement[0].Contains("Pen.Color")){
//									ColorElement[0] = ColorElement[0].Replace("Pen.Color","Pen.Brush");
//									colorR = "((SolidColorBrush)"+ColorElement[0]+").Color.R,";
//									colorG = "((SolidColorBrush)"+ColorElement[0]+").Color.G,";
//									colorB = "((SolidColorBrush)"+ColorElement[0]+").Color.B);";
//								}else{
//									colorR = ColorElement[0]+".R,";
//									colorG = ColorElement[0]+".G,";
//									colorB = ColorElement[0]+".B);";
//								}
//								lines[i] = string.Concat(elements[0],", ",colorR,colorG,colorB);
//							}catch{lines[i]=lines[i]+"//Unable to convert this color:    ((SolidColorBrush) <YourColor>).Color.R,.G,.B);";}
//						}
//-----------------------------------------------------------------------------------------------------
						if(NameOfGraphicObject.Length>0 && lines[i].Contains(NameOfGraphicObject)) lines[i] = lines[i].Replace(NameOfGraphicObject,"RenderTarget.");
						if(lines[i].Contains("SizeF")){
							lines[i] = @"//  "+lines[i];
						}
						lines[i] = lines[i].Replace("size.Width","txtLayout.Metrics.Width");
						lines[i] = lines[i].Replace(".DrawString(",".DrawTextLayout(  ");
						if(lines[i].Contains("RenderTarget.DrawTextLayout(")){
							string leader = lines[i].Substring(0, lines[i].IndexOf('R'));
							lines[i] = ConsolidateMultiline(leader, lines, ref i);
							try{
								var match = Regex.Match(lines[i], @"(?<=\().*(?=\);\z)");
								if(match.Success){
									var values = match.Value.Split(new char[]{','}); // split by commas
									var str1 = string.Format("{0}var txtLayout	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, {1}, txtFormat, (float)(ChartPanel.X + ChartPanel.W), 12f);",leader, values[0].Trim());
									int len = lines[i].IndexOf("t(")+1;
									var str2 = string.Format("{1}v1.X={2};{0}{1}v1.Y={3};", NL, leader, values[3].Trim(), values[4].Trim());
									var str3 = string.Format("{0}RenderTarget.DrawTextLayout(v1, txtLayout, {1}DX);", leader, values[2].Trim());
									lines[i] = string.Format("//{4}{0}{1}{0}{2}{0}{3}{0}",NL,str1,str2,str3, lines[i].Trim());
//									console.log(values); // ["A", " B", " brush", " x", " y", " z"]
								}
							}catch{
								lines[i] = @"// Convert7to8 ERROR    "+lines[i];
							}
						}
						lines[i] = lines[i].Replace("ChartControl.BarSpace", "ChartControl.Properties.BarDistance");
//-----------------------------------------------------------------------------------------------------
						if(lines[i].Contains(".Set(")){
							lines[i] = lines[i].Replace(".Set","[0] = ");
/*
							var elements = lines[i].Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries);
							if(elements.Length>0)elements[elements.Length-1]=elements[elements.Length-1]+";";
							for(int idx = 0; idx<elements.Length; idx++){
		//converts: a.Set(4,var1);  a.Set(x ,var1-EMA(9)[0]); a.Set( 3,-3.4); a.Set( 3,Closes[0][0]);
								reg1 = new Regex(@".Set\s*\(([\d\S\s]+),\s*([\s\S]*)\);");
								if(reg1.IsMatch(elements[idx]))
									elements[idx] = reg1.Replace(elements[idx],"[$1]=($2);");
								else {
		//converts:  a_2.Set(9);  a.Set(-3.3);  a.Set(-x);  a.Set(-EMA(3)[0]);
									reg1 = new Regex(@".Set\s*\(\s*([\-\(\)\[\]a-zA-Z0-9\.]+)\);");
									if(reg1.IsMatch(elements[idx]))
										elements[idx] = reg1.Replace(elements[idx],"[0]=($1);");
									else
										elements[idx] = elements[idx]+@" //could not convert";
								}
							}
							lines[i] = ConvertListToString(elements);
*/
						}

//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("Round2TickSize","RoundDownToTickSize");

						string pattern = @"\s*(\w+)\s*=\s*Color\.FromArgb\s*\(\s*(\w+)\s*,\s*(\w+)\s*\);";
						reg1 = new Regex(pattern,
						    RegexOptions.IgnoreCase
						    | RegexOptions.CultureInvariant
						    | RegexOptions.Compiled);
						if(reg1.IsMatch(lines[i])){
							lines[i] = reg1.Replace(lines[i], @"$3_frozen = $3.Clone(); $3_frozen.Opacity = $2/100.0; $3_frozen.Freeze(); $1 = $3_frozen;");
						}
						reg1 = new Regex(@"\s+Color\.");
						lines[i] = reg1.Replace(lines[i],@" Brushes.");
						reg1 = new Regex(@"\s+Color\s+");
						lines[i] = reg1.Replace(lines[i],@" Brush ");

//						if(lines[i].Contains("public Color")){
//							lines[i] = lines[i].Replace("Color.","Brushes.");
//							lines[i] = lines[i].Replace("Color","Brush");
//						}
//						if(lines[i].Contains("private Color")){
//							lines[i] = lines[i].Replace("Color.","Brushes.");
//							lines[i] = lines[i].Replace("Color","Brush");
//						}
						if(lines[i].Contains("new DataSeries"))
							lines[i] = lines[i].Replace("new DataSeries","new Series<double>")+@"//inside State.DataLoaded";
						else if(lines[i].Contains("private DataSeries"))
							lines[i] = lines[i].Replace("private DataSeries","private Series<double>");
						else if(lines[i].Contains("public DataSeries"))
							lines[i] = lines[i].Replace("public DataSeries","public Series<double>");
						if(lines[i].Contains("new IntSeries"))
							lines[i] = lines[i].Replace("new IntSeries","new Series<int>")+@"//inside State.DataLoaded";
						if(lines[i].Contains("public IntSeries"))
							lines[i] = lines[i].Replace("public IntSeries","public Series<int>");
						if(lines[i].Contains("new BoolSeries"))
							lines[i] = lines[i].Replace("new BoolSeries","new Series<bool>")+@"//inside State.DataLoaded";
						if(lines[i].Contains("public BoolSeries"))
							lines[i] = lines[i].Replace("public BoolSeries","public Series<bool>");
						if(lines[i].Contains("BoolSeries"))
							lines[i] = lines[i].Replace("BoolSeries","Series<bool>");
						if(lines[i].Contains("IntSeries"))
							lines[i] = lines[i].Replace("IntSeries","Series<int>");
						lines[i] = lines[i].Replace("FirstTickOfBar","IsFirstTickOfBar");
//						lines[i] = lines[i].Replace("","");
//						lines[i] = lines[i].Replace("","");
						lines[i] = lines[i].Replace(".ContainsValue(",".IsValidDataPoint(");
						lines[i] = lines[i].Replace(".IsValidPlot(",".IsValidDataPoint(");
						lines[i] = lines[i].Replace("this.AutoScale","this.IsAutoScale");
						lines[i] = lines[i].Replace("ChartControl.AxisColor","ChartControl.Properties.AxisPen.Brush");
						lines[i] = lines[i].Replace("ChartControl.BackColor","ChartControl.Properties.ChartBackground");
						lines[i] = lines[i].Replace("BarColor","BarBrush");
						lines[i] = lines[i].Replace("PlotColors","PlotBrushes");
						lines[i] = lines[i].Replace("CandleOutlineColor","CandleOutlineBrush");
						lines[i] = lines[i].Replace("BackBrushSeries","BackBrushes");
						lines[i] = lines[i].Replace("BackColorAllSeries","BackBrushesAll");
						lines[i] = lines[i].Replace("PlotColors[","PlotBrushes[");
						lines[i] = lines[i].Replace("PeriodType","BarsPeriodType");
						lines[i] = lines[i].Replace("Bars.Period","Bars.BarsPeriod");
						lines[i] = lines[i].Replace("BarsPeriod.Id","BarsPeriod.BarsPeriodType");
						lines[i] = lines[i].Replace("DateTime.Now()","NinjaTrader.Core.Globals.Now");
						lines[i] = lines[i].Replace("DateTime.Now","NinjaTrader.Core.Globals.Now");
						lines[i] = lines[i].Replace("ChartControl.ChartStyleType","ChartBars.Properties.ChartStyleType");
						lines[i] = lines[i].Replace("Cbi.Core.DbDir","Cbi.DB.DBDir");
						lines[i] = lines[i].Replace("Cbi.Core.InstallDir" ,"Core.Globals.InstallDir");
						lines[i] = lines[i].Replace("Cbi.Core.UserDataDir","Core.Globals.UserDataDir");

						reg1 = new Regex(@"(\s+)PlaySound\s*\(",
						    RegexOptions.IgnoreCase
						    | RegexOptions.CultureInvariant
						    | RegexOptions.Compiled);
						if(reg1.IsMatch(lines[i])){
							AddSoundFolderCode = true;
							lines[i] = lines[i].Replace("(","(AddSoundFolder(").Replace(");","));");
						}

//						reg1 = new Regex(@"\s+PlaySound\s*\((w+)");
//						if(reg1.IsMatch(lines[i])){
//							AddSoundFolderCode = true;
//							lines[i] = reg1.Replace(lines[i],@" PlaySound(AddSoundFolder(");
////							lines[i] = lines[i].Replace("PlaySound(","PlaySound(AddSoundFolder(");
//						}
						reg1 = new Regex(@"\s+Alert\s*\(");
						if(reg1.IsMatch(lines[i])){
							AddSoundFolderCode = true;
							var elem = lines[i].Split(new char[]{','});
							int start = 0;
							for(start = 0; start<elem.Length; start++) if(elem[start].Contains("Alert")) break;
							//Alert(tag,    x,    msg,    wav,    time,    A,  B);
							elem[start+3] = string.Format("AddSoundFolder({0})",elem[start+3]);
							lines[i] = string.Format("{0},{1},{2},{3},{4},{5},{6}",elem[0],elem[1],elem[2],elem[3],elem[4],elem[5],elem[6]);
						}
//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("ChartControl.Font","ChartControl.Properties.LabelFont");
						lines[i] = lines[i].Replace("ChartControl.GetXByBarIdx(","chartControl.GetXByBarIndex(ChartBars,");
						lines[i] = lines[i].Replace("ChartControl.GetYByValue(this,","chartScale.GetYByValue(");
						lines[i] = lines[i].Replace("ChartControl.GetYByValue(this ,","chartScale.GetYByValue(");
						lines[i] = lines[i].Replace("ChartControl.Invalidate ,","ForceRefresh");
//-----------------------------------------------------------------------------------------------------
//v7:  DrawTextFixed("tag","License failure", TextPosition.Center);
//v8:  Draw.TextFixed(this, string tag, string text, TextPosition textPosition)
//                                0             1                  2                     3                    4                  5                   6                  7
//v7:  DrawTextFixed(         "tag",          "Msg", TextPosition.Center,       ChartControl.AxisColor, ChartControl.Font, Color.Black,        ChartControl.BackColor, 10);
//v8:  Draw.TextFixed(this, string tag, string text, TextPosition textPosition, Brush textBrush,         SimpleFont font,  Brush outlineBrush, Brush areaBrush,        int areaOpacity)
//Draw.TextFixed(NinjaScriptBase owner, string tag, string text, TextPosition textPosition, bool isGlobal, string templateName)
						lines[i] = lines[i].Replace("DrawText(","Draw.Text(this, ");
						lines[i] = lines[i].Replace("DrawTextFixed(","Draw.TextFixed(this, ");
						lines[i] = lines[i].Replace("DrawType.","NinjaTrader.NinjaScript.DrawingTools.");
						lines[i] = lines[i].Replace(".DrawType",".GetType()");

//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("ILine","NinjaTrader.NinjaScript.DrawingTools.Line");
						if(!lines[i].Contains("RenderTarget.Draw")){
							lines[i] = lines[i].Replace("DrawLine(","Draw.Line(this, ");
						}
						lines[i] = lines[i].Replace("DrawRegion(","Draw.Region(this, ");
						lines[i] = lines[i].Replace("IHorizontalLine","NinjaTrader.NinjaScript.DrawingTools.HorizontalLine");
						lines[i] = lines[i].Replace("DrawHorizontalLine(","Draw.HorizontalLine(this, ");
						lines[i] = lines[i].Replace("IVerticalLine","NinjaTrader.NinjaScript.DrawingTools.VerticalLine");
						lines[i] = lines[i].Replace("DrawVerticalLine(","Draw.VerticalLine(this, ");
						lines[i] = lines[i].Replace("IDot","NinjaTrader.NinjaScript.DrawingTools.Dot");
						lines[i] = lines[i].Replace("DrawDot(","Draw.Dot(this, ");
						lines[i] = lines[i].Replace("DrawSquare(","Draw.Square(this, ");
						lines[i] = lines[i].Replace("DrawTriangleUp(","Draw.TriangleUp(this, ");
						lines[i] = lines[i].Replace("DrawTriangleDown(","Draw.TriangleDown(this, ");
						lines[i] = lines[i].Replace("IDiamond","NinjaTrader.NinjaScript.DrawingTools.Diamond");
						lines[i] = lines[i].Replace("DrawDiamond(","Draw.Diamond(this, ");
						lines[i] = lines[i].Replace("IArrowUp","NinjaTrader.NinjaScript.DrawingTools.ArrowUp");
						lines[i] = lines[i].Replace("DrawArrowUp(","Draw.ArrowUp(this, ");
						lines[i] = lines[i].Replace("IArrowDown","NinjaTrader.NinjaScript.DrawingTools.ArrowDown");
						lines[i] = lines[i].Replace("DrawArrowDown(","Draw.ArrowDown(this, ");
						lines[i] = lines[i].Replace("IGannFan","NinjaTrader.NinjaScript.DrawingTools.GannFan");
						lines[i] = lines[i].Replace("DrawGannFan(","Draw.GannFan(this, ");
						lines[i] = lines[i].Replace("IRay","NinjaTrader.NinjaScript.DrawingTools.Ray");
						lines[i] = lines[i].Replace("DrawRay(","Draw.Ray(this, ");
						lines[i] = lines[i].Replace("IRectangle","NinjaTrader.NinjaScript.DrawingTools.Rectangle");
						lines[i] = lines[i].Replace("DrawRectangle(","Draw.Rectangle(this, ");
						lines[i] = lines[i].Replace("Pen.Width","Stroke.Width");
						lines[i] = lines[i].Replace(".Locked",".IsLocked");
//-----------------------------------------------------------------------------------------------------
						reg1 = new Regex(@"Draw.[A-Z]\S*\s*\(\s*[a-zA-Z0-9]*\s*,([\s\S]*,[\s\S]*)\);");//converts: abc.Set(4,var1);  a5.Set(x ,var1); A1a.Set( 3,-3.4);
						if(reg1.IsMatch(lines[i])) lines[i] = lines[i].Replace("DashStyle.","DashStyleHelper.");
//need to convert RenderTarget.DrawLine(LinePen, LeftmostPixelOfLines, lineval2zupper, LeftmostPixelOfLines, lineval2zlower);  to RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zupper), new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zlower), LineBrush);

//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("public TemporaryMessageManager(Color topright_color, Color topleft_color, Color center_color, Color bottomright_color, Color bottomleft_color) { ","public TemporaryMessageManager(Brush topright_color, Brush topleft_color, Brush center_color, Brush bottomright_color, Brush bottomleft_color) { ");
//-----------------------------------------------------------------------------------------------------
						if(TrimmedLine.StartsWith("#region Properties")) AfterPropertiesDefined = true;
						if(AfterPropertiesDefined){
							if(this.AddSoundFolderCode && HasAddedSoundFolderCode==false){
		Print("450:  Inserting sound folder code");
								HasAddedSoundFolderCode=true;
								AddSoundFolderCode = false;
								lines.Insert(i-1, "//====================================================================");
								lines.Insert(i-1, "		}");
								lines.Insert(i-1, "			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, \"sounds\",wav);");
								lines.Insert(i-1, "		private string AddSoundFolder(string wav){");
								lines.Insert(i-1, "//====================================================================");
								i = i+5;
							}
		Print("'"+lines[i]+"'");
							if(TrimmedLine.StartsWith("#region NinjaScript generated code.")) AfterPropertiesDefined = false;
							if(TrimmedLine.StartsWith("public enum")) AfterPropertiesDefined = false;
		if(!AfterPropertiesDefined)Print("*******   Done with properties conversion ****************");
							if(TrimmedLine.StartsWith("[Gui.Design.DisplayNameAttribute(")) {
				//converts: [Gui.Design.DisplayNameAttribute("Line style")]    to  'Line Style'
								reg1 = new Regex(@"\[Gui.Design.DisplayNameAttribute\(""(?:\t)*([\s\S]+)""\)\]");
								if(reg1.IsMatch(TrimmedLine)) {
									PropertyDisplayName = reg1.Replace(TrimmedLine,@"$1").Replace('"','\n').Trim();
									PropertyDisplayNameLine = i;
									lines[i] = @"// "+TrimmedLine;
		Print("PropertyDisplayName: '"+PropertyDisplayName+"'");
								}
							}
							else if(TrimmedLine.StartsWith("[Gui.Design.DisplayName(")) {
				//converts: [Gui.Design.DisplayNameAttribute("Line style")]    to  'Line Style'
								reg1 = new Regex(@"\[Gui.Design.DisplayName\(""(?:\t)*([\s\S]+)""\)\]");
								if(reg1.IsMatch(TrimmedLine)) {
									PropertyDisplayName = reg1.Replace(TrimmedLine,@"$1").Replace('"','\n').Trim();
									PropertyDisplayNameLine = i;
									lines[i] = @"// "+TrimmedLine;
		Print("PropertyDisplayName: '"+PropertyDisplayName+"'");
								}
							}
							else if(TrimmedLine.Contains("[Category") || TrimmedLine.Contains("[GridCategory")) {
//		[Category("Lines")] or [GrigCategory("Lines")]   to   'Lines'
								reg1 = new Regex(@"\[(?:GridCategory|Category)\(""(?:\\t)*([\s\S]+)\""\)\]");
								CategoryName = reg1.Replace(TrimmedLine,@"$1").Replace('"','\n').Trim();
		Print("CategoryName: '"+CategoryName+"'");
								CategoryNameLine = i;
							} else if(TrimmedLine.StartsWith("public")){
								int linenum = Math.Max(PropertyDisplayNameLine,CategoryNameLine);
//		Print("linenum = "+linenum);
								if(PropertyDisplayNameLine>0){
									if(PropertyDisplayName.Length>0) {
										PropertyDisplayName = @"Name = """+PropertyDisplayName+@""", ";
										if(CategoryNameLine>0) lines[CategoryNameLine] = @"// "+lines[CategoryNameLine];
									}
									if(CategoryName.Length>0) CategoryName = @" GroupName = """+CategoryName+@"""";
									string result = @"[Display(Order=1, "+PropertyDisplayName+CategoryName+", ResourceType = typeof(Custom.Resource))]";
									lines[linenum] = lines[linenum]+NL+result;
		Print("  Full attribute: '"+result+"'");
								}
		Print("......blanking out property values");
								PropertyDisplayName = string.Empty;
								CategoryName=string.Empty;
								PropertyDisplayNameLine = -1;
								CategoryNameLine = -1;
							}
//		[Description("Line style for EndOfChopZone lines?")]
//
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptGeneral", Order = 0)]
						}
//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("GridCategory","Category");
						if(lines[i].Contains("BackColor")){
							reg1 = new Regex(@"[\s+]BackColor[\s=]");
							lines[i] = reg1.Replace(lines[i],@"BackBrush");
						}
						if(lines[i].Contains("!Historical")){
							reg1 = new Regex(@"!Historical[\s=]");
							lines[i] = reg1.Replace(lines[i],@"(State != State.Historical)");
						}else{
							reg1 = new Regex(@"\sHistorical[\s=]");
							lines[i] = reg1.Replace(lines[i],@"(State == State.Historical)");
						}
//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("private DashStyle","private DashStyleHelper");
						lines[i] = lines[i].Replace("public DashStyle","public DashStyleHelper");
//-----------------------------------------------------------------------------------------------------
						lines[i] = lines[i].Replace("NinjaTrader.Gui.Design.SerializableColor","Serialize");
						lines[i] = lines[i].Replace("Serialize.ToString(","Serialize.BrushToString(");
						lines[i] = lines[i].Replace("Serialize.FromString(","Serialize.StringToBrush(");
//-----------------------------------------------------------------------------------------------------
					}
					if(this.AddSoundFolderCode){
		Print("526:  Adding sound folder code to end");
						AddSoundFolderCode = false;
						lines.Add("//====================================================================");
						lines.Add("		private string AddSoundFolder(string wav){");
						lines.Add("			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @\"sounds\\\"+wav);");
						lines.Add("		}");
						lines.Add("//====================================================================");
					}
					File.WriteAllLines(newfilename,lines);
				}
	        }
		}
//================================================================================================================
	private string RemoveComments(string s){
		if(s.Contains("//")){
			int idx = s.IndexOf("//");
			s = s.Substring(0,idx);//strip out all '//' comments
		}
		if(s.Contains(@"/*")){
			MatchCollection matches = Regex.Matches(s, @"/\*.*?\*/");
			foreach (Match match in matches){
				s = s.Replace(match.Value,""); //strip out all "/*...*/" comments
			}
		}
		return s;
	}
	private string ConsolidateMultiline(string leaderstr, List<string> lines,ref int i){
		string s = RemoveComments(lines[i]).Trim();
		string result = string.Empty;
		if(s.EndsWith(";"))
			return string.Format("{0}{1}", leaderstr, s);
		else{
			result = string.Format("{0}{1}", leaderstr, s);
			var i2 = i+1;
			bool done = false;
			while(!done && lines.Count>i2){
				s = RemoveComments(lines[i2]).Trim();
				lines[i2] = string.Format(@"//{0}", lines[i2]);
				if(s.EndsWith(";")) done=true;
				result = string.Format("{0}{1}", result, s);
				i2++;
			}
			return result;
		}
	}

	private string ConvertListToString(string[] NewElements){
		string result = string.Empty;
		for(int k = 0; k<NewElements.Length; k++) {
		if(k==0) 
			result = NewElements[0];
		else 
			result = string.Concat(result,NewElements[k]);
		}
		return result;
	}
//================================================================================================================
	private string ConvertListToString(string[] NewElements, char Separator){
		string result = string.Empty;
		for(int k = 0; k<NewElements.Length; k++) {
		if(k==0) 
			result = NewElements[0];
		else 
			result = string.Concat(result,Separator,NewElements[k]);
		}
		return result;
	}
//================================================================================================================
	private string ConvertListToString(List<string> NewElements){
		return ConvertListToString(NewElements.ToArray());
	}
//================================================================================================================
	private static string KeepOnlyTheseCharacters(string instr, int MinASCII, int MaxASCII,string ReplacementStr){
		string ret = string.Empty;
		char[] str = instr.ToCharArray(0,instr.Length);
		for(int i = 0; i<str.Length; i++) {
			if((int)str[i]>=MinASCII && (int)str[i]<=MaxASCII) ret = string.Concat(ret,str[i].ToString());
			else ret = string.Concat(ret,ReplacementStr);
		}
		return ret;
	}
//================================================================================================================

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
        }

        #region Properties
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Utilities.Convert7to8[] cacheConvert7to8;
		public Utilities.Convert7to8 Convert7to8()
		{
			return Convert7to8(Input);
		}

		public Utilities.Convert7to8 Convert7to8(ISeries<double> input)
		{
			if (cacheConvert7to8 != null)
				for (int idx = 0; idx < cacheConvert7to8.Length; idx++)
					if (cacheConvert7to8[idx] != null &&  cacheConvert7to8[idx].EqualsInput(input))
						return cacheConvert7to8[idx];
			return CacheIndicator<Utilities.Convert7to8>(new Utilities.Convert7to8(), input, ref cacheConvert7to8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Utilities.Convert7to8 Convert7to8()
		{
			return indicator.Convert7to8(Input);
		}

		public Indicators.Utilities.Convert7to8 Convert7to8(ISeries<double> input )
		{
			return indicator.Convert7to8(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Utilities.Convert7to8 Convert7to8()
		{
			return indicator.Convert7to8(Input);
		}

		public Indicators.Utilities.Convert7to8 Convert7to8(ISeries<double> input )
		{
			return indicator.Convert7to8(input);
		}
	}
}

#endregion
