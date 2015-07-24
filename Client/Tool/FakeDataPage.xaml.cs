namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Xml.Linq;
    using IsolationLevel = System.Transactions.IsolationLevel;

    public partial class FakeDataPage : Page
    {
        private const string Digits = "1234567890";
        private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LettersDigits = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly IReadOnlyList<string> emailLocations = new[] { "@facebook.com", "@yahoo.com", "@google.com", "@baidu.com", "@outlook.com", "@live.com", "@qq.com", "@163.com", "@sohu.com", "@sina.com.cn" };
        private static readonly RoutedUICommand generateCommand = new RoutedUICommand("Generate Fake Data", "Generate", typeof(FakeDataPage));
        private static readonly IReadOnlyList<string> products = new string[] { "裸钻", "女戒", "男戒", "对戒", "吊坠", "项链", "手链", "耳钉", "翡翠", "碧玺", "男戒托", "女戒托" };
        private static readonly IReadOnlyList<byte> methods = new byte[] { 1, 2, 3, 4, 5 };
        private static readonly IReadOnlyList<byte> states = new byte[] { 1 };
        private static readonly IReadOnlyList<string> names = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ChineseNames.txt"));
        private static readonly IReadOnlyList<string> roads = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Roads.txt"));
        private static readonly IReadOnlyList<string> cuts = new[] { "EX", "VG" };
        private static readonly IReadOnlyList<string> clarities = new[] { "FL", "IF", "VVS1", "VVS2", "VS1", "VS2", "SI1", "SI2" };
        private static readonly IReadOnlyList<string> colors = new[] { "D", "E", "F", "G", "H", "I" };
        private static readonly IReadOnlyList<string> descriptions = new[] { "无荧光", "GIA证书", "豪华戒托", "卡迪亚款", "蒂芙尼款", "IGI证书", "EGL证书", "" };

        public FakeDataPage()
        {
            InitializeComponent();
        }

        public static RoutedUICommand GenerateCommand
        {
            get { return generateCommand; }
        }

        private void Generate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await GenerateAsync();

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
            });
        }

        private static async Task GenerateAsync()
        {
            var scope = default(TransactionScope);
            var connection = default(DbConnection);
            try
            {
                scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) }, TransactionScopeAsyncFlowOption.Enabled);
                connection = await SqlHelper.CreateConnectionAsync("DefaultConnection", true);

                DateTimeOffset begin, end;
                GetDateRange(out begin, out end);
                var employees = await RetrieveEmployeesAsync(connection);
                var now = begin;
                do
                {
                    var saleHeaderCount = CalculateSaleHeaderCount(now);
                    var saleLineCount = CalculateTransactionCount(now);
                    for (int pIndex = 0; pIndex < saleHeaderCount; pIndex++)
                    {
                        var dateTime = ChangeTime(now, pIndex);
                        var random = new Random(dateTime.GetHashCode());
                        var employee = employees[random.Next(0, employees.Count - 1)];
                        var id = await CreateSaleHeaderAsync(connection, employee, dateTime);

                        await CreateContactAsync(connection, id, employee, dateTime, 0);
                        await CreateContactAsync(connection, id, employee, dateTime, 1);

                        for (int tIndex = 0; tIndex < saleLineCount; tIndex++)
                        {
                            await CreateSaleLineAsync(connection, id, employee, dateTime, tIndex);
                        }
                    }

                    now = now.AddDays(1);
                } while (now <= end);

                scope.Complete();
            }
            finally
            {
                if (connection != null) connection.Dispose();
                if (scope != null) scope.Dispose();
            }
        }

        private static async Task<IReadOnlyList<string>> RetrieveEmployeesAsync(DbConnection connection)
        {
            var result = new List<string>();

            var reader = default(DbDataReader);
            try
            {
                reader = await connection.ExecuteReaderAsync("SELECT [full_name] FROM tc_employee");
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString(0));
                }
            }
            finally
            {
                if (reader != null) reader.Dispose();
            }

            return result;
        }

        private static async Task<Guid> CreateSaleHeaderAsync(DbConnection connection, string employee, DateTimeOffset date)
        {
            var random = new Random(date.GetHashCode() + employee.GetHashCode());
            int maxDayNumber, maxTotalNumber;
            {
                var raw = await connection.CreateCommand("SELECT MAX(sale_day_number) FROM tc_sale_header WHERE [created] >= CONVERT(DATETIMEOFFSET, @from) AND [created] < CONVERT(DATETIMEOFFSET, @to)")
                    .AddParameter("@from", DbType.AnsiString, 64, date.ToString("d", CultureInfo.InvariantCulture))
                    .AddParameter("@to", DbType.AnsiString, 64, date.AddDays(1).ToString("d", CultureInfo.InvariantCulture))
                    .ExecuteScalarAsync();
                maxDayNumber = Convert.IsDBNull(raw) ? 0 : (int)raw;
            }
            {
                var raw = await connection.CreateCommand("SELECT MAX(sale_total_number) FROM tc_sale_header")
                    .ExecuteScalarAsync();
                maxTotalNumber = Convert.IsDBNull(raw) ? 0 : (int)raw;
            }
            var id = Guid.NewGuid();
            await connection.CreateCommand("INSERT INTO [tc_sale_header]([id],[sale_number],[sale_day_number],[sale_total_number],[sales_person_name],[customer_name],[status],[created],[created_by],[modified],[modified_by])" +
                "VALUES(@id,@number_text,@day_number,@total_number,@sales_person_name,@customer_name,@status,@created,@created_by,@modified,@modified_by)")
                .AddParameter("@id", DbType.Guid, 0, id)
                .AddParameter("@number_text", DbType.AnsiString, 64, GenerateNumberText(date, maxDayNumber + 1, maxTotalNumber + 1))
                .AddParameter("@day_number", DbType.Int32, 0, maxDayNumber + 1)
                .AddParameter("@total_number", DbType.Int32, 0, maxTotalNumber + 1)
                .AddParameter("@sales_person_name", DbType.String, 64, employee)
                .AddParameter("@customer_name", DbType.String, 64, names[random.Next(0, names.Count - 1)])
                .AddParameter("@status", DbType.Byte, 0, states[random.Next(0, states.Count - 1)])
                .AddParameter("@created", DbType.DateTimeOffset, 0, date)
                .AddParameter("@created_by", DbType.String, 64, employee)
                .AddParameter("@modified", DbType.DateTimeOffset, 0, date)
                .AddParameter("@modified_by", DbType.String, 64, employee)
                .ExecuteNonQueryAsync();
            return id;
        }

        private static async Task CreateSaleLineAsync(DbConnection connection, Guid saleHeaderId, string employee, DateTimeOffset date, int index)
        {
            var random = new Random(date.GetHashCode() + saleHeaderId.GetHashCode() + employee.GetHashCode() + index);
            var product = products[random.Next(0, products.Count - 1)];
            await connection.CreateCommand("INSERT INTO [tc_sale_line]([id],[sale_id],[product_name],[product_description],[product_detail],[unit_price],[quantity],[status],[created],[created_by],[modified],[modified_by])" +
                "VALUES(@id,@sale_id,@product_name,@product_description,@product_detail,@unit_price,@quantity,@status,@created,@created_by,@modified,@modified_by)")
                .AddParameter("@id", DbType.Guid, 0, Guid.NewGuid())
                .AddParameter("@sale_id", DbType.Guid, 0, saleHeaderId)
                .AddParameter("@product_name", DbType.String, 64, product)
                .AddParameter("@product_description", DbType.String, 1024, descriptions[random.Next(0, descriptions.Count - 1)])
                .AddParameter("@product_detail", DbType.Xml, 0, GenerateProductDetail(random, product))
                .AddParameter("@unit_price", DbType.Currency, 0, (decimal)Math.Round((random.NextDouble() + .1D) * random.Next(10000, 10000 * 10000)))
                .AddParameter("@quantity", DbType.Int32, 0, random.Next(1, 3))
                .AddParameter("@status", DbType.Byte, 0, states[random.Next(0, states.Count - 1)])
                .AddParameter("@created", DbType.DateTimeOffset, 0, date)
                .AddParameter("@created_by", DbType.String, 64, employee)
                .AddParameter("@modified", DbType.DateTimeOffset, 0, date)
                .AddParameter("@modified_by", DbType.String, 64, employee)
                .ExecuteNonQueryAsync();
        }

        private static async Task CreateContactAsync(DbConnection connection, Guid saleHeaderId, string employee, DateTimeOffset date, int index)
        {
            var random = new Random(date.GetHashCode() + saleHeaderId.GetHashCode() + employee.GetHashCode() + index);
            var method = methods[random.Next(0, methods.Count - 1)];
            await connection.CreateCommand("INSERT INTO [tc_customer_contact]([id],[sale_id],[method],[value],[created],[created_by],[modified],[modified_by])" +
                "VALUES(@id,@sale_id,@method,@value,@created,@created_by,@modified,@modified_by)")
                .AddParameter("@id", DbType.Guid, 0, Guid.NewGuid())
                .AddParameter("@sale_id", DbType.Guid, 0, saleHeaderId)
                .AddParameter("@method", DbType.Byte, 0, method)
                .AddParameter("@value", DbType.String, 1024, GenerateRandomMethodValue(random, method))
                .AddParameter("@status", DbType.Byte, 0, states[random.Next(0, states.Count - 1)])
                .AddParameter("@created", DbType.DateTimeOffset, 0, date)
                .AddParameter("@created_by", DbType.String, 64, employee)
                .AddParameter("@modified", DbType.DateTimeOffset, 0, date)
                .AddParameter("@modified_by", DbType.String, 64, employee)
                .ExecuteNonQueryAsync();
        }

        private static void GetDateRange(out DateTimeOffset begin, out DateTimeOffset end)
        {
            var now = DateTimeOffset.Now;
            var year = now.Year;
            begin = new DateTimeOffset(year - 1, 1, 1, 0, 0, 0, 0, now.Offset);
            end = new DateTimeOffset(year + 1, 12, 31, 0, 0, 0, 0, now.Offset);
        }

        private static int CalculateSaleHeaderCount(DateTimeOffset value)
        {
            var day = value.Day;
            if (day < 10)
            {
                return day;
            }
            var number = (double)day / 10D;
            return (int)((number - Math.Floor(number)) * 10D);
        }

        private static int CalculateTransactionCount(DateTimeOffset value)
        {
            var number = (double)value.Day / 10D;
            return (int)Math.Ceiling(number);
        }

        private static string GenerateNumberText(DateTimeOffset date, int dayNumber, int totalNumber)
        {
            return string.Concat(
                date.Year.ToString("0000", CultureInfo.InvariantCulture),
                date.Month.ToString("00", CultureInfo.InvariantCulture),
                date.Day.ToString("00", CultureInfo.InvariantCulture),
                dayNumber.ToString("00", CultureInfo.InvariantCulture),
                totalNumber.ToString("000000", CultureInfo.InvariantCulture));
        }

        private static DateTimeOffset ChangeTime(DateTimeOffset date, object more)
        {
            var random = new Random(date.GetHashCode() + more.GetHashCode());
            return new DateTimeOffset(date.Year, date.Month, date.Day, random.Next(0, 23), random.Next(0, 59), random.Next(0, 59), random.Next(0, 999), date.Offset);
        }

        private static string GenerateRandomMethodValue(Random random, byte method)
        {
            switch (method)
            {
                // QQ
                case 1: return GenerateDigits(random, 10);
                // Phone
                case 2: return GenerateDigits(random, random.Next() % 2 == 0 ? 11 : 8);
                // Email
                case 3: return string.Format("{0}{1}", GenerateLettersDigits(random, random.Next(8, 12)), emailLocations[random.Next(0, emailLocations.Count - 1)]);
                // WeiXin
                case 4: return GenerateLettersDigits(random, random.Next(8, 12));
                // Other
                case 5: return roads[random.Next(0, roads.Count - 1)];
                default: throw new ArgumentOutOfRangeException("method", method, string.Format("The raw value of contact method '{0}' is invalid.", method));
            }
        }

        private static string GenerateProductDetail(Random random, string product)
        {
            var properties = default(IDictionary<string, object>);
            if (product == products[0])
            {
                properties = new Dictionary<string, object>()
                {
                    { "Certificate", GenerateDigits(random, 16) },
                    { "Caret", Math.Round((random.NextDouble() + .3) * random.Next(1, 10), 2).ToString("0.##", CultureInfo.InvariantCulture) },
                    { "Cut", cuts[random.Next(0, cuts.Count - 1)] },
                    { "Clarity", clarities[random.Next(0, clarities.Count - 1)] },
                    { "Color", colors[random.Next(0, colors.Count - 1)] },
                };
            }
            return ToDynamicPropertiesXml(properties);
        }

        private static string GenerateDigits(Random random, int count)
        {
            var result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(Digits[random.Next(0, Digits.Length - 1)]);
            }
            return result.ToString();
        }

        private static string GenerateLettersDigits(Random random, int count)
        {
            var result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(LettersDigits[random.Next(0, LettersDigits.Length - 1)]);
            }
            return result.ToString();
        }

        private static string ToDynamicPropertiesXml(IDictionary<string, object> properties)
        {
            var xmlDoc = new XDocument(new XElement("properties"));

            if (properties != null)
            {
                foreach (var item in properties)
                {
                    var xmlElementProperty = new XElement("property", item.Value);
                    xmlElementProperty.Add(new XAttribute("name", item.Key));
                    xmlDoc.Root.Add(xmlElementProperty);
                }
            }

            return xmlDoc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
