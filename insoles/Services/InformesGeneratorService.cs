using insoles.UserControls;
using ScottPlot;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public void GenerarInforme()
        {
            grf.rangePlot.Plot.SaveFig("range.png");
            grf.plot.Plot.SaveFig("GRF.png");

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


            document.Save("Sample.docx");


        }
    }
}
