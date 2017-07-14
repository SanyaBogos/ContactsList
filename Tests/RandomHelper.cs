using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsListTest
{
    public static class RandomHelper
    {
        private static Random _random;

        static RandomHelper()
        {
            _random = new Random(DateTime.Now.Millisecond);
        }

        public static string String()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 25);
        }

        public static bool[] GenerateBoolArray(int length)
        {
            //var random = new Random(DateTime.Now.Millisecond);
            var boolArray = new List<bool>();
            for (int i = 0; i < length; i++)
                boolArray.Add(_random.Next() % 2 == 0);
            return boolArray.ToArray();
        }

        public static char RandomCharForMode()
        {
            //var random = new Random(DateTime.Now.Millisecond);
            return _random.NextDouble() > 0.5 ? 's' : 'j';
        }
    }
}
