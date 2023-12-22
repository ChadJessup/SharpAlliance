using System.Diagnostics;

namespace SharpAlliance.Core
{
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class BufferedValue<T> where T : struct
    {
        public T Value
        {
            get => this.Current.Value;
            set
            {
                this.Back.Value = value;
                this.Back = Interlocked.Exchange(ref this.Current, this.Back);
            }
        }

        private ValueHolder Current = new ValueHolder();
        private ValueHolder Back = new ValueHolder();

        public static implicit operator T(BufferedValue<T> bv) => bv.Value;

        private string DebuggerDisplayString => $"{this.Current.Value}";

        private class ValueHolder
        {
            public T Value;
        }
    }
}
