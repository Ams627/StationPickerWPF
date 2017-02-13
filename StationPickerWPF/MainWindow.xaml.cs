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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StationPickerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Typing.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyboardEventHandler(
            //    (object s, KeyboardEventArgs e) => 
            //    {
            //        if (e.
            //        {

            //        } }
            //    ));
            InitializeComponent();
        }

        private void Typing_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Typing_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Typing_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All(x=>char.IsLetter(x)))
            {
                e.Handled = true;
            }
        }
    }
}
