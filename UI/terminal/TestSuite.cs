using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSuite {

    static public void AssertEquals(System.Object a, System.Object b) {
        Debug.Assert(a.Equals(b));
        if (a.Equals(b)) {
            Debug.Log($"success: {a} equals {b}");
        } else {
            Debug.LogError($"expected {b} but got {a}");
        }
    }
    static public void RunToolboxTests() {
        // ↖ ↑ ↗
        // ← · →
        // ↙ ↓ ↘

        // right →
        // limits: ↑, ↓
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.right), Direction.right);            //0  →, → = → <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.right), Direction.rightUp);        //1  ↗, → = ↗
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.right), Direction.up);                  //2  ↑, → = ↑ <- upper
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.right), Direction.up);              //3  ↖, → = ↑
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.right), Direction.up);                //4  ←, → = ↑ 
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.right), Direction.down);          //5  ↙, → = ↓ 
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.right), Direction.down);              //6  ↓, → = ↓ <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.right), Direction.rightDown);    //7  ↘, → = ↘

        // rightUp ↗
        // limits: ↖, ↘
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.rightUp), Direction.right);          //0  →, ↗ = →
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.rightUp), Direction.rightUp);      //1  ↗, ↗ = ↗ <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.rightUp), Direction.up);                //2  ↑, ↗ = ↑
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.rightUp), Direction.leftUp);        //3  ↖, ↗ = ↖ <- upper
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.rightUp), Direction.leftUp);          //4  ←, ↗ = ↖    
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.rightUp), Direction.leftUp);      //5  ↙, ↗ = ↖ 
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.rightUp), Direction.rightDown);       //6  ↓, ↗ = ↘ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.rightUp), Direction.rightDown);  //7  ↘, ↗ = ↘ <- lower 

        // up ↑
        // limits: ←, →
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.up), Direction.right);          //  →, ↑ = → <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.up), Direction.rightUp);      //  ↗, ↑ = ↗ 
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.up), Direction.up);                //  ↑, ↑ = ↑ <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.up), Direction.leftUp);        //  ↖, ↑ = ↖
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.up), Direction.left);            //  ←, ↑ = ← <- upper
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.up), Direction.left);         //  ↙, ↑ = ← 
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.up), Direction.left);             //  ↓, ↑ = ← 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.up), Direction.right);       //  ↘, ↑ = → 

        // leftUp ↖
        // limits: ↙, ↗
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.leftUp), Direction.rightUp);       //  →, ↖ = ↗ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.leftUp), Direction.rightUp);     //  ↗, ↖ = ↗ <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.leftUp), Direction.up);               //  ↑, ↖ = ↑ 
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.leftUp), Direction.leftUp);       //  ↖, ↖ = ↖ <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.leftUp), Direction.left);           //  ←, ↖ = ← 
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.leftUp), Direction.leftDown);   //  ↙, ↖ = ↙ <- upper 
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.leftUp), Direction.leftDown);       //  ↓, ↖ = ↙ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.leftUp), Direction.leftDown);  //  ↘, ↖ = ↙ 

        // left ←
        // limits: ↑, ↓
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.left), Direction.up);            //  →, ← = ↑ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.left), Direction.up);          //  ↗, ← = ↑ 
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.left), Direction.up);               //  ↑, ← = ↑ <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.left), Direction.leftUp);       //  ↖, ← = ↖ 
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.left), Direction.left);           //  ←, ← = ← <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.left), Direction.leftDown);   //  ↙, ← = ↙  
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.left), Direction.down);           //  ↓, ← = ↓ <- upper 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.left), Direction.down);      //  ↘, ← = ↓ 

        // leftDown ↙
        // limits: ↖, ↘
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.leftDown), Direction.rightDown);         //  →, ↙ = ↘ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.leftDown), Direction.leftUp);          //  ↗, ↙ = ↖ 
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.leftDown), Direction.leftUp);               //  ↑, ↙ = ↖ 
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.leftDown), Direction.leftUp);           //  ↖, ↙ = ↖ <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.leftDown), Direction.left);               //  ←, ↙ = ← 
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.leftDown), Direction.leftDown);       //  ↙, ↙ = ↙ <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.leftDown), Direction.down);               //  ↓, ↙ = ↓ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.leftDown), Direction.rightDown);     //  ↘, ↙ = ↘ <- upper

        // down ↓
        // limits: ←, →
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.down), Direction.right);         //  →, ↓ = → <- upper
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.down), Direction.right);       //  ↗, ↓ = → // bad ↗
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.down), Direction.right);            //  ↑, ↓ = → // bad: ↗
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.down), Direction.left);         //  ↖, ↓ = ← 
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.down), Direction.left);           //  ←, ↓ = ← <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.down), Direction.leftDown);   //  ↙, ↓ = ↙ 
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.down), Direction.down);           //  ↓, ↓ = ↓ <- clamp
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.down), Direction.rightDown); //  ↘, ↓ = ↘ 

        // rightDown ↘
        // limits: ↙, ↗
        AssertEquals(Toolbox.ClampDirection(Direction.right, Direction.rightDown), Direction.right);             //  →, ↘ = → 
        AssertEquals(Toolbox.ClampDirection(Direction.rightUp, Direction.rightDown), Direction.rightUp);         //  ↗, ↘ = ↗ <- upper
        AssertEquals(Toolbox.ClampDirection(Direction.up, Direction.rightDown), Direction.rightUp);              //  ↑, ↘ = ↗ 
        AssertEquals(Toolbox.ClampDirection(Direction.leftUp, Direction.rightDown), Direction.rightUp);          //  ↖, ↘ = ↗ // bad: ←
        AssertEquals(Toolbox.ClampDirection(Direction.left, Direction.rightDown), Direction.leftDown);           //  ←, ↘ = ↙ // bad: ←
        AssertEquals(Toolbox.ClampDirection(Direction.leftDown, Direction.rightDown), Direction.leftDown);       //  ↙, ↘ = ↙ <- lower
        AssertEquals(Toolbox.ClampDirection(Direction.down, Direction.rightDown), Direction.down);               //  ↓, ↘ = ↓ 
        AssertEquals(Toolbox.ClampDirection(Direction.rightDown, Direction.rightDown), Direction.rightDown);     //  ↘, ↘ = ↘ <- clamp
    }
}
