using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Convai.Scripts.Checks
{
    public static class ActiveInputFieldChecker
    {
        public static bool IsAnyInputFieldInUse()
        {
            // Check UGUI and TMP input fields using EventSystem
            if (EventSystem.current?.currentSelectedGameObject == null)
            {
                return false;
            }

            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            return selectedObject.GetComponent<InputField>() != null || selectedObject.GetComponent<TMP_InputField>() != null;
        }
    }
}
