using System.Collections.Generic;
using UnityEngine;

public interface IDoor {
    // public bool IsLocked();
    public void ActivateDoorknob(Vector3 position, Transform activator, List<DoorLock> doorLocks, HashSet<int> withKeySet = null, bool bypassKeyCheck = false, bool openOnly = false);
    // public void PickJiggleKnob(DoorLock doorlock);
    // public List<DoorLock> getDoorLocks();
}