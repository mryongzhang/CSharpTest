using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices.Automation;

namespace SlCallOffice
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic word = AutomationFactory.CreateObject("Word.Application");
            word.Visible = true;
            
            dynamic doc = word.Documents.Add();
            string insertText = "Hello Word from Silverlight 4!";
            dynamic range = doc.Range(0, 0);
            range.Text = insertText;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            dynamic excel = AutomationFactory.CreateObject("Excel.Application");
            excel.Visible = true;

            dynamic workbook = excel.workbooks;
            workbook.Add();
            dynamic sheet = excel.ActiveSheet;

            dynamic range;

            range = sheet.Range("A1");
            range.Value = "Hello from Silverlight";
            range = sheet.Range("A2");
            range.Value = "100";
            range = sheet.Range("A3");
            range.Value = "50";
            range = sheet.Range("A4");
            range.Formula = "=@Sum(A2..A3)";
            range.Calculate();
        }
    }
}
