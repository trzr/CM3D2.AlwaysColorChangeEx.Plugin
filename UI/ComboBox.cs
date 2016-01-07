/*
 * this code's original:  http://wiki.unity3d.com/index.php?title=PopupList
 */
using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin
{
 
public class ComboBox
{
    private static bool forceToUnShow = false; 
    private static int useControlID = -1;
    private bool isClickedComboButton = false;
    private int selectedItemIndex = 0;
 
    public Rect rect;
    private GUIContent buttonContent;
    private GUIContent[] listContent;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;
    private GUIStyle listStyle;
 
    public ComboBox( Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle ) {
        this.rect = rect;
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = "button";
        this.boxStyle = "box";
        this.listStyle = listStyle;
        initIndex();
    }
 
    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle){
        this.rect = rect;
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = buttonStyle;
        this.boxStyle = boxStyle;
        this.listStyle = listStyle;
        initIndex();
    }
    private void initIndex() {
        for (int i=0; i<listContent.Length; i++) {
            if (buttonContent.text == listContent[i].text) {
                selectedItemIndex = i;
                break;
            }
        }
        
    }
 
    public int Show()
    {
        if( forceToUnShow ) {
            forceToUnShow = false;
            isClickedComboButton = false;
        }
 
        bool done = false;
        int controlID = GUIUtility.GetControlID( FocusType.Passive );       
 
        switch( Event.current.GetTypeForControl(controlID) ) {
            case EventType.mouseUp: 
                done |= isClickedComboButton;
                break;
        }       
 
        if( GUI.Button( rect, buttonContent, buttonStyle ) ) {
            if( useControlID == -1 ) {
                useControlID = controlID;
                isClickedComboButton = false;
            }
 
            if( useControlID != controlID ) {
                forceToUnShow = true;
                useControlID = controlID;
            }
            isClickedComboButton = true;
        }
 
        if( isClickedComboButton ) {
            var listRect = new Rect( rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
                      rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length );
 
            GUI.Box( listRect, "", boxStyle );
            int newSelectedItemIndex = GUI.SelectionGrid( listRect, selectedItemIndex, listContent, 1, listStyle );
            if( newSelectedItemIndex != selectedItemIndex ) {
                selectedItemIndex = newSelectedItemIndex;
                buttonContent = listContent[selectedItemIndex];
            }
        }
 
        isClickedComboButton &= !done;
        return selectedItemIndex;
    }

    public bool IsClickedComboButton {
        get { return  isClickedComboButton;}
    }

    public int ItemCount {
        get { return listContent.Length; }
    }
    public int SelectedItemIndex{
        get {  return selectedItemIndex;  }
        set {  selectedItemIndex = value; }
    }
}
}
