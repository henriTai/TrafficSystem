#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
// Author: Henri Tainio
/// <summary>
/// Object tagger's purpose is to to visualize waypoint nodes in scene view by adding tag icons to them. If icons don't show,
/// turn 3D icons of from Gizmos-menu. ATM, only default color coding (by lane number) is implemented. 
/// </summary>
public static class ObjectTagger
{
    /// <summary>
    /// A color scheme for visualizing lane 
    /// </summary>
    public enum TagColorScheme
    {
        /* Each lane (left / right 1-3) has its own color.
         * Bus lanes are marked yellow
         * Connect points are larger squares, other nodes are smaller rounds
         * Unconnected end nodes are larger RED squares
         */
        ByLaneNumber
    }

    /// <summary>
    /// Unity's default icon types.
    /// </summary>
    public enum IconType
    {
        Label = 0,
        Small = 1,
        Pix16 = 2
    };

    /// <summary>
    /// Node icon colors, uses Unity's default icon colors.
    /// </summary>
    public enum IconColor
    {
        Gray,
        Blue,
        Jade,
        Green,
        Yellow,
        Orange,
        Red,
        Purple
    };
    /// <summary>
    /// An array of icon counts of each Unity's default icon types (label, small, big).
    /// </summary>
    [SerializeField]
    static readonly int[] iconCount = { 8, 16, 16 };

    /// <summary>
    /// An array of label icons
    /// </summary>
    [SerializeField]
    static readonly GUIContent[] labelIconArray =
    {
        EditorGUIUtility.IconContent("sv_label_0"),
        EditorGUIUtility.IconContent("sv_label_1"),
        EditorGUIUtility.IconContent("sv_label_2"),
        EditorGUIUtility.IconContent("sv_label_3"),
        EditorGUIUtility.IconContent("sv_label_4"),
        EditorGUIUtility.IconContent("sv_label_5"),
        EditorGUIUtility.IconContent("sv_label_6"),
        EditorGUIUtility.IconContent("sv_label_7")
    };
    /// <summary>
    /// An array of small icons
    /// </summary>
    [SerializeField]
    static readonly GUIContent[] smallIconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot1_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot2_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot3_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot4_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot5_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot6_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot7_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot8_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot9_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot10_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot11_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot12_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot13_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot14_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot15_sml")
    };

    /// <summary>
    /// An array of large icons
    /// </summary>
    [SerializeField]
    static readonly GUIContent[] pix16IconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot1_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot2_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot5_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot7_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot10_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot12_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot13_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot15_pix16_gizmo")
    };
    /// <summary>
    /// Adds a sceneview tag icon to a gameobject.
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    /// <param name="i">Selected icontype (enum)</param>
    /// <param name="index">Indexed color of the tag</param>
    public static void SetIcon(GameObject g, IconType i, int index)
    {
        int div = iconCount[(int) i];
        int ind = index % div;
        switch(i)
        {
            case IconType.Label:
                var iconL = labelIconArray[ind];
                var eguL = typeof(EditorGUIUtility);
                var flagsL = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsL = new object[] { g, iconL.image };
                var setIconL = eguL.GetMethod("SetIconForObject", flagsL, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconL.Invoke(null, argsL);
                break;
            case IconType.Small:
                var iconS = smallIconArray[ind];
                var eguS = typeof(EditorGUIUtility);
                var flagsS = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsS = new object[] { g, iconS.image };
                var setIconS = eguS.GetMethod("SetIconForObject", flagsS, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconS.Invoke(null, argsS);
                break;
            case IconType.Pix16:
                var iconP = pix16IconArray[ind];
                var eguP = typeof(EditorGUIUtility);
                var flagsP = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsP = new object[] { g, iconP.image };
                var setIconP = eguP.GetMethod("SetIconForObject", flagsP, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconP.Invoke(null, argsP);
                break;
        }
    }
    /// <summary>
    /// A gameobject tag method variant with option to use either a round or cornered dot tag.
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    /// <param name="i">Selected icon type. Applies if the selected icon type is Label - otherwise 'round' boolean determines the icon type</param>
    /// <param name="c">Selected color</param>
    /// <param name="round">Is the tag a round or 4-corner dot.</param>
    public static void SetIcon(GameObject g, IconType i, IconColor c, bool round)
    {
        switch (c)
        {
            case IconColor.Gray:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 0);
                }
                else
                {
                    SetIcon(g, i, 8);
                }
                break;
            case IconColor.Blue:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 1);
                }
                else
                {
                    SetIcon(g, i, 9);
                }
                break;
            case IconColor.Jade:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 2);
                }
                else
                {
                    SetIcon(g, i, 10);
                }
                break;
            case IconColor.Green:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 3);
                }
                else
                {
                    SetIcon(g, i, 11);
                }
                break;
            case IconColor.Yellow:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 4);
                }
                else
                {
                    SetIcon(g, i, 12);
                }
                break;
            case IconColor.Orange:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 5);
                }
                else
                {
                    SetIcon(g, i, 13);
                }
                break;
            case IconColor.Red:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 6);
                }
                else
                {
                    SetIcon(g, i, 14);
                }
                break;
            case IconColor.Purple:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 7);
                }
                else
                {
                    SetIcon(g, i, 15);
                }
                break;
        }
    }
    /// <summary>
    /// Sets given gameobject's tag icon as an unconnected connect node (A large gray 4-cornered icon)
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    public static void SetAsUnconnectedConnectNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 8);
    }
    /// <summary>
    /// Sets given gameobject's tag icon as unconnected normal node (A large gray round dot icon)
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    public static void SetAsUnconnectedNormalNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 0);
    }
    /// <summary>
    /// Sets given gameobject's tag icon as a buslane connect node (A large yellow 4-cornered icon)
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    public static void SetAsBusConnectNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 12);
    }
    /// <summary>
    /// Sets given gameobject's tag icon as a normal buslane node (A small yellow round dot icon)
    /// </summary>
    /// <param name="g">Tagged gameobject</param>
    public static void SetAsBusNormalNode(GameObject g)
    {
        SetIcon(g, IconType.Small, 4);
    }
    /// <summary>
    /// Tags a list of nodes (gameobjects) of given lane index using selected color scheme.
    /// </summary>
    /// <param name="scheme">Selected color scheme. ATM there is only a default color scheme.</param>
    /// <param name="laneIndex">Lane index (order is right hand side lanes (from center, 0-2), lhs lanes (from center, 3-5)</param>
    /// <param name="nodeList">A list of tagged (node) gameobjects.</param>
    public static void SetLaneIcons(TagColorScheme scheme, int laneIndex, ref List<GameObject> nodeList)
    {
        switch(scheme)
        {
            case TagColorScheme.ByLaneNumber:
                foreach (GameObject g in nodeList)
                {
                    ByLaneNumber(laneIndex, g);
                }
                break;
        }
    }
    /// <summary>
    /// Tags an array of nodes of given lane index using selected color scheme.
    /// </summary>
    /// <param name="scheme">Selected color scheme. ATM there is only a default color scheme.</param>
    /// <param name="laneIndex">Lane index (order is right hand side lanes (from center, 0-2), lhs lanes (from center, 3-5)</param>
    /// <param name="nodeList">An array of tagged nodes.</param>
    public static void SetLaneIcons(TagColorScheme scheme, int laneIndex, ref Nodes[] nodeList)
    {
        switch (scheme)
        {
            case TagColorScheme.ByLaneNumber:
                foreach (Nodes n in nodeList)
                {
                    ByLaneNumber(laneIndex, n.gameObject);
                }
                break;
        }
    }
    /// <summary>
    /// Sets node's (gameobject) color using lane index.
    /// </summary>
    /// <param name="laneIndex">Lane index (order is right hand side lanes (from center, 0-2), lhs lanes (from center, 3-5)</param>
    /// <param name="g">Tagged gameobject</param>
    public static void ByLaneNumber(int laneIndex, GameObject g)
    {
        IconColor c = IconColor.Gray;
        switch(laneIndex)
        {
            case 0:
                c = IconColor.Blue;
                break;
            case 1:
                c = IconColor.Jade;
                break;
            case 2:
                c = IconColor.Green;
                break;
            case 3:
                c = IconColor.Orange;
                break;
            case 4:
                c = IconColor.Red;
                break;
            case 5:
                c = IconColor.Purple;
                break;
        }

        Nodes n = g.GetComponent<Nodes>();
        bool busLane = n.BusLane;
        bool connected = true;
        bool connectNode = n.LaneStartNode;

        if (n.InNode == null)
        {
            connected = false;
        }
        if (n.OutNode == null)
        {
            connected = false;
        }

        if (connectNode)
        {
            if (!connected)
            {
                SetAsUnconnectedConnectNode(g);
            }
            else if (busLane)
            {
                SetAsBusConnectNode(g);
            }
            else
            {
                SetIcon(g, IconType.Pix16, c, false);
            }
        }
        else
        {
            if (!connected)
            {
                SetAsUnconnectedNormalNode(g);
            }
            else if (busLane)
            {
                SetAsBusNormalNode(g);
            }
            else
            {
                SetIcon(g, IconType.Small, c, true);
            }
        }

    }

}

#endif