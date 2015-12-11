using System;

namespace ShowPlanParser
{
    public class ShowPlanParameter
    {
        public ShowPlanParameter(string name, string sqlType, int size, byte precision = 0, byte scale = 0)
        {
            Name = name;
            SqlType = sqlType;
            Size = size;
            Precision = precision;
            Scale = scale;

            if (sqlType.Equals("datetime2", StringComparison.InvariantCultureIgnoreCase))
                Size = 7;
        }

        public string Name { get;  }
        public string SqlType { get;  }
        public int Size { get;  }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
    }
}