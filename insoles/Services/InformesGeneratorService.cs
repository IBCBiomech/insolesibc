using insoles.UserControls;
using ScottPlot;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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

        /**
         * Método para generar infornme
         */
        public async Task<string> GenerarInforme()
        {
            grf.rangePlot.Plot.SaveFig("range.png");
            grf.plot.Plot.SaveFig("GRF.png");
            grf.normPlot.Plot.SaveFig("norm.png");

            grafoMariposa.plot.Plot.SaveFig("butterfly.png");

            await heatmap.SaveFigs();

            // Creating a new document.



            WordDocument document = new WordDocument();

            WParagraph paragraph = new WParagraph(document);

            //Adding a new section to the document.
            WSection section = document.AddSection() as WSection;
            //Set Margin of the section
            section.PageSetup.Margins.All = 72;
            //Set page size of the section
            //section.PageSetup.PageSize = new System.Drawing.SizeF(612, 792);


            paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;

            // Gets the image stream.
            IWPicture seatpicture = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"Images\seat.png"));

            IWTextRange textRange = paragraph.AppendText("\t\t\t\t\t\t\t\t");
            IWPicture ibcpicture = paragraph.AppendPicture(new System.Drawing.Bitmap(@"Images\ibc.png"));


            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            textRange = paragraph.AppendText("SW. Salud y Seguridad en el Trabajo") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            textRange = paragraph.AppendText("INFORME DE VALORACIÓN BIOMECÁNICA") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";

            paragraph = (WParagraph)section.AddParagraph();
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            //Nombre:


            textRange = paragraph.AppendText("NOMBRE:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);

            //NIS:
            //Nombre:
            textRange = paragraph.AppendText("NIS:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            //Exploración:
            //Nombre:
            textRange = paragraph.AppendText("Exploración:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            textRange = paragraph.AppendText("Test cinético de la marcha") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);

            //Fecha:
            //Nombre:
            textRange = paragraph.AppendText("Fecha:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
            textRange = paragraph.AppendText("Los laboratorios de biomecánica de IBC poseen la autorización del Departament de Salut de la Generalitat de Catalunya con los códigos de registro: E08589869 y E08592300") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";

            paragraph.AppendBreak(BreakType.PageBreak);
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            textRange = (WTextRange)paragraph.AppendText("DATOS DEL PACIENTE");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";


            paragraph.AppendBreak(BreakType.PageBreak);
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            textRange = (WTextRange)paragraph.AppendText("1. HISTORIA CLÍNICA");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";

            paragraph.AppendBreak(BreakType.PageBreak);
            textRange = (WTextRange)paragraph.AppendText("2. ANÁLISIS CINÉTICO DE LA MARCHA");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);


            textRange = (WTextRange)paragraph.AppendText("Intervalo de la marcha");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture rango = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\range.png"));

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            textRange = (WTextRange)paragraph.AppendText("Total de la marcha");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture total = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\GRF.png"));

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            textRange = (WTextRange)paragraph.AppendText("Distribución del centro de presiones");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture butterfly = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\butterfly.png"));

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            textRange = (WTextRange)paragraph.AppendText("Distribución máxima");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture maxpressure = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\heatmap_max.png"));

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            textRange = (WTextRange)paragraph.AppendText("Distribución media");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture avgpressure = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\heatmap_avg.png"));

            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);

            textRange = (WTextRange)paragraph.AppendText("Gráficas de fuerza normalizadas");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = false;
            textRange.CharacterFormat.FontName = "Calibri";
            paragraph.AppendBreak(BreakType.LineBreak);
            paragraph.AppendBreak(BreakType.LineBreak);
            IWPicture grfnorm = paragraph.AppendPicture(System.Drawing.Image.FromFile(@"C:\Users\Deva\wsibc\insolesibc\insoles\bin\Debug\net6.0-windows\norm.png"));

            paragraph.AppendBreak(BreakType.PageBreak);
            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            textRange = (WTextRange)paragraph.AppendText("3. CONCLUSIONES");
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.Bold = true;
            textRange.CharacterFormat.FontName = "Calibri";

            string path = @"%HOMEDRIVE%%HOMEPATH%\insoles";
            string filename = FileNameGenerator() + ".docx";
            string filenamePath = path + Path.DirectorySeparatorChar + filename;
            document.Save(Environment.ExpandEnvironmentVariables(filenamePath));

            return filenamePath;
        }
    }
}
