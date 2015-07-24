namespace Home
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using HomeResources = Home.Properties.Resources;
    using IsolationLevel = System.Transactions.IsolationLevel;

    public partial class DateDimensionDataPage : Page
    {
        private static readonly RoutedUICommand generateCommand = new RoutedUICommand("Generate Date Dimension Data", "Generate", typeof(DateDimensionDataPage));

        public DateDimensionDataPage()
        {
            InitializeComponent();
        }

        public static RoutedUICommand GenerateCommand
        {
            get { return generateCommand; }
        }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        private void Generate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Task.Factory.StartNew(async state =>
            {
                try
                {
                    dynamic value = state;
                    await GenerateAsync(value.StartDate, value.EndDate);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Generate fake data completed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);
                    }));
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show(Application.Current.MainWindow, string.Format("{1}{0}{2}", Environment.NewLine, ex.Message, ex.StackTrace), "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
                    }));
                }
            },
            new
            {
                StartDate = this.StartDate.Value,
                EndDate = this.EndDate.Value,
            });
        }

        private static async Task GenerateAsync(DateTime startDate, DateTime endDate)
        {
            var days = new[]
            {
                HomeResources.Sunday,
                HomeResources.Monday,
                HomeResources.Tuesday,
                HomeResources.Wednesday,
                HomeResources.Thursday,
                HomeResources.Friday,
                HomeResources.Saturday
            };

            var scope = default(TransactionScope);
            var connection = default(DbConnection);
            try
            {
                scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) }, TransactionScopeAsyncFlowOption.Enabled);
                connection = await SqlHelper.CreateConnectionAsync("DefaultConnection", true);

                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    int year = date.Year,
                        month = date.Month,
                        dayOfYear = date.DayOfYear,
                        dayOfMonth = date.Day,
                        dayOfWeek = (int)date.DayOfWeek + 1;

                    await connection.CreateCommand(
                        "INSERT INTO [tcb_DimDate]([date],[date_name],[year],[year_name],[month],[month_name],[day_of_year],[day_of_year_name],[day_of_month],[day_of_month_name],[month_of_year],[month_of_year_name])" +
                        "VALUES(@date,@date_name,@year,@year_name,@month,@month_name,@day_of_year,@day_of_year_name,@day_of_month,@day_of_month_name,@month_of_year,@month_of_year_name)")
                        .AddParameter("@date", DbType.DateTime, 0, date)
                        .AddParameter("@date_name", DbType.String, 1024, string.Format(HomeResources.DateName, year, month.ToString("00"), dayOfMonth.ToString("00"), days[dayOfWeek - 1]))
                        .AddParameter("@year", DbType.DateTime, 0, date)
                        .AddParameter("@year_name", DbType.String, 1024, string.Format(HomeResources.YearName, year))
                        .AddParameter("@month", DbType.DateTime, 0, date)
                        .AddParameter("@month_name", DbType.String, 1024, string.Format(HomeResources.MonthYear, year, month))
                        .AddParameter("@day_of_year", DbType.Int32, 0, dayOfYear)
                        .AddParameter("@day_of_year_name", DbType.String, 1024, string.Format(HomeResources.DayOfYearName, dayOfYear))
                        .AddParameter("@day_of_month", DbType.Int32, 0, dayOfMonth)
                        .AddParameter("@day_of_month_name", DbType.String, 1024, string.Format(HomeResources.DayOfMonthName, dayOfMonth))
                        .AddParameter("@month_of_year", DbType.Int32, 0, month)
                        .AddParameter("@month_of_year_name", DbType.String, 1024, string.Format(HomeResources.MonthOfYearName, month))
                        .ExecuteNonQueryAsync();
                }

                scope.Complete();
            }
            finally
            {
                if (connection != null) connection.Dispose();
                if (scope != null) scope.Dispose();
            }
        }
    }
}
