using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LayerType {
    Water,
    UI,
}

public static class Layers
{
    public static LayerMaskItem Water => layerMaskItems[LayerType.Water.ToString()];
    public static LayerMaskItem UI => layerMaskItems[LayerType.UI.ToString()];

    static Dictionary<string, LayerMaskItem> layerMaskItems = new Dictionary<string, LayerMaskItem>();
    static bool initialized = false;

    public static void Init()
    {
        if (initialized) return;

        foreach(string name in System.Enum.GetNames(typeof(LayerType))) {
            if (System.Enum.IsDefined(typeof(LayerType), name)) {
                layerMaskItems.Add( name, new LayerMaskItem(name) );
            }
        }

        initialized = true;
    }
}

public struct LayerMaskItem {
    public LayerMaskItem(string layerName) {
        _name = layerName;
        _mask = LayerMask.GetMask(layerName);
    }
    string _name;
    int _mask;
    public string name => _name;
    public int mask => _mask;
    public int value => LayerUtils.ToLayer(_mask);
    public bool ContainsLayer(int layer) { return LayerUtils.LayerMaskContainsLayer(_mask, layer); }
    public override string ToString() { return _name + " | " + value + " | " + _mask; }
}

public static class LayerUtils {
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
