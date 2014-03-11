using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace AsyncWpfSample {
    delegate long calcDelegate();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            ResultText.Content = "Result: ";
            ((Button)sender).IsEnabled = false;

            long result = await LongRunCalcAsync(40);
            ResultText.Content += result.ToString();

            ((Button)sender).IsEnabled = true;
        }

        static async Task<long> LongRunCalcAsync(long num) {
            return await Task.Run(() => {
                return LongRun.Calculate(num);
            });
        }
    }
}
