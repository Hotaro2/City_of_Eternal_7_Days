using System;
using UnityEngine;

namespace VN.MiniGames
{
    public enum MiniGameVariableValueType
    {
        Bool,
        Int,
        Float,
        String
    }

    public enum MiniGameVariableOperation
    {
        Set,
        Add
    }

    [Serializable]
    public sealed class MiniGameInkVariableChange
    {
        [SerializeField] private string variableName;
        [SerializeField] private MiniGameVariableOperation operation = MiniGameVariableOperation.Set;
        [SerializeField] private MiniGameVariableValueType valueType = MiniGameVariableValueType.Bool;
        [SerializeField] private bool boolValue = true;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private string stringValue;

        public string VariableName => variableName;
        public MiniGameVariableOperation Operation => operation;
        public MiniGameVariableValueType ValueType => valueType;
        public bool BoolValue => boolValue;
        public int IntValue => intValue;
        public float FloatValue => floatValue;
        public string StringValue => stringValue;

        public object GetSetValue()
        {
            return valueType switch
            {
                MiniGameVariableValueType.Bool => boolValue,
                MiniGameVariableValueType.Int => intValue,
                MiniGameVariableValueType.Float => floatValue,
                MiniGameVariableValueType.String => stringValue ?? string.Empty,
                _ => null
            };
        }
    }
}
