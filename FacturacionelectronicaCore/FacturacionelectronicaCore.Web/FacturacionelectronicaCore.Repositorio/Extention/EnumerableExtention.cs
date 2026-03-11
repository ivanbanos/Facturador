using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace EstacionesSevicio.Respositorio.Extention
{
    public static class EnumerableExtention
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                
                // Handle nullable types by getting the underlying type
                Type propertyType = prop.PropertyType;
                Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                
                // If it's a nullable type, use the underlying type for the column
                Type columnType = underlyingType ?? propertyType;
                
                // Add the column with AllowDBNull set to true for nullable types
                DataColumn column = new DataColumn(prop.Name, columnType);
                if (underlyingType != null)
                {
                    column.AllowDBNull = true;
                }
                
                table.Columns.Add(column);
            }

            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    object value = props[i].GetValue(item);
                    // Convert null values to DBNull for DataTable
                    values[i] = value ?? DBNull.Value;
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
