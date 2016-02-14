/*
 * this class is reference form :  http://wiki.unity3d.com/index.php?title=PopupList
 */
using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin
{
public abstract class ComboBoxBase
{
    protected static bool forceToUnShow = false; 
    protected static int useControlID = -1;
    protected bool isClickedComboButton = false;
    protected int selectedItemIndex = 0;

    protected float itemWidth;
    protected float itemHeight;
 
    protected GUIContent buttonContent;
    protected GUIContent[] listContent;
    protected GUIStyle buttonStyle;
    protected GUIStyle boxStyle;
    protected GUIStyle listStyle;
 
    protected ComboBoxBase( GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle )
        : this(buttonContent, listContent, "button", "box", listStyle)    {
    }
 
    protected ComboBoxBase(GUIContent buttonContent, GUIContent[] listContent, 
                           GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = buttonStyle;
        this.boxStyle = boxStyle;
        this.listStyle = listStyle;
        initIndex();
        InitSize();
    }

    protected void InitSize() {
        int maxLength = 0;
        foreach (GUIContent c in listContent) {
            if (maxLength < c.text.Length) maxLength = c.text.Length;
        }
        itemWidth = maxLength*9f;
        itemHeight = listStyle.CalcHeight(listContent[0], 1.0f);

    }
    protected void initIndex() {
        for (int i=0; i<listContent.Length; i++) {
            if (buttonContent.text == listContent[i].text) {
                selectedItemIndex = i;
                break;
            }
        }
    }
    public int SelectItem(string item) {
        string itemLow = item.ToLower();
        for (int i=0; i< listContent.Length; i++) {
            if (listContent[i].text.ToLower() == itemLow) {
                selectedItemIndex = i;
                return i;
            }
        }
        return -1;
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
 
public class ComboBox : ComboBoxBase
{
    public Rect rect;
 
    public ComboBox( Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle )
        : base(buttonContent, listContent, listStyle) {
        this.rect = rect;
    }
 
    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
        : base(buttonContent, listContent, buttonStyle, boxStyle, listStyle) {
        this.rect = rect;
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
            var listRect = new Rect( rect.x, rect.y + itemHeight,
                      rect.width, itemHeight * listContent.Length );
 
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
}

/// <summary>
/// GUILayout版のコンボボックスクラス. 
/// </summary>
public class ComboBoxLO : ComboBoxBase
{
    private readonly bool labelFixed;
    public ComboBoxLO( GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle ) 
        : base(buttonContent, listContent, listStyle) {
    }
 
    public ComboBoxLO(GUIContent buttonContent, GUIContent[] listContent, 
                      GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle, bool labelFixed)
        : base(buttonContent, listContent, buttonStyle, boxStyle, listStyle) {
        this.labelFixed = labelFixed;
    }
    public void SetItemWidth(float itemWidth) {
        this.itemWidth = itemWidth;
    }
    public int Show(GUILayoutOption buttonOpt)
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
 
        bool expand = isClickedComboButton;
        if (expand) GUILayout.BeginVertical(boxStyle, GUILayout.Width(itemWidth));
        try {
            if( GUILayout.Button(buttonContent, buttonStyle, buttonOpt ) ) {
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
                float height = itemHeight * listContent.Length;
                int newSelectedItemIndex = GUILayout.SelectionGrid(selectedItemIndex, listContent, 1, listStyle, 
                                                                   GUILayout.Width(itemWidth), GUILayout.Height(height));
                if( newSelectedItemIndex != selectedItemIndex ) {
                    selectedItemIndex = newSelectedItemIndex;
                    // ラベル指定に応じて
                    if (!labelFixed) {
                        buttonContent = listContent[selectedItemIndex];
                    }
                }
            }
        } finally {
            if(expand) GUILayout.EndVertical();
        }
 
        isClickedComboButton &= !done;
        return selectedItemIndex;
    }

}
}