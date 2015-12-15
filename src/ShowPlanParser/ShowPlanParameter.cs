using System;
using System.Data;

namespace ShowPlanParser
{
    public class ShowPlanParameter
    {
        public ShowPlanParameter(string name, SqlDbType sqlType, int size, byte precision, byte scale , object value)
        {
            Name = name;
            SqlType = sqlType;
            Size = size;
            Precision = precision;
            Scale = scale;
            Value = value;

            if (sqlType == SqlDbType.DateTime2)
                Size = 7;
        }

        public string Name { get;  }
        public SqlDbType SqlType { get;  }
        public int Size { get;  }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public object Value { get; set; }
    }
}