using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Octet<T> {
    Dictionary<Direction, T> items;
    public T down;
    public T rightDown;
    public T right;
    public T rightUp;
    public T up;

    public T this[Direction key] {
        get {
            switch (key) {
                default:
                case Direction.right:
                case Direction.left:
                    if (right != null) {
                        return right;
                    } else if (rightUp != null) {
                        return rightUp;
                    } else if (rightDown != null) {
                        return rightDown;
                    } else if (up != null) {
                        return up;
                    } else {
                        return down;
                    }
                case Direction.rightUp:
                case Direction.leftUp:
                    if (rightUp != null) {
                        return rightUp;
                    } else if (up != null) {
                        return up;
                    } else if (right != null) {
                        return right;
                    } else if (rightDown != null) {
                        return rightDown;
                    } else {
                        return down;
                    }
                case Direction.up:
                    // return up;
                    if (up != null) {
                        return up;
                    } else if (rightUp != null) {
                        return rightUp;
                    } else if (right != null) {
                        return right;
                    } else if (rightDown != null) {
                        return rightDown;
                    } else {
                        return down;
                    }
                case Direction.down:
                    // return down;
                    if (down != null) {
                        return down;
                    } else if (rightDown != null) {
                        return rightDown;
                    } else if (right != null) {
                        return right;
                    } else if (rightUp != null) {
                        return rightUp;
                    } else {
                        return up;
                    }
                case Direction.rightDown:
                case Direction.leftDown:
                    if (rightDown != null) {
                        return rightDown;
                    } else if (right != null) {
                        return right;
                    } else if (down != null) {
                        return down;
                    } else if (rightUp != null) {
                        return rightUp;
                    } else {
                        return up;
                    }
            }
        }

        set {
            switch (key) {
                default:
                case Direction.left:
                    right = value;       // sprite flipped
                    break;
                case Direction.leftUp:
                    rightUp = value;       // sprite flipped
                    break;

                case Direction.up:
                    up = value;
                    break;

                case Direction.rightUp:
                    rightUp = value;
                    break;

                case Direction.right:
                    right = value;
                    break;

                case Direction.rightDown:
                    rightDown = value;
                    break;

                case Direction.down:
                    down = value;
                    break;
                case Direction.leftDown:
                    rightDown = value;       // sprite flipped
                    break;

            }
        }
    }

}