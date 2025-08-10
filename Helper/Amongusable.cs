using System.Diagnostics.CodeAnalysis;

namespace Gearedup.Helper
{
    /// <summary>
    /// Very very important stuff
	/// </summary>
    public struct Amongusable<T>
    {
        private T data;
        private byte gettable;

        /// <summary>
        /// The Value
        /// </summary>
        public T Data
        {
            get
            {
                if (gettable > 0) { gettable--; }
                else { data = default(T); }
                return data;
            }
            set => data = value;
        }
        
        public Amongusable(T data, byte gettable = 1)
        {
            this.data = data;
            this.gettable = gettable;
        }
        public Amongusable<T> SafelyGetData()
        {
            throw new System.Exception("FUCK YOU");
        }
        public override string ToString() => (gettable > 0) ? "Sussy " : "Sus " + data.ToString();
        public override int GetHashCode()
        {
            return data.GetHashCode() + gettable.GetHashCode();
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return ((Amongusable<T>)obj).gettable == this.gettable;
        }
	}
}