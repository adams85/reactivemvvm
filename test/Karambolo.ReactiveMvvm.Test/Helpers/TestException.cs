using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    public class TestException : Exception, IEquatable<TestException>
    {
        public TestException() { }

        public TestException(string message) : base(message) { }

        public bool Equals(TestException other)
        {
            return other != null ? string.Equals(Message, other.Message) : false;
        }

        public override bool Equals(object obj)
        {
            return obj is TestException other ? Equals(other) : false;
        }

        public override int GetHashCode()
        {
            return Message?.GetHashCode() ?? 0;
        }
    }
}
