using System.Data;
using System.Reflection;

namespace MachineUtilizationApi.Extensions
{
    public static class CommonExtensions
    {

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
