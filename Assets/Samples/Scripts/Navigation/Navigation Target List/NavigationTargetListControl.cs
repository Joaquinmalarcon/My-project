/*===============================================================================
Copyright (C) 2024 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Immersal.Samples.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ScrollRect))]
    public class NavigationTargetListControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_ButtonTemplate = null;
        [SerializeField]
        private RectTransform m_ContentParent = null;
        [SerializeField]
        int m_MaxButtonsOnScreen = 4;
        private List<GameObject> m_Buttons = new List<GameObject>();

        public void GenerateButtons()
        {
            if (m_Buttons.Count > 0)
            {
                DestroyButtons();
            }

            foreach (KeyValuePair<NavigationTargets.NavigationCategory, List<GameObject>> entry in NavigationTargets.NavigationTargetsDict)
            {
                // --- ENCABEZADO DE CATEGORÍA ---
                GameObject header = new GameObject("Header_" + entry.Key.ToString());
                header.transform.SetParent(m_ContentParent, false);

                RectTransform headerRect = header.AddComponent<RectTransform>();
                float btnWidth = m_ButtonTemplate.GetComponent<RectTransform>().sizeDelta.x;
                headerRect.sizeDelta = new Vector2(btnWidth, 40f);

                TMPro.TextMeshProUGUI headerText = header.AddComponent<TMPro.TextMeshProUGUI>();

                string categoryLabel = entry.Key == NavigationTargets.NavigationCategory.EntradaSalida
                    ? "Entrada/Salida"
                    : entry.Key.ToString();

                headerText.text = "── " + categoryLabel + " ──";
                headerText.fontSize = 18;
                headerText.fontStyle = TMPro.FontStyles.Bold;
                headerText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                headerText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

                m_Buttons.Add(header);
                // --------------------------------

                foreach (GameObject go in entry.Value)
                {
                    IsNavigationTarget isNavigationTarget = go.GetComponent<IsNavigationTarget>();
                    string targetName = isNavigationTarget.targetName;
                    Sprite icon = isNavigationTarget.icon;

                    GameObject button = Instantiate(m_ButtonTemplate, m_ContentParent);
                    m_Buttons.Add(button);
                    button.SetActive(true);
                    button.name = string.Format("button {0}", targetName);

                    NavigationTargetListButton navigationTargetListButton = button.GetComponent<NavigationTargetListButton>();
                    navigationTargetListButton.SetText(targetName);
                    navigationTargetListButton.SetIcon(icon);
                    navigationTargetListButton.SetTarget(go);
                }
            }

            // calcular tamaño del RectTransform
            float x = m_ButtonTemplate.GetComponent<RectTransform>().sizeDelta.x;
            float y = m_ButtonTemplate.GetComponent<RectTransform>().sizeDelta.y * Mathf.Min(m_Buttons.Count, m_MaxButtonsOnScreen);
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(x, y);

            ScrollToTop();
        }

        private void DestroyButtons()
        {
            foreach (GameObject button in m_Buttons)
            {
                Destroy(button);
            }
            m_Buttons.Clear();
        }

        private void ScrollToTop()
        {
            transform.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }
    }
}