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
using System.Windows.Browser;

namespace SLandJS
{
    [ScriptableType()]
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            HtmlPage.RegisterScriptableObject("page", this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ScriptObject scriptObject = (ScriptObject)HtmlPage.Window.GetProperty("createText");
            scriptObject.InvokeSelf("Jerry", "Hello World");
        }

        [ScriptableMember]
        public void ChangeText(string newText)
        {
            txbTest.Text = "It's invoking by Javascript " + newText;
        }
    }
}
