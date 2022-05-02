// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// public class MyHashSetComparer : IEqualityComparer<HashSet<string>> {
//     public bool Equals(HashSet<string> x, HashSet<string> y)
//        => x?.SetEquals(y) ?? false;

//     public int GetHashCode(HashSet<string> obj) {
//         unchecked {
//             return obj.Aggregate(17, (current, item) => current * 31 + item.GetHashCode());
//         }
//     }
// }