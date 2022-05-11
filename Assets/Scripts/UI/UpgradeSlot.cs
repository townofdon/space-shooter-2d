using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    class UpgradeSlot {
        List<Image> fills = new List<Image>();
        List<Image> backgrounds = new List<Image>();
        int curUpgradeLevel = 0;
        int maxUpgradeLevel = 0;
        Color activeColorBG;
        Color activeColor;
        Color inactiveColorBG;
        Color inactiveColor;
        Color hiddenColor;

        public UpgradeSlot(GameObject upgradeSlotsGO, int initialLevel = 0, int maxLevel = 0) {
            foreach (Transform child in upgradeSlotsGO.transform) {
                Image bg = child.transform.GetChild(0).GetComponent<Image>();
                Image fill = child.transform.GetChild(1).GetComponent<Image>();
                backgrounds.Add(bg);
                fills.Add(fill);
            }
            curUpgradeLevel = initialLevel;
            maxUpgradeLevel = maxLevel;
            UpdateGUI();
        }

        public void SetUpgradeLevel(int level) {
            curUpgradeLevel = level;
            UpdateGUI();
        }

        public void SetMaxUpgrades(int max) {
            maxUpgradeLevel = max;
            UpdateGUI();
        }

        public void SetActiveColorBG(Color color) {
            activeColorBG = color;
            UpdateGUI();
        }
        public void SetActiveColor(Color color) {
            activeColor = color;
            UpdateGUI();
        }
        public void SetInactiveColorBG(Color color) {
            inactiveColorBG = color;
            UpdateGUI();
        }
        public void SetInactiveColor(Color color) {
            inactiveColor = color;
            UpdateGUI();
        }
        public void SetHiddenColor(Color color) {
            hiddenColor = color;
            UpdateGUI();
        }

        void UpdateGUI() {
            for (int i = 0; i < fills.Count; i++) {
                if (i + 1 > maxUpgradeLevel) {
                    SetHidden(fills[i]);
                    SetHidden(backgrounds[i]);
                    continue;
                }

                if (i + 1 > curUpgradeLevel) {
                    SetInactive(fills[i]);
                    SetInactive(backgrounds[i], true);
                    continue;
                }

                SetActive(fills[i]);
                SetActive(backgrounds[i], true);
            }
        }

        void SetActive(Image img, bool isBG = false) {
            img.color = isBG ? activeColorBG : activeColor;
        }

        void SetInactive(Image img, bool isBG = false) {
            img.color = isBG ? inactiveColorBG : inactiveColor;
        }

        void SetHidden(Image img) {
            img.color = hiddenColor;
        }
    }
}
