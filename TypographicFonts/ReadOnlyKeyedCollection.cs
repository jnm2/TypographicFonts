using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace jnm2.TypographicFonts
{
    public class ReadOnlyKeyedCollection<TKey, TItem> : IReadOnlyList<TItem>
    {
        private readonly KeyedCollection<TKey, TItem> source;
        
        public ReadOnlyKeyedCollection(IEnumerable<TItem> initialItems, Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer = null) 
            : this(CreateDelegateCollection(initialItems, keySelector, comparer))
        {
        }
        public ReadOnlyKeyedCollection(KeyedCollection<TKey, TItem> source)
        {
            this.source = source;
        }

        private static DelegateKeyedCollection<TKey, TItem> CreateDelegateCollection(IEnumerable<TItem> initialItems, Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var r = new DelegateKeyedCollection<TKey, TItem>(keySelector, comparer);
            foreach (var item in initialItems)
                r.Add(item);
            return r;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return source.Count; }
        }

        public TItem this[int index]
        {
            get { return source[index]; }
        }

        public bool TryGetValue(TKey key, out TItem item)
        {
            if (!Contains(key))
            {
                item = default(TItem);
                return false;
            }

            item = this[key];
            return true;
        }

        public TItem this[TKey key]
        {
            get { return source[key]; }
        }
        
        public bool Contains(TKey key)
        {
            return source.Contains(key);
        }
        public bool Contains(TItem item)
        {
            return source.Contains(item);
        }

        public int IndexOf(TItem item)
        {
            return source.IndexOf(item);
        }
    }
}