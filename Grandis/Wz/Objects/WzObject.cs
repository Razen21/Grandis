using Grandis.Wz.Utility.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Objects
{
    public abstract class WzObject : IEnumerable<WzObject>
    {
        private readonly bool _canContainChildren;
        private readonly ChildrenCollection? _children;

        internal WzObject(string name, WzObject parent, WzFile parentFile, bool canContainChildren, WzObjectType type)
        {
            Name = name;
            Parent = parent;
            ParentFile = parentFile;
            _canContainChildren = canContainChildren;

            if (_canContainChildren)
                _children = new ChildrenCollection();
            
                

            Type = type;
        }

        public string Name { get; }
        public string Path { get; }
        public WzObject Parent { get; }
        public WzFile ParentFile { get; }
        public WzObjectType Type { get; }

        public virtual int ChildCount 
            => _canContainChildren ? _children!.Count : 0;

        public virtual bool HasChild(string name)
            => _canContainChildren && _children!.Contains(name);
        

        public virtual WzObject this[string childName]
        {
            get
            {
                if (!_canContainChildren)
                {
                    throw new NotSupportedException("This WzObject cannot contain children.");
                }
                return _children![childName];
            }
        }

        internal void Add(WzObject o)
        {
            _children?.Add(o);
        }


        public virtual IEnumerator<WzObject> GetEnumerator()
        {
            return _canContainChildren
                ? _children!.GetEnumerator()
                : Enumerable.Empty<WzObject>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ChildrenCollection : KeyedCollection<string, WzObject>
        {
            internal ChildrenCollection() : base(null, 4) { }

            protected override string GetKeyForItem(WzObject item)
            {
                return item.Name;
            }
        }

    }
}
