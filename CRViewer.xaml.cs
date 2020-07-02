using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CHEORptAnalyzer
{
    /// <summary>
    /// Interaction logic for CRViewer.xaml
    /// </summary>
    public partial class CRViewer : Window
    {
        public CRViewer()
        {
            InitializeComponent();

            this.Loaded += (s, e) => { crvReport.Owner = Window.GetWindow(this); };
        }
        
        public void LoadReport(string reportPath)
        {
            //System.Windows.Window window = new System.Windows.Window();
            //crvReport.Owner = window;

            //System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
            //helper.Owner = new System.Windows.Interop.WindowInteropHelper(this).Owner;

            ReportDocument reportDocument = new ReportDocument();

            try
            {
                reportDocument.Load(reportPath);
                crvReport.ViewerCore.ReportSource = reportDocument;
            }
            catch (Exception ex)
            {
                //Logs.Instance.log.Error(ex.Message, ex);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }


        
    }
}
