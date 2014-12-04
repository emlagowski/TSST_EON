using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ExtSrc
{
    public class BindingDictionary<TKey, TValue> : BindingList<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> dic;

        public Dictionary<TKey, TValue>.ValueCollection Values
        { get { return dic.Values; } }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        { get { return dic.Keys; } }

        public BindingDictionary()
        {
            dic = new Dictionary<TKey, TValue>();
        }

        public BindingDictionary(IEqualityComparer<TKey> comparer)
        {
            dic = new Dictionary<TKey, TValue>(comparer);
        } 
        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        } 

        public new void Add(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                dic.Add(item.Key, item.Value);
                base.Add(item);
            }
            catch
            {
                //Console.WriteLine("The key is duplicated!");
            }
        }

        protected override void RemoveItem(int index)
        {
            KeyValuePair<TKey, TValue> item = base[index];
            try
            {
                dic.Remove(item.Key);
            }
            catch
            {
                //Console.WriteLine("The key doesn't exit!");
            }
            base.RemoveItem(index);
        }

        public bool TryGetValue(TKey key ,out TValue value)
        {
            return dic.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            bool result;
            try
            {
                result = dic.Remove(key);
                var index = (from c in base.Items where c.Key.Equals(key) select base.IndexOf(c)).First();
                base.RemoveAt(index);
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
