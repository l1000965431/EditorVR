#if UNITY_EDITOR
using System;
using UnityEngine;

namespace UnityEditor.Experimental.EditorVR.Tools
{
	sealed class CreateLightMenu : MonoBehaviour, IMenu
	{
		[SerializeField]
		GameObject[] m_HighlightObjects;

		public Action<LightType> selectLight;
		public Action close;

		public bool visible
		{
			get { return gameObject.activeSelf; }
			set { gameObject.SetActive(value); }
		}

        public void SelectLight(int type)
        {
            selectLight((LightType)type);

            // the order of the objects in m_HighlightObjects is matched to the values of the LightType enum elements
            for (var i = 0; i < m_HighlightObjects.Length; i++)
            {
                var go = m_HighlightObjects[i];
                go.SetActive(i == type);
            }
        }

        public GameObject menuContent
		{
			get { return gameObject; }
		}

		public void Close()
		{
			close();
		}
	}
}
#endif
