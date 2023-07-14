using insoles.UserControls;
using ScottPlot;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace insoles.Services
{
    public class InformesGeneratorService : IInformesGeneratorService
    {
        private GRF grf;
        private GrafoMariposa grafoMariposa;
        private Heatmap heatmap;
        public InformesGeneratorService(GRF grf, GrafoMariposa grafoMariposa, Heatmap heatmap) 
        { 
            this.grf = grf;
            this.grafoMariposa = grafoMariposa;
            this.heatmap = heatmap;
        }
        private string FileNameGenerator()
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.Month.ToString().PadLeft(2, '0');
            string day = now.Day.ToString().PadLeft(2, '0');
            string hour = now.Hour.ToString().PadLeft(2, '0');
            string minute = now.Minute.ToString().PadLeft(2, '0');
            string second = now.Second.ToString().PadLeft(2, '0');
            string milisecond = now.Millisecond.ToString().PadLeft(3, '0');
            string filename = year + month + day + '-' + hour + '-' + minute + '-' + second + '-' + milisecond;
            return filename;
        }
        public async Task<string> GenerarInforme()
        {
            grf.rangePlot.Plot.SaveFig("range.png");
            grf.plot.Plot.SaveFig("GRF.png");

            grafoMariposa.plot.Plot.SaveFig("butterfly.png");

            await heatmap.SaveFigs();

            // Creating a new document.
            WordDocument document = new WordDocument();
            //Adding a new section to the document.
            WSection section = document.AddSection() as WSection;
            //Set Margin of the section
            section.PageSetup.Margins.All = 72;
            //Set page size of the section
            section.PageSetup.PageSize = new System.Drawing.SizeF(612, 792);



            IWParagraph paragraph = section.HeadersFooters.Header.AddParagraph();

            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            WTextRange textRange = paragraph.AppendText("InnerFEET Pressure Register Tool") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.FontName = "Calibri";



            //Appends paragraph.
            paragraph = section.AddParagraph();
            paragraph.ParagraphFormat.FirstLineIndent = 36;
            paragraph.BreakCharacterFormat.FontSize = 12f;
            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de GRF:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;

            // Gets the image stream.
            IWPicture picture = paragraph.AppendPicture(new System.Drawing.Bitmap(@"GRF.png")) as WPicture;


            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de STDDEV:") as WTextRange;
            IWPicture picture2 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"Range.png")) as WPicture;

            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de Mariposa:") as WTextRange;
            IWPicture picture3 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"butterfly.png")) as WPicture;

            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de Presiones Maximo:") as WTextRange;
            IWPicture picture4 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"heatmap_max.png")) as WPicture;

            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de Presiones Medio:") as WTextRange;
            IWPicture picture5 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"heatmap_avg.png")) as WPicture;

            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de Presiones Mínimo:") as WTextRange;
            IWPicture picture6 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"heatmap_min.png")) as WPicture;

            string path = "C:\\Users\\" + Environment.UserName + "\\Documents";
            string filename = FileNameGenerator() + ".docx";
            string filenamePath = path + Path.DirectorySeparatorChar + filename;
            document.Save(filenamePath);

            return filenamePath;
        }
    }
}
