using UnityEditor;
using UnityEngine;

/// <summary>
/// 在 PlayerView / EnemyView（及继承 <see cref="ActorBaseView"/>）的 Inspector 中展示绑定角色的 PropSet。
/// </summary>
[CustomEditor(typeof(ActorBaseView), true)]
public class ActorBaseViewEditor : UnityEditor.Editor
{
    static readonly GUIContent PropsHeader = new GUIContent("角色 PropSet", "运行模式下显示已绑定 ActorBase 的属性数值");

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(PropsHeader, EditorStyles.boldLabel);

        var view = (ActorBaseView)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("进入运行模式后，将显示与本 View 绑定的 Presenter 的 PropSet（需在场景中已由 ActorSpawn 初始化）。", MessageType.Info);
            return;
        }

        ActorBase presenter = view.Presenter;
        if (presenter == null)
        {
            EditorGUILayout.HelpBox("当前未绑定 ActorBase（尚未 Init 或已 Dispose）。", MessageType.Warning);
            return;
        }

        PropSet props = presenter.m_PropSet;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        for (int i = 1; i < (int)PropType.Max; i++)
        {
            PropType pt = (PropType)i;
            DrawPropRow(pt.ToString(), props[pt]);
        }

        EditorGUILayout.EndVertical();
    }

    static void DrawPropRow(string label, float value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(160f), GUILayout.ExpandWidth(false));
        EditorGUILayout.SelectableLabel(value.ToString("0.###"), GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.EndHorizontal();
    }
}
