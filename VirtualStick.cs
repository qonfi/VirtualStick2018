using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.EventSystems;


namespace VirtualStick{
    public interface IVirtualStick {
        /// <summary>
        /// VirtualStick の入力量。x,y 方向それぞれで -1~1の値をとる。
        /// </summary>
        Vector2 StickInput { get; }
    }
    
    /// <summary>
    /// バーチャルスティック(パッド)を実現するコンポーネント。Image などに取り付けて使用する。デルタ値を使用する設定にするとタッチパッドやマウス方式、使用しない設定だとスティック方式の入力になる。
    /// </summary>
    public class VirtualStick : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IVirtualStick {
        public Vector2 StickInput { get; private set; }

        /// <summary>
        ///  デルタ値を使うと、前フレームでの座標と現在地の距離が入力となる(タッチパッド、マウス的)。使わない場合は、タッチ開始点と現在地の距離が入力となる(ジョイスティック的)。
        /// </summary>
        [SerializeField] private bool UsesDeltaInput;
        /// <summary>
        /// ドラッグ開始点とドラッグ先との最大距離。
        /// </summary>
        public float MaxStickMovement { get; private set; }
        /// <summary>
        /// 1フレーム内での最大移動量。
        /// </summary>
        public float MaxStickDelta { get; private set; }

        /// <summary>
        /// 特定領域から始まったドラッグのID。フレームをまたいでも同一のタッチを示す。
        /// 該当するものがない場合は初期値としてnullを入れる。
        /// </summary>
        private int? draggingFingerID = null;
        private Vector2 beginDragPosition;


        private void Start() {
            MaxStickMovement = 200f;    // テスト用
            MaxStickDelta = 10f;            // テスト用
        }


        // このオブジェクトがドラッグされたとき、
        // ドラッグを開始した指の id (フレームをまたいでも多分変わらない方) を記憶しておく。
        public void OnBeginDrag(PointerEventData eventData) {
            if (draggingFingerID != null) { return; } // すでに入力が行われているときは新しく始めない
            if (Input.touchCount < 1) { return; }
            draggingFingerID = eventData.pointerId;
            Touch touch = Input.GetTouch(draggingFingerID.Value);
            beginDragPosition = touch.position;
        }


        // OnBeginDrag で特定した fingerID の指を追跡する。タッチ開始地点からドラッグ先までの距離が入力となる。
        private void Update() {
            if (Input.touchCount < 1) {
                ResetInput();
                return;
            }

            Touch draggingTouch = Input.GetTouch(draggingFingerID.Value);

            if (UsesDeltaInput) {
                CalcDeltaMovement(draggingTouch);
            } else {
                CalcDragDistance(draggingTouch, beginDragPosition);
            }
        }


        private void ResetInput() {
            StickInput = Vector2.zero;
        }
        


        /// <summary>
        /// 1フレーム内でのドラッグ距離を入力とする。開始点は入力量に関係なく、指が止まれば入力はゼロになる。マウスやタッチパネルのスクロール的な入力。
        /// </summary>
        /// <param name="touch"></param>
        private void CalcDeltaMovement(Touch touch) {
            Vector2 delta = touch.deltaPosition;                                                             // 最新Updateと 前フレームUpdate でのドラッグ距離(解像度に依存?)。
            float compressedMagnitude = Mathf.Min(delta.magnitude, MaxStickDelta);    // ドラッグ距離を制限する。
            Vector2 compressedDelta = delta.normalized * compressedMagnitude;      // 制限した距離をもとの方向に再び適用する。
            Vector2 result = compressedDelta / MaxStickDelta;                                 // 結果 / 最大距離   で最大距離に対する入力の割合を計算する。
            StickInput = result;
        }


        /// <summary>
        /// ドラッグ開始点から終了点までの距離を入力とする。指が止まっていても、開始点から離れていればその分入力が行われていることになる。コントローラのスティック的な入力。
        /// </summary>
        /// <param name="touch"></param>
        /// <param name="startPosition"></param>
        private void CalcDragDistance(Touch touch, Vector2 startPosition) {
            Vector2 distance = touch.position - startPosition;                                                    // ドラッグ開始地点からドラッグ先までの距離(解像度に依存?)。スティックの倒れた距離とも言える。
            float compressedMagnitude = Mathf.Min(distance.magnitude, MaxStickMovement);  // ドラッグ距離を制限する。
            Vector2 compressedDistance = distance.normalized * compressedMagnitude;        // 制限した距離をもとの方向に再び適用する。
            Vector2 result = compressedDistance / MaxStickMovement;                                // 結果 / 最大距離  で最大距離に対する、入力の割合を計算する。
            StickInput = result;
        }


        // ドラッグ中のものとは違う指が触れた時でも、指が離れた時 OnEndDrag が呼ばれることに注意。
        public void OnEndDrag(PointerEventData eventData) {
            if (Input.touchCount < 1) { return; }
            if (eventData.pointerId != draggingFingerID.Value) { return; } // 追跡していた指以外のものがドラッグ終了した場合...のはずだが稀にごく妙な挙動をする。致命的な問題ではないと思われる。
            draggingFingerID = null;
        }
        

        // 使っていないが、IBeginDragHandler を使うなら IDragHandler も必要だとマニュアルに書いてあったので。
        public void OnDrag(PointerEventData eventData) { }
    }
}