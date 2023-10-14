using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using UnityEngine;
using Object = UnityEngine.Object;
using Comfort.Common;
using UnityEngine.UI;

namespace AT.AIMarker
{
    [BepInPlugin("AT.AIMarker", "AT.AI指示器AIMarker", "1.0.0.0")]
    public class AIMarkercore : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> OPEN;
        public static ConfigEntry<float> positionx;
        public static ConfigEntry<float> positiony;
        public static ConfigEntry<float> positionz;
        private static ManualLogSource logger;
        public void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"AIMarker: Loading");
            OPEN = Config.Bind<KeyCode>("按键设置(KeyCode Settings)", "标记开启按键(AIMarker KeyCode)", KeyCode.L, "AI指示器启动按钮(AIMarker open button)");
            positionx = Config.Bind("标记调整(Marker Settings)", "标记X轴调整(AIMarker X-axis position)", 0f, new ConfigDescription("标记X轴调整(AIMarker X-axis position)", new AcceptableValueRange<float>(-10f, 10f)));
            positiony = Config.Bind("标记调整(Marker Settings)", "标记X轴调整(AIMarker Y-axis position)", 1.8f, new ConfigDescription("标记Y轴调整(AIMarker Y-axis position)", new AcceptableValueRange<float>(-10f, 10f)));
            positionz = Config.Bind("标记调整(Marker Settings)", "标记X轴调整(AIMarker Z-axis position)", 0f, new ConfigDescription("标记Z轴调整(AIMarker Z-axis position)", new AcceptableValueRange<float>(-10f, 10f)));
        }
        private GameObject[] AItarget;
        private float timer = 0;
        private bool[] AIhealth;
        private bool isAIMarker = false;
        private static GameWorld gameWorld;
        private AbstractGame game;
        public static AssetBundle AIMarkerBundle;
        public static Object AIMarkerPrefab { get; private set; }
        public static bool Entermap() => Singleton<GameWorld>.Instantiated;
        void Start()
        {
            isAIMarker = false;
            AItarget = new GameObject[999];
            AIhealth = new bool[999];
            gameWorld = Singleton<GameWorld>.Instance;
            game = Singleton<AbstractGame>.Instance;
            if (AIMarkerPrefab == null)
            {
                String aimarkercore = Path.Combine(Environment.CurrentDirectory, "BepInEx/plugins/atmod/aimarker");
                if (!File.Exists(aimarkercore))
                    return;
                AIMarkerBundle = AssetBundle.LoadFromFile(aimarkercore);
                if (AIMarkerBundle == null)
                    return;
                AIMarkerPrefab = AIMarkerBundle.LoadAsset("Assets/aimarker/aimarker.prefab");
                Console.WriteLine("AI指示器模块加载完毕");
            }
        }
        void Update()
        {
            if(gameWorld==null)
            {
                gameWorld = Singleton<GameWorld>.Instance;
                game = Singleton<AbstractGame>.Instance;
            }
            if (AIMarkerPrefab == null && game.InRaid && Camera.main.transform.position != null)
            {
                isAIMarker = false;
                AItarget = new GameObject[999];
                AIhealth = new bool[999];
                gameWorld = Singleton<GameWorld>.Instance;
                game = Singleton<AbstractGame>.Instance;
                if (AIMarkerPrefab == null)
                {
                    String aimarkercore = Path.Combine(Environment.CurrentDirectory, "BepInEx/plugins/atmod/aimarker");
                    if (!File.Exists(aimarkercore))
                        return;
                    AIMarkerBundle = AssetBundle.LoadFromFile(aimarkercore);
                    if (AIMarkerBundle == null)
                        return;
                    AIMarkerPrefab = AIMarkerBundle.LoadAsset("Assets/aimarker/aimarker.prefab");
                    Console.WriteLine("AI指示器模块加载完毕");
                }
            }
            if (Input.GetKeyDown(OPEN.Value))//AI指示器是否启动
            {
                isAIMarker = !isAIMarker;
                timer = 0;
                if (isAIMarker)
                {
                    eggtarget();
                }
                else
                {
                    if (AItarget != null)
                    {
                        for (int i = 1; i < 100; i++)//获取全部AI
                        {
                            Destroy(AItarget[i]);
                        }
                    }
                }
            }
            if (isAIMarker && Entermap())
            {
                timer += Time.deltaTime;
                if(timer>=3)
                {
                    if (AItarget != null)
                    {
                        for (int i = 1; i < 100; i++)//获取全部AI
                        {
                            Destroy(AItarget[i]);
                        }
                    }
                    eggtarget();
                    timer = 0;
                }
                if (gameWorld.AllAlivePlayersList.Count >= 2)
                {
                    for (int i = 1; i < gameWorld.AllAlivePlayersList.Count; i++)//获取全部AI
                    {
                        AIhealth[i] = gameWorld.AllAlivePlayersList[i].ActiveHealthController.IsAlive;
                        if (!AIhealth[i])
                        {
                            Destroy(AItarget[i]);
                        }
                        if (AItarget[i] != null && AIhealth[i])
                        {
                            AItarget[i].transform.position = gameWorld.AllAlivePlayersList[i].Transform.position;
                        }
                    }
                }
            }
        }
        private void eggtarget()
        {
            if (Entermap() && gameWorld.AllAlivePlayersList.Count >= 2 && AIMarkerPrefab != null)
            {
                for (int i = 1; i < gameWorld.AllAlivePlayersList.Count; i++)//获取全部AI
                {
                    AItarget[i] = Instantiate(AIMarkerPrefab, gameWorld.AllAlivePlayersList[i].Transform.position, gameWorld.AllAlivePlayersList[i].Transform.rotation) as GameObject;
                    AItarget[i].transform.position = gameWorld.AllAlivePlayersList[i].Transform.position;
                    if (gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.assault |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.marksman |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerBully |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerKojaniy |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerGluharAssault |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerGluharSecurity |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerGluharScout |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerGluharSnipe |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerSanitar |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.assaultGroup |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerTagilla |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.gifter |
                       gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerZryachiy)
                    {
                        Image[] AItargetImagegroup = AItarget[i].GetComponentsInChildren<Image>();
                        foreach (Image child in AItargetImagegroup)
                        {
                            AItargetImagegroup[0].color = Color.yellow;
                            AItargetImagegroup[1].color = Color.yellow;
                            AItargetImagegroup[2].color = Color.yellow;
                        }
                    }
                    if (gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossBully |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossKilla |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossKilla |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossKojaniy |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossGluhar |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossSanitar |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossTagilla |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossKnight |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.bossZryachiy |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerBirdEye |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.followerBigPipe)
                    {
                        Image[] AItargetImagegroup = AItarget[i].GetComponentsInChildren<Image>();
                        foreach (Image child in AItargetImagegroup)
                        {
                            AItargetImagegroup[0].color = Color.red;
                            AItargetImagegroup[1].color = Color.red;
                            AItargetImagegroup[2].color = Color.red;
                        }
                    }
                    if (gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.pmcBot)
                    {
                        Image[] AItargetImagegroup = AItarget[i].GetComponentsInChildren<Image>();
                        foreach (Image child in AItargetImagegroup)
                        {
                            AItargetImagegroup[0].color = Color.blue;
                            AItargetImagegroup[1].color = Color.blue;
                            AItargetImagegroup[2].color = Color.blue;
                        }
                    }
                    if (gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.exUsec)
                    {
                        Image[] AItargetImagegroup = AItarget[i].GetComponentsInChildren<Image>();
                        foreach (Image child in AItargetImagegroup)
                        {
                            AItargetImagegroup[0].color = Color.black;
                            AItargetImagegroup[1].color = Color.black;
                            AItargetImagegroup[2].color = Color.black;
                        }
                    }
                    if (gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.sectantWarrior |
                        gameWorld.AllAlivePlayersList[i].Profile.Info.Settings.Role == WildSpawnType.sectantPriest)
                    {
                        Image[] AItargetImagegroup = AItarget[i].GetComponentsInChildren<Image>();
                        foreach (Image child in AItargetImagegroup)
                        {
                            AItargetImagegroup[0].color = Color.white;
                            AItargetImagegroup[1].color = Color.white;
                            AItargetImagegroup[2].color = Color.white;
                        }
                    }
                }
            }
        }
    }
    public class AIMarkerenemy : MonoBehaviour
    {
        private GameObject player;
        public Image enemyuesd;
        public Image enemyuesdup;
        public Image enemyuesdlow;
        public Image radarback;
        public Image target;
        public Image targetup;
        public Image targetlow;
        private Transform targettransform;
        private bool findenemy;
        private Vector2 pos;
        // Start is called before the first frame update
        void Start()
        {
            findenemy = true;
            targettransform = this.transform;
            if (player == null)
            {
                player = GameObject.Find("FPS Camera");//找到玩家
            }
        }

        // Update is called once per frame
        void Update()
        {
            float dis = Vector3.Distance(player.transform.position, this.transform.position);
            float dismax = 0;
            float x = (this.transform.position.x - player.transform.position.x) / 100;
            float z = (this.transform.position.z - player.transform.position.z) / 100;
            float y = (this.transform.position.y - player.transform.position.y);
            dismax = dis;
            if (dismax >= 100) dismax = 100;
            if (findenemy && dis < 100)
            {
                findenemy = false;

            }
            if (!findenemy)
            {
                if (y > 2.5f)
                {
                    Closeenemyimage();
                    targetup.GetComponent<Image>().enabled = true;
                }
                if (y < -2.5f)
                {
                    Closeenemyimage();
                    targetlow.GetComponent<Image>().enabled = true;
                }
                if (y < 2.5f && y > -2.5f)
                {
                    Closeenemyimage();
                    target.GetComponent<Image>().enabled = true;
                }
            }
            targettransform.position = new Vector3(this.transform.position.x+AIMarkercore.positionx.Value, this.transform.position.y+AIMarkercore.positiony.Value, this.transform.position.z+ AIMarkercore.positionz.Value);
            float minX = target.GetPixelAdjustedRect().width / 2;
            float maxX = Screen.width - minX;
            float minY = target.GetPixelAdjustedRect().height / 2;
            float maxY = Screen.height - minY;
            target.GetComponent<RectTransform>().localScale = new Vector3(3f - (dismax / 100) * 2.2f, 3f - (dismax / 100) * 2.2f, 1);
            targetlow.GetComponent<RectTransform>().localScale = new Vector3(3f - (dismax / 100) * 2.2f, 3f - (dismax / 100) * 2.2f, 1);
            targetup.GetComponent<RectTransform>().localScale = new Vector3(3f - (dismax / 100) * 2.2f, 3f - (dismax / 100) * 2.2f, 1);
            pos = Camera.main.WorldToScreenPoint(targettransform.position);
            if (Vector3.Dot((targettransform.position - player.transform.position), player.transform.forward) < 0)
            {
                if (pos.x < Screen.width / 2)
                {
                    pos.x = maxX;
                }
                else pos.x = minX;
            }
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            target.transform.position = pos;
            targetlow.transform.position = pos;
            targetup.transform.position = pos;
        }
        private void Closeenemyimage()
        {
            target.GetComponent<Image>().enabled = false;
            targetup.GetComponent<Image>().enabled = false;
            targetlow.GetComponent<Image>().enabled = false;
        }
        
    }
}
