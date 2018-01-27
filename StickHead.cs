using System.Collections.Generic;
using UnityEngine.UI;
//using UnityEditor;
using UnityEngine;
using System;


namespace VirtualStick{
    public class StickHead : MonoBehaviour {
        [SerializeField] private RectTransform StickHeadImage;
        [SerializeField] private Text DebugText;
        private float StickRangeRadius;

        private IVirtualStick stick;         // インターフェイスを型にすると、public や [SerializeField]などでもインスペクタに表示することはできない。

        [SerializeField] private bool ShowsStickHead = true;
        [SerializeField] private bool ShowsDebugText = true;


        private void Start() {
            stick = GetComponent<IVirtualStick>();
            var rect = GetComponent<RectTransform>();
            StickRangeRadius = rect.sizeDelta.x / 2;
        }


        private void Update() {
            if (StickHeadImage == null) { return; }
            if (DebugText == null) { return; }
            if (StickRangeRadius < 0.1f) { return; }

            if (ShowsStickHead) {
                StickHeadImage.anchoredPosition = stick.StickInput * StickRangeRadius;
            }

            if (ShowsDebugText) {
                DebugText.text = stick.StickInput.ToString();
            }
        }
    }
}