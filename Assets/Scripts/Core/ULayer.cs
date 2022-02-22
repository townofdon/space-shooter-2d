using System.Collections.Generic;
using UnityEngine;

namespace Core {

    /**
    * LAYER UTIL - ONE SCRIPT TO HOUSE EVERY LAYER MASK, ETC.
    *
    * USAGE:
    *
    * - Add all of your layers to the ULayerType enum
    * - Update static getters to include all of your layers
    *
    * ```
    * ULayer.init(); // initialize the ULayer in Awake() or Start()
    * ULayer.Water.mask(); // get mask for Water layer
    * ULayer.Ground.value(); // get layer number for Ground layer
    * ```
    *
    * PROS:
    *
    * - one-time setup
    * - update single file when adding/updating layers
    * - warns if a layer does not exist
    * - allows for intellisense autosuggest for layers (e.g. start typing "Layer.Gr" to get the ground layer)
    *
    * CONS:
    *
    * - must initialize upon app start
    */

    public enum ULayerType {
        Default,
        UI,
        FX,
        Player,
        Explosions,
        Projectiles,
    }

    public static class ULayer
    {
        public static ULayerMaskItem Default => layerMaskItems[ULayerType.Default.ToString()];
        public static ULayerMaskItem UI => layerMaskItems[ULayerType.UI.ToString()];
        public static ULayerMaskItem FX => layerMaskItems[ULayerType.FX.ToString()];
        public static ULayerMaskItem Player => layerMaskItems[ULayerType.Player.ToString()];
        public static ULayerMaskItem Explosions => layerMaskItems[ULayerType.Explosions.ToString()];
        public static ULayerMaskItem Projectiles => layerMaskItems[ULayerType.Projectiles.ToString()];

        static Dictionary<string, ULayerMaskItem> layerMaskItems = new Dictionary<string, ULayerMaskItem>();
        static bool initialized = false;

        /// <summary>Initialize layers (call in Awake or Start)</summary>
        public static void Init()
        {
            if (initialized) return;

            foreach(string name in System.Enum.GetNames(typeof(ULayerType))) {
                layerMaskItems.Add(name, new ULayerMaskItem(name) );
            }

            initialized = true;
        }
    }

    public struct ULayerMaskItem {
        public ULayerMaskItem(string layerName) {
            _name = layerName;
            _mask = LayerMask.GetMask(layerName);
            if (_mask == 0 && layerName != "Default") Debug.LogWarning("Warning: layer \"" + layerName + "\" may not exist");
        }
        string _name;
        int _mask;
        public string name => _name;
        public int mask => _mask;
        public int value => ULayerUtils.ToLayer(_mask);
        public bool ContainsLayer(int layer) { return ULayerUtils.LayerMaskContainsLayer(_mask, layer); }
        public override string ToString() { return _name + " | " + value + " | " + _mask; }
    }

    public static class ULayerUtils {
        // check to see whether a LayerMask contains a layer
        // see: https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html
        public static bool LayerMaskContainsLayer(int mask, int layer) {
            bool contains = ((mask & (1 << layer)) != 0);
            return contains;
        }

        // get the layer num from a layermask
        // see: https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-3021162
        public static int ToLayer(int layerMask) {
            int result = layerMask > 0 ? 0 : 31;
            while( layerMask > 1 ) {
                layerMask = layerMask >> 1;
                result++;
            }
            return result;
        }
    }
}

