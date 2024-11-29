using DG.Tweening;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.Editor
{

    [CustomPropertyDrawer(typeof(TweenEase))]
    public class TweenEaseEditor : PropertyDrawer
    {
        public VisualTreeAsset _inspectorXML;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI.
            var container = new VisualElement();

            _inspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/_Project/Editor/UIToolkit/tween-ease-custom-inspector.uxml");

            if (_inspectorXML)
            {
                var inspectorXML = _inspectorXML.Instantiate();

                var ease = inspectorXML.Q<EnumField>("EnumField_Ease");
                var curve = inspectorXML.Q<CurveField>("CurveField_Curve");

                ease.label = property.displayName;
                
                if((Ease)ease.value == Ease.INTERNAL_Custom)
                {
                    curve.visible = true;
                    curve.style.display = DisplayStyle.Flex;
                }
                else
                {
                    curve.visible = false;
                    curve.style.display = DisplayStyle.None;
                }

                ease.RegisterValueChangedCallback((value) =>
                {
                    if ((Ease)value.newValue == Ease.INTERNAL_Custom)
                    {
                        curve.visible = true;
                        curve.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        curve.visible = false;
                        curve.style.display = DisplayStyle.None;
                    }
                });

                container.Add(inspectorXML);
            }

            return container;
        }
    }
}
