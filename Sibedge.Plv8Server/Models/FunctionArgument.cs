namespace Sibedge.Plv8Server.Models
{
    /// <summary> Argument of plv8-function with custom DB-type </summary>
    public class FunctionArgument
    {
        /// <summary> Initializes a new instance of the <see cref="FunctionArgument"/> class. </summary>
        public FunctionArgument()
        {
        }

        /// <summary> Initializes a new instance of the <see cref="FunctionArgument"/> class. </summary>
        /// <param name="value"> Argument value </param>
        /// <param name="sqlType"> Database type </param>
        public FunctionArgument(object value, string sqlType)
        {
            this.SqlType = sqlType;
            this.Value = value;
        }

        /// <summary> Database type </summary>
        public string SqlType { get; set; }

        /// <summary> Argument value </summary>
        public object Value { get; set; }
    }
}
