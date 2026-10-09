using System;
using System.Collections.Generic;
using DOTS.GamePlay.Minimap;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    sealed class MinimapGestureManipulator : PointerManipulator
    {
        readonly MinimapViewport view;
        readonly Action changed;
        readonly Dictionary<int, Vector2> pointers = new();
        Vector2 previousCenter;
        float previousDistance;

        public MinimapGestureManipulator(MinimapViewport view, Action changed)
        {
            this.view = view;
            this.changed = changed;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(Down);
            target.RegisterCallback<PointerMoveEvent>(Move);
            target.RegisterCallback<PointerUpEvent>(Up);
            target.RegisterCallback<PointerCancelEvent>(Cancel);
            target.RegisterCallback<PointerCaptureOutEvent>(CaptureOut);
            target.RegisterCallback<WheelEvent>(Wheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            ReleaseAll();
            target.UnregisterCallback<PointerDownEvent>(Down);
            target.UnregisterCallback<PointerMoveEvent>(Move);
            target.UnregisterCallback<PointerUpEvent>(Up);
            target.UnregisterCallback<PointerCancelEvent>(Cancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(CaptureOut);
            target.UnregisterCallback<WheelEvent>(Wheel);
        }

        void Down(PointerDownEvent evt)
        {
            if (!target.enabledInHierarchy || (evt.pointerType != UnityEngine.UIElements.PointerType.touch && evt.button != 0) || pointers.Count >= 2) return;
            pointers[evt.pointerId] = target.WorldToLocal(evt.position);
            target.CapturePointer(evt.pointerId);
            Rebase();
            target.AddToClassList("is-dragging");
            evt.StopPropagation();
        }

        void Move(PointerMoveEvent evt)
        {
            if (!pointers.ContainsKey(evt.pointerId)) return;
            pointers[evt.pointerId] = target.WorldToLocal(evt.position);
            Measure(out var center, out var distance);
            if (pointers.Count == 2 && previousDistance > 4f && distance > 4f)
                view.ZoomAt(distance / previousDistance, previousCenter);
            view.PanBy(center - previousCenter);
            Rebase();
            changed();
            evt.StopPropagation();
        }

        void Up(PointerUpEvent evt) { if (Release(evt.pointerId)) evt.StopPropagation(); }
        void Cancel(PointerCancelEvent evt) => Release(evt.pointerId);
        void CaptureOut(PointerCaptureOutEvent evt) { pointers.Remove(evt.pointerId); Rebase(); }

        bool Release(int id)
        {
            if (!pointers.Remove(id)) return false;
            target.ReleasePointer(id);
            Rebase();
            return true;
        }

        public void ReleaseAll()
        {
            while (pointers.Count > 0)
            {
                int id = 0;
                foreach (var pointer in pointers) { id = pointer.Key; break; }
                Release(id);
            }
        }

        void Wheel(WheelEvent evt)
        {
            if (!target.enabledInHierarchy) return;
            view.ZoomAt(Mathf.Exp(-evt.delta.y * 0.07f), target.WorldToLocal(evt.mousePosition));
            changed();
            evt.StopPropagation();
        }

        void Rebase()
        {
            Measure(out previousCenter, out previousDistance);
            if (pointers.Count == 0) target.RemoveFromClassList("is-dragging");
        }

        void Measure(out Vector2 center, out float distance)
        {
            center = Vector2.zero;
            var first = Vector2.zero;
            distance = 0;
            int count = 0;
            foreach (var pointer in pointers)
            {
                center += pointer.Value;
                if (count++ == 0) first = pointer.Value;
                else distance = Vector2.Distance(first, pointer.Value);
            }
            if (count > 0) center /= count;
        }
    }
}
