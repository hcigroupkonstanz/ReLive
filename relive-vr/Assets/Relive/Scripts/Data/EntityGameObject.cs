using System.Collections.Generic;
using System.Linq;
using HCIKonstanz.Colibri;
using Relive.Playback.Data;
using UnityEngine;

namespace Relive.Data
{
    public class EntityGameObject : MonoBehaviour
    {
        // FIXME: workaround for syncing with web tools
        public static readonly List<EntityGameObject> ActiveEntities = new List<EntityGameObject>();

        // FIXME: quick workaround for web sync
        public delegate void EntityChangeHandler(EntityGameObject entityGo);
        public static event EntityChangeHandler OnEntityChanged;

        // Data
        private Entity entity;
        public Entity Entity
        {
            get => entity;
            set
            {
                entity = value;
                gameObject.SetActive(entity.IsVisible);
            }
        }
        public List<State> States;
        public PlaybackSession PlaybackSession;
        private int playbackStateIndex = -1;


        // UI
        public PropertyToolPanel propertyToolPanel;
        public bool IsSelected = false;


        // Outline
        private Outline outline;
        public static Color DEFAULT_OUTLINE_COLOR = Color.white;
        public static float DEFAULT_OUTLINE_WIDTH = 2;
        private List<string> preventOutlineList = new List<string> {
            "mediaroom",
            "avatar",
            "tracker"
        };

        // Hover Info
        private EntityHoverInfo hoverInfo;
        private bool showHoverInfoAlways = false;


        // Visibility
        public delegate void VisibleChangedHandler(bool visible);

        public event VisibleChangedHandler OnVisibleChanged;

        public delegate void SelectedChangedHandler(bool visible);

        public event SelectedChangedHandler OnSelectedChanged;

        public int PlaybackStateIndex
        {
            get
            {
                // Prevent that playbackStateIndex produces an out of range exception at the end of a session
                if (States.Count > playbackStateIndex)
                {
                    return playbackStateIndex;
                }
                else
                {
                    return States.Count - 1;
                }
            }
            set
            {
                playbackStateIndex = value;
            }
        }

        public bool Visible
        {
            get { return gameObject.activeSelf; }
            set
            {
                if (Entity.IsVisible)
                {
                    gameObject.SetActive(value);
                }
            }
        }

        public bool Hide
        {
            get { return !Entity.IsVisible; }
            set
            {
                if (value == !Entity.IsVisible)
                    return;

                Entity.IsVisible = !value;
                if (!Entity.IsVisible)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    if (PlaybackStateIndex > -1)
                    {
                        gameObject.SetActive(States[PlaybackStateIndex].status == "active");
                    }
                }

                OnEntityChanged(this);
            }
        }

        public State GetCurrentState
        {
            get
            {
                if (PlaybackStateIndex > -1)
                    return States[PlaybackStateIndex];
                return null;
            }
        }

        // FIXME: workaround for syncing with web tools
        private void Awake()
        {
            ActiveEntities.Add(this);
        }


        private void Start()
        {
            OnSelectedChanged += ShowDefaultOutline;
        }

        public object GetData(string attribute)
        {
            return Entity.GetData(attribute) ?? GetCurrentState?.GetData(attribute);
        }

        private void ShowDefaultOutline(bool show)
        {
            if (!OutlineAllowed()) return;
            if (!outline) outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = DEFAULT_OUTLINE_COLOR;
            outline.OutlineWidth = DEFAULT_OUTLINE_WIDTH;
            outline.enabled = show;
        }

        public void ShowOutline(Color color)
        {
            if (!OutlineAllowed()) return;
            if (!outline) outline = gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.OutlineColor = color;
        }

        public void HideOutline()
        {
            if (outline)
            {
                outline.enabled = false;
            }
        }

        public void ShowHoverInfo()
        {
            if (showHoverInfoAlways) return;
            if (!gameObject.activeSelf) return;
            if (!hoverInfo)
            {
                GameObject hoverInfoGameObject = Instantiate(XRUIPrefabs.Instance.EntityHoverInfoPrefab);
                hoverInfo = hoverInfoGameObject.GetComponent<EntityHoverInfo>();
                hoverInfo.EntityGameObject = this;
                hoverInfoGameObject.transform.name = "HoverInfo " + Entity.name;
            }
            hoverInfo.gameObject.SetActive(true);
        }

        public void ShowHoverInfoAlways()
        {
            ShowHoverInfo();
            showHoverInfoAlways = true;
        }

        public void HideHoverInfo(bool force = false)
        {
            if (!force && showHoverInfoAlways) return;
            if (hoverInfo) hoverInfo.gameObject.SetActive(false);
        }

        public void DisableHoverInfoAlways()
        {
            showHoverInfoAlways = false;
            HideHoverInfo();
        }

        // private void ShowSessionOutline(bool show)
        // {
        //     if (show)
        //     {
        //         if (!IsSelected)
        //         {
        //             ShowOutline(PlaybackSession.SessionColor);
        //         }
        //     }
        // }

        // private GameObject selectionSphere;

        private bool OutlineAllowed()
        {
            // HACKY: Objects with many polygons like the mediaroom have problems with outline
            foreach (string objectName in preventOutlineList)
            {
                if ((Entity.filePaths != null && Entity.filePaths.ToLower().Contains(objectName)) || (Entity.attachments != null && Entity.attachments.Any(a => a.content.ToLower().Contains(objectName))))
                {
                    return false;
                }
            }
            return true;
        }

        void Update()
        {
            if (PlaybackSession.ShowOutline && !IsSelected && (!outline || outline.OutlineColor != PlaybackSession.SessionColor || !outline.enabled))
            {
                ShowOutline(PlaybackSession.SessionColor);
            }
            else if (!PlaybackSession.ShowOutline)
            {
                if (!IsSelected)
                {
                    HideOutline();
                }
                else
                {
                    ShowDefaultOutline(true);
                }
            }

            if (!showHoverInfoAlways && ShowAllEntityNamesButton.Instance.ShowAllEntityNames)
            {
                ShowHoverInfoAlways();
            }
            else if (showHoverInfoAlways && !ShowAllEntityNamesButton.Instance.ShowAllEntityNames)
            {
                DisableHoverInfoAlways();
            }
        }

        void OnDisable()
        {
            HideHoverInfo(true);
        }

        void OnDestroy()
        {
            ActiveEntities.Remove(this);
            if (hoverInfo) Destroy(hoverInfo.gameObject);
        }

        private GameObject GenerateSelectionSphere()
        {
            if (TryGetComponent(out Collider collider))
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphere.GetComponent<Collider>());
                sphere.GetComponent<MeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");
                sphere.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.1f);
                return sphere;
            }

            return null;
        }

        public void SetVisible(bool value)
        {
            Visible = value;
            OnVisibleChanged?.Invoke(value);
        }

        public void ChangeSelected()
        {
            IsSelected = !IsSelected;
            OnSelectedChanged?.Invoke(IsSelected);
        }

        public static EntityGameObject IsEntityGameObject(GameObject gameObject)
        {
            EntityGameObject entityGameObject = null;
            if (entityGameObject = gameObject.GetComponent<EntityGameObject>())
            {

            }
            else if (gameObject.transform.parent && gameObject.transform.parent.name != DataPlayerObjects.Instance.gameObject.name)
            {
                entityGameObject = gameObject.transform.parent.GetComponent<EntityGameObject>();
            }
            return entityGameObject;
        }
    }
}