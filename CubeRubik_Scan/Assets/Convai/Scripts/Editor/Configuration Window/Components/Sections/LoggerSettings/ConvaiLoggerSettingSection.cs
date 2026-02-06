using System;
using System.Reflection;
using Convai.Scripts.LoggerSystem;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.LoggerSettings
{
    [UxmlElement]
    public partial class ConvaiLoggerSettingSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "logger-setting";

        private readonly LoggerSettingsLogic _loggerSettingsLogic;

        public ConvaiLoggerSettingSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("header", "Logger Settings", "header"));
            _loggerSettingsLogic = new LoggerSettingsLogic();
            CreateLoggerTable();
            VisualElement row = new()
            {
                name = "header-row", style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexStart, alignItems = Align.Center }
            };

            Button selectAll = new() { name = "select-all", text = "Select All" };
            selectAll.AddToClassList("button");
            selectAll.clicked += _loggerSettingsLogic.SelectAllOnClicked;

            Button clearAll = new() { name = "clear-all", text = "Clear All" };
            clearAll.AddToClassList("button");
            clearAll.clicked += _loggerSettingsLogic.ClearAllOnClicked;

            row.Add(selectAll);
            row.Add(clearAll);
            Add(row);
        }

        private void CreateLoggerTable()
        {
            VisualElement loggerTable = new();
            loggerTable.AddToClassList("logger-table");
            Add(loggerTable);
            CreateHeaders(loggerTable);
            VisualElement lastRow = null;
            foreach (FieldInfo fieldInfo in typeof(Scripts.LoggerSystem.LoggerSettings).GetFields())
            {
                VisualElement tableRow = new();
                Label categoryName = new(fieldInfo.Name);
                Toggle selectAll = CreateSelectAllForCategory(fieldInfo);
                _loggerSettingsLogic.AddToSelectAllDictionary(fieldInfo, selectAll);
                categoryName.AddToClassList("logger-table-element");
                tableRow.Add(selectAll);
                tableRow.Add(categoryName);
                CreateSeverityTogglesForCategory(fieldInfo, tableRow);
                tableRow.AddToClassList("logger-table-row");
                loggerTable.Add(tableRow);
                lastRow = tableRow;
            }

            lastRow?.AddToClassList("logger-table-row-last");
        }


        private static void CreateHeaders(VisualElement loggerTable)
        {
            VisualElement tableHeader = new();
            VisualElement selectAll = new Label("Select All");
            VisualElement category = new Label("Category");
            selectAll.AddToClassList("logger-table-element");
            category.AddToClassList("logger-table-element");
            tableHeader.Add(selectAll);
            tableHeader.Add(category);
            foreach (ConvaiUnityLogger.LogLevel logLevel in Enum.GetValues(typeof(ConvaiUnityLogger.LogLevel)))
            {
                if (logLevel == ConvaiUnityLogger.LogLevel.None)
                {
                    continue;
                }

                VisualElement label = new Label(logLevel.ToString());
                label.AddToClassList("logger-table-element");
                tableHeader.Add(label);
            }

            tableHeader.AddToClassList("logger-table-row");
            tableHeader.AddToClassList("logger-table-row-first");
            loggerTable.Add(tableHeader);
        }

        private Toggle CreateSelectAllForCategory(FieldInfo fieldInfo)
        {
            Toggle selectAll = new() { value = _loggerSettingsLogic.IsAllSelectedForCategory(fieldInfo) };
            selectAll.RegisterValueChangedCallback(evt => { _loggerSettingsLogic.OnSelectAllClicked(fieldInfo, evt.newValue); });
            selectAll.AddToClassList("logger-table-element");
            return selectAll;
        }

        private void CreateSeverityTogglesForCategory(FieldInfo fieldInfo, VisualElement severityContainer)
        {
            foreach (ConvaiUnityLogger.LogLevel enumType in Enum.GetValues(typeof(ConvaiUnityLogger.LogLevel)))
            {
                if (enumType == ConvaiUnityLogger.LogLevel.None)
                {
                    continue;
                }

                Toggle toggle = new() { value = _loggerSettingsLogic.GetLogLevelEnabledStatus(fieldInfo, enumType) };

                void Callback(ChangeEvent<bool> evt)
                {
                    _loggerSettingsLogic.OnToggleClicked(fieldInfo, enumType, evt.newValue);
                }

                toggle.UnregisterValueChangedCallback(Callback);
                toggle.RegisterValueChangedCallback(Callback);
                toggle.AddToClassList("logger-table-element");
                severityContainer.Add(toggle);
                _loggerSettingsLogic.AddToToggleDictionary(fieldInfo, toggle);
            }

            severityContainer.AddToClassList("severity-container");
        }
    }
}
