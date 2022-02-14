using System;
using UnityEngine;

namespace Core
{
  
  /// <summary>
  /// Serializable wrapper for System.Guid.
  /// Can be implicitly converted to/from System.Guid.
  ///
  /// Author: Searous
  /// </summary>
  [Serializable]
  public struct UUID : ISerializationCallbackReceiver {
      private Guid guid;
      [SerializeField] private string serializedGuid;
  
      public UUID(Guid guid) {
          this.guid = guid;
          serializedGuid = null;
      }
  
      public override bool Equals(object obj) {
          return obj is UUID guid &&
                  this.guid.Equals(guid.guid);
      }
  
      public override int GetHashCode() {
          return -1324198676 + guid.GetHashCode();
      }
  
      public void OnAfterDeserialize() {
          try {
              guid = Guid.Parse(serializedGuid);
          } catch {
              guid = Guid.Empty;
              Debug.LogWarning($"Attempted to parse invalid GUID string '{serializedGuid}'. GUID will set to System.Guid.Empty");
          }
      }
  
      public void OnBeforeSerialize() {
          serializedGuid = guid.ToString();
      }
  
      public override string ToString() => guid.ToString();
  
      public static bool operator ==(UUID a, UUID b) => a.guid == b.guid;
      public static bool operator !=(UUID a, UUID b) => a.guid != b.guid;
      public static implicit operator UUID(Guid guid) => new UUID(guid);
      public static implicit operator Guid(UUID serializable) => serializable.guid;
      public static implicit operator UUID(string serializedGuid) => new UUID(Guid.Parse(serializedGuid));
      public static implicit operator string(UUID serializedGuid) => serializedGuid.ToString();
  }
}
