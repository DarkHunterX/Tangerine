using ConfigManager.UI.InteractiveValues;
using System;
using TangerineBaseMods.Config.Types;
using TangerineBaseMods.Patches;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;


namespace TangerineBaseMods.Config.InteractiveValues
{
    internal class InteractiveLoadingGacha : InteractiveValue
    {
        private readonly InputField[] inputsPercent = new InputField[9];
        private readonly InputField[] inputsWeight = new InputField[9];
        private readonly Slider[] sliders = new Slider[9];
        private const int sliderMin = 0;
        private const int sliderMax = 20;

        public InteractiveLoadingGacha(object value, Type valueType) : base(value, valueType) { }

        public override bool SupportsType(Type type) => type == typeof(LoadingGacha);

        public override void RefreshUIForValue()
        {
            base.RefreshUIForValue();

            if (Value is LoadingGacha test)
            {
                for (int i = 0; i < inputsWeight.Length; i++)
                    inputsWeight[i].text = GetValueFromWeight(i).ToString();
            }
        }

        public override void ConstructUI(GameObject parent)
        {
            try
            {
                base.ConstructUI(parent);

                GameObject editorContainer = UIFactory.CreateGridGroup(mainContent, "EditorContent", new Vector2(250f, 25f), new Vector2(5f, 5f), new Color(1, 1, 1, 0));
                UIFactory.SetLayoutElement(editorContainer, minWidth: 300, flexibleWidth: 9999);

                for (int i = 0; i < Enum.GetNames(typeof(LoadingImageTable.LoadingImgType)).Length; i++)
                    AddEditorRow(i, editorContainer);

                RefreshUIForValue();
            }
            catch (Exception ex)
            {
                ConfigManager.ConfigManager.LogSource.LogMessage(ex);
            }
        }

        internal void AddEditorRow(int index, GameObject groupObj)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(groupObj, $"EditorRow_{(LoadingImageTable.LoadingImgType)index+1}", false, true, true, true, 5, default, new Color(1, 1, 1, 0));

            Text label = UIFactory.CreateLabel(row, "RowLabel", $"{(LoadingImageTable.LoadingImgType)index+1}:", TextAnchor.MiddleRight, Color.cyan);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 70, flexibleWidth: 0, minHeight: 25);

            UniverseLib.UI.Models.InputFieldRef inputFieldPercent = UIFactory.CreateInputField(row, "InputField_Probability", "...");
            UIFactory.SetLayoutElement(inputFieldPercent.Component.gameObject, minWidth: 60, minHeight: 25, flexibleWidth: 0);

            UniverseLib.UI.Models.InputFieldRef inputFieldWeight = UIFactory.CreateInputField(row, "InputField_Weight", "...");
            UIFactory.SetLayoutElement(inputFieldWeight.Component.gameObject, minWidth: 40, minHeight: 25, flexibleWidth: 0);

            // set color & block user input on display probability
            inputFieldPercent.Component.interactable = false;
            inputFieldPercent.Component.textComponent.color = Color.yellow;

            inputsPercent[index] = inputFieldPercent.Component;
            inputsWeight[index] = inputFieldWeight.Component;

            inputFieldWeight.OnValueChanged += (string value) =>
            {
                try
                {
                    int val = int.Parse(value);
                    if (val < sliderMin || val > sliderMax)
                        throw new Exception();

                    SetValueToWeight(index, val);
                    sliders[index].value = val;
                    SetValueToProbability();

                    inputsWeight[index].textComponent.color = Color.white;
                }
                catch
                {
                    inputsWeight[index].textComponent.color = Color.red;
                }
            };

            GameObject sliderObj = UIFactory.CreateSlider(row, "Slider", out Slider slider);
            UIFactory.SetLayoutElement(sliderObj, minHeight: 25, minWidth: 70, flexibleWidth: 999, flexibleHeight: 0);
            sliders[index] = slider;
            slider.wholeNumbers = true;
            slider.minValue = sliderMin;
            slider.maxValue = sliderMax;
            slider.value = GetValueFromWeight(index);

            slider.onValueChanged.AddListener((float value) =>
            {
                var val = (int)value;
                SetValueToWeight(index, val);
                inputsWeight[index].text = val.ToString();
            });
        }

        internal void SetValueToProbability()
        {
            var totalWeight = 0;
            foreach (var slider in sliders)
                totalWeight += (int)slider.value;

            for (int i = 0; i < inputsPercent.Length; i++)
            {
                var weight = int.Parse(inputsWeight[i].text);
                if (i == 0 && totalWeight == 0)
                    inputsPercent[i].text = $"{CalcRate(100, 100).ToString("00.00")}%";
                else if (totalWeight == 0)
                    inputsPercent[i].text = $"{CalcRate(0, 100).ToString("00.00")}%";
                else
                    inputsPercent[i].text = $"{CalcRate(weight, totalWeight).ToString("00.00")}%";
            }
        }

        internal double CalcRate(int weight, int totalWeight)
        {
            return (double)weight / totalWeight * 100;
        }

        internal int GetValueFromWeight(int index)
        {
            var test = (LoadingGacha)Value;
            switch ((LoadingImageTable.LoadingImgType)index+1)
            {
                case LoadingImageTable.LoadingImgType.Character:
                    return test.CharRate;
                case LoadingImageTable.LoadingImgType.Skin:
                    return test.SkinRate;
                case LoadingImageTable.LoadingImgType.Weapon:
                    return test.WepRate;
                case LoadingImageTable.LoadingImgType.Card:
                    return test.CardRate;
                case LoadingImageTable.LoadingImgType.Chip:
                    return test.ChipRate;
                case LoadingImageTable.LoadingImgType.Armor:
                    return test.ArmorRate;
                case LoadingImageTable.LoadingImgType.Item:
                    return test.ItemRate;
                case LoadingImageTable.LoadingImgType.Skill:
                    return test.SkillRate;
                case LoadingImageTable.LoadingImgType.Other:
                    return test.OtherRate;
                default:
                    return 5;
            }
        }

        internal void SetValueToWeight(int index, int value)
        {
            var test = (LoadingGacha)Value;
            switch ((LoadingImageTable.LoadingImgType)index+1)
            {
                case LoadingImageTable.LoadingImgType.Character:
                    test.CharRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Skin:
                    test.SkinRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Weapon:
                    test.WepRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Card:
                    test.CardRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Chip:
                    test.ChipRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Armor:
                    test.ArmorRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Item:
                    test.ItemRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Skill:
                    test.SkillRate = value;
                    break;
                case LoadingImageTable.LoadingImgType.Other:
                    test.OtherRate = value;
                    break;
            }
            Value = test;
            Owner.SetValueFromIValue();
        }
    }
}