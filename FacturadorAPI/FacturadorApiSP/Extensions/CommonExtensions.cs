using System.Data;
using System.Reflection;
using System.Text;

namespace MachineUtilizationApi.Extensions
{
    public static class CommonExtensions
    {


        public static string getLienaTarifas(string v1, string v2, string v3, string v4, bool after = false)
        {
            var spacesInPage = 40 / 4;
            var tabs = new StringBuilder();
            if (true)
            {
                tabs.Append(v1.Substring(0, v1.Length < 12 ? v1.Length : 12));
                var whitespaces = 12 - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));

                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));

                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));
                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
            else
            {
                tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
                var whitespaces = spacesInPage - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));

                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));

                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));
                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
        }

        public static string Centrar(this string text)
        {
                var whitespaces = (40 - text.Length) / 2;
                var tabs = new StringBuilder();
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);
                return tabs.ToString() + text;
            
        }
        public static string formatoTotales(this string linea, string extra)
        {
            var result = linea;
            var tabs = new StringBuilder();
            tabs.Append(linea);
            var whitespaces = 49 - linea.Length - extra.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(extra);
            return tabs.ToString();
        }
        public static DataTable ConvertToDataTable<T>(T[] array)
        {
            PropertyInfo[] properties = array.GetType().GetElementType().GetProperties();
            DataTable dt = CreateDataTable(properties);
            if (array.Length != 0)
            {
                foreach (object o in array)
                    FillData(properties, dt, o);
            }
            return dt;
        }

        private static DataTable CreateDataTable(PropertyInfo[] properties)

        {
            DataTable dt = new DataTable();
            DataColumn dc = null;
            foreach (PropertyInfo pi in properties)
            {
                dc = new DataColumn();
                dc.ColumnName = pi.Name;
                if(pi.PropertyType.Name == "States")
                {
                    dc.DataType = typeof(System.Int32);
                }
                else
                {
                    dc.DataType = Nullable.GetUnderlyingType(
            pi.PropertyType) ?? pi.PropertyType;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        private static void FillData(PropertyInfo[] properties, DataTable dt, Object o)
        {
            DataRow dr = dt.NewRow();
            foreach (PropertyInfo pi in properties)
            {
                if (pi.Name == "States")
                {

                    dr[pi.Name] = Int32.Parse(pi.GetValue(o, null).ToString());
                }
                else
                {
                    dr[pi.Name] = pi.GetValue(o, null);
                }
            }
            dt.Rows.Add(dr);
        }

        public static DataTable ToStringDataTable(this IEnumerable<string> elements)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Value", typeof(string))
            {
                AllowDBNull = false
            });
            foreach (var value in elements)
            {
                var row = dataTable.NewRow();
                row["Value"] = value;
                dataTable.Rows.Add(row);
            }


            return dataTable;
        }

        public static DataTable ToLongDataTable(this IEnumerable<long> elements)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Value", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var value in elements)
            {
                var row = dataTable.NewRow();
                row["Value"] = value;
                dataTable.Rows.Add(row);
            }


            return dataTable;
        }

        public static DataTable ToUuidDataTable(this IEnumerable<Guid> elements)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Value", typeof(Guid))
            {
                AllowDBNull = false
            });
            foreach (var value in elements)
            {
                var row = dataTable.NewRow();
                row["Value"] = value;
                dataTable.Rows.Add(row);
            }


            return dataTable;
        }

        public static DataTable ToIntDataTable(this IEnumerable<int> elements)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Value", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var value in elements)
            {
                var row = dataTable.NewRow();
                row["Value"] = value;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public static DataTable ToNullableIntDataTable(this IEnumerable<int?> elements)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Value", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var value in elements)
            {
                var row = dataTable.NewRow();
                row["Value"] = value;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public static IEnumerable<T> ConvertToList<T>(this DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName.ToLower())
                .ToList();

            var properties = typeof(T).GetProperties();

            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();

                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        if (Type.GetTypeCode(row[pro.Name].GetType()) == TypeCode.Single
                        || Type.GetTypeCode(row[pro.Name].GetType()) == TypeCode.Double
                        || Type.GetTypeCode(row[pro.Name].GetType()) == TypeCode.Decimal)
                        {
                            pro.SetValue(objT, Convert.ToSingle(row[pro.Name]));
                        }
                        else
                        {
                            if (row[pro.Name] is System.DBNull)
                            {

                                pro.SetValue(objT, null);
                            }
                            else
                            {
                                pro.SetValue(objT, row[pro.Name]);
                            }
                        }
                    }
                }

                return objT;
            });
        }

        public static string Hash(this string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyHash(this string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
