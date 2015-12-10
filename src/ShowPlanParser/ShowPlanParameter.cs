namespace ShowPlanParser
{
    public class ShowPlanParameter
    {
        public ShowPlanParameter(string name, string sqlType, int size)
        {
            Name = name;
            SqlType = sqlType;
            Size = size;
        }

        public string Name { get;  }
        public string SqlType { get;  }
        public int Size { get;  }
    }
}