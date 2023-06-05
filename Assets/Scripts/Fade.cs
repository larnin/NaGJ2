
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] float m_transitionDuration = 0.5f;
    [SerializeField] Color m_color = Color.black;

    Image m_render;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<ShowLoadingScreenEvent>.Subscriber(OnFade));
        m_subscriberList.Subscribe();

        m_render = GetComponentInChildren<Image>(true);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnFade(ShowLoadingScreenEvent e)
    {
        if (e.start)
        {
            gameObject.SetActive(true);
            m_render.DOColor(m_color, m_transitionDuration);
        }
        else
        {
            gameObject.SetActive(true);
            Color targetColor = m_color;
            targetColor.a = 0;
            m_render.DOColor(targetColor, m_transitionDuration).OnComplete(()=> {gameObject?.SetActive(false); });
        }
    }
}
