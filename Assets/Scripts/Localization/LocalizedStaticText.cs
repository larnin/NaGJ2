using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NLocalization
{
    public class LocalizedStaticText : MonoBehaviour
    {
        [SerializeField] LocText m_text;

        private void Start()
        {
            string str = m_text.GetText();

            var text = GetComponent<Text>();
            if(text != null)
            {
                text.text = str;
                return;
            }
            var tmp = GetComponent<TMP_Text>();
            if(tmp != null)
            {
                tmp.text = str;
                return;
            }
            Debug.LogError("No text component found in the gameobject " + name);
        }
    }
}