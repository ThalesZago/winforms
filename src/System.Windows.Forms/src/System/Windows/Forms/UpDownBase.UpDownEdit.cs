// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Drawing;
using static Interop;

namespace System.Windows.Forms
{
    public abstract partial class UpDownBase
    {
        internal class UpDownEdit : TextBox
        {
            private readonly UpDownBase _parent;
            private bool _doubleClickFired;

            /////////////////////////////////////////////////////////////////////
            // Constructors
            //
            /////////////////////////////////////////////////////////////////////
            internal UpDownEdit(UpDownBase parent)
            : base()
            {
                SetStyle(ControlStyles.FixedHeight |
                         ControlStyles.FixedWidth, true);

                SetStyle(ControlStyles.Selectable, false);

                this._parent = parent;
            }

            public override string Text
            {
                get
                {
                    return base.Text;
                }
                set
                {
                    bool valueChanged = (value != base.Text);
                    base.Text = value;
                    if (valueChanged)
                    {
                        AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
                    }
                }
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new UpDownEditAccessibleObject(this, _parent);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Clicks == 2 && e.Button == MouseButtons.Left)
                {
                    _doubleClickFired = true;
                }
                _parent.OnMouseDown(_parent.TranslateMouseEvent(this, e));
            }

            /// <summary>
            ///
            ///  Handles detecting when the mouse button is released.
            ///
            /// </summary>
            protected override void OnMouseUp(MouseEventArgs e)
            {
                Point pt = new Point(e.X, e.Y);
                pt = PointToScreen(pt);

                MouseEventArgs me = _parent.TranslateMouseEvent(this, e);
                if (e.Button == MouseButtons.Left)
                {
                    if (!_parent.ValidationCancelled && User32.WindowFromPoint(pt) == Handle)
                    {
                        if (!_doubleClickFired)
                        {
                            _parent.OnClick(me);
                            _parent.OnMouseClick(me);
                        }
                        else
                        {
                            _doubleClickFired = false;
                            _parent.OnDoubleClick(me);
                            _parent.OnMouseDoubleClick(me);
                        }
                    }
                    _doubleClickFired = false;
                }

                _parent.OnMouseUp(me);
            }

            internal override void WmContextMenu(ref Message m)
            {
                // Want to make the SourceControl to be the UpDownBase, not the Edit.
                if (ContextMenuStrip != null)
                {
                    WmContextMenu(ref m, _parent);
                }
                else
                {
                    WmContextMenu(ref m, this);
                }
            }

            /// <summary>
            ///  Raises the <see cref='Control.KeyUp'/>
            ///  event.
            /// </summary>
            protected override void OnKeyUp(KeyEventArgs e)
            {
                _parent.OnKeyUp(e);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                _parent.SetActiveControl(this);
                _parent.InvokeGotFocus(_parent, e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                _parent.InvokeLostFocus(_parent, e);
            }

            // Microsoft: Focus fixes. The XXXUpDown control will
            //         also fire a Leave event. We don't want
            //         to fire two of them.
            // protected override void OnLeave(EventArgs e) {
            //     parent.OnLeave(e);
            // }

            // Create our own accessibility object to map the accessible name
            // back to our parent.  They should track.
            internal class UpDownEditAccessibleObject : ControlAccessibleObject
            {
                readonly UpDownBase _parent;

                public UpDownEditAccessibleObject(UpDownEdit owner, UpDownBase parent) : base(owner)
                {
                    _parent = parent;
                }

                public override string Name
                {
                    get
                    {
                        return _parent.AccessibilityObject.Name;
                    }
                    set
                    {
                        _parent.AccessibilityObject.Name = value;
                    }
                }

                public override string KeyboardShortcut
                {
                    get
                    {
                        return _parent.AccessibilityObject.KeyboardShortcut;
                    }
                }
            }
        }
    }}
