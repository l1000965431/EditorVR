#if UNITY_EDITOR
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;
using UnityEngine.InputNew;

namespace UnityEditor.Experimental.EditorVR.Tools
{
	[MainMenuItem("Light", "Create", "Create lights in the scene")]
	sealed class CreateLightTool : MonoBehaviour, ITool, IStandardActionMap, IConnectInterfaces, IInstantiateMenuUI,
		IUsesRayOrigin, IUsesSpatialHash, IUsesViewerScale, ISelectTool
	{
		[SerializeField]
        CreateLightMenu m_MenuPrefab;

        [SerializeField]
        Color m_DefaultColor = Color.cyan;

        [SerializeField]
        float m_IntensityScaling = 5f;
        [SerializeField]
        float m_IntensityMin = 1f;

        [SerializeField]
        float m_RangeScaling = 1000f;
        [SerializeField]
        float m_RangeMin = 10f;

        const float k_DrawDistance = 0.075f;

        GameObject m_ToolMenu;

        GameObject m_CurrentGameObject;
        Light m_Light;

        Vector3 m_StartPoint = Vector3.zero;
        Vector3 m_EndPoint = Vector3.zero;

        LightType m_SelectedLightType = LightType.Point;

        LightCreationStates m_State = LightCreationStates.StartPoint;

        public Transform rayOrigin { get; set; }

        enum LightCreationStates
        {
            StartPoint,
            EndPoint,
        }

        void Start()
		{
			m_ToolMenu = this.InstantiateMenuUI(rayOrigin, m_MenuPrefab);
			var createLightMenu = m_ToolMenu.GetComponent<CreateLightMenu>();
			this.ConnectInterfaces(createLightMenu, rayOrigin);
            createLightMenu.selectLight = SetSelectedLight;
            createLightMenu.close = Close;
		}

		public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
		{
			var standardInput = (Standard)input;

			switch (m_State)
			{
				case LightCreationStates.StartPoint:
				{
					HandleStartPoint(standardInput, consumeControl);
					break;
				}
                case LightCreationStates.EndPoint:
				{
					UpdatePositions();
                    SetIntensityScaling();
                    SetRangeScaling();

                    CheckForTriggerRelease(standardInput, consumeControl);
					break;
				}
			}
		}

        void SetSelectedLight(LightType type)
        {
            m_SelectedLightType = type;
        }

        void HandleStartPoint(Standard standardInput, ConsumeControlDelegate consumeControl)
		{
			if (standardInput.action.wasJustPressed)
			{
                m_CurrentGameObject = ObjectUtils.CreateEmptyGameObject();
                m_CurrentGameObject.name = "Light";
                m_Light = m_CurrentGameObject.AddComponent<Light>();
                m_Light.color = m_DefaultColor;

                // Set starting minimum scale (don't allow zero scale object to be created)
                const float kMinScale = 0.0025f;
				var viewerScale = this.GetViewerScale();
				m_CurrentGameObject.transform.localScale = Vector3.one * kMinScale * viewerScale;
				m_StartPoint = rayOrigin.position + rayOrigin.forward * k_DrawDistance * viewerScale;
				m_CurrentGameObject.transform.position = m_StartPoint;

				m_State = LightCreationStates.EndPoint;

				this.AddToSpatialHash(m_CurrentGameObject);

				consumeControl(standardInput.action);
				Selection.activeGameObject = m_CurrentGameObject;
			}
		}

        void SetIntensityScaling()
        {
            var intensity = Mathf.Clamp(m_EndPoint.y - m_StartPoint.y, 0 , 10); 
            //Debug.Log("creation intensity (y axis): " + intensity * m_IntensityScaling);
            m_Light.intensity = m_IntensityMin + (intensity * m_IntensityScaling);
        }


        // TODO - scale range by horizontal distance, not x or z
        void SetRangeScaling()
        {
            // var range = Mathf.Abs(m_EndPoint.x - m_StartPoint.x);
            var distance = Vector3.Distance(m_StartPoint, new Vector3(m_EndPoint.x, m_StartPoint.y, m_EndPoint.z));
            //Debug.Log("creation range: (x axis) " + range * m_RangeScaling);
            var scaling = distance * m_RangeScaling * this.GetViewerScale();
            m_Light.range = m_RangeMin + scaling;
        }

        void UpdatePositions()
		{
			m_EndPoint = rayOrigin.position + rayOrigin.forward * k_DrawDistance * this.GetViewerScale();
			//m_CurrentGameObject.transform.position = (m_StartPoint + m_EndPoint) * 0.5f;
		}

		void CheckForTriggerRelease(Standard standardInput, ConsumeControlDelegate consumeControl)
		{
			// Ready for next object to be created
			if (standardInput.action.wasJustReleased)
			{
				m_State = LightCreationStates.StartPoint;

				consumeControl(standardInput.action);
			}
		}

		void Close()
		{
			this.SelectTool(rayOrigin, GetType());
		}

		void OnDestroy()
		{
			ObjectUtils.Destroy(m_ToolMenu);
		}
	}
}
#endif
