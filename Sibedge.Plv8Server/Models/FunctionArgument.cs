namespace Sibedge.Plv8Server.Models
{
    public class FunctionArgument
    {
        public FunctionArgument()
        {
        }

        public FunctionArgument(object value, string sqlType)
        {
            this.SqlType = sqlType;
            this.Value = value;
        }

        public string SqlType { get; set; }

        public object Value { get; set; }
    }
}
