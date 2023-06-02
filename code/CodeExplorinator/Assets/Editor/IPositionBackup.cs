using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IPositionBackup
{
    /// <summary>
    /// Saves the current position internally. Call RestorePositionBackup to restore the position.
    /// </summary>
    public void MakePositionBackup();
    /// <summary>
    /// Restores the position which was last saved by MakePositionBackup
    /// </summary>
    public void RestorePositionBackup();
}
