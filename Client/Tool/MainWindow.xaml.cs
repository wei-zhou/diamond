namespace Home
{
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

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var command = (ApplicationCommand)e.Command;
            this.Frame.Navigate(Activator.CreateInstance(command.PageType));
        }
    }
}
