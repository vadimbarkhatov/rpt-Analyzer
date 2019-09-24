using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHEORptAnalyzer
{
    public class ListTuple<T>
    {
        public string Text;
        public T Obj;
        public List<string> SearchResults = new List<string>();

        public override string ToString()
        {
            return Text;
        }
    }
}
