namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public static class ApplicationCommands
    {
        private static readonly IEnumerable<RoutedCommand> commands = new RoutedCommand[]
        {
            new ApplicationCommand("Generate Fake Data", "FakeData", typeof(FakeDataPage)),
            new ApplicationCommand("Generate Date Dimension Data", "DateDimensionData", typeof(DateDimensionDataPage)),
        };

        public static ICommand FakeData
        {
            get { return commands.Single(c => c.Name == "FakeData"); }
        }

        public static ICommand DateDimensionData
        {
            get { return commands.Single(c => c.Name == "DateDimensionData"); }
        }
    }
}
