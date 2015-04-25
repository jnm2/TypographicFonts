using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace jnm2.TypographicFonts
{
    public class DelegateKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private readonly Func<TItem, TKey> keySelector;

        public DelegateKeyedCollection(Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer = null) : base(comparer)
        {
            this.keySelector = keySelector;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return keySelector.Invoke(item);
        }
    }
}