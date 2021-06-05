//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DungeonArchitect.Utils
{
    [StructLayout(LayoutKind.Explicit), Serializable]
    public struct DungeonUID : IComparable, 
        IComparable<System.Guid>,
        IEquatable<System.Guid>,
        IComparable<DungeonUID>,
        IEquatable<DungeonUID>
    {
        [FieldOffset(0)] 
        public System.Guid Guid;

        [FieldOffset(0), SerializeField] private Int32 A;
        [FieldOffset(4), SerializeField] private Int32 B;
        [FieldOffset(8), SerializeField] private Int32 C;
        [FieldOffset(12), SerializeField] private Int32 D;

        public static DungeonUID NewUID()
        {
            return new DungeonUID()
            {
                Guid = System.Guid.NewGuid()
            };
        }

        public static readonly DungeonUID Empty = new DungeonUID()
        {
            Guid = System.Guid.Empty
        };

        public static implicit operator System.Guid(DungeonUID uid)
        {
            return uid.Guid;
        }

        public static bool operator==(DungeonUID a, DungeonUID b)
        {
            return a.Guid == b.Guid;
        }
        
        public static bool operator!=(DungeonUID a, DungeonUID b)
        {
            return a.Guid != b.Guid;
        }

        public bool IsValid()
        {
            return Guid != System.Guid.Empty;
        }
        
        public int CompareTo(object obj)
        {
            if (obj == null) return -1;
            if (obj is DungeonUID)
            {
                return ((DungeonUID) obj).Guid.CompareTo(Guid);
            }

            if (obj is System.Guid)
            {
                return ((System.Guid) obj).CompareTo(Guid);
            }

            return -1;
        }

        public int CompareTo(System.Guid other)
        {
            return Guid.CompareTo(other);
        }

        public int CompareTo(DungeonUID other)
        {
            return other.Guid.CompareTo(Guid);
        }

        public bool Equals(System.Guid other)
        {
            return Guid.Equals(other);
        }

        public bool Equals(DungeonUID other)
        {
            return other.Guid.Equals(Guid);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is DungeonUID)
            {
                return ((DungeonUID) obj).Guid.Equals(Guid);
            }

            if (obj is System.Guid)
            {
                return ((System.Guid) obj).Equals(Guid);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public override string ToString()
        {
            return Guid.ToString();
        }
    }
}