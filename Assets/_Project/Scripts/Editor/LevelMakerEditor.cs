﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SweetSugar.Scriptable.Rewards;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.GUI;
using SweetSugar.Scripts.GUI.Boost;
using SweetSugar.Scripts.Integrations;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.Items._Interfaces;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.Level.ItemsPerLevel.Editor;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.TargetScripts.TargetEditor;
using SweetSugar.Scripts.TargetScripts.TargetSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SweetSugar.Scripts.Editor
{
    [InitializeOnLoad]
    public class LevelMakerEditor : EditorWindow
    {
        //private bool groupEnabled;
        //private bool myBool = true;
        //private float myFloat = 1.23f;
        private int levelNumber = 1;
        private int subLevelNumber = 1;

        public int SubLevelNumber
        {
            get { return subLevelNumber; }
            set
            {
                subLevelNumber = value;
                levelData.currentSublevelIndex = subLevelNumber - 1;
            }
        }

        private int subLevelNumberTotal = 1;

        private SquareTypes squareType;
        private ItemForEditor itemBrush;
        private string FileName = "1_1.txt";
        private Vector2 scrollViewVector;
        private bool update;
        private static int selected;
        private string[] toolbarStrings = {"Editor", "Settings"};
        
        //Added_feature
        private string[] sectionsString = {"Blocks", "Items", "Directions", "Teleports", "Spawners"};

        private static LevelMakerEditor window;
        //private bool score_settings;
        //private bool target_description_show;
        private IEnumerable<ItemForEditor>[] itemsForEditor;
        private IEnumerable<ItemForEditor> itemsForEditorFull;
        string levelPath = "Assets/SweetSugar/Resources/Levels/";

        private bool enableGoogleAdsProcessing;
        private bool enableChartboostAdsProcessing;
        private LevelData levelData;
        private Texture[] arrows = new Texture[6];
        private Texture[] teleports = new Texture[2];
        private Texture[] arrows_enter = new Texture[4];
        private LevelScriptable levelScriptable;

        //Added_feature
        private Texture[] spawner = new Texture[10];
        private Texture[] Rotation = new Texture[10];
        private Spawners spawnerType;
        //private bool RemoveFlag = false;
        //private bool EditFlag = false;
        private SingleSpawn EditableSpawner;
        private SpawnerState CurrentSpawnerState;
        private IEnumerable<SpawnerState> _SpawnerStates;

        private enum SpawnerState
        {
            None,
            Add,
            Remove,
            Edit,
            Clear
        }

        [MenuItem("Sweet Sugar/Level editor")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (LevelMakerEditor) GetWindow(typeof(LevelMakerEditor), false, "Game editor");
            window.Show();
        }

        [MenuItem("Sweet Sugar/Settings/Game settings")]
        public static void GameSettings()
        {
            Init();
            selected = 1;
        }

        public static void ShowWindow()
        {
            GetWindow(typeof(LevelMakerEditor));
        }

        private void OnFocus()
        {
            window = (LevelMakerEditor) GetWindow(typeof(LevelMakerEditor));
            levelScriptable = Resources.Load("Levels/LevelScriptable") as LevelScriptable;
            targetEditorScriptable =
                AssetDatabase.LoadAssetAtPath<TargetEditorScriptable>(
                    "Assets/SweetSugar/Resources/Levels/TargetEditorScriptable.asset");
            LoadLevel(levelNumber);

            if (levelData != null)
            {
                if (levelData.GetField(subLevelNumber - 1).maxRows <= 0)
                    levelData.GetField(subLevelNumber - 1).maxRows = 9;
                if (levelData.GetField(subLevelNumber - 1).maxCols <= 0)
                    levelData.GetField(subLevelNumber - 1).maxCols = 9;
                Initialize();
            }

            customSkin = Resources.Load("SweetSkin") as GUISkin;
            separateBar =
                (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/Items/obstacle-bar.png",
                    typeof(Texture));
            arrows[0] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/arrow.png", typeof(Texture));
            arrows[1] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/arrow_left.png", typeof(Texture));
            arrows[2] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/arrow_right.png", typeof(Texture));
            arrows[3] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/arrow_up.png", typeof(Texture));
            arrows[4] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/circle arrow.png", typeof(Texture));
            arrows[5] = (Texture) AssetDatabase.LoadAssetAtPath(
                "Assets/SweetSugar/Textures_png/EditorSprites/circle arrows.png", typeof(Texture));

            teleports[0] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/teleport_icon1.png", typeof(Texture));
            teleports[1] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/teleport_icon2.png", typeof(Texture));

            arrows_enter[0] =
                (Texture) AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red.png",
                    typeof(Texture));
            arrows_enter[1] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_left.png", typeof(Texture));
            arrows_enter[2] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_right.png", typeof(Texture));
            arrows_enter[3] =
                (Texture) AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_up.png",
                    typeof(Texture));


            //Added_feature
            spawner[0] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/Dispenser_Top.png", typeof(Texture));
            spawner[1] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/Dispenser_Down.png", typeof(Texture));
            spawner[2] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/Dispenser_Left.png", typeof(Texture));
            spawner[3] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/Dispenser_Right.png", typeof(Texture));

            Rotation[0] =
                (Texture) AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/LeftRotation.png",
                    typeof(Texture));
            Rotation[1] =
                (Texture) AssetDatabase.LoadAssetAtPath(
                    "Assets/SweetSugar/Textures_png/EditorSprites/RightRotation.png", typeof(Texture));


            if (EditorSceneManager.GetActiveScene().name == "game")
            {
                LevelManager lm = Camera.main.GetComponent<LevelManager>();
                InitScript initscript = Camera.main.GetComponent<InitScript>();
            }

            //gotFocus = true;
        }

        private void OnLostFocus()
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
            //gotFocus = false;
        }

        private void Initialize()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            mat = new Material(shader);
            subLevelNumberTotal = GetSubLevelsCount();
            if (levelNumber < 1)
                levelNumber = 1;
            var num = 0;
            var simpleItem = Resources.Load<GameObject>("Items/Item").GetComponent<ItemSimple>();
            var simpleitems = simpleItem.GetComponent<IColorableComponent>().GetSprites(levelNumber)
                .Select(i => new ItemForEditor
                {
                    Item = simpleItem.gameObject,
                    ItemType = simpleItem.currentType,
                    Texture = i.texture,
                    order = 100
                }).ToList().ForEachY(i =>
                {
                    i.Color = num;
                    num++;
                });
            Resources.LoadAll("Items");
            var bonusitems = Resources.LoadAll<EditorIcon>("Items")
                .Select(i => (EditorIcon) i)
                .GroupBy(i => i.GetType())
                .Select(
                    i => /* i.Key == typeof(ItemMarmalade) ? i.Where(x => ((ItemMarmalade)x).secondItem == null) : */ i)
                .SelectMany(group => group)
                .Select(i => new ItemForEditor
                {
                    Item = i.gameObject,
                    Color = color,
                    ItemType = i.itemType,
                    colors = i.GetComponent<IColorableComponent>(),
                    Texture = i.GetComponent<Item>().sprRenderer.FirstOrDefault().sprite.texture,
                    order = 100
                });
            packageTexture = bonusitems.First(i => i.ItemType == ItemsTypes.PACKAGE).Texture;
            var l1 = bonusitems.ToList();
            // for (int i = 0; i < 5; i++)
            // {
            //     var item = bonusitems.First(x => x.ItemType == ItemsTypes.PACKAGE);
            //     l1.Add(item.DeepCopy());
            // }

            var list = l1.GroupBy(i => i.ItemType).Select(group => group.OrderBy(i => i.Item.name).ToArray()).ToArray()
                .ForEachY(group =>
                {
                    num = 0;
                    group.ForEachY(i =>
                    {
                        i.Color = num;
                        num++;
                    });
                }).SelectMany(i => i).ToArray();
            // for (int i = 0; i < list.Count(); i++)
            // {
            //     if (list[i].ItemType == ItemsTypes.PACKAGE)
            //     {
            //         list[i].Texture = list[i].Texture.AlphaBlend(simpleitems.ElementAt(list[i].Color).Texture);
            //     }
            // }
            itemsForEditorFull = simpleitems.Union(list);
            itemsForEditor = simpleitems.Union(bonusitems.GroupBy(i => i.ItemType).SelectMany(i => i))
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(i => i.Index / 6)
                .Select(i => i.Select(x => x.Value))
                .ToArray();
            if (levelData.target.prefabs.All(i => i.GetComponent<IColorableComponent>()))
            {
                for (var index = 0; index < levelData.subTargetsContainers.Count; index++)
                {
                    var levelDataSubTargetsContainer = levelData.subTargetsContainers[index];
                    levelDataSubTargetsContainer.extraObject = levelDataSubTargetsContainer.targetPrefab
                        .GetComponent<IColorableComponent>().GetSprite(levelNumber, index);
                }
            }

            winReward = Resources.Load<WinReward>("Scriptable/WinReward");
            
            _squareTypeItems = EnumUtil.GetValues<SquareTypes>();

            _SpawnerStates = EnumUtil.GetValues<SpawnerState>();

            _squareTypeItems = EnumUtil.GetValues<SquareTypes>();
            squareTypeGroupped = _squareTypeItems.Select((x, i) => new {Index = i, Value = x}).GroupBy(i => i.Index / 6)
                .Select(i => i.Select(x => x.Value))
                .ToArray();
            var squareTypeItems = _squareTypeItems;
            var squareTypeses = squareTypeItems as SquareTypes[] ?? squareTypeItems.ToArray();
            squareTextures = new Texture2DSize[squareTypeses.Count()];
            for (var index = 0; index < squareTypeses.Length; index++)
            {
                SquareTypes squareTypeItem = squareTypeses[index];
                if (squareTypeItem == SquareTypes.NONE) continue;
                var squareTexture = Square.GetSquareTexture(squareTypeItem);
                squareTextures[index] = squareTexture;
            }
        }

        private void InitializeSublevel()
        {
            levelData.GetField(subLevelNumber - 1).levelSquares =
                new SquareBlocks[levelData.GetField(subLevelNumber - 1).maxCols *
                                 levelData.GetField(subLevelNumber - 1).maxRows];
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                SquareBlocks sqBlocks = new SquareBlocks();
                sqBlocks.block = SquareTypes.EmptySquare;
                sqBlocks.obstacle = SquareTypes.NONE;

                levelData.GetField(subLevelNumber - 1).levelSquares[i] = sqBlocks;
            }

            ResetDirection();
        }

        private void OnGUI()
        {
//        GUI.skin = customSkin;
            if (!levelData.fields.Any())
            {
                OnFocus();
                return;
            }

            //		if (!gotFocus) return;
            UnityEngine.GUI.changed = false;


            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            int oldSelected = selected;
            selected = GUILayout.Toolbar(selected, toolbarStrings, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            scrollViewVector =
                EditorGUILayout.BeginScrollView(scrollViewVector, GUILayout.Width(window.position.width));

            if (oldSelected != selected)
                scrollViewVector = Vector2.zero;

            if (selected == 0 && levelData.fields.Any())
            {
                if (levelData != null)
                {
                    //                if (EditorSceneManager.GetActiveScene().name == "game")
                    {
                        GUILevelSelector();
                        GUILayout.Space(10);


                        GUILimit();
                        GUILayout.Space(10);

                        GUIColorLimit();
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Bomb timer", GUILayout.Width(80));
                        GUILayout.Space(70);
                        levelData.GetField(subLevelNumber - 1).bombTimer = EditorGUILayout.IntField("",
                            levelData.GetField(subLevelNumber - 1).bombTimer, GUILayout.Width(50));
                        GUILayout.EndHorizontal();
                        
                        //spawner exits var, enable if any spawner, its handy job
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Spawner Exits", GUILayout.Width(100));
                        GUILayout.Space(50);
                        levelData.SpawnerExits = EditorGUILayout.Toggle(levelData.SpawnerExits, GUILayout.Width(30));
                        GUILayout.Space(5);
                        GUILayout.Label("<-- Enable only if you set ingredient as spawner.", GUILayout.Width(400));
                        GUILayout.Space(50);
                        GUILayout.EndHorizontal();
                        
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Ingredient Generate Only From Spawner.", GUILayout.Width(230));
                        GUILayout.Space(50);
                        levelData.generateIngredientOnlyFromSpawner = EditorGUILayout.Toggle(levelData.generateIngredientOnlyFromSpawner, GUILayout.Width(50));
                        GUILayout.Space(5);
                        GUILayout.Label("<-- Enable only if you want to generate ingredient only from spawner.",
                                GUILayout.Width(400));
                        GUILayout.Space(50);
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUIStars();
                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();
                        {
//                        GUIMamalade();
                            //                        GUILayout.Space(10);

                            GUINoRegen();
                            //                        GUILayout.Space(10);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);

                        GUISwitchSubLevel();
                        GUILayout.Space(10);

                        GUITarget();
                        GUILayout.Space(10);

                        GUILevelSize();
                        GUILayout.Space(10);

                        GUISections();
                        GUILayout.Space(10);

                        if (section == 0)
                        {
                            GUIBlocks();
                            GUILayout.Space(10);
                        }

                        if (section == 1)
                        {
                            GUIItems();
                            GUILayout.Space(10);
                        }

                        if (section == 2)
                        {
                            GUIDirections();
                            GUILayout.Space(10);
                        }

                        if (section == 3)
                        {
                            GUITeleport();
                            GUILayout.Space(10);
                        }

                        //Added_feature
                        if (section == 4)
                        {
                            GUISpawners();
                            GUILayout.Space(10);
                        }
                        GUIGameField();
                    }
                    //                else
                    //                    GUIShowWarning();
                }
            }
            else if (selected == 1)
            {
                
            }
            EditorGUILayout.EndScrollView();
            if (UnityEngine.GUI.changed && !EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();

            if (enableGoogleAdsProcessing)
                RunOnceGoogle();

            if (enableChartboostAdsProcessing)
                RunOnceChartboost();
        }

        private void GUISwitchSubLevel()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent("Switch sub level if no match", "Switch sub level if no match"),
                    GUILayout.Width(150));
                bool switchSublevelNoMatch = false;
                switchSublevelNoMatch = EditorGUILayout.Toggle(levelData.GetField().switchSublevelNoMatch, GUILayout.Width(50));
                if (switchSublevelNoMatch != levelData.GetField().switchSublevelNoMatch)
                {
                    levelData.GetField().switchSublevelNoMatch = switchSublevelNoMatch;
                    dirtyLevel = true;
                    // SaveLevel();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void CheckSeparateLevels()
        {
            // if (GUILayout.Button("Re-generate separate levels", GUILayout.Width(300)))
            // {
            //     var ls = Resources.Load("Levels/LevelScriptable") as LevelScriptable;
            //     for (int i = 0; i < ls.levels.Count(); i++)
            //     {
            //         ScriptableLevelManager.CreateFileLevel( i+1, ls.levels[i]);
            //     }
            // }
        }


        private void GUIShowWarning()
        {
            GUILayout.Space(100);
            GUILayout.Label("CAUTION!", EditorStyles.boldLabel, GUILayout.Width(600));
            GUILayout.Label("Please open scene - game ( Assets/SweetSugar/Scenes/game.unity )", EditorStyles.boldLabel,
                GUILayout.Width(600));
        }
        #region GUIDialogs

        private void GUIDialogs()
        {
            GUILayout.Label("GUI elements:", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Space(10);
            ShowMenuButton("Menu Play", "MenuPlay");
            ShowMenuButton("Menu Complete", "MenuComplete");
            ShowMenuButton("Menu Failed", "MenuFailed");
            ShowMenuButton("PreFailed", "PreFailed");
//        ShowMenuButton("Pause", "MenuPause");
            ShowMenuButton("Boost Shop", "BoostShop");
            ShowMenuButton("Live Shop", "LiveShop");
//        ShowMenuButton("Boost Info", "BoostInfo");
            ShowMenuButton("Settings", "Settings");
//        ShowMenuButton("Tutorial", "Tutorial");
        }

        private void ShowMenuButton(string label, string name)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.label, GUILayout.Width(100));

            GUILayout.EndHorizontal();
        }

        public static void SetSearchFilter(string filter, int filterMode)
        {
            SearchableEditorWindow[] windows =
                (SearchableEditorWindow[]) Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
            SearchableEditorWindow hierarchy = null;
            foreach (SearchableEditorWindow window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {
                    hierarchy = window;
                    break;
                }
            }

            if (hierarchy == null)
                return;

            MethodInfo setSearchType =
                typeof(SearchableEditorWindow).GetMethod("SetSearchFilter",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = {filter, filterMode, false};

            setSearchType.Invoke(hierarchy, parameters);
        }

        #endregion

        #region ads_settings

        private void RunOnceGoogle()
        {
            if (Directory.Exists("Assets/PlayServicesResolver"))
            {
                Debug.Log("assets try reimport");
            }
        }

        private void RunOnceChartboost()
        {
            enableChartboostAdsProcessing = false;
        }
       

        #endregion
        
        #region settings

        private bool share_settings;
        private bool target_settings;

        private void ResetSettings()
        {
            LevelManager lm = Camera.main.GetComponent<LevelManager>();
            lm.showPopupScores = false;
            lm.scoresColors[0] = new Color(251f / 255f, 80 / 255f, 1 / 255f);
            lm.scoresColors[1] = new Color(255 / 255f, 193 / 255f, 22 / 255f);
            lm.scoresColors[2] = new Color(237 / 255f, 13 / 255f, 233 / 255f);
            lm.scoresColors[3] = new Color(41 / 255f, 157 / 255f, 255 / 255f);
            lm.scoresColors[4] = new Color(255 / 255f, 255 / 255f, 38 / 255f);
            lm.scoresColors[5] = new Color(37 / 255f, 219 / 255f, 0 / 255f);

            lm.scoresColorsOutline[0] = new Color(138 / 255f, 1 / 255f, 0 / 255f);
            lm.scoresColorsOutline[1] = new Color(184 / 255f, 77 / 255f, 1 / 255f);
            lm.scoresColorsOutline[2] = new Color(128 / 255f, 0 / 255f, 128 / 255f);
            lm.scoresColorsOutline[3] = new Color(0 / 255f, 64 / 255f, 182 / 255f);
            lm.scoresColorsOutline[4] = new Color(174 / 255f, 104 / 255f, 0 / 255f);
            lm.scoresColorsOutline[5] = new Color(19 / 255f, 111 / 255f, 0 / 255f);

            InitScript initscript = Camera.main.GetComponent<InitScript>();
            initscript.CapOfLife = 5;
            initscript.TotalTimeForRestLifeHours = 0;
            initscript.TotalTimeForRestLifeMin = 15;
            initscript.TotalTimeForRestLifeSec = 0;
            lm.FailedCost = 12;
            lm.ExtraFailedMoves = 5;
            lm.ExtraFailedSecs = 30;
            EditorUtility.SetDirty(lm);
        }

        #endregion

        private string GetLevelKey(int number)
        {
            return string.Format("Level.{0:000}.StarsCount", number);
        }

        #region leveleditor

        private void TestLevel(bool playNow = true, bool testByPlay = true)
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
            if (EditorSceneManager.GetActiveScene().name != "game")
                EditorSceneManager.OpenScene("Assets/SweetSugar/Scenes/game.unity");
            LevelManager lm = Camera.main.GetComponent<LevelManager>();
            PlayerPrefs.SetInt("OpenLevelTest", levelNumber);
            PlayerPrefs.SetInt("OpenLevel", levelNumber);
            PlayerPrefs.Save();

            if (playNow)
            {
                if (EditorApplication.isPlaying)
                    EditorApplication.isPlaying = false;
                else
                    EditorApplication.isPlaying = true;
            }
        }


        private void GUILevelSelector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Level editor", GUILayout.Width(150));
                if (GUILayout.Button("Test level", GUILayout.Width(158)))
                {
                    TestLevel();
                }

                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    dirtyLevel = true;
                    SaveLevel(levelNumber);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Level", GUILayout.Width(50));
                GUILayout.Space(100);
                if (GUILayout.Button("<<", GUILayout.Width(50)))
                {
                    PreviousLevel();
                    OpenTarget();
                }

                string changeLvl = GUILayout.TextField(" " + levelNumber, GUILayout.Width(50));
                try
                {
                    if (int.Parse(changeLvl) != levelNumber)
                    {
                        subLevelNumber = 1;
                        if (LoadLevel(int.Parse(changeLvl)))
                        {
                            levelNumber = int.Parse(changeLvl);
                            OpenTarget();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (GUILayout.Button(">>", GUILayout.Width(50)))
                {
                    NextLevel();
                    OpenTarget();
                }

                if (GUILayout.Button(new GUIContent("+", "add level"), GUILayout.Width(20)))
                {
                    AddLevel();
                }

                if (GUILayout.Button(new GUIContent("- ", "remove current level"), GUILayout.Width(20)))
                {
                    RemoveLevel();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            //Sub Level

            GUILayout.BeginHorizontal();
            {
//            GUILayout.Space(60);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sub Level", GUILayout.Width(80));
                GUILayout.Space(70);
                if (GUILayout.Button("<<", GUILayout.Width(50)))
                {
                    PreviousSubLevel();
                }

                GUILayout.Label(" " + SubLevelNumber + " / " + subLevelNumberTotal, GUILayout.Width(50));

                if (GUILayout.Button(">>", GUILayout.Width(50)))
                {
                    NextSubLevel();
                }

                if (GUILayout.Button(new GUIContent("+", "add sublevel"), GUILayout.Width(20)))
                {
                    AddSubLevel();
                }

                if (GUILayout.Button(new GUIContent("- ", "remove current sublevel"), GUILayout.Width(20)))
                {
                    RemoveSubLevel();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        private void GUILevelSize()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            int oldValue = levelData.GetField(subLevelNumber - 1).maxRows +
                           levelData.GetField(subLevelNumber - 1).maxCols;
            GUILayout.Label("Columns", GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.GetField(subLevelNumber - 1).maxCols = EditorGUILayout.IntField("",
                levelData.GetField(subLevelNumber - 1).maxCols, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rows", GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.GetField(subLevelNumber - 1).maxRows = EditorGUILayout.IntField("",
                levelData.GetField(subLevelNumber - 1).maxRows, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (levelData.GetField(subLevelNumber - 1).maxRows < 3)
                levelData.GetField(subLevelNumber - 1).maxRows = 3;
            if (levelData.GetField(subLevelNumber - 1).maxCols < 3)
                levelData.GetField(subLevelNumber - 1).maxCols = 3;
            // if (levelData.GetField(subLevelNumber - 1).maxRows > 11)
            //     levelData.GetField(subLevelNumber - 1).maxRows = 11;
            if (levelData.GetField(subLevelNumber - 1).maxCols > 11)
                levelData.GetField(subLevelNumber - 1).maxCols = 11;
            if (oldValue != levelData.GetField(subLevelNumber - 1).maxRows +
                levelData.GetField(subLevelNumber - 1).maxCols)
            {
                Initialize();
                InitializeSublevel();
                ClearLevel();
                dirtyLevel = true;
                // SaveLevel();
            }
        }

        private void GUILimit()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Limit", EditorStyles.label, GUILayout.Width(50));
                GUILayout.Space(100);
                LIMIT limitTypeSave = levelData.limitType;
                int oldLimit = levelData.limit;
                levelData.limitType = (LIMIT) EditorGUILayout.EnumPopup(levelData.limitType, GUILayout.Width(93));
                if (levelData.limitType == LIMIT.MOVES)
                    levelData.limit = EditorGUILayout.IntField(levelData.limit, GUILayout.Width(50));
                else
                {
                    GUILayout.BeginHorizontal();
                    int limitMin = EditorGUILayout.IntField(levelData.limit / 60, GUILayout.Width(30));
                    GUILayout.Label(":", GUILayout.Width(10));
                    int limitSec =
                        EditorGUILayout.IntField(levelData.limit - (levelData.limit / 60) * 60, GUILayout.Width(30));
                    levelData.limit = limitMin * 60 + limitSec;
                    GUILayout.EndHorizontal();
                }

                if (levelData.limit <= 0)
                    levelData.limit = 1;
                if (limitTypeSave != levelData.limitType || oldLimit != levelData.limit)
                    dirtyLevel = true;
                // 	SaveLevel();
            }
            GUILayout.EndHorizontal();
        }

        private void GUIColorLimit()
        {
            GUILayout.BeginHorizontal();

            int saveInt = levelData.colorLimit;
            GUILayout.Label("Color limit", EditorStyles.label, GUILayout.Width(100));
            GUILayout.Space(50);
            levelData.colorLimit = (int) GUILayout.HorizontalSlider(levelData.colorLimit, 3, 6, GUILayout.Width(100));
            levelData.colorLimit = EditorGUILayout.IntField("", levelData.colorLimit, GUILayout.Width(50));
            if (levelData.colorLimit < 3)
                levelData.colorLimit = 3;
            if (levelData.colorLimit > 6)
                levelData.colorLimit = 6;

            GUILayout.EndHorizontal();

            if (saveInt != levelData.colorLimit)
            {
                dirtyLevel = true;
                // SaveLevel();
            }
        }


        private void GUIStars()
        {
            GUILayout.BeginHorizontal();
//        GUILayout.Space(35);

            //GUILayout.BeginVertical();

            GUILayout.Label("Stars", GUILayout.Width(30));

            GUILayout.Space(120);
            GUILayout.BeginHorizontal();
            int s = 0;
            s = EditorGUILayout.IntField("", levelData.star1, GUILayout.Width(50));
            if (s != levelData.star1)
            {
                levelData.star1 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star1 <= 0)
                levelData.star1 = 100;
            s = EditorGUILayout.IntField("", levelData.star2, GUILayout.Width(50));
            if (s != levelData.star2)
            {
                levelData.star2 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star2 < levelData.star1)
                levelData.star2 = levelData.star1 + 10;
            s = EditorGUILayout.IntField("", levelData.star3, GUILayout.Width(50));
            if (s != levelData.star3)
            {
                levelData.star3 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star3 < levelData.star2)
            {
                levelData.star3 = levelData.star2 + 10;
                dirtyLevel = true;
                // SaveLevel();
            }

            GUILayout.EndHorizontal();
            //GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void GUIMamalade()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(30);

                GUILayout.Label("Marmalade", GUILayout.Width(70));
                bool s = false;
                s = EditorGUILayout.Toggle(levelData.enableMarmalade, GUILayout.Width(50));
                if (s != levelData.enableMarmalade)
                {
                    levelData.enableMarmalade = s;
                    dirtyLevel = true;
                    // SaveLevel();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GUINoRegen()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent("Don't Regen", "Don't regenerate if no matches possible"),
                    GUILayout.Width(80));
                bool s = false;
                GUILayout.Space(70);
                s = EditorGUILayout.Toggle(levelData.GetField().noRegenLevel, GUILayout.Width(50));
                if (s != levelData.GetField().noRegenLevel)
                {
                    levelData.GetField().noRegenLevel = s;
                    dirtyLevel = true;
                    // SaveLevel();
                }

                GUILayout.Label(new GUIContent("Scroll", "Scroll level to hide squares with no target items"),
                    GUILayout.Width(80));
                //bool scroll = false;
                GUILayout.Space(70);
                levelData.GetField().scrollLevel =
                    EditorGUILayout.Toggle(levelData.GetField().scrollLevel, GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();
        }

        void UpdateTarget(int newselectedTarget, TargetLevel targetLevel, TargetContainer target)
        {
            // levelData.SetTarget(newselectedTarget);
            targetLevel.ChangeTarget(target, newselectedTarget, levelData, targetEditorScriptable);
            dirtyLevel = true;
        }

        private void GUITarget()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Target", GUILayout.ExpandWidth(false));
                // target_settings = EditorGUILayout.Foldout(target_settings, "");
                GUILayout.Space(110);
                // if (target_settings)
                {
                    GUILayout.BeginVertical();
                    {
                        var targetLevel = GetTargetLevel();
                        int starsInd = targetEditorScriptable.targets.FindIndex(i => i.name == "Stars");
                        var subTargets = targetLevel.targets;
                        var targetObjects = subTargets.GroupBy(i => i.targetType.type).FirstOrDefault();
                        int selectedTarget = targetObjects?.FirstOrDefault().targetType.type ?? 0;
                        int newselectedTarget = selectedTarget;
                        GUILayout.BeginHorizontal();
                        {
                            newselectedTarget = EditorGUILayout.Popup(selectedTarget, levelData.GetTargetsNames(),
                                GUILayout.Width(100), GUILayout.ExpandWidth(false));
                            if (GUILayout.Button("Target editor", GUILayout.Width(160)))
                            {
                                OpenTarget();
                            }
                        }
                        GUILayout.EndHorizontal();

                        TargetContainer target = null;
                        if (targetObjects != null)
                        {
                            target = targetObjects.FirstOrDefault().targetType.GetTarget();
                            if (target.setCount == SetCount.Manually)
                            {
                                foreach (var targetObject in targetObjects)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        EditorGUILayout.LabelField(targetObject.sprites.FirstOrDefault().icon.name,
                                            GUILayout.Width(50));
                                        EditorGUI.BeginChangeCheck();
                                        targetObject.CountDrawer.count =
                                            EditorGUILayout.IntField(targetObject.CountDrawer.count,
                                                GUILayout.Width(50));
                                        if (EditorGUI.EndChangeCheck())
                                            targetLevel.saveData();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }

                        if (newselectedTarget != selectedTarget || update)
                        {
                            update = false;
                            UpdateTarget(newselectedTarget, targetLevel, target);
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tutorial", GUILayout.ExpandWidth(false));
                GUILayout.Space(103);
                int newselectedTutorial = EditorGUILayout.Popup(levelData.selectedTutorial, GetTutorialNames(),
                    GUILayout.Width(160), GUILayout.ExpandWidth(false));
                if (levelData.selectedTutorial != newselectedTutorial)
                {
                    levelData.selectedTutorial = newselectedTutorial;
                    dirtyLevel = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        string[] GetTutorialNames() => new[]
        {
            "Disabled", "SIMPLE", ItemsTypes.HORIZONTAL_STRIPED.ToString(), ItemsTypes.PACKAGE.ToString(),
            ItemsTypes.TimeBomb.ToString()
        };

        private void OpenTarget()
        {
            var asset = PrepareTargetLevel();

            Selection.activeObject = asset;
        }

        private TargetLevel PrepareTargetLevel()
        {
            var asset = GetTargetLevel();
            if (asset == null)
            {
                asset = CreateInstance<TargetLevel>();
                asset.name = "Level" + levelNumber;
                string assetPathAndName =
                    AssetDatabase.GenerateUniqueAssetPath("Assets/SweetSugar/Resources/Levels/Targets/TargetLevel" +
                                                          levelNumber + ".asset");
                AssetDatabase.CreateAsset(asset, assetPathAndName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return asset;
        }

        private TargetLevel GetTargetLevel()
        {
            var asset = Resources.Load<TargetLevel>("Levels/Targets/TargetLevel" + levelNumber);
            return asset;
        }


        private bool HideAnotherTargetBlock(string blockName)
        {
            var targets = levelData.GetTargetsNames();
            var currentTarget = levelData.target.name;
            if (IsBlockTarget(blockName, targets) && !blockName.Contains(currentTarget) &&
                (blockName.Contains("Jelly") || blockName.Contains("Sugar")))
                return true;
            return false;
        }

        private bool IsBlockTarget(string blockName, string[] targets)
        {
            var list = targets.Where(i => blockName.Contains(i));
            return list.Count() > 0;
        }

        private void GUISections()
        {
            GUILayout.BeginHorizontal();
            {
                var saveSetion = section;
                section = GUILayout.Toolbar(section, sectionsString, GUILayout.Width(450));
                //if (section != saveSetion && section == 3) squareBlockSelected = null;
                //Add_feature
                if (section != saveSetion && (section == 3 || section == 4)) squareBlockSelected = null;
            }
            GUILayout.EndHorizontal();
        }

        private void GUITeleport()
        {
            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button(new GUIContent("Clear", "clear all teleports"), GUILayout.Width(50),
                        GUILayout.Height(50)))
                    {
                        squareBlockSelected = null;
                        for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
                        {
                            levelData.GetField(subLevelNumber - 1).levelSquares[i].isEnterTeleport = false;
                            levelData.GetField(subLevelNumber - 1).levelSquares[i].teleportCoordinatesLinkedBack =
                                Vector2Int.one * -1;
                            levelData.GetField(subLevelNumber - 1).levelSquares[i].teleportCoordinatesLinked =
                                Vector2Int.one * -1;
                        }

                        dirtyLevel = true;
                        // SaveLevel();
                    }

                    GUILayout.Space(10);

                    GUILayout.Label("Click to the field, once for enter then for exit.", EditorStyles.boldLabel);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
        
        //Added_feature
        private int Column, Row = -1;

        private void GUISpawners()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Spawners:", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                        foreach (SpawnerState States in _SpawnerStates)
                        {
                            if (CurrentSpawnerState == States && (CurrentSpawnerState != SpawnerState.None &&
                                                                  CurrentSpawnerState != SpawnerState.Clear))
                                UnityEngine.GUI.backgroundColor = Color.gray;
                            if (GUILayout.Button(
                                new GUIContent(States.ToString() + "", "changes to " + States.ToString() + " State"),
                                GUILayout.Width(80), GUILayout.Height(50)))
                            {
                                CurrentSpawnerState = States;
                                //Debug.Log("" + CurrentSpawnerState);

                                switch (CurrentSpawnerState)
                                {
                                    case SpawnerState.Add:
                                    {
                                    }
                                        break;
                                    case SpawnerState.Clear:
                                    {
                                        for (int i = 0;
                                            i < levelData.GetField(subLevelNumber - 1).levelSquares.Length;
                                            i++)
                                        {
                                            levelData.GetField(subLevelNumber - 1).levelSquares[i].spawners =
                                                new List<SingleSpawn>();
                                        }

                                        dirtyLevel = true;
                                    }
                                        break;
                                    case SpawnerState.Edit:
                                    {
                                    }
                                        break;
                                    case SpawnerState.None:
                                    {
                                    }
                                        break;
                                    case SpawnerState.Remove:
                                    {
                                    }
                                        break;
                                    default:
                                        break;
                                }

                                Column = Row = -1;
                            }

                            UnityEngine.GUI.backgroundColor = Color.white;
                        }

                        UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                    }
                    GUILayout.EndHorizontal();

                    if (CurrentSpawnerState == SpawnerState.Edit)
                        if (Column >= 0 && Row >= 0)
                        {
                            SquareBlocks squareBlock = levelData.GetBlock(Row, Column);
                            if (squareBlock != null)
                            {
                                if (squareBlock.spawners.Count > 0)
                                {
                                    GUILayout.BeginHorizontal(GUILayout.Width(100));
                                    {
                                        //Dispenser Rotating Option
                                        GUILayout.BeginHorizontal(GUILayout.Width(100));
                                        {
                                            GUILayout.Label(" " + squareBlock.spawners[0].SpawnId,
                                                GUILayout.Width(100));
                                            if (GUILayout.Button(new GUIContent(Rotation[0], " Rotate Left"),
                                                GUILayout.Width(50), GUILayout.Height(50)))
                                            {
                                                squareBlock.spawners[0].rotationType =
                                                    CheckForRotation(squareBlock.spawners[0].rotationType, true);
                                            }

                                            if (GUILayout.Button(new GUIContent(Rotation[1], " Rotate right"),
                                                GUILayout.Width(50), GUILayout.Height(50)))
                                            {
                                                squareBlock.spawners[0].rotationType =
                                                    CheckForRotation(squareBlock.spawners[0].rotationType, false);
                                            }
                                        }
                                        GUILayout.EndHorizontal();

                                        //Selecting Spawner Type and selecting persentage of spawning

                                        GUILayout.BeginVertical(GUILayout.Width(100));
                                        {
                                            //make sure only one ingredient Dispenser
                                            if (squareBlock.spawners[0].SpawnersType == Spawners.Ingredient)
                                            {
                                                if (squareBlock.spawners[0].SpawnersType_2 == Spawners.Ingredient)
                                                    squareBlock.spawners[0].SpawnersType_2 = Spawners.None;
                                                else if (squareBlock.spawners[0].IngredentSpawner_01.Ingredient_01 &&
                                                         squareBlock.spawners[0].IngredentSpawner_01.Ingredient_02)
                                                {
                                                    squareBlock.spawners[0].SpawnersType_2 = Spawners.None;
                                                }
                                            }

                                            //First spawner
                                            squareBlock.spawners[0].SpawnersType =
                                                (Spawners) EditorGUILayout.EnumPopup(
                                                    squareBlock.spawners[0].SpawnersType, GUILayout.Width(250));
                                            if (squareBlock.spawners[0].SpawnersType != Spawners.Ingredient)
                                            {
                                                GUILayout.BeginVertical(GUILayout.Width(100));
                                                {
                                                    squareBlock.spawners[0].SpawnPersentage =
                                                        EditorGUILayout.FloatField(" Persentage ",
                                                            squareBlock.spawners[0].SpawnPersentage,
                                                            GUILayout.Width(300));
                                                }
                                                GUILayout.EndVertical();
                                            }
                                            else
                                            {
                                                //for ingredient
                                                GUILayout.BeginHorizontal(GUILayout.Width(100));
                                                {
                                                    GUILayout.Label(" Ingredient_01 ", GUILayout.Width(100));
                                                    squareBlock.spawners[0].IngredentSpawner_01.Ingredient_01 =
                                                        EditorGUILayout.Toggle(
                                                            squareBlock.spawners[0].IngredentSpawner_01.Ingredient_01,
                                                            GUILayout.Width(100));
                                                    GUILayout.Label(" Ingredient_02 ", GUILayout.Width(100));
                                                    squareBlock.spawners[0].IngredentSpawner_01.Ingredient_02 =
                                                        EditorGUILayout.Toggle(
                                                            squareBlock.spawners[0].IngredentSpawner_01.Ingredient_02,
                                                            GUILayout.Width(100));
                                                }
                                                GUILayout.EndHorizontal();

                                                GUILayout.Label(
                                                    " (!) This dispenser spawn ingredient one after another  ",
                                                    GUILayout.Width(700));
                                                
                                                
                                                GUILayout.Label(
                                                    " (!) Do not place the ingredients in field manually. (!)",
                                                    GUILayout.Width(700));
                                            }

                                            //second spawner
                                            squareBlock.spawners[0].SpawnersType_2 =
                                                (Spawners) EditorGUILayout.EnumPopup(
                                                    squareBlock.spawners[0].SpawnersType_2, GUILayout.Width(250));
                                            if (squareBlock.spawners[0].SpawnersType_2 != Spawners.Ingredient)
                                            {
                                                GUILayout.BeginVertical(GUILayout.Width(100));
                                                {
                                                    squareBlock.spawners[0].SpawnPersentage_2 =
                                                        EditorGUILayout.FloatField(" Persentage ",
                                                            squareBlock.spawners[0].SpawnPersentage_2,
                                                            GUILayout.Width(300));
                                                }
                                                GUILayout.EndVertical();
                                            }
                                            else
                                            {
                                                //for ingredient
                                                GUILayout.BeginHorizontal(GUILayout.Width(100));
                                                {
                                                    GUILayout.Label(" Ingredient_01 ", GUILayout.Width(100));
                                                    squareBlock.spawners[0].IngredentSpawner_02.Ingredient_01 =
                                                        EditorGUILayout.Toggle(
                                                            squareBlock.spawners[0].IngredentSpawner_02.Ingredient_01,
                                                            GUILayout.Width(50));
                                                    GUILayout.Label(" Ingredient_02 ", GUILayout.Width(100));
                                                    squareBlock.spawners[0].IngredentSpawner_02.Ingredient_02 =
                                                        EditorGUILayout.Toggle(
                                                            squareBlock.spawners[0].IngredentSpawner_02.Ingredient_02,
                                                            GUILayout.Width(50));
                                                }
                                                GUILayout.EndHorizontal();

                                                GUILayout.Label(
                                                    " (!) This dispenser spawn ingredient one after another  ",
                                                    GUILayout.Width(700));
                                            }
                                        }
                                        GUILayout.EndVertical();

                                        //GUILayout.BeginVertical(GUILayout.Width(100));
                                        //{

                                        //}
                                        //GUILayout.EndVertical();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        ////Added_feature
        //private void SetSpawnerData(SingleSpawn Spawner)
        //{
        //    levelData.GetField(SubLevelNumber - 1).Spawners[Spawner.position.x].Spawns[Spawner.position.y] = Spawner;
        //}


        private int arrow_index;
        private int selectDirectionType;

        private void GUIDirections()
        {
            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Tools:", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(new GUIContent(arrows_enter[0], "enter point on/off"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            selectDirectionType = 1;
                        }

                        if (GUILayout.Button(new GUIContent(arrows[4], "rotate one arrow"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            selectDirectionType = 0;
                        }

                        if (GUILayout.Button(new GUIContent(arrows[5], "rotate all"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            arrow_index = (int) Mathf.Repeat(arrow_index + 1, 4);
                            var squares = levelData.GetField(subLevelNumber - 1).levelSquares;
                            foreach (var item in squares)
                            {
                                item.direction = GetDirectionByIndex(arrow_index);
                            }

                            SetEnterPoint(GetDirectionByIndex(arrow_index));
                        }

                        if (GUILayout.Button(new GUIContent("Default", "default all direction"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            arrow_index = 0;
                            ResetDirection();
                            // SaveLevel();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void ResetDirection()
        {
            var squares = levelData.GetField(subLevelNumber - 1).levelSquares;
            foreach (var item in squares)
            {
                item.direction = GetDirectionByIndex(arrow_index);
            }

            SetEnterPoint(GetDirectionByIndex(arrow_index));
            dirtyLevel = true;
        }

        private void SetEnterPoint(Vector2 direction)
        {
            levelData.GetField(subLevelNumber - 1).levelSquares.ToList().ForEach(i => i.enterSquare = false);
            if (direction == Vector2.down)
            {
                for (int col = 0; col < levelData.GetField(subLevelNumber - 1).maxCols; col++)
                {
                    var squareBlocks = levelData.GetBlock(0, col);
                    if (squareBlocks.teleportCoordinatesLinkedBack == Vector2Int.one * -1)
                        squareBlocks.enterSquare = true;
                }
            }

            if (direction == Vector2.up)
            {
                for (int col = 0; col < levelData.GetField(subLevelNumber - 1).maxCols; col++)
                {
                    var squareBlocks = levelData.GetBlock(levelData.GetField(subLevelNumber - 1).maxRows - 1, col);
                    if (squareBlocks.teleportCoordinatesLinkedBack == Vector2Int.one * -1)
                        squareBlocks.enterSquare = true;
                }
            }

            if (direction == Vector2.left)
            {
                for (int row = 0; row < levelData.GetField(subLevelNumber - 1).maxRows; row++)
                {
                    var squareBlocks = levelData.GetBlock(row, levelData.GetField(subLevelNumber - 1).maxCols - 1);
                    if (squareBlocks.teleportCoordinatesLinkedBack == Vector2Int.one * -1)
                        squareBlocks.enterSquare = true;
                }
            }

            if (direction == Vector2.right)
            {
                for (int row = 0; row < levelData.GetField(subLevelNumber - 1).maxRows; row++)
                {
                    var squareBlocks = levelData.GetBlock(row, 0);
                    if (squareBlocks.teleportCoordinatesLinkedBack == Vector2Int.one * -1)
                        squareBlocks.enterSquare = true;
                }
            }
        }

        private int GetIndexByDirection(Vector2 direction)
        {
            if (direction == Vector2.right)
                return 3;
            if (direction == Vector2.left)
                return 1;
            if (direction == Vector2.up)
                return 2;
            return 0;
        }

        private Vector2 GetDirectionByIndex(int index)
        {
            if (index == 0) return Vector2.down;
            if (index == 1) return Vector2.left;
            if (index == 2) return Vector2.up;
            if (index == 3) return Vector2.right;
            return Vector2.down;
        }

        private Texture GetArrowByAngle(float angle)
        {
            return arrows[(int) (angle % 90)];
        }

        private Texture GetArrowByVector(Vector2 direction, bool enterPoint)
        {
            var arr = arrows;
            if (enterPoint) arr = arrows_enter;
            if (direction == Vector2.up)
                return arr[3];
            if (direction == Vector2.left)
                return arr[1];
            if (direction == Vector2.right)
                return arr[2];
            return arr[0];
        }

        private void GUIItems()
        {
            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Tip: right click to change sprite for level", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(new GUIContent("Clear", "clear all field"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            ClearItems();
                            // SaveLevel();
                        }

                        GUILayout.BeginVertical();
                        {
                            foreach (var val in itemsForEditor)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    foreach (var item in val)
                                    {
                                        GameObject item1 = item.Item;
                                        if (!item1) continue;
                                        if (item.ItemType != ItemsTypes.NONE)
                                            item.SetColor(color, levelNumber);
                                        var texture = item.Texture;
                                        if (item.ItemType == ItemsTypes.PACKAGE)
                                        {
                                            try
                                            {
                                                texture = packageTexture.AlphaBlend(item.Texture);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.Log(e);
                                            }
                                        }

                                        if (itemBrush != null && (itemBrush.ItemType == item.ItemType &&
                                                                  item.ItemType == ItemsTypes.NONE &&
                                                                  color == item.Color && itemBrush
                                                                      .Texture == item.Texture))
                                            UnityEngine.GUI.backgroundColor = Color.gray;
                                        else if (itemBrush != null && (itemBrush.ItemType == item.ItemType &&
                                                                       item.ItemType != ItemsTypes.NONE && itemBrush
                                                                           .Texture == item.Texture))
                                            UnityEngine.GUI.backgroundColor = Color.gray;
                                        if (GUILayout.Button(new GUIContent(texture, item1.name),
                                            GUILayout.Width(50), GUILayout.Height(50)))
                                        {
                                            Event current = Event.current;
                                            if (current.button == 1)
                                            {
                                                dirtyLevel = true;
                                                SaveLevel(levelNumber);
                                                ItemsPerLevelEditor.ShowWindow(item1, levelNumber);
                                            }
                                            else
                                            {
                                                itemBrush = item.DeepCopy();
                                                if (item.ItemType == ItemsTypes.NONE) color = itemBrush.Color;
                                            }
                                        }

                                        UnityEngine.GUI.backgroundColor = Color.white;
                                    }
                                }
                                if (val == itemsForEditor.Last())
                                {
                                    if (GUILayout.Button(new GUIContent("X", "Clear block"), GUILayout.Width(50),
                                        GUILayout.Height(50)))
                                    {
                                        itemBrush = null;
                                    }
                                }

                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void ClearItems()
        {
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                levelData.GetField(subLevelNumber - 1).levelSquares[i].item = null;
            }

            dirtyLevel = true;
        }

        private void GUIBlocks()
        {
            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Tools:", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                        foreach (SquareTypes squareTypeItem in _squareTypeItems)
                        {
//                                if (HideAnotherTargetBlock(squareTypeItem.ToString())) continue;
                            if (squareTypeItem == SquareTypes.NONE) continue;
                            var squareTexture = Square.GetSquareTexture(squareTypeItem);
                            if (squareType == squareTypeItem)
                                UnityEngine.GUI.backgroundColor = Color.gray;
                            if (squareTexture.Texture2D != null && GUILayout.Button(
                                    new GUIContent(squareTexture.Texture2D, squareTypeItem.ToString()),
                                    GUILayout.Width(50), GUILayout.Height(50)))
                            {
                                squareType = squareTypeItem;
                                deleteBlock = false;
                                separateBarBrush = false;
                            }

                            UnityEngine.GUI.backgroundColor = Color.white;
                        }

                        UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        if (separateBarBrush)
                            UnityEngine.GUI.backgroundColor = Color.gray;
                        if (GUILayout.Button(
                            new GUIContent(separateBar, "Separate bar prevent items from falling or moving through"),
                            GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            squareType = SquareTypes.NONE;
                            separateBarBrush = !separateBarBrush;
                        }

                        UnityEngine.GUI.backgroundColor = Color.white;

                        if (GUILayout.Button(new GUIContent("Clear", "clear all field"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            ClearLevel();
                            // SaveLevel();
                        }

                        if (GUILayout.Button(new GUIContent("X", "Clear block"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            squareType = SquareTypes.NONE;
                            separateBarBrush = false;
                        }

                        if (GUILayout.Button(
                            new GUIContent("Fill+", "Fill with selected block, second click change or clear filling"),
                            GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            FillLevel();
                        }

                        if (deleteBlock)
                            UnityEngine.GUI.backgroundColor = Color.gray;
                        if (GUILayout.Button(new GUIContent("-1", "to Delete a block from layer"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            deleteBlock = !deleteBlock;
                            separateBarBrush = false;
                        }

                        UnityEngine.GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void ClearLevel()
        {
            layers = 1;
            layer = 0;
            foreach (var squareBlocks in levelData.GetField(subLevelNumber - 1).levelSquares)
            {
                squareBlocks.blocks.Clear();
                squareBlocks.block = SquareTypes.EmptySquare;
                squareBlocks.blockLayer = 1;
                squareBlocks.obstacle = SquareTypes.NONE;
                squareBlocks.obstacleLayer = 1;
                squareBlocks.blocks.Add(new SquareTypeLayer {squareType = squareBlocks.block});
                squareBlocks.separatorIndexes = new bool[4];
            }

            dirtyLevel = true;
        }

        private void FillLevel()
        {
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                var squareBlocks = levelData.GetField(subLevelNumber - 1).levelSquares[i];
                var tempType = squareType;
                if ((squareBlocks.block == squareType && Square.GetLayersCount(squareType) == squareBlocks.blockLayer)
                    || (squareBlocks.obstacle == squareType &&
                        Square.GetLayersCount(squareType) == squareBlocks.obstacleLayer))
                {
                    var sqPos = squareBlocks.position;
                    squareType = SquareTypes.EmptySquare;
                    SetSquareType(sqPos.x, sqPos.y);
                    squareType = tempType;
                }
                else if (squareBlocks.block == SquareTypes.EmptySquare ||
                         (squareBlocks.block == squareType || squareBlocks.obstacle == squareType))
                {
                    var sqPos = squareBlocks.position;
                    SetSquareType(sqPos.x, sqPos.y);
                }
            }

            dirtyLevel = true;
        }

        private SquareBlocks squareBlockSelected; //for teleport linking

        class RectTexture
        {
            public Texture2DSize square;
            public Rect rect;
        }

        private void GUIGameField()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Layers " + (layer + 1) + " / " + layers, EditorStyles.label, GUILayout.Width(100));
                GUILayout.Space(0);
                layer = (int) GUILayout.HorizontalSlider(layer, layers - 1, 0, GUILayout.Width(100));
                GUILayout.Space(50);
                rotateBig = GUILayout.Toggle(rotateBig, new GUIContent("rotate", "rotate big object"),
                    GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            List<RectTexture> rects = new List<RectTexture>();
            GUILayout.BeginVertical();
            for (int row = 0; row < GetField().maxRows; row++)
            {
                GUILayout.BeginHorizontal();

                for (int col = 0; col < GetField().maxCols; col++)
                {
                    Color squareColor = new Color(0.8f, 0.8f, 0.8f);
                    var imageButton = new object();
                    SquareBlocks squareBlock = levelData.GetBlock(row, col);
                    
                    squareBlock.position = new Vector2Int(col, row);
                    
                    //GUILayout.Label("c-"+ col + " & r-"+row);
                    
                    Vector2Int size = Vector2Int.one;
                    var blockTextures = Square.GetSquareTextures(squareBlock, layer);
                    
                    var blockTexturesArray = new List<Texture2DSize>();
                    var itemTextures = new Texture2D[2];
                    if (squareBlock.block == SquareTypes.NONE)
                    {
                        imageButton = "X";
                        squareColor = new Color(0.8f, 0.8f, 0.8f);
                    }
                    else
                    {
                        imageButton = blockTextures?[0];
                        squareColor = Color.white;
                    }

                    blockTexturesArray = blockTextures.ToList();
                    if (section == 0 || section == 1)
                    {
                        if (squareBlock.block != SquareTypes.NONE)
                        {
                            //                        imageButton = squareBlock.item?.Texture;
                            if (squareBlock.item != null && squareBlock.item.Item != null &&
                                squareBlock.item?.ItemType != ItemsTypes.INGREDIENT)
                            {
                                blockTexturesArray.Add(new Texture2DSize(squareBlock.item?.Item
                                    .GetComponent<IColorableComponent>().GetSprite(levelNumber, squareBlock.item
                                        .Color).texture, Vector2Int.one, squareBlock.item.order, rotateBig));
                            }
                            else
                            {
                                var t = Square.GetSquareTexture(SquareTypes.EmptySquare);
                                if (!blockTexturesArray.Any(i => i.Texture2D.name == t.Texture2D.name))
                                    blockTexturesArray.Insert(0,
                                        new Texture2DSize(t.Texture2D, Vector2Int.one, t.order, rotateBig));
                                if (squareBlock.item?.Texture != null)
                                    blockTexturesArray.Add(new Texture2DSize(squareBlock.item?.Texture, Vector2Int.one,
                                        squareBlock.item.order, rotateBig));
                            }

                            if (squareBlock.separatorIndexes.Any(i => i))
                            {
                                blockTexturesArray.AddRange(squareBlock.GetSeparatorTexture(separateBar));
                            }
                        }

                        if (squareBlock.item?.ItemType == ItemsTypes.PACKAGE)
                        {
                            // if (squareBlock.item?.Texture == null)
                            var texture1 = itemsForEditorFull.First(i => i.ItemType == ItemsTypes.PACKAGE).Texture;
                            var texture2 = squareBlock.item?.Item.GetComponent<IColorableComponent>()
                                .GetSprite(levelNumber, squareBlock.item.Color).texture;
                            if (texture1.width == texture2.width && texture1.height == texture2.height)
                                blockTexturesArray.Add(new Texture2DSize(texture1?.AlphaBlend(texture2), Vector2Int.one,
                                    1000, rotateBig));
                        }
                    }

                    if (section == 2)
                    {
                        imageButton = GetArrowByVector(squareBlock.direction, squareBlock.enterSquare);
                    }

                    if (section == 3)
                    {
                        if (squareBlock.teleportCoordinatesLinkedBack != Vector2Int.one * -1)
                        {
                            imageButton = teleports[1];
                        }

                        if (squareBlock.isEnterTeleport)
                            imageButton = teleports[0];
                    }
                    
                    if (section == 4)
                    {
                        if (squareBlock.spawners.Count > 0)
                        {
                            imageButton = GetRotatedSpawnerTexture(squareBlock.spawners[0].rotationType);
                        }
                    }

                    UnityEngine.GUI.color = squareColor;
                    if (GUILayout.Button(imageButton as Texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        if (section == 0)
                        {
                            /*if (squareType == SquareTypes.Cake)
                            {
                                var squarePrototype = Square.GetLayersCount(squareType);
                                for (var i = 0; i < squarePrototype; i++)
                                    SetSquareType(col, row);
                            }
                            else
                            {*/
                                SetSquareType(col, row);
                            //}
                        }
                        if (section == 1)
                            SetItem(col, row);
                        else if (section == 2)
                        {
                            if (selectDirectionType == 0)
                                SetArrow(col, row);
                            else if (selectDirectionType == 1)
                                levelData.GetBlock(row, col).enterSquare = !levelData.GetBlock(row, col).enterSquare;
                        }
                        else if (section == 3)
                        {
                            if (squareBlock.isEnterTeleport ||
                                squareBlock.teleportCoordinatesLinkedBack != Vector2Int.one * -1)
                            {
                                squareBlockSelected = null;
                                squareBlock.isEnterTeleport = false;
                                squareBlock.teleportCoordinatesLinkedBack = Vector2Int.one * -1;
                                squareBlock.teleportCoordinatesLinked = Vector2Int.one * -1;
                            }
                            else if (squareBlockSelected == null)
                            {
                                squareBlockSelected = squareBlock;
                                squareBlockSelected.isEnterTeleport = true;
                            }
                            else
                            {
                                levelData.GetBlock(row, col).enterSquare = false;
                                squareBlock.teleportCoordinatesLinkedBack = squareBlockSelected.position;
                                squareBlockSelected.teleportCoordinatesLinked = squareBlock.position;
                                squareBlockSelected = null;
                            }
                        }
                        
                        //Added_feature
                        else if (section == 4)
                        {
                            switch (CurrentSpawnerState)
                            {
                                case SpawnerState.Add:
                                {
                                    SetSpawnerType(col, row);
                                    levelData.GetBlock(row, col).enterSquare = true;
                                    Column = -1;
                                    Row = -1;
                                }
                                    break;
                                case SpawnerState.Edit:
                                {
                                    Column = col;
                                    Row = row;
                                    //Debug.Log("Edit Selected");
                                }
                                    break;
                                case SpawnerState.Remove:
                                {
                                    RemoveSpawnerTyps(col, row);
                                    levelData.GetBlock(row, col).enterSquare = false;
                                    Column = -1;
                                    Row = -1;
                                }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }


                    var lastRect = GUILayoutUtility.GetLastRect();
                    var texture2DSizes = blockTexturesArray.WhereNotNull().OrderByDescending(i => i.order).ToArray();
                    foreach (var texture2DSiz in texture2DSizes)
                    {
                        if (texture2DSiz.Texture2D)
                        {
                            var rectTexture = new RectTexture() {square = texture2DSiz, rect = lastRect};
                            rects.Add(rectTexture);
                        }
                    }

                    if (lastRect != Rect.zero && lastRect.position != Vector2.zero)
                        squareBlock.guiRect = lastRect;
                    // if (section != 2 && section != 3)
                    //     DrawLayeredTextures(rectTexture);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            if (section != 2 && section != 3 && section != 4)
            {
                var orderedEnumerable = rects.OrderBy(i => i.square.order);
                foreach (var rectTexture in orderedEnumerable)
                {
                    DrawTexture(rectTexture.square, rectTexture.rect);
                }
            }

            SetupArrows();
            DrawLines();
        }

        //Added_feature
        private Texture GetRotatedSpawnerTexture(RotationType rotationType)
        {
            Texture res = null;

            switch (rotationType)
            {
                case RotationType.Top:
                    return spawner[0];
                case RotationType.Bottom:
                    return spawner[1];
                case RotationType.Left:
                    return spawner[2];
                case RotationType.right:
                    return spawner[3];
                default:
                    break;
            }

            return res;
        }

        RotationType CheckForRotation(RotationType type, bool LeftRotation)
        {
            RotationType res = type;

            switch (res)
            {
                case RotationType.Top:
                    if (LeftRotation)
                        return RotationType.Left;
                    return RotationType.right;
                case RotationType.Left:
                    if (LeftRotation)
                        return RotationType.Bottom;
                    return RotationType.Top;
                case RotationType.Bottom:
                    if (LeftRotation)
                        return RotationType.right;
                    return RotationType.Left;
                case RotationType.right:
                    if (LeftRotation)
                        return RotationType.Top;
                    return RotationType.Bottom;
                default:
                    break;
            }

            return res;
        }
        
        private FieldData GetField() => levelData.GetField(subLevelNumber - 1);

        private void DrawTexture(Texture2DSize texture2DSize, Rect lastRect)
        {
            var texture2D = texture2DSize.Texture2D;
            if (texture2D != null)
            {
                var rect = new Rect(lastRect.position.x + 4 + texture2DSize.offset.x,
                    lastRect.position.y + 4 + texture2DSize.offset.y, texture2DSize.size.x * 40 * texture2DSize
                        .sizeMod.x, texture2DSize.size.y * 40 * texture2DSize.sizeMod.y);
                if (texture2DSize.size.magnitude >= 2 && texture2DSize.rotate && texture2D.width < 2048)
                {
                    texture2D = texture2D.rotateTexture();
                    rect = new Rect(lastRect.position.x + 4 + texture2DSize.offset.x,
                        lastRect.position.y + 4 + texture2DSize.offset.y, texture2DSize.size.y * 40,
                        texture2DSize.size.x * 40);
                }
                else if (texture2DSize.rotate)
                    texture2D = texture2D.rotateTexture();

                UnityEngine.GUI.DrawTexture(rect, texture2D);
            }
        }

        private void DrawLines()
        {
            if (section != 1) return;
            var marmalades = levelData.GetField().levelSquares
                .Where(i => i.item != null && i.item.EnableMarmaladeTargets);
            Handles.color = Color.red;
            foreach (var marmalade in marmalades)
            {
                //            Handles.BeginGUI();
                foreach (var targetPosition in marmalade.item.TargetMarmaladePositions)
                {
                    Handles.DrawLine(marmalade.guiRect.center, levelData.GetBlock(targetPosition).guiRect.center);
                    RotateGizmo(marmalade.guiRect.center, levelData.GetBlock(targetPosition).guiRect.center, 20f);
                    RotateGizmo(marmalade.guiRect.center, levelData.GetBlock(targetPosition).guiRect.center, -20f);
                }
                //            Handles.EndGUI();
            }
        }

        private void RotateGizmo(Vector3 square, Vector3 pivot, float angle)
        {
            var rightPoint = Vector3.zero;
            var dir = (square - pivot) * 0.1f;
            dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;
            rightPoint = dir + pivot;
            Handles.DrawLine(pivot, rightPoint);
        }

        private void SetItem(int col, int row)
        {
            SquareBlocks squareBlock = levelData.GetBlock(row, col);
            if (squareBlock.blocks.Any(i =>
                i.squareType == SquareTypes.SolidBlock || i.squareType == SquareTypes.UndestrBlock ||
                i.squareType == SquareTypes.NONE)) return;
            ItemForEditor itemTemp = squareBlock.item;
            if (itemTemp == null)
                itemTemp = itemBrush.DeepCopy();
            else if (itemBrush?.ItemType != ItemsTypes.NONE ||
                     (itemTemp.ItemType == ItemsTypes.NONE && itemBrush?.ItemType == ItemsTypes.NONE))
                itemTemp = itemBrush?.DeepCopy();
            else if (itemBrush.ItemType == ItemsTypes.NONE && itemTemp.ItemType != ItemsTypes.NONE)
                itemTemp.Color = itemBrush.Color;
            //        if (itemTemp.ItemType != ItemsTypes.NONE)
            if (itemTemp != null && itemBrush != null && itemsForEditorFull.First(i => i.ItemType == itemTemp.ItemType)
                .Item.GetComponent<IColorableComponent>()?.GetSprites(levelNumber).Count() > 1)
                itemTemp.Texture =
                    itemsForEditorFull.FirstOrDefault(i => i.ItemType == itemTemp.ItemType).Item
                        .GetComponent<IColorableComponent>().GetSprites(levelNumber)[color].texture;
            
            squareBlock.item = itemTemp;
        }

        private void SetupArrows()
        {
            for (int row = 0; row < levelData.GetField(subLevelNumber - 1).maxRows; row++)
            {
                for (int col = 0; col < levelData.GetField(subLevelNumber - 1).maxCols; col++)
                {
                    SquareBlocks squareBlock = levelData.GetBlock(row, col);
                    if (section == 2 || section == 3)
                    {
                        if (squareBlock.teleportCoordinatesLinkedBack != Vector2Int.one * -1)
                        {
                            Handles.BeginGUI();
                            Handles.color = Color.red;
                            Vector2 position1 = squareBlock.guiRect.center;
                            Vector2 position2 = levelData.GetBlock(squareBlock.teleportCoordinatesLinkedBack).guiRect
                                .center;
                            Handles.DrawLine(position1, position2);
                            float angle = AngleInDeg(position1, position2);
                            Handles.ArrowHandleCap(0, position2,
                                Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.Euler(new Vector3(0, -95, 0)),
                                50,
                                EventType.Repaint);
                            Handles.EndGUI();
                        }
                    }
                }
            }
        }

        //This returns the angle in radians
        public static float AngleInRad(Vector3 vec1, Vector3 vec2)
        {
            return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
        }

        //This returns the angle in degress
        public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
        {
            return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
        }


        private void SetArrow(int col, int row)
        {
            SquareBlocks squareBlock = levelData.GetBlock(row, col);
            int index = GetIndexByDirection(squareBlock.direction);
            index = (int) Mathf.Repeat(index + 1, 4);
            // if (squareBlock.block != SquareTypes.NONE)
            // {
            squareBlock.direction = GetDirectionByIndex(index);
            // }
        }


        private void SaveLevel(int _levelNumber)
        {
            squareBlockSelected = null;
            if (dirtyLevel)
            {
                if (!FileName.Contains(".txt"))
                    FileName += ".txt";
                SaveToScriptable(_levelNumber);
                // SaveCommonValues();
                // SaveField();
                dirtyLevel = false;
            }
        }

        private void AddLevel()
        {
            squareBlockSelected = null;
            SaveLevel(levelNumber);
            levelNumber = GetLastLevel() + 1;
            SubLevelNumber = 1;
            ScriptableLevelManager.CreateFileLevel(levelNumber, levelData);
            levelData = ScriptableLevelManager.LoadLevel(levelNumber, "Levels/Level_");
            Initialize();
            ClearLevel();
            ClearItems();
            SaveLevel(levelNumber);
            PrepareTargetLevel();
            subLevelNumberTotal = GetSubLevelsCount();
        }

        private int GetLastLevel()
        {
            int lastLevel = LoadingManager.GetLastLevelNum();
            if (lastLevel == 0) lastLevel = levelScriptable.levels.Count;
            return lastLevel;
        }

        private void AddSubLevel()
        {
            squareBlockSelected = null;
            SaveLevel(levelNumber);
            levelData.AddNewField();
            SubLevelNumber = GetSubLevelsCount();
            Initialize();
            InitializeSublevel();
            SaveLevel(levelNumber);
            subLevelNumberTotal = GetSubLevelsCount();
        }

        private void RemoveSubLevel()
        {
            if (GetSubLevelsCount() > 1)
            {
                SaveLevel(levelNumber);
                levelData.RemoveField();
                SubLevelNumber = GetSubLevelsCount();
                Initialize();
                // InitializeSublevel();
                SaveLevel(levelNumber);
                subLevelNumberTotal = GetSubLevelsCount();
            }
        }

        private int GetSubLevelsCount()
        {
            return levelData.fields.Count; //GetSublevelsScriptable();
        }

        private int GetSublevelsScriptable()
        {
            return levelScriptable.levels[levelNumber - 1].fields.Count;
        }

        private void RemoveLevel()
        {
            PreviousLevel();
            File.Delete(levelPath + "Level_" + (levelNumber + 1) + ".asset");
            File.Delete(levelPath + "Targets/" + "TargetLevel" + (levelNumber + 1) + ".asset");
            AssetDatabase.Refresh();
        }

        private void NextLevel()
        {
            SaveLevel(levelNumber);
            if (levelNumber + 1 <= GetLastLevel())
            {
                levelNumber++;
                SubLevelNumber = 1;
                LoadLevel(levelNumber);
            }
        }

        private void PreviousLevel()
        {
            SaveLevel(levelNumber);
            levelNumber--;
            SubLevelNumber = 1;
            if (levelNumber < 1)
                levelNumber = 1;
            if (!LoadLevel(levelNumber))
            {
                levelNumber++;
                LoadLevel(levelNumber);
            }
        }

        private void NextSubLevel()
        {
            if (SubLevelNumber + 1 <= GetSubLevelsCount())
            {
                SaveLevel(levelNumber);
                SubLevelNumber++;
            }

            // if (!LoadLevel(levelNumber, SubLevelNumber))
            // 	SubLevelNumber--;
        }

        private void PreviousSubLevel()
        {
            SaveLevel(levelNumber);
            SubLevelNumber--;
            if (SubLevelNumber < 1)
                SubLevelNumber = 1;
            // if (!LoadLevel(levelNumber, SubLevelNumber))
            // SubLevelNumber++;
        }

        private bool dirtyLevel;
        private int section;
        //private bool gotFocus;
        private static Material mat;
        private int color;
        private Texture2D packageTexture;
        private GUISkin customSkin;
        private int selectedTutorial;
        private TargetEditorScriptable targetEditorScriptable;
        private Texture2DSize[] squareTextures;
        private WinReward winReward;

        private int layers
        {
            get => GetField().layers;
            set => GetField().layers = value;
        }

        private int layer;
        private IEnumerable<SquareTypes> _squareTypeItems;
        private IEnumerable<SquareTypes>[] squareTypeGroupped;
        private bool deleteBlock;
        private bool rotateBig;
        private Texture2D separateBar;
        private bool separateBarBrush;
        private int m_separateIndex;
        private SquareBlocks m_squareBlock;

        private void SetSquareType(int col, int row)
        {
            dirtyLevel = true;
            SquareBlocks squareBlock = levelData.GetBlock(row, col);
            m_squareBlock = squareBlock;
            if (separateBarBrush)
            {
                GenericMenu menu = new GenericMenu();
                AddMenuItem(menu, "LEFT", 0, squareBlock);
                AddMenuItem(menu, "RIGHT", 1, squareBlock);
                AddMenuItem(menu, "UP", 2, squareBlock);
                AddMenuItem(menu, "DOWN", 3, squareBlock);
                AddMenuItem(menu, "NONE", 4, squareBlock);
                menu.ShowAsContext();
                return;
            }

            if (squareType == SquareTypes.EmptySquare || squareType == SquareTypes.NONE)
            {
                squareBlock.blocks.Clear();
                squareBlock.blocks.Add(new SquareTypeLayer {squareType = squareType});
                squareBlock.block = squareType;
                squareBlock.blockLayer = 1;
                squareBlock.obstacle = SquareTypes.NONE;
                squareBlock.obstacleLayer = 1;
            }
            else
            {
                Square squarePrototype = Square.GetBlockPrefab(squareType).Last().GetComponent<Square>();
                if (!deleteBlock)
                {
                    if (!squarePrototype.IsObstacle() && squareType != SquareTypes.EmptySquare &&
                        squareType != SquareTypes.NONE)
                    {
                        if (squareBlock.block == squareType)
                        {
                            squareBlock.blockLayer =
                                (int) Mathf.Repeat(squareBlock.blockLayer, Square.GetLayersCount(squareType)) + 1;
                        }
                        else
                        {
                            squareBlock.blockLayer = 1;
                        }
                        squareBlock.block = squareType;
                    }
                    else if (squarePrototype.IsObstacle())
                    {
                        if (squareBlock.obstacle == squareType)
                        {
                            squareBlock.obstacleLayer = (int) Mathf.Repeat(squareBlock.obstacleLayer,
                                Square.GetLayersCount(squareType)) + 1;
                        }
                        else
                        {
                            squareBlock.obstacleLayer = 1;
                        }
                        squareBlock.obstacle = squareType;
                    }
                    
                    AddBlock(squareBlock, squarePrototype);
                }
                else if (layer > 0)
                {
                    DeleteBlock(squareBlock, squarePrototype);
                }
            }

            // update = true;
            // SaveLevel();
            // GetSquare(col, row).type = (int) squareType;
        }

        //Add_feature
        private void SetSpawnerType(int col, int row)
        {
            dirtyLevel = true;
            SquareBlocks squareBlock = levelData.GetBlock(row, col);
            if (!(squareBlock.spawners.Count > 0))
            {
                SingleSpawn NewSingleSpawn = new SingleSpawn()
                {
                    SpawnId = " Spawner" + "_" + UnityEngine.Random.Range(1000, 9999) + " ",
                    SpawnersType = Spawners.None,
                    SpawnersType_2 = Spawners.None,
                    SpawnPersentage = 0,
                    SpawnPersentage_2 = 0,
                    rotationType = RotationType.Top
                };
                squareBlock.spawners.Add(NewSingleSpawn);
            }
        }


        void AddMenuItem(GenericMenu menu, string menuPath, int index, SquareBlocks squareBlocks)
        {
            bool squareBlocksSeparatorIndex = false;
            if (index < squareBlocks.separatorIndexes.Length)
                squareBlocksSeparatorIndex = squareBlocks.separatorIndexes[index];
            menu.AddItem(new GUIContent(menuPath), squareBlocksSeparatorIndex, OnSeparateSelected, index);
        }

        private void OnSeparateSelected(object userdata)
        {
            m_separateIndex = (int) userdata;
            if (m_squareBlock.separatorIndexes.Length > m_separateIndex)
                m_squareBlock.separatorIndexes[m_separateIndex] = !m_squareBlock.separatorIndexes[m_separateIndex];
            else
            {
                for (int i = 0; i < m_squareBlock.separatorIndexes.Length; i++)
                {
                    m_squareBlock.separatorIndexes[i] = false;
                }
            }
        }

        //Add_feature
        private void RemoveSpawnerTyps(int col, int row)
        {
            dirtyLevel = true;
            SquareBlocks squareBlock = levelData.GetBlock(row, col);
            squareBlock.spawners.Clear();
        }
        
        private void DeleteBlock(SquareBlocks squareBlock, Square squarePrototype)
        {
            var index = Mathf.Min(layer, squareBlock.blocks.Count - 1);
            if (index < 0) return;
            if (squareBlock.blocks[index].squareType == SquareTypes.BigBlock)
            {
                var originalPos = squareBlock.blocks[index].originalPos;
                var size = squareBlock.blocks[index].size;
                for (int x = originalPos.x; x < originalPos.x + size.x; x++)
                {
                    for (int y = originalPos.y; y < originalPos.y + size.y; y++)
                    {
                        var originalBlock = levelData.GetBlock(y, x);
                        originalBlock.blocks.RemoveAt(index);
                        originalBlock.MergeEmptySquares();
                    }
                }
            }
            else
                squareBlock.blocks.RemoveAt(index);
            var sum = levelData.GetField().levelSquares.Max(i => i.blocks.Count);
            if (sum < layers)
                layers = sum;
            if (layer >= layers) layer = layers - 1;
        }

        private void  AddBlock(SquareBlocks squareBlock, Square squarePrototype)
        {
            // layer++;
            var list = new List<SquareBlocks>();
            var xMax = squarePrototype.sizeInSquares.x;
            var yMax = squarePrototype.sizeInSquares.y;
            if (rotateBig)
            {
                yMax = squarePrototype.sizeInSquares.x;
                xMax = squarePrototype.sizeInSquares.y;
            }
            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    list.Add(levelData.GetBlock(squareBlock.position.y + y, squareBlock.position.x + x));
                }
            }
            int[] layersCount = new int[list.Count];
            for (var index = 0; index < list.Count; index++)
            {
                var squareBlockse = list[index];
                /*if (squarePrototype.type == SquareTypes.Cake)
                {
                    layersCount[index] = Mathf.Min(layer, squareBlockse.blocks.Count - 1) + 12;
                }
                else
                {*/
                    layersCount[index] = Mathf.Min(layer, squareBlockse.blocks.Count - 1) + 1;
                //}
            }

            var currentlayer = layersCount.Max();

            foreach (var squareBlockse in list)
            {
                ExpandBlocksList(squareBlockse, currentlayer);
                var squareTypeLayer = new SquareTypeLayer {squareType = squareType};
                if (squareBlock != squareBlockse)
                    squareTypeLayer.anotherSquare = true;
                
                /*if (squareType == SquareTypes.Cake)
                    squareTypeLayer.anotherSquare = false;*/
                
                squareTypeLayer.rotate = rotateBig;
                squareTypeLayer.originalPos = squareBlock.position;
                squareTypeLayer.size = new Vector2Int(xMax, yMax);
                squareBlockse.blocks.Insert(currentlayer, squareTypeLayer);
            }
            
            Square.GetSquareTextures(squareBlock, layer);

            if (squareBlock.blocks.Count() > layers)
                layers = squareBlock.blocks.Count();
            if (currentlayer > layer) layer = currentlayer;
            layer = Mathf.Clamp(layer, 0, layers - 1);
        }

        private void ExpandBlocksList(SquareBlocks sqBlock, int maxLayer)
        {
            var blocksCount = sqBlock.blocks.Count;
            if (maxLayer >= blocksCount)
            {
                for (int i = 0; i < maxLayer - blocksCount; i++)
                {
                    sqBlock.blocks.Add(new SquareTypeLayer {squareType = SquareTypes.EmptySquare});
                }
            }
        }

        private void SaveToScriptable(int _levelNumber)
        {
            levelData.InitTargetObjects();
            SquareBlocks[] levelSquares = levelData.GetField(subLevelNumber - 1).levelSquares;
            for (int i = 0; i < levelSquares.Length; i++)
            {
                var item = levelSquares[i];
                var dir = item.direction;

                if (dir == Vector2.zero) item.direction = Vector2.down;
            }

            bool enterSquaresExist = levelSquares.Any(i => i.enterSquare);
            if (!enterSquaresExist)
            {
                for (int col = 0; col < levelData.GetField(subLevelNumber - 1).maxCols; col++)
                {
                    levelData.GetBlock(0, col).enterSquare = true;
                }
            }

            if (levelScriptable.levels.Count() < _levelNumber)
                levelScriptable.levels.Add(levelData);
            else
                levelScriptable.levels[_levelNumber - 1] = levelData.DeepCopy(_levelNumber);
            ScriptableLevelManager.SaveLevel(levelPath, _levelNumber, levelData);

//        EditorUtility.SetDirty(levelScriptable);
//        AssetDatabase.SaveAssets();
        }

        private bool LoadLevel(int currentLevel)
        {
            levelData = LoadingManager.LoadlLevel(currentLevel, "Levels/Level_");
            squareBlockSelected = null;
            layer = layers - 1;

            // PlayerPrefs.SetInt("OpenLevelTest", currentLevel);
            // PlayerPrefs.Save();

            if (levelData != null)
            {
                Initialize();
                return true;
            }

            return false;
        }

        #endregion
    }
}