#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;

namespace ARCeye
{
    [ExecuteInEditMode]
    public class POIIconInfoHelper : MonoBehaviour
    {
        [SerializeField]
        private Object m_POIColorFolder;

        [SerializeField]
        private Object m_POIGrayFolder;

        [SerializeField]
        private SpriteAtlas m_POIColorAtlas;

        [SerializeField]
        private SpriteAtlas m_POIGrayAtlas;

        [SerializeField]
        private List<POIIconInfoItem> m_POIColorList;

        [SerializeField]
        private List<POIIconInfoItem> m_POIGrayList;


        private string poiColorPath {
            get {
                return AssetDatabase.GetAssetPath(m_POIColorFolder);
            }
        }

        private string poiGrayPath {
            get {
                return AssetDatabase.GetAssetPath(m_POIGrayFolder);
            }
        }


        public void ConfigurePOILists()
        {
            if(!CheckFolderReferenceValidation())
            {
                return;
            }

            ConfigurePOIColorList();
            ConfigurePOIGrayList();
        }


        // POI Icon의 이름이 '이름_dpCode' 형태인 경우.
        public void ConfigurePOIColorList()
        {
            // 배열 초기화.
            m_POIColorList = new List<POIIconInfoItem>();

            // 선택된 경로 하위의 모든 Texture2D를 가져옴.
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { poiColorPath });

            Sprite[] sprites = new Sprite[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                string[] elem = sprites[i].name.Split("_");
                string name = elem[0];
                int dpCode = int.Parse(elem[1]);

                POIIconInfoItem item = new POIIconInfoItem();
                item.dpCode = dpCode;
                item.icon = sprites[i];

                m_POIColorList.Add(item);
            }

            // 이름 오름차순으로 정렬.
            m_POIColorList.Sort((a, b) => a.icon.name.CompareTo(b.icon.name));
        }

        // m_POIColorList의 { dpCode : 이름 }의 관계를 이용하여 m_POIGrayList를 생성한다.
        // POI Icon의 이름이 'UI_이름' 형태인 경우.
        public void ConfigurePOIGrayList()
        {
            // 배열 초기화.
            m_POIGrayList = new List<POIIconInfoItem>();

            // 선택된 경로 하위의 모든 Texture2D를 가져옴.
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { poiGrayPath });

            // 선택된 디렉토리 하위의 모든 아이콘 파일들을 순회한다.
            // UI_이름 형태의 파일들을 순회하며, m_POIColorList에서 '이름'과 일치하는 값을 가지는 인스턴스를 찾아 dpCode를 매칭시킨다.
            Sprite[] sprites = new Sprite[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                
                string name = sprites[i].name;

                POIIconInfoItem matchedColorItem = m_POIColorList.Find(e => e.icon.name.ToLower().Contains(name.ToLower()));
                if(matchedColorItem != null)
                {
                    POIIconInfoItem item = new POIIconInfoItem();
                    item.dpCode = matchedColorItem.dpCode;
                    item.icon = sprites[i];

                    m_POIGrayList.Add(item);
                }
            }
        }

        // 할당된 POIColorFolder와 POIGrayFolder가 유효한 폴더 경로인지 확인.
        private bool CheckFolderReferenceValidation()
        {
            if(!IsDirectory(m_POIColorFolder))
            {
                Debug.Log("'POIColorFolder에 할당된 오브젝트가 디렉토리가 아님");
                return false;
            }
            if(!IsDirectory(m_POIGrayFolder))
            {
                Debug.Log("'POIGrayFolder에 할당된 오브젝트가 디렉토리가 아님");
                return false;
            }
            return true;
        }

        private bool IsDirectory(Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            return AssetDatabase.IsValidFolder(assetPath);   
        }
    }

    [CustomEditor(typeof(POIIconInfoHelper))]
    public class POIIconInfoHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            string message = "'초기화' 버튼을 눌러 'POI Color List'와 'POI Gray List'를 생성한다. 생성이 완료되면 각 리스트를 'POIIconInfo' ScriptableObject에 복사한다.";

            EditorGUILayout.HelpBox(message, MessageType.Info);

            if(GUILayout.Button("초기화"))
            {
                ((POIIconInfoHelper) target).ConfigurePOILists();
            }
        }
    }
}
#endif