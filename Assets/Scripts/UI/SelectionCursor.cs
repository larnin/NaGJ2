﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectionCursor : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();
    Image m_image;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
        m_image.gameObject.SetActive(false);

        m_subscriberList.Add(new Event<DisplaySelectionRectangleEvent>.Subscriber(DisplaySelection));
        m_subscriberList.Add(new Event<HideSelectionRectangleEvent>.Subscriber(HideSelection));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void DisplaySelection(DisplaySelectionRectangleEvent e)
    {
        m_image.gameObject.SetActive(true);

        Vector3 center = (e.pos1 + e.pos2) / 2;

        m_image.rectTransform.position = center;
        var scale = m_image.rectTransform.lossyScale;
        float width = Mathf.Abs(e.pos1.x - e.pos2.x) / scale.x;
        float height = Mathf.Abs(e.pos1.y - e.pos2.y) / scale.y;
        m_image.rectTransform.sizeDelta = new Vector2(width, height);

        m_image.color = e.color;
    }

    void HideSelection(HideSelectionRectangleEvent e)
    {
        m_image.gameObject.SetActive(false);
    }
}
