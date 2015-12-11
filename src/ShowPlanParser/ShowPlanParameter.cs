using System;

namespace ShowPlanParser
{
    public class ShowPlanParameter
    {
        public ShowPlanParameter(string name, string sqlType, int size)
        {
            Name = name;
            SqlType = sqlType;
            Size = size;

            if (sqlType.Equals("datetime2", StringComparison.InvariantCultureIgnoreCase))
                Size = 7;
        }

        public string Name { get;  }
        public string SqlType { get;  }
        public int Size { get;  }
    }
}