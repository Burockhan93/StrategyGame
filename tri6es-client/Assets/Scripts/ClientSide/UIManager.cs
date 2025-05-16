using Shared.DataTypes;
using Shared.Game;
using Shared.HexGrid;
using Shared.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIManager : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    #region Public

    public float timeFactor;

    public String time;

    public LayerMask mask;

    public static UIManager instance;

    public PrefabManager prefabManager;

    public WeatherManager weatherManager;

    public HexMeshGrid hexMeshGrid;

    public GameObject startMenu;
    public TMP_InputField usernameField;

    public FeedBackManager FeedBackManager;

    public HexMapCamera camera;

    //Joystick Controls
    public Joystick joystick;
    float moveTimer;


    public GameObject BuildComponentPrefab;
    public GameObject RessourceElementPrefab;
    public GameObject BarPrefab;
    public GameObject LevelDisplayPrefab;
    public GameObject ButtonPrefab;
    public GameObject RessourceSelectorPrefab;
    // public GameObject SelectableRessourceElementPrefab;
    public GameObject MapViewPrefab;
    public GameObject GroupPrefab;
    public GameObject TroopbarPrefab;
    public GameObject StructureInformationPrefab;
    public GameObject RessourceBarPrefab;
    public GameObject ToolchainPrefab;
    public GameObject RefineryInformationPrefab;
    public GameObject RecipeSelectPrefab;
    public GameObject MarketInformationPrefab;
    public GameObject RessourceTogglePrefab;
    public GameObject BarracksInformationPrefab;
    public GameObject UpgradePrefab;
    public GameObject PrioPrefab;
    public GameObject GuildHouseInformationPrefab;
    public GameObject GuildHouseLevelingPanelPrefab;
    public GameObject GuildHouseInventoryPanelPrefab;
    public GameObject GuildHouseBoostPanelPrefab;
    public GameObject LibraryResearchPanelPrefab;

    public GameObject AvatarSelection;
    public GameObject MenuPanel;
    public GameObject Buttons;
    public GameObject ChatPanel;
    public InputField ChatInput;
    public GameObject ChatHistory;
    public GameObject ChatFilterDropdown;
    public GameObject TribeMenu;
    public GameObject ContentPrefab;
    public GameObject ActiveQuestPrefab;
    public GameObject QuestSuccessPanel;
    public GameObject ActiveQuests;
    public GameObject completedQuests;
    public GameObject QuestMenu;
    public GameObject FeedbackPanel;
    public GameObject Tutorial;

    public HexCell selectedCell;

    public Sprite[] ressourceImages;
    public Sprite truhenImage;
    public Sprite[] troopImages;
    public Sprite[] ResearchImages;

    public Sprite[] Bakeries;
    public Sprite[] Butchers;
    public Sprite[] CoalMines;
    public Sprite[] CowFarms;
    public Sprite[] Fishers;
    public Sprite[] Headquarters;
    public Sprite[] Quarries;
    public Sprite[] Smelters;
    public Sprite[] Storages;
    public Sprite[] Tanners;
    public Sprite[] WheatFarm;
    public Sprite[] Woodcutters;
    public Sprite[] Markets;
    public Sprite[] LandRoads;
    public Sprite[] Bridges;
    public Sprite[] Barracks;
    public Sprite[] Workshops;
    public Sprite[] Smiths;
    public Sprite[] AssemblyPoints;
    public Sprite[] Towers;
    public Sprite[] Libraries;
    public Sprite[] GuildHouses;

    public Sprite[] Avatars;
    public Sprite[] Buildings_Tutorial;
    public Image BuildingtoShow;
    public TextMeshProUGUI buildingInfo;
    public Sprite BattleDeclare;

    public Type placeBuildingType;

    /// 0: tribe, 1: guild. Make sure guild comes last, as it is added/removed dynamically to the end of the list.
    public int selectedChatType = 0;    
    private readonly string[] chatTypeCodes = {"T", "G"};
    #endregion

    #region Private
    private HexCoordinates tmp;

    private Dictionary<Type, Sprite[]> buildingImages;

    Sprite GetBuildingSprite(Building building)
    {
        return GetBuildingSprite(building.GetType(), building.Level);
    }

    Sprite GetBuildingSprite(Type buildingType, int level)
    {
        return buildingImages[buildingType][level - 1];
    }

    private GameObject currentContent;

    public readonly static Dictionary<RessourceType, int> ressourceIndizes = new Dictionary<RessourceType, int>{
        { RessourceType.WOOD, 0 },
        { RessourceType.STONE, 1 },
        { RessourceType.IRON, 2 },
        { RessourceType.COAL, 3 },
        { RessourceType.WHEAT, 4 },
        { RessourceType.COW, 5 },
        { RessourceType.FOOD, 6 },
        { RessourceType.LEATHER, 7 },
        { RessourceType.SWORD, 8 },
        { RessourceType.BOW, 9 },
        { RessourceType.SPEAR, 10 },
        { RessourceType.IRON_ARMOR, 11 },
        { RessourceType.LEATHER_ARMOR, 12 },
    };

    private List<Action> updateStructurePanelFunctions = new List<Action>();

    private List<HexCell> highlightedCells;

    private Action updateMapFunction;

    #endregion

    #region MenuOpenClose

    public void OpenBuildMenu()
    {
        MenuPanel.transform.GetChild(4).GetComponent<Draggable>().Open(0);
        ResetFunctions();

        MenuPanel.transform.SetSiblingIndex(MenuPanel.transform.parent.childCount - 2);

        if (currentContent != null) Destroy(currentContent);

        currentContent = Instantiate(ContentPrefab, MenuPanel.transform.Find("Content Panel"));

        if (Client.instance.Player.Tribe != null)
        {
            foreach (Type buildingType in Client.instance.Player.Tribe.AvailableBuildings) {
                if (buildingType != typeof(Headquarter))
                {
                    AddBuildingDisplay(currentContent.transform, buildingType);
                }
            }
        }
        else
        {
            AddBuildingDisplay(currentContent.transform, typeof(Headquarter));
        }
        MenuPanel.GetComponent<ScrollRect>().content = currentContent.GetComponent<RectTransform>();
        UpdateStructurePanel();
    }

    private void OpenSharedInventory()
    {
        /* MenuPanel.transform.GetChild(4).GetComponent<Draggable>().Open(1);
         this.updateStructurePanelFunctions.Clear();

         MenuPanel.transform.SetSiblingIndex(MenuPanel.transform.parent.childCount - 2);

         if (currentContent != null)
             Destroy(currentContent);

         currentContent = Instantiate(ContentPrefab, MenuPanel.transform.Find("Content Panel"));

         MenuPanel.GetComponent<ScrollRect>().content = currentContent.GetComponent<RectTransform>();
         AddButton(currentContent.transform, new Action(delegate { openTribeChat(); }), "Chat");
         Player currentPlayer = Client.instance.Player;
         if (currentPlayer.Tribe != null)
         {
             AddCapacityBar(currentContent.transform, (InventoryBuilding)Client.instance.Player.Tribe.HQ);
             //Show Inventory
             AddInventory(currentContent.transform, (InventoryBuilding) Client.instance.Player.Tribe.HQ);
         }

         AddTroopInventory(currentContent.transform, currentPlayer);
         UpdateStructurePanel();
         */
        if (Client.instance.Player.Tribe != null)
            TribeMenu.SetActive(true);
    }
    public void openQuestMenu() {
        QuestMenu.SetActive(true);
    }

    public void closeTribeMenu() {
        TribeMenu.SetActive(false);
    }
    public void closeQuestMenu() {
        QuestMenu.SetActive(false);
    }
    public void OpenTutorial()
    {
        
        Tutorial.SetActive(true);
    }
    public void CloseTutorial()
    {
        Tutorial.SetActive(false);
    }

    public void OnTuTorialBuildingDropDown(int val)
    {
        if (val <= 21)
        {
            BuildingtoShow.sprite = Buildings_Tutorial[val];
        }
        else
        {
            BuildingtoShow.sprite = null;
        }
        switch (val)
        {
            case 0:
                buildingInfo.text = "The Headquarter can be placed to found a new Tribe. Different levels of the Headquarter grant access to other Buildings. The Headquarter also includes an Inventory for the Tribe which can be accessed from anywhere. Ressources in the Inventory can be used to build other Buildings or be refined into better Ressources or Troops. By Building Roads ressources could be transported to buildings, which produce further complicated ressources";
                break;
            case 1:
                buildingInfo.text = "The Assembly Points are where your troops assemble to attack the enemie's ProtectBuildings. Every Assembly Point holds as much soldier as its current level allows. Since they could be built outside of your territory they are the perfect way to station your soldiers if you plan to declare war on other tribes.";
                break;
            case 2:
                buildingInfo.text= "The Bakery is used to process Wheat and Wood into Food.";
                break;
            case 3:
                buildingInfo.text = "The Barracks produces soldier for your tribe. Currently Knights, Archers, Spearmen and Siege Engines can be produced. Their usage varies depending on the battle. Don't forget! Without a military you are exposed to enemy attack. ";
                break;
            case 4:
                buildingInfo.text = "It is basically used for transport and functions so much like a road. A bridge needs to be placed on water.";
                break;
            case 5:
                buildingInfo.text = "The Butcher is used to process Cows into Food.";
                break;
            case 6:
                buildingInfo.text = "The Coalmine is used to produce Coal from a nearby Coalressource. The Coalmine needs to be placed adjacent to atleast one Coalressource. More adjacent Coalressources will improve the efficiency of the mine.";
                break;
            case 7:
                buildingInfo.text = "The Cowfarm is used to get Cows from a nearby Cowressource. The Cowfarm needs to be placed adjacent to atleast one Cow. More adjacent Cows will improve the efficiency of the farm.";
                break;
            case 8:
                buildingInfo.text = "The Fisher is used to gain Food from a nearby Fishressource. The Fisher needs to be placed adjacent to Fishressource. More adjacent Fish will impove the efficiency of the Fisher.";
                break;
            case 9:
                buildingInfo.text = "The guild house is needed in order to be able to join a guild. Guilds are basically pacts between tribes. They bring so much advantage to a tribe. However because you need to share ressources with your guild members, sometimes this advantage could be abused by guild members. It is good to jnow who to trust!";
                break;
            case 10:
                buildingInfo.text = "TThe library is used to research new technologies.";
                break;
            case 11:
                buildingInfo.text = "The Market can be used to exchange any Ressource into any other Ressource. Higher levels of the market will offer a better trade ratio between those Ressources. In order to use market though you need to connect it to other Inventory Buildings. Eventhough connecting the buildings is easy and doesnt cost much, it is good to remember that you could always build a limited number of roads!";
                break;
            case 12:
                buildingInfo.text = "The Quarry is used to produce Stone from nearby Rocks. The Quarry needs to be placed adjacent to atleast one Rock. More adjacent Rocks will improve the efficiency of the Quarry.";
                break;
            case 13:
                buildingInfo.text = "Road is the most important construction for the civilastion. Without them you can't transport your ressources and troops. Without a good infrastructure you will fail as a civilastion. Construct them with care!";
                break;
            case 14:
                buildingInfo.text = "The Smelter is used to process Stone and Coal into Iron, which then again could be used to produce weapons. Don't forget! A soldier's weapon is his honour!";
                break;
            case 15:
                buildingInfo.text = "Remember the iron you produced in smelters? Now the Weapon Smith will use them to create weapons for troops. They are the only way to get your hands on the weapons. Treat them with care!";
                break;
            case 16:
                buildingInfo.text = "The Storage is used to store Ressources. Higher Levels will improve the Capacity of the Storage. The Storage can also be used as a distributor for the Ressources. It is a good idea to improve your storage capacity early in the world";
                break;
            case 17:
                buildingInfo.text = "The Tanner is used to process Cows into Leather. Leather is a valueable item. You need to plan it well how much of the cows should be used to produce food and how much for the leather.";
                break;
            case 18:
                buildingInfo.text = "The Tower is used to protect and expand your territory. If you place a tower at your border, under normal circumstances it will expands your border by five hexagons. By expanding your territory you claim all of the ressources in this region. However you are probably going to anger other tribes as well";
                break;
            case 19:
                buildingInfo.text = "The Wheatfarm is used to produce Wheat from nearby crops. The Wheatfarm needs to be placed adjacent to atleast one Crop. More adjacent Crops will increase the efficiency of the farm.";
                break;
            case 20:
                buildingInfo.text = "he Woodcutter is used to produce Wood from nearby Woodressources. The Woodcutter needs to be placed adjacent atleast one Tree or Bush. More adjacent Trees or Bushes will increase the efficiency and Trees are more efficient than Bushes.";
                break;
            case 21:
                buildingInfo.text = "The Workshop is used to construct Siege Engines, which are a type of Troop specialized in attacking enemy buildings. Without them you can't destroy buildings!. If you are planning a war, you must have siege engines.";
                break;
            case 22:
                buildingInfo.text = "Ruins is the what is left the buildings after an enemy attack. They can be repaired.";
                break;
            case 23:
                buildingInfo.text = "Buildings that could hold troops. You are only allowed to attack at this kind of buildings. Barracks, Assembly Points, Towers and most importantly Headquarters are protect buildings. It means they could hold soldiers and potentially can be attacked";
                break;
            case 24:
                buildingInfo.text = "Inventory Buildings are the buildings that hold any kind of inventory. They van't be attacked and their inventory space will grow if you upgrdae them.";
                break;
            case 25:
                buildingInfo.text = "Buildings that produce an asset. These buildings are always set near a ressource. Coal Mine, Fisher, Cow farm Quarry, Wheat Farm etc.";
                break;

            default:
                break;
        }
    }

    public void openFeedback(Feedback feedback)
    {
        FeedBackManager.gameObject.SetActive(true);
        FeedBackManager.takeFeedback(feedback);
    }

    public void closeFeedback()
    {
        FeedbackPanel.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "";
        FeedbackPanel.SetActive(false);
    }

    public void OpenMapView()
    {
        MenuPanel.transform.GetChild(4).GetComponent<Draggable>().Open(2);
        ResetFunctions();

        MenuPanel.transform.SetSiblingIndex(MenuPanel.transform.parent.childCount - 2);

        if (currentContent != null)
        {
            Destroy(currentContent);
        }

        currentContent = Instantiate(ContentPrefab, MenuPanel.transform.Find("Content Panel"));

        MenuPanel.GetComponent<ScrollRect>().content = currentContent.GetComponent<RectTransform>();

        GameObject mapView = Instantiate(MapViewPrefab, currentContent.transform);
        if (GameLogic.initialized)
        {
            RawImage image = mapView.transform.GetChild(0).GetComponent<RawImage>();
            RawImage imageTransparent = mapView.transform.GetChild(1).GetComponent<RawImage>();

            List<Tuple<int, int>> Coordinates = MapView.PlayerCoordinates();
            if (Coordinates.Count < 10)
            {
                for (int i = 0; i < Coordinates.Count; i++)
                {
                    mapView.transform.GetChild(2).GetChild(i).gameObject.SetActive(true);
                    if (GameLogic.Players[i].Avatar < 255)
                    {
                        mapView.transform.GetChild(2).GetChild(i).GetComponentInChildren<Image>().sprite = Avatars[GameLogic.Players[i].Avatar];
                    }
                    else
                    {
                        mapView.transform.GetChild(2).GetChild(i).GetComponentInChildren<Image>().sprite = null;
                    }

                    mapView.transform.GetChild(2).GetChild(i).gameObject.transform.localPosition = new Vector3(Coordinates[i].Item1, Coordinates[i].Item2, 0);
                }
            }
            else
            {
                FeedbackPanel.SetActive(true);
                FeedbackPanel.GetComponentInChildren<TMP_Text>().SetText("More than 10 players will not be shown on the map!");
            }

            updateMapFunction = () => image.texture = MapView.getMapView(GameLogic.grid, 0);
            updateMapFunction();
            updateMapFunction = () => imageTransparent.texture = MapView.getMapView(GameLogic.grid, 1);
            updateMapFunction();
        }
        else
        {
            Debug.Log("Map is not initialized yet.");
        }
        UpdateStructurePanel();
    }

    private void OpenStructurePanel(Structure structure)
    {
        ResetFunctions();

        MenuPanel.transform.SetAsLastSibling();

        if (currentContent != null) Destroy(currentContent);

        Transform contentpanel = MenuPanel.transform.Find("Content Panel");

        currentContent = Instantiate(ContentPrefab, contentpanel);

        MenuPanel.GetComponent<ScrollRect>().content = currentContent.GetComponent<RectTransform>();

        AddStructureInformation(currentContent.transform, structure);

        Canvas.ForceUpdateCanvases();

        if (structure is Building)
        {
            Building building = (Building)structure;

            Tribe playerTribe = Client.instance.Player.Tribe;
            Tribe otherTribe = GameLogic.GetTribe(building.Tribe);

            // check for player tribe
            if (playerTribe != null)
            {
                if (building.Tribe == playerTribe.Id && !(building is Ruin))
                {
                    // building of our tribe

                    if (building is Barracks)
                    {
                        AddBarracksInformation(currentContent.transform, (Barracks)building);
                    }
                    else if (building is TroopProductionBuilding)
                    {
                        AddTroopProductionInformation(currentContent.transform, (TroopProductionBuilding)building);
                    }

                    if (building is RefineryBuilding)
                    {
                        AddRefineryInformation(currentContent.transform, (RefineryBuilding)building);
                    }

                    if (building is ProductionBuilding)
                    {
                        AddProductionInformation(currentContent.transform, (ProductionBuilding)building);
                    }

                    if (building is MultiRefineryBuilding)
                    {
                        AddRecipeSelect(currentContent.transform, (MultiRefineryBuilding)building);
                    }

                    if (building is Market)
                    {
                        AddMarketInformation(currentContent.transform, (Market)building);
                    }

                    // troop inventory
                    if (building is ProtectedBuilding)
                    {
                        ProtectedBuilding protectedBuilding = (ProtectedBuilding)building;
                        AddTroopInventory(currentContent.transform, protectedBuilding.Cell, protectedBuilding.TroopInventory);
                        if (building is AssemblyPoint)
                        {
                            addPriorityPanel(currentContent.transform, (AssemblyPoint)protectedBuilding);
                        }
                    }
                    else if (building is TroopProductionBuilding)
                    {
                        TroopProductionBuilding troopBuilding = (TroopProductionBuilding)building;
                        AddTroopInventory(currentContent.transform, troopBuilding.Cell, troopBuilding.TroopInventory);
                    }

                    // building ressource inventory
                    if (building is InventoryBuilding)
                    {
                        AddInventory(currentContent.transform, (InventoryBuilding)building);
                    }

                    if (building is Library)
                    {
                        AddLibraryResearchOptions(currentContent.transform);
                    }

                    // upgrade button
                    if (building.Level < building.MaxLevel && !(building is Ruin || building is GuildHouse))
                    {
                        AddUpgradeButton(currentContent.transform, building.Recipes[building.Level]);
                    }

                    if (building is GuildHouse)
                    {
                        if (playerTribe.HasNoGuild())
                        {
                            void OnPressCreateGuild()
                            {
                                ClientSend.RequestCreateGuild(building.Cell.coordinates);
                                ClosePanel();
                            }

                            AddButton(currentContent.transform, OnPressCreateGuild, "Create Guild");
                        }
                        else
                        {
                            void OnPressLeaveGuild()
                            {
                                ClientSend.RequestLeaveGuild(playerTribe.GuildId, building.Cell.coordinates);
                                ClosePanel();
                            }

                            Guild guild = GameLogic.GetGuild(playerTribe.GuildId);
                            
                            AddGuildInformation(currentContent.transform, guild);
                            AddGuildLevelingPanel(currentContent.transform, guild);
                            AddGuildInventoryPanel(currentContent.transform, guild);
                            AddGuildBoostPanel(currentContent.transform, (GuildHouse)building);
                            AddButton(currentContent.transform, OnPressLeaveGuild, "Leave Guild");
                        }
                    }

                    if (building is InventoryBuilding)
                    {
                        AddRessourceSelector(currentContent.transform, (InventoryBuilding)building);
                    }

                    if (building is Headquarter)
                    {
                        AddBuffInventory(currentContent.transform, Client.instance.Player.Tribe);
                        AddButton(currentContent.transform, new Action(delegate { ClientSend.RequestLeaveTribe(); ClosePanel(); }), "Leave Tribe");
                    }

                    // Destroy button on every building except headquarter
                    if (!(building is Headquarter))
                    {
                        AddButton(currentContent.transform, new Action(delegate { ClientSend.RequestDestroyBuilding((Building)structure); ClosePanel(); }), "Destroy");
                    }
                }
                else if (building is ProtectedBuilding &&  
                         // Only allow an attack if guild IDs differ (i.e. not part of the same guild)
                         // But if both have ID_NO_GUILD (both are guild-less), then attacking each other is fine.
                         (otherTribe.GuildId != playerTribe.GuildId ||
                         (otherTribe.HasNoGuild() && playerTribe.HasNoGuild())))
                {
                    // Scout/attack tribes that are not part of they player's tribe's guild

                    foreach (Tuple<byte, TroopInventory> tpl in ((ProtectedBuilding)building).scoutResults)
                    {
                        bool hasGuildMemberScouted = playerTribe.HasGuild() && 
                                                     GameLogic.GetGuild(playerTribe.GuildId).Members
                                                         .Any(tribe => tpl.Item1 == tribe.Id);
                        
                        if (tpl.Item1 == playerTribe.Id || 
                            (playerTribe.HasResearched(Research.CARRIER_PIGEON) && hasGuildMemberScouted))
                        {
                            ProtectedBuilding protectedBuilding = (ProtectedBuilding)building;
                            AddTroopInventory(currentContent.transform, protectedBuilding.Cell, tpl.Item2);
                            break;
                        }
                    }

                    // check for protected hq
                    if (building is Headquarter && ((Headquarter)building).isProtected())
                    {
                    }
                    else
                    {
                        // protected building of enemy tribe
                        List<Building> aps = GameLogic.getAssemblyPointsInRange(Client.instance.Player.Tribe, building);
                        //AssemblyPoints are on cooldown available.
                        if (aps.Count < 1) return;
                        
                        foreach (Building b in aps)
                        {
                            AddButton(currentContent.transform, () =>
                            {
                                ClosePanel();
                                //check whether this assemblies have soldier
                                int soldiers = 0;
                                foreach (KeyValuePair<TroopType, int> kvp in ((AssemblyPoint)b).TroopInventory.Troops)
                                {
                                    //Debug.Log("troop: " + kvp.Key + " sayi: " + kvp.Value);
                                    soldiers += kvp.Value;

                                }
                                if (soldiers < 1)
                                {
                                    Feedback feedback = new Feedback("No Soldier in This Assembly Point");
                                    feedback.feedbackStyle = Feedback.FeedbackStyle.ui;
                                    openFeedback(feedback);                                 
                                }
                                else
                                {
                                    ClientSend.Fight(b.Cell.coordinates, this.selectedCell.coordinates);
                                    FindObjectOfType<HexMeshGrid>().DeclareBattle(this.selectedCell.coordinates);
                                }
                               
                            }, "Attack");

                            AddTroopInventory(currentContent.transform, b.Cell, ((AssemblyPoint)b).TroopInventory, ((AssemblyPoint)b).Cooldown);
                        }
                    }
                }
                else if (building is GuildHouse)
                {
                    AddGuildInformation(currentContent.transform, GameLogic.GetGuild(otherTribe.GuildId));

                    if (playerTribe.HasGuildHouse() && playerTribe.HasNoGuild() && otherTribe.HasGuild())
                    {
                        void OnPressJoinGuild()
                        {
                            ClientSend.RequestJoinGuild(otherTribe.GuildId, building.Cell.coordinates);
                            ClosePanel();
                        }

                        AddButton(currentContent.transform, OnPressJoinGuild, "Join Guild");
                    }
                    else if (playerTribe.HasGuild() && playerTribe.GuildId == otherTribe.GuildId)
                    {
                        void OnPressLeaveGuild()
                        {
                            ClientSend.RequestLeaveGuild(otherTribe.GuildId, building.Cell.coordinates);
                            ClosePanel();
                        }

                        AddButton(currentContent.transform, OnPressLeaveGuild, "Leave Guild");
                    }
                    else
                    {
                        // Consider stuff like declaring war
                    }
                }
                else {
                    //repair button and Salvage Button
                    if (building is Ruin)
                    {
                        if (building.Cell.GetCurrentTribe() == Client.instance.Player.Tribe.Id)
                        {
                            AddRepairButton(currentContent.transform, building.Recipes[building.Level - 1]);
                            AddSalvageButton(currentContent.transform, building.Recipes[building.Level - 1]);
                        }
                    }
                }
            }
        }
        Canvas.ForceUpdateCanvases();

        //RefreshLayoutGroupsImmediateAndRecursive(cp.gameObject);
        StartCoroutine(reEnableAfterFrame());
        UpdateStructurePanel();
    }

    IEnumerator reEnableAfterFrame() {
        MenuPanel.transform.Find("Content Panel").GetChild(0).gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        MenuPanel.transform.Find("Content Panel").GetChild(0).gameObject.SetActive(true);
        float height = MenuPanel.transform.Find("Content Panel").GetChild(0).GetComponent<VerticalLayoutGroup>().preferredHeight;
        MenuPanel.transform.GetChild(4).GetComponent<Draggable>().OpenHalfWay(height);
    }
    public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
    {
        var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);

        foreach (var layoutGroup in componentsInChildren)

        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        var parent = root.GetComponent<LayoutGroup>();

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
    }

    public void ReloadStructure(HexCoordinates structureCoordinates)
    {
        HexCell cell = GameLogic.grid.GetCell(structureCoordinates);
        if (cell == null || cell.Structure == null)
            return;

        OpenStructurePanel(cell.Structure);
    }

    public void ClosePanel()
    {
        MenuPanel.transform.GetChild(4).GetComponent<Draggable>().Close();
        ResetFunctions();
    }



    #endregion

    #region UIElements

    private void AddToolchainElement(Transform parent, ProgressBuilding building)
    {
        GameObject group = Instantiate(GroupPrefab, parent);

        group.transform.GetChild(0).GetComponent<TMP_Text>().text = "Toolchain";

        GameObject toolchain = Instantiate(ToolchainPrefab, group.transform);

        Transform inputRecipe = toolchain.transform.GetChild(0);
        Transform outputRecipe = toolchain.transform.GetChild(2);
        if (building is RefineryBuilding)
        {
            foreach (KeyValuePair<RessourceType, int> kvp in ((RefineryBuilding)building).InputRecipe)
            {
                GameObject ressourceElement = Instantiate(RessourceElementPrefab, inputRecipe);
                ressourceElement.transform.GetChild(0).GetComponent<Image>().color = kvp.Key.ToColor();
                ressourceElement.transform.GetChild(1).GetComponent<TMP_Text>().text = kvp.Value.ToString();
            }
            foreach (KeyValuePair<RessourceType, int> kvp in ((RefineryBuilding)building).OutputRecipe)
            {
                GameObject ressourceElement = Instantiate(RessourceElementPrefab, outputRecipe);
                ressourceElement.transform.GetChild(0).GetComponent<Image>().color = kvp.Key.ToColor();
                ressourceElement.transform.GetChild(1).GetComponent<TMP_Text>().text = kvp.Value.ToString();
            }
        }
        else
        {
            inputRecipe.gameObject.SetActive(false);
            if (building is ProductionBuilding)
            {
                GameObject ressourceElement = Instantiate(RessourceElementPrefab, outputRecipe);
                ressourceElement.transform.GetChild(0).GetComponent<Image>().color = ((ProductionBuilding)building).ProductionType.ToColor();
                ressourceElement.transform.GetChild(1).GetComponent<TMP_Text>().text = ((ProductionBuilding)building).Gain.ToString();
            }
        }

    }

    private void AddStructureInformation(Transform parent, Structure structure)
    {
        GameObject group = Instantiate(StructureInformationPrefab, parent);

        group.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().SetText(structure.GetFriendlyName());


        if (structure is Ressource)
        {
            Ressource r = (Ressource)structure;
            group.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().SetText("");
            group.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[ressourceIndizes[r.ressourceType]];
            if (!(r is Grass)) {
                AddRessourceInformation(group.transform.GetChild(1), (Ressource)structure);
            }
        }
        else if (structure is Collectable)
        {
            Collectable c = (Collectable)structure;
            group.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().SetText("");
            group.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = truhenImage;
            AddCollectableInformation(group.transform.GetChild(1), (Collectable)structure);
          
        }
        else if (structure is Building)
        {
            Building building = (Building)structure;
            group.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = GetBuildingSprite(building);
            group.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().SetText("Level " + building.Level.ToString());

            if (Client.instance.Player.Tribe == null)
            {


                if (building is Headquarter)
                {
                    if (Client.instance.Player.Avatar == (byte)255)
                    {
                        GameObject AvatarSelectionMenu;

                        for (int i = 0; i < Avatars.Length; i++)
                        {
                            var j = i;
                            AvatarSelectionMenu = (Instantiate(AvatarSelection, parent));
                            AvatarSelectionMenu.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Avatars[i];
                            AvatarSelectionMenu.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(
                                () =>
                                {
                                    Client.instance.Player.Avatar = (byte)j;
                                    if (!group.transform.GetComponentInChildren<Button>())
                                        AddButton(group.transform, JoinTribeCallback, "Join Tribe");

                                });

                            AvatarSelectionMenu.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().SetText("KAWAII :3");
                        }

                    }
                    else
                    {
                        AddButton(group.transform, JoinTribeCallback, "Join Tribe");
                    }


                }
            }
            else
            {

                if (Client.instance.Player.Tribe.Id == building.Tribe)
                {
                    AddBuildingInformation(group.transform, building);
                }
            }
        }
    }

    private void AddRessourceInformation(Transform parent, Ressource ressource)
    {
        AddProgressBar(parent.GetChild(0), ressource);
        AddButton(parent.GetChild(1), new Action(delegate { HarvestCallback(ressource); }), "Harvest");
    }

    private void AddCollectableInformation(Transform parent, Collectable collectable) 
    {
        AddButton(parent.GetChild(1), new Action(delegate { CollectCallback(collectable); }), "Collect");
    }

    private void AddBuildingInformation(Transform parent, Building building)
    {
        if (building is ProtectedBuilding)
        {
            ProtectedBuilding protectedBuilding = (ProtectedBuilding)building;
            AddHealthBar(parent.GetChild(1).GetChild(0), protectedBuilding);
            AddTroopBar(parent.GetChild(1).GetChild(0), protectedBuilding.TroopInventory);
        }
        else if (building is TroopProductionBuilding)
        {
            TroopProductionBuilding troopBuilding = (TroopProductionBuilding)building;
            AddTroopBar(parent.GetChild(1).GetChild(0), troopBuilding.TroopInventory);
        }
    }

    private void AddBuildingDisplay(Transform parent, Type buildingType)
    {
        GameObject buildingDisplay = Instantiate(BuildComponentPrefab, parent);
        
        Tribe tribe = Client.instance.Player.Tribe;
        
        //Image image = buildingDisplay.transform.Find("Image").GetComponent<Image>();
        //GameObject prefab = prefabManager.GetPrefab(buildingType);
        //image.sprite = Sprite.Create(AssetPreview.GetAssetPreview(prefab), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        //buildingDisplay.transform.Find("Image").GetComponent<Image>().sprite = buildingImages[BuildingIndizes[buildingType]];
        if (buildingImages.ContainsKey(buildingType))
        {
            Sprite sprite = GetBuildingSprite(buildingType, 1);
            if (buildingType == typeof(GuildHouse) && tribe.HasGuild())
            {
                // GuildHouse level should correspond to guild level.
                Guild guild = GameLogic.GetGuild(tribe.GuildId);
                sprite = GetBuildingSprite(buildingType, guild.Level);
            }
            buildingDisplay.transform.GetChild(2).GetComponent<Image>().sprite = sprite;
        }

        buildingDisplay.transform.Find("Name").GetComponent<TMP_Text>().SetText(Structure.GetFriendlyName(buildingType));

        Transform costPanel = buildingDisplay.transform.Find("Cost Panel");

        Dictionary<RessourceType, int> cost;

        GameObject counter = buildingDisplay.transform.GetChild(0).gameObject;

        Building building = (Building)Activator.CreateInstance(buildingType);

        if (tribe != null)
        {
            counter.GetComponent<TMP_Text>().SetText($"{tribe.CurrentBuildings[buildingType].Count}/{tribe.GetBuildingLimit(building)}");
        }
        cost = building.Recipes[0];

        foreach (RessourceType ressourceType in cost.Keys)
        {
            AddRessourceCost(costPanel, ressourceType, cost[ressourceType], Client.instance.Player.Tribe.HQ.Inventory);
        }

        if (costPanel.transform.childCount <= 0)
        {
            buildingDisplay.transform.Find("Free").gameObject.SetActive(true);
        }

        //Initiate Info Panel
        GameObject infoPanel = buildingDisplay.transform.GetChild(5).gameObject;
        infoPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = Structure.GetFriendlyName(buildingType);
        infoPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = building.description;

        //Add rotation function to info button
        buildingDisplay.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(delegate { FlipCard(buildingDisplay, infoPanel); });

        // buildingDisplay.GetComponent<Button>().onClick.AddListener(delegate () { ClientSend.RequestPlaceBuilding(this.selectedCell.coordinates, buildingType); ClosePanel(MenuPanel); });
        buildingDisplay.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            this.placeBuildingType = buildingType;
            RefreshCellHighlighting();
            ClosePanel();
        });

    }
    private void AddBuffTime(Transform parent, TribeBuff buff)
    {
        GameObject bar = Instantiate(BarPrefab, parent);
        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(0, 200, 0, 255);
        if (buff.isRessource)
        {
            bar.transform.GetChild(2).GetComponent<TMP_Text>().text = buff.rtype.ToFriendlyString() + " + 1";
        }
        else if (buff.isTroop)
        {
            bar.transform.GetChild(2).GetComponent<TMP_Text>().text = buff.ttype.ToFriendlyString() + " * 2";
        }

        updateStructurePanelFunctions.Add(new Action(delegate { UpdateTimeBar(bar, buff.maxTime - buff.ticks, (float)buff.ctime / (float)buff.maxTime); }));
    }
    private void AddBuffInventory(Transform parent, Tribe tribe)
    {
        GameObject inventoryGroup = Instantiate(GroupPrefab, parent);

        inventoryGroup.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Buff Inventory");

        foreach (TribeBuff buff in tribe.Buffs)
        {
            AddBuffTime(inventoryGroup.transform, buff);
        }
    }


    private void AddRessourceSelector(Transform parent, InventoryBuilding inventoryBuilding)
    {
        GameObject group = Instantiate(GroupPrefab, parent);
        group.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Outgoing Ressources");

        //Find all connected Buildings that share a ressource in inventoryBuilding.Outgoing and destination.Incoming
        List<Tuple<InventoryBuilding, List<RessourceType>>> connectedBuildings = new List<Tuple<InventoryBuilding, List<RessourceType>>>();
        foreach (InventoryBuilding cmp in inventoryBuilding.ConnectedInventories.Keys)
        {
            List<RessourceType> fittingTypes = new List<RessourceType>();
            foreach (RessourceType outgoingType in inventoryBuilding.Inventory.Outgoing)
            {
                if (cmp.Inventory.Incoming.Contains(outgoingType))
                {
                    fittingTypes.Add(outgoingType);
                }
            }
            if (fittingTypes.Count > 0)
            {
                connectedBuildings.Add(new Tuple<InventoryBuilding, List<RessourceType>>(cmp, fittingTypes));
            }
        }

        //Add a RessourceSelector for each Building and each shared Ressourcetype
        foreach (Tuple<InventoryBuilding, List<RessourceType>> tpl in connectedBuildings)
        {
            GameObject ressourceSelector = Instantiate(RessourceSelectorPrefab, group.transform);
            ressourceSelector.transform.GetChild(0).GetComponent<TMP_Text>().text = tpl.Item1.GetFriendlyName();

            foreach (RessourceType ressourceType in tpl.Item2)
            {
                GameObject ressourceToggle = Instantiate(RessourceTogglePrefab, ressourceSelector.transform.GetChild(1));
                Color c = ressourceType.ToColor();

                ressourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];
                ressourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];

                //GameObject cross = selectableRessourceElement.transform.GetChild(0).GetChild(0).gameObject;
                Toggle toggle = ressourceToggle.transform.GetChild(0).GetComponent<Toggle>();

                ressourceToggle.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ClientSend.RequestChangeAllowedRessource(
                        inventoryBuilding.Cell.coordinates,
                        tpl.Item1.Cell.coordinates,
                        ressourceType,
                        !inventoryBuilding.AllowedRessources[tpl.Item1][ressourceType]
                    );
                });

                updateStructurePanelFunctions.Add(delegate { this.UpdateRessourceToggle(toggle, inventoryBuilding, tpl.Item1, ressourceType); });
            }
        }
    }
    private void AddRessourceCost(Transform parent, RessourceType ressourceType, int cost, BuildingInventory buildingInventory)
    {
        GameObject re = Instantiate(RessourceElementPrefab, parent);
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateRessourceElement(re, ressourceType, cost, buildingInventory); }));
        re.GetComponentInChildren<TMP_Text>().SetText(cost.ToString());
        re.transform.GetChild(0).GetComponentInChildren<Image>().sprite = ressourceImages[(int)ressourceType];
    }

    private void AddRessourceElement(Transform parent, RessourceType ressourceType, int cost, BuildingInventory buildingInventory)
    {
        GameObject re = Instantiate(RessourceElementPrefab, parent);

        re.GetComponentInChildren<TMP_Text>().SetText(cost.ToString());
        re.transform.GetChild(0).GetComponentInChildren<Image>().sprite = ressourceImages[(int)ressourceType];
    }

    private void AddHealthBar(Transform parent, ProtectedBuilding building)
    {
        GameObject bar = Instantiate(BarPrefab, parent);

        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        bar.transform.GetChild(2).GetComponent<TMP_Text>().text = "Health";
        updateStructurePanelFunctions.Add(() => UpdateBar(bar, building.Health, building.MaxHealth));
    }

    private void AddProgressBar(Transform parent, Ressource ressource)
    {
        GameObject bar = Instantiate(BarPrefab, parent);

        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(0, 200, 0, 255);
        bar.transform.GetChild(2).GetComponent<TMP_Text>().text = "Progress";
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateTimeBar(bar, ressource.MaxProgress - ressource.Progress, (float)ressource.Progress / (float)ressource.MaxProgress); }));
    }

    private void AddProgressBar(Transform parent, ProgressBuilding building)
    {
        GameObject bar = Instantiate(BarPrefab, parent);

        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(200, 128, 128, 255);
        bar.transform.GetChild(2).GetComponent<TMP_Text>().text = "Progress";
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateTimeBar(bar, building.MaxProgress - building.Progress, (float)building.Progress / (float)building.MaxProgress); }));
    }
    private void AddProgressBar(Transform parent, float coolDown)
    {
        GameObject bar = Instantiate(BarPrefab, parent);

        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(200, 128, 128, 255);
        bar.transform.GetChild(2).GetComponent<TMP_Text>().text = "Progress";
        //updateStructurePanelFunctions.Add(new Action(delegate { UpdateTimeBar(bar, building.MaxProgress - building.Progress, (float)building.Progress / (float)building.MaxProgress); }));
    }

    private void AddTroopBar(Transform parent, TroopInventory troops)
    {
        GameObject bar = Instantiate(BarPrefab, parent);

        bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(200, 128, 128, 255);
        bar.transform.GetChild(2).GetComponent<TMP_Text>().text = "Troops";
        updateStructurePanelFunctions.Add(() => UpdateBar(bar, troops.TroopLimit - troops.GetAvailableSpace(), troops.TroopLimit));
    }

    private void AddTroopBar(Transform parent, Player player, TroopType type)
    {
        //Reference objects for better access
        GameObject bar = Instantiate(TroopbarPrefab, parent);
        Transform frontside = bar.transform.GetChild(0);
        Transform backside = bar.transform.GetChild(1);

        //Set necessary fields for DragController ChangeOrderOfStrategy Callback
        frontside.GetChild(1).GetComponent<DragController>().playerInventory = true;
        frontside.GetChild(1).GetComponent<DragController>().coordinates = player.Position;

        //Wether or not the current TroopType is active
        frontside.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestChangeActiveOfStrategy(type, !player.TroopInventory.Strategy.Find(tpl => tpl.Item1 == type).Item2); });
        updateStructurePanelFunctions.Add(new Action(delegate { frontside.GetChild(2).GetChild(1).gameObject.SetActive(!player.TroopInventory.Strategy.Find(tpl => tpl.Item1 == type).Item2); }));

        //Progress bar
        frontside.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().color = Color.HSVToRGB((float)type * 0.2f, 1, 1);
        frontside.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = type.ToString();
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateBar(frontside.GetChild(0).gameObject, player.TroopInventory.Troops[type], player.TroopInventory.Troops[type] + player.TroopInventory.GetAvailableSpace()); }));

        //Update the size of the overall object
        bar.transform.GetComponent<RectTransform>().sizeDelta = frontside.transform.GetComponent<RectTransform>().sizeDelta;

        //Deactivate the backside
        backside.gameObject.SetActive(false);
    }

    private void AddTroopBar(Transform parent, HexCell cell, TroopInventory inventory, TroopType type)
    {
        //Reference objects for better access
        GameObject bar = Instantiate(TroopbarPrefab, parent);
        Transform frontside = bar.transform.GetChild(0);
        Transform backside = bar.transform.GetChild(1);
        Transform takeSelector = backside.GetChild(1).GetChild(0);
        Transform giveSelector = backside.GetChild(1).GetChild(1);
        Slider takeSlider = takeSelector.GetChild(0).GetComponent<Slider>();
        Slider giveSlider = giveSelector.GetChild(0).GetComponent<Slider>();

        //Set necessary fields for DragController ChangeOrderOfStrategy Callback
        frontside.GetChild(1).GetComponent<DragController>().playerInventory = false;
        frontside.GetChild(1).GetComponent<DragController>().coordinates = cell.coordinates;
        backside.GetChild(2).GetComponent<DragController>().playerInventory = false;
        backside.GetChild(2).GetComponent<DragController>().coordinates = cell.coordinates;

        //Wether or not the current TroopType is active
        frontside.GetChild(2).GetComponent<Button>().onClick.AddListener(() => ClientSend.RequestChangeActiveOfStrategy(cell.coordinates, type, !inventory.Strategy.Find(tpl => tpl.Item1 == type).Item2));
        updateStructurePanelFunctions.Add(() => frontside.GetChild(2).GetChild(1).gameObject.SetActive(!inventory.Strategy.Find(tpl => tpl.Item1 == type).Item2));

        Color troopColor = Color.HSVToRGB((float)type * 0.3f, 0.6f, 1);

        //Progress bar
        frontside.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().color = troopColor;
        frontside.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = type.ToFriendlyString();
        backside.GetChild(0).GetComponent<TMP_Text>().text = type.ToFriendlyString();

        backside.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().color = troopColor;
        backside.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().color = troopColor;
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateBar(frontside.GetChild(0).gameObject, inventory.Troops[type], inventory.Troops[type] + inventory.GetAvailableSpace()); }));

        //Listeners on Slider
        takeSlider.onValueChanged.AddListener(_ => takeSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = takeSlider.value.ToString());
        giveSlider.onValueChanged.AddListener(_ => giveSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = giveSlider.value.ToString());
        updateStructurePanelFunctions.Add(() => UpdateTroopSlider(takeSlider, inventory, type));
        updateStructurePanelFunctions.Add(() => UpdateTroopSlider(giveSlider, Client.instance.Player.TroopInventory, type));
        takeSelector.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
        {
            // TODO: ui for selecting destination building
            HexCoordinates dest = new HexCoordinates(0, 0);
            ClientSend.MoveTroops(cell.coordinates, dest, type, (int)takeSelector.GetChild(0).GetComponent<Slider>().value);
            FlipTroopbar(frontside.gameObject, backside.gameObject, bar.transform);
        });
        giveSelector.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
        {
            // TODO: ui for selecting destination building
            HexCoordinates dest = new HexCoordinates(0, 0);
            ClientSend.MoveTroops(cell.coordinates, dest, type, -(int)giveSelector.GetChild(0).GetComponent<Slider>().value);
            FlipTroopbar(frontside.gameObject, backside.gameObject, bar.transform);
        });

        //Update the size of the overall object
        bar.transform.GetComponent<RectTransform>().sizeDelta = frontside.transform.GetComponent<RectTransform>().sizeDelta;


        //Deactivate the backside
        backside.gameObject.SetActive(false);

        //Flip Listeners
        backside.GetComponent<Button>().onClick.AddListener(() => FlipTroopbar(frontside.gameObject, backside.gameObject, bar.transform));
        frontside.GetComponent<Button>().onClick.AddListener(() => FlipTroopbar(frontside.gameObject, backside.gameObject, bar.transform));
    }

    private void AddButton(Transform parent, Action funk, String text, bool interactable = true)
    {
        GameObject btn = Instantiate(ButtonPrefab, parent);
        btn.transform.GetChild(0).GetComponent<TMP_Text>().SetText(text);
        btn.GetComponent<Button>().onClick.AddListener(delegate { funk(); });
        btn.GetComponent<Button>().interactable = interactable;
    }

    private void AddLevelDisplay(Transform parent, Building building)
    {
        GameObject lvlDisplay = Instantiate(LevelDisplayPrefab, parent);
        lvlDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = building.Level.ToString() + "/" + building.MaxLevel.ToString();
    }

    private void AddRessourceBar(Transform parent, InventoryBuilding building, RessourceType type)
    {
        GameObject ressourceBar = Instantiate(RessourceBarPrefab, parent);
        Transform frontside = ressourceBar.transform.GetChild(0);
        Transform backside = ressourceBar.transform.GetChild(1);
        Slider takeSlider = backside.GetChild(1).GetChild(0).GetChild(0).GetComponent<Slider>();
        Slider giveSlider = backside.GetChild(1).GetChild(1).GetChild(0).GetComponent<Slider>();
        Slider ressourceLimitSlider = backside.GetChild(1).GetChild(2).GetChild(0).GetComponent<Slider>();

        Color ressourceColor = type.ToColor();

        /*-- Init frontside --*/
        //Progressbar
        frontside.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = type.ToFriendlyString();
        frontside.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().color = ressourceColor;
        updateStructurePanelFunctions.Add(() => UpdateBar(
            frontside.GetChild(0).gameObject,
            building.Inventory.GetRessourceAmount(type),
            building.Inventory.GetLimit(type)
        ));

        /*-- Init backside --*/

        backside.GetChild(0).GetComponent<TMP_Text>().text = type.ToFriendlyString();

        //Init & Update functions for sliders

        //Take and give sliders dont need to exist on Headquarter
        if (!(building is Headquarter))
        {
            UpdateRessourceSlider(takeSlider, building.Inventory, Client.instance.Player.Tribe.HQ.Inventory, type);
            UpdateRessourceSlider(giveSlider, Client.instance.Player.Tribe.HQ.Inventory, building.Inventory, type);
            takeSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = takeSlider.value.ToString();
            giveSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = giveSlider.value.ToString();
            updateStructurePanelFunctions.Add(new Action(delegate { UpdateRessourceSlider(takeSlider, building.Inventory, Client.instance.Player.Tribe.HQ.Inventory, type); }));
            UpdateRessourceSlider(takeSlider, building.Inventory, Client.instance.Player.Tribe.HQ.Inventory, type);
            takeSlider.value = takeSlider.maxValue;
            takeSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = ressourceColor;

            updateStructurePanelFunctions.Add(new Action(delegate { UpdateRessourceSlider(giveSlider, Client.instance.Player.Tribe.HQ.Inventory, building.Inventory, type); }));
            UpdateRessourceSlider(giveSlider, Client.instance.Player.Tribe.HQ.Inventory, building.Inventory, type);
            giveSlider.value = giveSlider.maxValue;
            giveSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = ressourceColor;

            takeSlider.onValueChanged.AddListener(delegate { takeSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = takeSlider.value.ToString(); });
            giveSlider.onValueChanged.AddListener(delegate { giveSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = giveSlider.value.ToString(); });
            backside.GetChild(1).GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestMoveRessources(building, Client.instance.Player.Tribe.HQ, type, (int)takeSlider.value); FlipTroopbar(frontside.gameObject, backside.gameObject, ressourceBar.transform); });
            backside.GetChild(1).GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestMoveRessources(Client.instance.Player.Tribe.HQ, building, type, (int)takeSlider.value); FlipTroopbar(frontside.gameObject, backside.gameObject, ressourceBar.transform); });
        }
        else
        {
            backside.GetChild(1).GetChild(0).gameObject.SetActive(false);
            backside.GetChild(1).GetChild(1).gameObject.SetActive(false);
        }

        // TODO: what is this slider? is this supposed to give players the ability to change ressource limits?
        //RessourceLimitSlider
        ressourceLimitSlider.value = building.Inventory.GetLimit(type);
        ressourceLimitSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = ressourceLimitSlider.value.ToString();
        updateStructurePanelFunctions.Add(new Action(delegate { UpdateRessourceLimitSlider(ressourceLimitSlider, building.Inventory, type); }));
        ressourceLimitSlider.onValueChanged.AddListener(delegate { ressourceLimitSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = ressourceLimitSlider.value.ToString(); });
        UpdateRessourceLimitSlider(ressourceLimitSlider, building.Inventory, type);
        ressourceLimitSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = ressourceColor;
        if (building.Inventory.Limits.ContainsKey(type))
            ressourceLimitSlider.value = building.Inventory.Limits[type];
        else
            ressourceLimitSlider.value = ressourceLimitSlider.maxValue;
        backside.GetChild(1).GetChild(2).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestUpdateRessourceLimit(building, type, (int)ressourceLimitSlider.value); FlipTroopbar(frontside.gameObject, backside.gameObject, ressourceBar.transform); });

        //Update the size of the overall object
        ressourceBar.transform.GetComponent<RectTransform>().sizeDelta = frontside.transform.GetComponent<RectTransform>().sizeDelta;

        //Deactivate backside
        backside.gameObject.SetActive(false);

        //Flip Listeners
        backside.GetComponent<Button>().onClick.AddListener(delegate { FlipTroopbar(frontside.gameObject, backside.gameObject, ressourceBar.transform); });
        frontside.GetComponent<Button>().onClick.AddListener(delegate { FlipTroopbar(frontside.gameObject, backside.gameObject, ressourceBar.transform); });
    }

    private void AddInventory(Transform parent, InventoryBuilding building)
    {
        GameObject Inventory = Instantiate(GroupPrefab, parent);

        Inventory.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Inventory");

        foreach (RessourceType ressourceType in building.Inventory.Limits.Keys)
        {
            this.AddRessourceBar(Inventory.transform, building, ressourceType);
            // GameObject bar = Instantiate(BarPrefab, Inventory.transform);
            // bar.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = ressourceType.ToColor();
            // bar.transform.GetChild(2).GetComponent<TMP_Text>().text = ressourceType.ToString();
            // UpdateBar(bar, building.Inventory.Storage[ressourceType], building.Inventory.Storage[ressourceType] + building.Inventory.AvailableSpace(ressourceType));
            // updateStructurePanelFunctions.Add(new Action(delegate {
            //     UpdateBar(
            //         bar,
            //         building.Inventory.Storage[ressourceType],
            //         building.Inventory.Storage[ressourceType] + building.Inventory.AvailableSpace(ressourceType)
            //     );
            // }));
        }
    }

    private void AddTroopInventory(Transform parent, Player player)
    {
        GameObject inventoryGroup = Instantiate(GroupPrefab, parent);

        inventoryGroup.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Troop Inventory");

        player.TroopInventory.Strategy.ForEach(tpl =>
        {
            AddTroopBar(inventoryGroup.transform, player, tpl.Item1);
        });
    }

    private void AddTroopInventory(Transform parent, HexCell cell, TroopInventory inventory,int cooldown=-1)
    {
        GameObject inventoryGroup = Instantiate(GroupPrefab, parent);
        if (cooldown == -1) { inventoryGroup.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Troop Inventory"); }
        else { inventoryGroup.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Troop Inventory" + $" CoolDown: {cooldown}"); }
        //AddProgressBar(inventoryGroup.transform);
        inventory.Strategy.ForEach(tpl => AddTroopBar(inventoryGroup.transform, cell, inventory, tpl.Item1));
    }

    private void AddRepairButton(Transform parent, Dictionary<RessourceType, int> cost)
    {
        GameObject RepairList = Instantiate(UpgradePrefab, parent);

        RepairList.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Cost");

        foreach (RessourceType ressourceType in cost.Keys)
        {
            AddRessourceCost(RepairList.transform.GetChild(1), ressourceType, (int)(0.5 * cost[ressourceType]), Client.instance.Player.Tribe.HQ.Inventory);
        }

        AddButton(RepairList.transform.GetChild(2), RepairCallback, "Repair");
    }
    private void AddSalvageButton(Transform parent, Dictionary<RessourceType, int> cost) {
        GameObject SalvagePanel = Instantiate(UpgradePrefab, parent);
        SalvagePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Recovered Material";
        foreach (RessourceType rs in cost.Keys)
        {
            AddRessourceElement(SalvagePanel.transform.GetChild(1), rs, (int)(cost[rs] * 0.5), Client.instance.Player.Tribe.HQ.Inventory);
        }
        AddButton(SalvagePanel.transform.GetChild(2), SalvageCallback, "Salvage");
    }

    private void AddUpgradeButton(Transform parent, Dictionary<RessourceType, int> cost)
    {
        GameObject UpgradeList = Instantiate(UpgradePrefab, parent);

        UpgradeList.transform.GetChild(0).GetComponent<TMP_Text>().SetText("Cost");

        foreach (RessourceType ressourceType in cost.Keys)
        {
            AddRessourceCost(UpgradeList.transform.GetChild(1), ressourceType, cost[ressourceType], Client.instance.Player.Tribe.HQ.Inventory);
        }

        AddButton(UpgradeList.transform.GetChild(2), UpgradeCallback, "Upgrade");
    }

    private void AddRefineryInformation(Transform parent, RefineryBuilding refineryBuilding)
    {
        GameObject refineryInformation = Instantiate(RefineryInformationPrefab, parent);

        GameObject progressBar = refineryInformation.transform.GetChild(0).gameObject;

        updateStructurePanelFunctions.Add(new Action(delegate
        {
            UpdateTimeBar(progressBar, refineryBuilding.MaxProgress - refineryBuilding.Progress, (float)refineryBuilding.Progress / (float)refineryBuilding.MaxProgress);
        }));

        Transform input = refineryInformation.transform.GetChild(1).GetChild(0);
        foreach (KeyValuePair<RessourceType, int> kvp in refineryBuilding.InputRecipe)
        {
            AddRessourceCost(input, kvp.Key, kvp.Value, refineryBuilding.Inventory);
        }
        Transform output = refineryInformation.transform.GetChild(1).GetChild(1);
        foreach (KeyValuePair<RessourceType, int> kvp in refineryBuilding.OutputRecipe)
        {
            AddRessourceElement(output, kvp.Key, kvp.Value, refineryBuilding.Inventory);
        }
    }

    private void AddProductionInformation(Transform parent, ProductionBuilding productionBuilding)
    {
        GameObject refineryInformation = Instantiate(RefineryInformationPrefab, parent);

        GameObject progressBar = refineryInformation.transform.GetChild(0).gameObject;

        updateStructurePanelFunctions.Add(new Action(delegate
        {
            UpdateTimeBar(progressBar, productionBuilding.MaxProgress - productionBuilding.Progress, (float)productionBuilding.Progress / (float)productionBuilding.MaxProgress);
        }));

        Transform output = refineryInformation.transform.GetChild(1).GetChild(1);

        AddRessourceElement(output, productionBuilding.ProductionType, productionBuilding.Gain, productionBuilding.Inventory);
    }

    private void AddRecipeSelect(Transform parent, MultiRefineryBuilding building)
    {
        // create recipe select
        GameObject recipeSelect = Instantiate(RecipeSelectPrefab, parent);

        // fill toggle group
        ToggleGroup toggleGroup = recipeSelect.transform.GetChild(0).GetChild(1).GetComponent<ToggleGroup>();

        foreach (RessourceType ressource in building.GetRecipeOutputs()) {
            // create ressource toggle
            GameObject ressourceToggle = Instantiate(RessourceTogglePrefab, toggleGroup.transform);
            Transform transform = ressourceToggle.transform;
            Toggle toggle = transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = toggleGroup;

            // add sprites
            Transform child = transform.GetChild(0);
            Sprite sprite = ressourceImages[(int)ressource];
            child.GetChild(0).GetComponent<Image>().sprite = sprite;
            child.GetChild(1).GetComponent<Image>().sprite = sprite;

            // add onclick
            transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"Selecting output {ressource} for {building} at {building.Cell.coordinates}");
                ClientSend.RequestRecipeChange(building, ressource);
            });

            // keep updating selected toggle
            updateStructurePanelFunctions.Add(() => { toggle.isOn = building.CurrentOutput == ressource; });
        }
    }

    private void AddMarketInformation(Transform parent, Market market)
    {
        GameObject marketInformation = Instantiate(MarketInformationPrefab, parent);


        ToggleGroup inputToggleGroup = marketInformation.transform.GetChild(0).GetChild(0).GetComponent<ToggleGroup>();

        foreach (RessourceType ressourceType in Inventory.GetListOfAllRessources())
        {
            if (ressourceType == market.TradeOutput)
            {
                continue;
            }
            GameObject ressourceToggle = Instantiate(RessourceTogglePrefab, inputToggleGroup.transform);
            Toggle toggle = ressourceToggle.transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = inputToggleGroup;

            ressourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];
            ressourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];

            ressourceToggle.transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                Debug.Log("Change Input Recipe");
                ClientSend.RequestUpdateMarketRessource(market, ressourceType, true);
            });

            updateStructurePanelFunctions.Add(delegate { UpdateRessourceToggle(toggle, market, ressourceType, true); });
        }

        ToggleGroup outputToggleGroup = marketInformation.transform.GetChild(0).GetChild(1).GetComponent<ToggleGroup>();

        foreach (RessourceType ressourceType in Inventory.GetListOfAllRessources())
        {
            if (ressourceType == market.TradeInput)
            {
                continue;
            }
            GameObject ressourceToggle = Instantiate(RessourceTogglePrefab, outputToggleGroup.transform);
            Toggle toggle = ressourceToggle.transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = outputToggleGroup;

            ressourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];
            ressourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int)ressourceType];

            ressourceToggle.transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                Debug.Log("Change Output Recipe");
                ClientSend.RequestUpdateMarketRessource(market, ressourceType, false);
            });

            updateStructurePanelFunctions.Add(delegate { UpdateRessourceToggle(toggle, market, ressourceType, false); });
        }
    }

    private void AddLibraryResearchOptions(Transform parent)
    {
        Tribe tribe = Client.instance.Player.Tribe;
        if (tribe == null)
        {
            return;
        }

        for (int i=0;; ++i)
        {
            // If there is no research code 'i', Name will be null. Loop until that happens.
            // (Makes it easier to obtain names, descriptions, etc.)
            Research researchElement = new Research(i);
            if (researchElement.Name == null)
            {
                break;
            }
            
            // Start constructing the panel for the current Research object.
            
            GameObject researchPanel = Instantiate(LibraryResearchPanelPrefab, parent);
            
            // Child 0: Counter (de-activated, probably not needed)
            // Child 1: Cost
            // Child 2: Image
            // Child 3: Name
            // Child 4: Researched
            // Child 5: Info Card
            //  - Child 0: Title
            //  - Child 1: Info Text
            // Child 6: Info Button
            
            TMP_Text nameText = researchPanel.transform.GetChild(3).GetComponent<TMP_Text>();
            nameText.text = researchElement.Name;

            if (tribe.HasResearched(researchElement.Code))
            {
                // Show "Researched" instead of cost.
                researchPanel.transform.GetChild(1).gameObject.SetActive(false);
                researchPanel.transform.GetChild(4).gameObject.SetActive(true);
            }
            else
            {
                // Show cost and also take care of button click to "buy" the research.
                Transform costTransform = researchPanel.transform.GetChild(1);
                
                foreach (KeyValuePair<RessourceType, int> pair in researchElement.Costs)
                {
                    RessourceType type = pair.Key;
                    int cost = pair.Value;
                    AddRessourceCost(costTransform, type, cost, tribe.HQ.Inventory);
                }

                researchPanel.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ClientSend.RequestResearch(researchElement.Code);
                    ClosePanel();
                });
            }
            
            // Make sure the images in the array have the same order as the codes in Research
            if (ResearchImages.Length > researchElement.Code)
            {
                Sprite sprite = ResearchImages[researchElement.Code];
                researchPanel.transform.GetChild(2).GetComponent<Image>().sprite = sprite;
            }
            
            // Text that describes what the effect of the research is (same as in AddBuildingDisplay).
            GameObject infoPanel = researchPanel.transform.GetChild(5).gameObject;
            infoPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = researchElement.Name;
            infoPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = researchElement.Description;

            // Flip animation when info button is pressed.
            researchPanel.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(delegate { FlipCard(researchPanel, infoPanel); });
        }
    }

    private void AddGuildInformation(Transform parent, Guild guild)
    {
        GameObject infoPanel = Instantiate(GuildHouseInformationPrefab, parent);
        
        // Child 0: Menu Name (TMP)
        // Child 1: Info menu
        //  - Child 0: Info text (TMP)
        Transform infoTransform = infoPanel.transform.GetChild(1);

        TMP_Text infoText = infoTransform.GetChild(0).GetComponent<TMP_Text>();

        if (guild == null)
        {
            infoText.text = "Status: no guild";
        }
        else
        {
            bool isSameGuild = Client.instance.Player.Tribe.GuildId == guild.Id;
            
            string levelText = $"Level: {guild.Level}/3";
            string membersText = $"Members: {guild.Members.Count}/{guild.GetMaxMembers()}";
            string statusText = $"Status: {(isSameGuild ? "same guild" : "other guild")}";

            infoText.text = string.Join("\n", new List<string> {levelText, membersText, statusText});
        }
    }

    private void AddGuildLevelingPanel(Transform parent, Guild guild)
    {
        if (guild.Level >= 3)
        {
            // Max level, nothing to level up. Do not show the menu at all.
            return;
        }
        
        GameObject levelingPanel = Instantiate(GuildHouseLevelingPanelPrefab, parent);

        // Child 0: Menu Name (TMP)
        // Child 1: Leveling menu
        //  - Child 0: Res0
        //      - Child 0: Image 
        //      - Child 1: RemainderCount (TMP)
        //      - Child 2: DonationAmount (TMP)
        //      - Child 3: Button +
        //      - Child 4: Button -
        //  - Child 1: Res1
        //      ...
        //  ...
        //  - Child 5: Donate
        //      - Child 0: Button
        Transform levelingTransform = levelingPanel.transform.GetChild(1);

        Transform res0 = levelingTransform.GetChild(0);
        Transform res1 = levelingTransform.GetChild(1);
        Transform res2 = levelingTransform.GetChild(2);
        Transform res3 = levelingTransform.GetChild(3);
        Transform res4 = levelingTransform.GetChild(4);

        ProcessGuildLevelingResNode(res0, 0, guild);
        ProcessGuildLevelingResNode(res1, 1, guild);
        ProcessGuildLevelingResNode(res2, 2, guild);
        ProcessGuildLevelingResNode(res3, 3, guild);
        ProcessGuildLevelingResNode(res4, 4, guild);

        Button donateButton = levelingTransform.GetChild(5).GetChild(0).GetComponent<Button>();
        donateButton.onClick.AddListener(() =>
        {
            List<TMP_Text> donationAmountTexts = new List<TMP_Text>
            {
                res0.GetChild(2).GetComponent<TMP_Text>(),
                res1.GetChild(2).GetComponent<TMP_Text>(),
                res2.GetChild(2).GetComponent<TMP_Text>(),
                res3.GetChild(2).GetComponent<TMP_Text>(),
                res4.GetChild(2).GetComponent<TMP_Text>(),
            };

            SortedDictionary<RessourceType, int> donationMap = new SortedDictionary<RessourceType, int>();
            SortedDictionary<RessourceType, int> progressMap = guild.GetCurrentProgressMap();
            for (int i = 0; i < progressMap.Count; ++i)
            {
                bool isSuccess = int.TryParse(donationAmountTexts[i].text, out int donationAmount);
                if (isSuccess)
                {
                    donationMap.Add(progressMap.ElementAt(i).Key, donationAmount);
                }
            }

            bool isMoreThanZeroDonated = donationMap.Values.Any(donationAmount => donationAmount > 0);
            if (isMoreThanZeroDonated)
            {
                byte tribeId = Client.instance.Player.Tribe.Id;
                ClientSend.RequestGuildDonation(guild.Id, tribeId, donationMap);
                
                // Currently don't have a reliable way to update remainder counts in UI.
                // For now, just close the panel. Re-opening will show the correct values for remaining progress.
                ClosePanel();
            }
        });
    }
    
    private void ProcessGuildLevelingResNode(Transform resNode, int resNodeIndex, Guild guild)
    {
        SortedDictionary<RessourceType, int> progressMap = guild.GetCurrentProgressMap();
        if (resNodeIndex >= progressMap.Count)
        {
            // Level 2 -> Level 3 only has three resource type for leveling. De-activate unused ones.
            resNode.gameObject.SetActive(false);
            return;
        }

        if (progressMap.ElementAt(resNodeIndex).Value <= 0)
        {
            // Hide elements that have already reached their goal.
            resNode.gameObject.SetActive(false);
            return;
        }
        
        // Res N
        //  - Child 0: Image 
        //  - Child 1: RemainderCount (TMP)
        //  - Child 2: DonationAmount (TMP)
        //  - Child 3: Button +
        //  - Child 4: Button -
        Image resourceImage = resNode.GetChild(0).GetComponent<Image>();
        TMP_Text remainderCountText = resNode.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text donationAmountText = resNode.GetChild(2).GetComponent<TMP_Text>();
        Button incButton = resNode.GetChild(3).GetComponent<Button>();
        Button decButton = resNode.GetChild(4).GetComponent<Button>();

        RessourceType resourceType = progressMap.ElementAt(resNodeIndex).Key;
        resourceImage.sprite = ressourceImages[(int)resourceType];

        int remainderCount = progressMap.ElementAt(resNodeIndex).Value;
        remainderCountText.text = $"({remainderCount}): ";
        
        incButton.onClick.AddListener(() =>
        {
            bool isSuccess = int.TryParse(donationAmountText.text, out int donationAmount);
            if (isSuccess)
            {
                Tribe tribe = Client.instance.Player.Tribe;
                RessourceType type = progressMap.ElementAt(resNodeIndex).Key;
                
                // Prevent donations that are more than necessary/possible.
                remainderCount = progressMap.ElementAt(resNodeIndex).Value;
                donationAmount = Math.Min(remainderCount, donationAmount + 1);
                donationAmount = Math.Min(tribe.tribeInventory.GetRessourceAmount(type), donationAmount);
                donationAmountText.text = donationAmount.ToString();
            }
        });
        
        decButton.onClick.AddListener(() =>
        {
            bool isSuccess = int.TryParse(donationAmountText.text, out int donationAmount);
            if (isSuccess)
            {
                donationAmount = Math.Max(0, donationAmount - 1);
                donationAmountText.text = donationAmount.ToString();
            }
        });
    }

    
    private void AddGuildInventoryPanel(Transform parent, Guild guild)
    {
        if (guild.Level < 2)
        {
            return;
        }

        GameObject inventoryPanel = Instantiate(GuildHouseInventoryPanelPrefab, parent);
        
        // Child 0: Menu Name
        // Child 1: Inventory
        //  - Child 0: Res0
        //      - Child 0: Image
        //      - Child 1: Text (Stored)
        //      - Child 2: Text (Amount)
        //      - Child 3: Button (+)
        //      - Child 4: Button (-)
        //  - ...
        //  - Child 4: Res4
        //      - ...
        //  - Child 5: Deposit 
        //      - Child 0: Button
        //  - Child 6: Withdraw
        //      - Child 0: Button
        Transform inventoryTransform = inventoryPanel.transform.GetChild(1);
        
        Transform res0 = inventoryTransform.GetChild(0);
        Transform res1 = inventoryTransform.GetChild(1);
        Transform res2 = inventoryTransform.GetChild(2);
        Transform res3 = inventoryTransform.GetChild(3);
        Transform res4 = inventoryTransform.GetChild(4);

        ProcessGuildInventoryResNode(res0, 0, guild);
        ProcessGuildInventoryResNode(res1, 1, guild);
        ProcessGuildInventoryResNode(res2, 2, guild);
        ProcessGuildInventoryResNode(res3, 3, guild);
        ProcessGuildInventoryResNode(res4, 4, guild);

        // Function used by "Deposit" and "Withdraw" buttons to get the amounts entered into the UI.
        Dictionary<RessourceType, int> GetAmounts()
        {
            List<TMP_Text> amountTexts = new List<TMP_Text>
            {
                res0.GetChild(2).GetComponent<TMP_Text>(),
                res1.GetChild(2).GetComponent<TMP_Text>(),
                res2.GetChild(2).GetComponent<TMP_Text>(),
                res3.GetChild(2).GetComponent<TMP_Text>(),
                res4.GetChild(2).GetComponent<TMP_Text>(),
            };

            Dictionary<RessourceType, int> amountMap = new Dictionary<RessourceType, int>();
            for (int i = 0; i < guild.Inventory.Resources.Count; ++i)
            {
                bool isSuccess = int.TryParse(amountTexts[i].text, out int amount);
                if (isSuccess)
                {
                    RessourceType type = guild.Inventory.Resources.ElementAt(i).Key;
                    amountMap[type] = amount;
                }
            }

            return amountMap;
        }

        Button depositButton = inventoryTransform.GetChild(5).GetChild(0).GetComponent<Button>();
        depositButton.onClick.AddListener(() =>
        {
            Dictionary<RessourceType, int> amountMap = GetAmounts();
            bool isMoreThanZeroAmount = amountMap.Values.Any(amount => amount > 0);
            if (isMoreThanZeroAmount)
            {
                byte tribeId = Client.instance.Player.Tribe.Id;
                ClientSend.RequestDepositToGuild(guild.Id, tribeId, amountMap);
                ClosePanel();
            }
        });

        Button withdrawButton = inventoryTransform.GetChild(6).GetChild(0).GetComponent<Button>();
        withdrawButton.onClick.AddListener(() =>
        {
            Dictionary<RessourceType, int> amountMap = GetAmounts();
            bool isMoreThanZeroAmount = amountMap.Values.Any(amount => amount > 0);
            if (isMoreThanZeroAmount)
            {
                byte tribeId = Client.instance.Player.Tribe.Id;
                ClientSend.RequestWithdrawalFromGuild(guild.Id, tribeId, amountMap);
                ClosePanel();
            }
        });
    }

    private void ProcessGuildInventoryResNode(Transform resNode, int resNodeIndex, Guild guild)
    {
        GuildInventory guildInventory = guild.Inventory;
        
        // Res N
        //  - Child 0: Image 
        //  - Child 1: Stored (Text)
        //  - Child 2: Amount (Text)
        //  - Child 3: Button +
        //  - Child 4: Button -
        Image image = resNode.GetChild(0).GetComponent<Image>();
        TMP_Text storedText = resNode.GetChild(1).GetComponent<TMP_Text>(); // What is in the inventory
        TMP_Text amountText = resNode.GetChild(2).GetComponent<TMP_Text>(); // What you want to deposit/withdraw
        Button incButton = resNode.GetChild(3).GetComponent<Button>();
        Button decButton = resNode.GetChild(4).GetComponent<Button>();

        RessourceType resourceType = guildInventory.Resources.ElementAt(resNodeIndex).Key;
        image.sprite = ressourceImages[(int) resourceType];

        int stored = guildInventory.Resources.ElementAt(resNodeIndex).Value;
        storedText.text = $"({stored}): ";
        
        incButton.onClick.AddListener(() =>
        {
            // Amount: the number you adjust by pressing +/-.
            bool isSuccess = int.TryParse(amountText.text, out int amount);
            if (isSuccess)
            {
                Tribe tribe = Client.instance.Player.Tribe;
                
                RessourceType type = guildInventory.Resources.ElementAt(resNodeIndex).Key;
                    
                stored = guildInventory.Resources.ElementAt(resNodeIndex).Value;
                int tribeAmount = tribe.tribeInventory.GetRessourceAmount(type);

                // Player can't pass this value by pressing '+'. 
                // Just a rough way to prevent user from incrementing infinitely.
                int maxAmount = Math.Max(stored, tribeAmount);

                amount = Math.Min(maxAmount, amount + 1);
                amountText.text = amount.ToString();
            }
        });
        
        decButton.onClick.AddListener(() =>
        {
            bool isSuccess = int.TryParse(amountText.text, out int amount);
            if (isSuccess)
            {
                amount = Math.Max(0, amount - 1);
                amountText.text = amount.ToString();
            }
        });
    }
    
    private void AddGuildBoostPanel(Transform parent, GuildHouse guildHouse)
    {
        GameObject boostPanel = Instantiate(GuildHouseBoostPanelPrefab, parent);
        
        // Child 0: Menu Name (TMP)
        // Child 1: Boost menu
        //  - Child 0: Base resources
        //  - Child 1: Advanced resources
        //  - Child 2: Weapons
        Transform boostTransform = boostPanel.transform.GetChild(1);
        
        if (guildHouse.Level < 2)
        {
            boostTransform.GetChild(1).gameObject.SetActive(false);
            boostTransform.GetChild(2).gameObject.SetActive(false);
        }
        else if (guildHouse.Level < 3)
        {
            boostTransform.GetChild(2).gameObject.SetActive(false);
        }
        
        Debug.Log($"Selections are: {guildHouse.BoostBaseResource}, {guildHouse.BoostAdvancedResource}, {guildHouse.BoostWeapon}");

        // Boost base resources
        ToggleGroup baseResourceBoostToggleGroup = boostTransform.GetChild(0).GetComponent<ToggleGroup>();
        foreach (RessourceType resourceType in GuildHouse.BaseResourceTypes)
        {
            GameObject resourceToggle = Instantiate(RessourceTogglePrefab, baseResourceBoostToggleGroup.transform);
            Toggle toggle = resourceToggle.transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = baseResourceBoostToggleGroup;

            resourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                ClientSend.RequestBoostBaseResourceSelectionGuild(guildHouse, resourceType);
            });
            
            updateStructurePanelFunctions.Add(delegate { UpdateBoostBaseResourceToggle(toggle, guildHouse, resourceType); });
        }
        
        // Boost advanced resources
        ToggleGroup advancedResourceBoostToggleGroup = boostTransform.GetChild(1).GetComponent<ToggleGroup>();
        foreach (RessourceType resourceType in GuildHouse.AdvancedResourceTypes)
        {
            GameObject resourceToggle = Instantiate(RessourceTogglePrefab, advancedResourceBoostToggleGroup.transform);
            Toggle toggle = resourceToggle.transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = advancedResourceBoostToggleGroup;

            resourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                ClientSend.RequestBoostAdvancedResourceSelectionGuild(guildHouse, resourceType);
            });
            
            updateStructurePanelFunctions.Add(delegate { UpdateBoostAdvancedResourceToggle(toggle, guildHouse, resourceType); });
        }
        
        // Boost weapons
        ToggleGroup weaponsBoostToggleGroup = boostTransform.GetChild(2).GetComponent<ToggleGroup>();
        foreach (RessourceType resourceType in GuildHouse.WeaponTypes)
        {
            GameObject resourceToggle = Instantiate(RessourceTogglePrefab, weaponsBoostToggleGroup.transform);
            Toggle toggle = resourceToggle.transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = weaponsBoostToggleGroup;

            resourceToggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ressourceImages[(int) resourceType];
            resourceToggle.transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                ClientSend.RequestBoostWeaponSelectionGuild(guildHouse, resourceType);
            });
            
            updateStructurePanelFunctions.Add(delegate { UpdateBoostWeaponToggle(toggle, guildHouse, resourceType); });
        }
    }

    private void AddTroopProductionInformation(Transform parent, TroopProductionBuilding building)
    {
        GameObject information = Instantiate(RefineryInformationPrefab, parent);

        GameObject progressBar = information.transform.GetChild(0).gameObject;
        updateStructurePanelFunctions.Add(() => UpdateTimeBar(progressBar, building.MaxProgress - building.Progress, (float)building.Progress / (float)building.MaxProgress));

        Transform input = information.transform.GetChild(1).GetChild(0);
        foreach (KeyValuePair<RessourceType, int> kvp in building.InputRecipe)
        {
            AddRessourceElement(input, kvp.Key, kvp.Value, building.Inventory);
        }

        Transform output = information.transform.GetChild(1).GetChild(1);
        GameObject element = Instantiate(RessourceElementPrefab, output);
        element.transform.GetChild(0).GetComponent<Image>().sprite = troopImages[(int)building.OutputTroop];
        element.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "1";
    }

    private void AddBarracksInformation(Transform parent, Barracks barracks)
    {
        GameObject barracksInformation = Instantiate(BarracksInformationPrefab, parent);

        GameObject progressBar = barracksInformation.transform.GetChild(0).gameObject;
        updateStructurePanelFunctions.Add(new Action(delegate
        {
            UpdateBar(progressBar, barracks.Progress, barracks.MaxProgress);
        }));

        // TODO: unify ui with recipe select of multi refinery building?

        Transform inputToggleGroup = barracksInformation.transform.GetChild(1).GetChild(0);
        foreach (KeyValuePair<RessourceType, int> kvp in barracks.InputRecipe)
        {
            AddRessourceCost(inputToggleGroup.transform, kvp.Key, kvp.Value, barracks.Inventory);
        }

        ToggleGroup toggleGroup = barracksInformation.transform.GetChild(1).GetChild(1).GetComponent<ToggleGroup>();
        Transform outputToggleGroup = barracksInformation.transform.GetChild(1).GetChild(1);
        foreach (TroopType troop in barracks.GetAvailableTroopRecipes())
        {
            // create ressource toggle
            GameObject ressourceToggle = Instantiate(RessourceTogglePrefab, toggleGroup.transform);
            Transform transform = ressourceToggle.transform;
            Toggle toggle = transform.GetChild(0).GetComponent<Toggle>();
            toggle.group = toggleGroup;

            // add sprites
            Transform child = transform.GetChild(0);
            Sprite sprite = troopImages[(int)troop];
            child.GetChild(0).GetComponent<Image>().sprite = sprite;
            child.GetChild(1).GetComponent<Image>().sprite = sprite;

            // add onclick
            transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"Selecting troop {troop} for Barracks at {barracks.Cell.coordinates}");
                ClientSend.RequestChangeTroopRecipeOfBarracks(barracks, troop);
            });

            // keep updating selected toggle
            updateStructurePanelFunctions.Add(() => { toggle.isOn = barracks.OutputTroop == troop; });
        }


    }
    private void addPriorityPanel(Transform parent, AssemblyPoint building)
    {
        GameObject prioPanel = Instantiate(PrioPrefab, parent);
        prioPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = building.priority.ToString();
        Transform plusbutton = prioPanel.transform.GetChild(2);
        Transform minusbutton = prioPanel.transform.GetChild(3);
        plusbutton.GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestChangePriority(building.priority, true); });
        minusbutton.GetComponent<Button>().onClick.AddListener(delegate { ClientSend.RequestChangePriority(building.priority, false); });
        updateStructurePanelFunctions.Add(delegate { this.updatePriority(prioPanel, building); });

    }
    private void updatePriority(GameObject panel, AssemblyPoint ap)
    {
        panel.transform.GetChild(1).GetComponent<TMP_Text>().text = ap.priority.ToString();
    }

    #endregion

    #region UIElementUpdaters

    private void ResetFunctions()
    {
        updateStructurePanelFunctions.Clear();
        updateMapFunction = null;
    }

    public void UpdateUIElements() {
        if (this.MenuPanel.activeSelf == true)
        {
            this.UpdateStructurePanel();
        }
    }

    public void UpdateMap()
    {
        if (updateMapFunction != null)
            updateMapFunction();
    }

    private void UpdateStructurePanel()
    {
        foreach (Action func in updateStructurePanelFunctions)
            func();
    }

    private void UpdateSelectableRessourceElement(GameObject sre, InventoryBuilding originBuilding, InventoryBuilding destinationBuilding, RessourceType ressourceType)
    {
        sre.SetActive(!originBuilding.AllowedRessources[destinationBuilding][ressourceType]);
    }

    private void UpdateRessourceToggle(Toggle toggle, InventoryBuilding originBuilding, InventoryBuilding destinationBuilding, RessourceType ressourceType)
    {
        toggle.isOn = originBuilding.AllowedRessources[destinationBuilding][ressourceType];
    }

    private void UpdateRessourceToggle(Toggle toggle, Market market, RessourceType type, bool isInput)
    {
        if (isInput)
            toggle.isOn = market.InputRecipe.ContainsKey(type);
        else
            toggle.isOn = market.OutputRecipe.ContainsKey(type);
    }
    
    private void UpdateBoostBaseResourceToggle(Toggle toggle, GuildHouse guildHouse, RessourceType type)
    {
        toggle.isOn = (guildHouse.BoostBaseResource == type);
    }
    
    private void UpdateBoostAdvancedResourceToggle(Toggle toggle, GuildHouse guildHouse, RessourceType type)
    {
        Debug.Log("Change at advanced: " + type);
        toggle.isOn = (guildHouse.BoostAdvancedResource == type);
    }
    
    private void UpdateBoostWeaponToggle(Toggle toggle, GuildHouse guildHouse, RessourceType type)
    {
        Debug.Log("Change at weapons: " + type);
        toggle.isOn = (guildHouse.BoostWeapon == type);
    }

    private void UpdateBar(GameObject bar, int currentAmount, int maxAmount)
    {
        bar.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(((float)currentAmount) / ((float)maxAmount), 0);
        bar.transform.GetChild(1).GetComponent<TMP_Text>().text = currentAmount.ToString() + "/" + maxAmount.ToString();
    }

    private void UpdateBar(GameObject bar, float fillAmount)
    {
        bar.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(fillAmount, 0);
        bar.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(fillAmount, 0);
    }

    private void UpdateTimeBar(GameObject bar, int ticks, float fillAmount)
    {
        bar.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(fillAmount, 1);
        bar.transform.GetChild(1).GetComponent<TMP_Text>().text = TimeSpan.FromSeconds(ticks * 5).ToString();
    }

    private void UpdateRessourceLimitSlider(Slider slider, BuildingInventory inventory, RessourceType type)
    {
        slider.minValue = 0;
        slider.maxValue = inventory.GetLimit(type);
    }

    private void UpdateRessourceSlider(Slider slider, BuildingInventory originInventory, BuildingInventory destinationInventory, RessourceType type)
    {
        slider.minValue = 0;
        int maxValue = originInventory.GetRessourceAmount(type);
        // if (originInventory.RessourceLimits.ContainsKey(type))
        //     maxValue = originInventory.RessourceLimits[type];

        maxValue = Mathf.Min(maxValue, destinationInventory.AvailableSpace(type));
        slider.maxValue = maxValue;
    }

    private void UpdateTroopSlider(Slider slider, TroopInventory troopInventory, TroopType type)
    {
        slider.minValue = 0;
        slider.maxValue = troopInventory.Troops[type];
        slider.value = slider.maxValue;
    }

    private void UpdateRessourceElement(GameObject ressourceElement, RessourceType ressourceType, int cost, BuildingInventory buildingInventory)
    {
        if (buildingInventory.GetRessourceAmount(ressourceType) >= cost)
            ressourceElement.transform.GetComponentInChildren<TMP_Text>().color = new Color32(255, 255, 255, 255);
        else
            ressourceElement.transform.GetComponentInChildren<TMP_Text>().color = new Color32(255, 64, 64, 255);
    }

    public void FlipTroopbar(GameObject front, GameObject back, Transform parent)
    {
        if (!back.activeSelf)
            LeanTween.size(parent.transform.GetComponent<RectTransform>(), back.transform.GetComponent<RectTransform>().sizeDelta, 0.2f);
        else
            LeanTween.size(parent.transform.GetComponent<RectTransform>(), front.transform.GetComponent<RectTransform>().sizeDelta, 0.2f);

        LeanTween.scaleY(front, 0, 0.2f).setEase(LeanTweenType.easeInQuad).setOnComplete(
            new Action(delegate {
                back.SetActive(!back.activeSelf);
                LeanTween.scaleY(front, 1, 0.2f).setEase(LeanTweenType.easeOutQuad);
            })
        );
    }

    public void FlipCard(GameObject card, GameObject otherCard)
    {
        LeanTween.scaleY(card, 0, 0.2f).setEase(LeanTweenType.easeInQuad).setOnComplete(
            new Action(delegate {
                otherCard.SetActive(!otherCard.activeSelf);
                LeanTween.scaleY(card, 1, 0.2f).setEase(LeanTweenType.easeOutQuad);
            })
        );
    }

    public void ResetCellHighlighting()
    {
        if (highlightedCells != null)
            hexMeshGrid.ResetCellHighlighting(highlightedCells);
        highlightedCells = null;
    }

    public void RefreshCellHighlighting() {
        ResetCellHighlighting();

        if (placeBuildingType == null)
            return;

        Building building = (Building)Activator.CreateInstance(this.placeBuildingType);
        List<HexCell> cells = GameLogic.grid.GetCell(Client.instance.Player.Position).GetNeighbors(1);
        if (building is Headquarter)
            building.Tribe = 255;
        else
            building.Tribe = Client.instance.Player.Tribe.Id; ;

        cells = cells.FindAll(elem => building.IsPlaceableAt(elem));
        this.highlightedCells = cells;
        hexMeshGrid.HighlightCells(cells);
    }
    public void setChatHistory(String history)
    {
        ChatHistory.GetComponent<TMP_Text>().text = history;
    }
    public void newChatMessage(String message, string playerName, int chatType)
    {
        // Would be nice to have chat types in different colors (TMP_Text::color),
        // but not possible with the current approach (treat each message as separate TMP_Text?)
        TMP_Text tmpText = ChatHistory.GetComponent<TMP_Text>();
        tmpText.text += "\n[" + chatTypeCodes[chatType] + "] " + playerName + ": " + message;
    }
    public void sendChatMessage()
    {
        ClientSend.sendChatMessage(ChatInput.text, selectedChatType);
        ChatInput.text = "";
    }

    public void OnChatFilterChanged(int val)
    {
        if (val >= 0 && val <= 1)
        {
            // 0: tribe
            // 1: guild
            selectedChatType = val;
        }
    }

    /// <summary>
    /// Removes the "Guild" filter from the chat dropdown menu. The filter is not needed for players w/o a guild.
    /// </summary>
    public void RemoveChatFilterGuild()
    {
        TMP_Dropdown tmpDropdown = ChatFilterDropdown.GetComponent<TMP_Dropdown>();
        if (tmpDropdown != null)
        {
            TMP_Dropdown.OptionData optionData = tmpDropdown.options.Find(o => o.text.Equals("Guild"));
            bool isRemoved = tmpDropdown.options.Remove(optionData);
            if (isRemoved)
            {
                selectedChatType = 0;
                tmpDropdown.value = 0;
            }
        }
    }

    /// <summary>
    /// Adds the "Guild" filter from the chat dropdown menu. E.g. needed when a tribe joins a guild.
    /// </summary>
    public void AddChatFilterGuild()
    {
        TMP_Dropdown tmpDropdown = ChatFilterDropdown.GetComponent<TMP_Dropdown>();
        
        if (tmpDropdown != null)
        {
            bool containsGuildFilter = tmpDropdown.options.Find(o => o.text.Equals("Guild")) != null;
            if (containsGuildFilter)
            {
                // Make sure "Guild" is not added more than once.
                return;
            }
            
            tmpDropdown.AddOptions(new List<string> {"Guild"});
            tmpDropdown.value = selectedChatType;
        }
    }
    
    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists ,destroying obj!");
            Destroy(this);
        }


    }

    //public Sprite[] Bakeries;
    //public Sprite[] Butchers;
    //public Sprite[] CoalMines;
    //public Sprite[] CowFarms;
    //public Sprite[] Fisher;
    //public Sprite[] Headquarters;
    //public Sprite[] Quarries;
    //public Sprite[] Smelter;
    //public Sprite[] Storages;
    //public Sprite[] Tanners;
    //public Sprite[] WheatFarm;
    //public Sprite[] Woodcutters;
    //public Sprite[] Markets;
    //public Sprite[] LandRoads;
    //public Sprite[] Bridges;
    //public Sprite[] Barracks;

    public void Start()
    {
        ClosePanel();

        // As with PrefabManager::BuildingPrefabs, each entry in Sprite[] corresponds to the building's level.
        // If your HQ is level 2, then the level 2 sprite will show in the HQ's menu.
        buildingImages = new Dictionary<Type, Sprite[]>
        {
            { typeof(Bakery), Bakeries },
            { typeof(Butcher), Butchers },
            { typeof(CoalMine), CoalMines },
            { typeof(CowFarm), CowFarms },
            { typeof(Fisher), Fishers },
            { typeof(Headquarter), Headquarters },
            { typeof(Quarry), Quarries },
            { typeof(Smelter), Smelters },
            { typeof(Storage), Storages },
            { typeof(Tanner), Tanners },
            { typeof(WheatFarm), WheatFarm },
            { typeof(Woodcutter), Woodcutters },
            { typeof(Market), Markets },
            { typeof(LandRoad), LandRoads },
            { typeof(Bridge), Bridges },
            { typeof(Barracks), Barracks },
            { typeof(Workshop), Workshops },
            { typeof(Tower), Towers },
            { typeof(WeaponSmith), Smiths },
            { typeof(ArmorSmith), Smiths },
            { typeof(IronMine), CoalMines },
            { typeof(AssemblyPoint), AssemblyPoints },
            { typeof(Ruin), Bridges },
            { typeof(Library), Libraries },
            { typeof(GuildHouse), GuildHouses },
        };
    }

    public void StructureInfo()
    {
        hexMeshGrid.aboveInformation(Client.instance.Player.Position);
    }
    static void Quit()
    {
        Debug.Log("Quitting the Player");
    }

    public void Update()
    {
        if (this.selectedCell == null || !GameLogic.PlayerInRange(this.selectedCell.coordinates, Client.instance.Player))
        {
            this.selectedCell = null;
        }
        time = GameLogic.ctime;

        if (time == "Day")
        {
            GameObject.Find("Directional Light").GetComponent<Light>().intensity = 1;
        }
        else if (time == "Dawn")
        {
            GameObject.Find("Directional Light").GetComponent<Light>().intensity = (float)0.75;
        }
        else if (time == "Dusk")
        {
            GameObject.Find("Directional Light").GetComponent<Light>().intensity = (float)0.5;
        }
        else if (time == "Night")
        { 
            GameObject.Find("Directional Light").GetComponent<Light>().intensity = (float)0.25;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape pressed");

            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
            Client.instance.Disconnect();
            LoadManager.instance.ReturnToTitleScreen();
        }
        else if (Input.GetKeyDown("space") || Input.GetKeyDown("m"))
        {
            HexMapCamera camera = FindObjectOfType<HexMapCamera>();
            camera.HandleMove();
            ClosePanel();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            hexMeshGrid.aboveInformation(Client.instance.Player.Position);
        }

        //Move with Touch
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;
        moveTimer+=Time.deltaTime;
        if(horizontal !=0 && vertical !=0 && moveTimer > 1)
        {
            moveTimer=0;
            HexCoordinates co = returnCoordinate(vertical, horizontal).Value;
            if(co != null)  
            ClientSend.UpdatePosition(co);
        }
        //Debug.Log("h: " + horizontal + " v: " + vertical);

    }

    Nullable<HexCoordinates> returnCoordinate(float ver,float hor)
    {
        HexDirection dir;
        if (ver >= 0)
        {
            if (hor < -0.7f && hor >-1) dir = HexDirection.E;
            else if(hor>-0.7f && hor<0f) dir = HexDirection.NW;
            else if(hor>0f && hor<0.7f) dir = HexDirection.NE;
            else dir = HexDirection.W;
        }else 
        {
            if (hor < -0.7f && hor > -1) dir = HexDirection.E;
            else if (hor > -0.7f && hor < 0f) dir = HexDirection.SW;
            else if (hor > 0f && hor < 0.7f) dir = HexDirection.SE;
            else dir = HexDirection.W;
        }
        HexCell cell = GameLogic.grid.GetCell(Client.instance.Player.Position.InDirection(dir));
        if (cell != null)
        {
            camera.focusedCell = cell;
            return cell.coordinates;
        }else return null;
       
    }


    //HexCell cell = GameLogic.grid.GetCell(Client.instance.Player.Position);
    //List<Structure> cells = cell.GetNeighborStructures<Structure>(1);
    //foreach(Structure str in cells)
    //{
    //    Debug.Log(str);
    //}



    #endregion

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (camera.panning)
            return;
        Debug.Log("OnPointerClick");
        if (!GameLogic.initialized)
        {
            return;
        }

        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit, 1000f, ~mask))
        {
            HexCell cell = GameLogic.grid.GetCell(hit.point);
            this.selectedCell = cell;
            if (GameLogic.PlayerInRange(cell.coordinates, Client.instance.Player))
            {
                if (this.placeBuildingType != null)
                {
                    ClientSend.RequestPlaceBuilding(this.selectedCell.coordinates, this.placeBuildingType);
                }
                else if (cell.Structure is Structure)
                {
                    OpenStructurePanel(cell.Structure);
                }
            }
            this.placeBuildingType = null;
            this.ResetCellHighlighting();
        }
    }

    #region CALLBACKS

    public void HarvestCallback(Ressource ressource)
    {
        ClientSend.RequestHarvest(ressource.Cell.coordinates);

        ClosePanel();
    }

    public void CollectCallback(Collectable collectable) 
    {
        ClientSend.RequestCollect(collectable.Cell.coordinates);

        ClosePanel();
    }

    public void OpenBuildingMenuCallback()
    {
        OpenBuildMenu();
        this.placeBuildingType = null;
        this.ResetCellHighlighting();
    }

    public void OpenSharedInventoryCallback()
    {
        OpenSharedInventory();
    }

    public void UpgradeCallback()
    {
        ClientSend.UpgradeBuilding(this.selectedCell.coordinates);
        ClosePanel();
    }

    public void RepairCallback()
    {
        ClientSend.RepairBuilding(this.selectedCell.coordinates);
        ClosePanel();
    }
    public void SalvageCallback()
    {
        ClientSend.SalvageBuilding(this.selectedCell.coordinates);
        ClosePanel();
    }

    public void JoinTribeCallback()
    {
        ClientSend.RequestJoinTribe(this.selectedCell.coordinates);
        ClosePanel();
    }

    public void ConnectToServerCallback()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas canvas = transform.root.GetComponent<Canvas>();
        Camera.main.transform.position -= (1 / canvas.scaleFactor) * new Vector3(eventData.delta.x, 0, eventData.delta.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        camera.panning = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        camera.panning = false;
    }
    #endregion

    void OnGUI()
    {
        if (!Debug.isDebugBuild)
        {
            // Only for debugging purposes to have resources available whenever necessary.
            return;
        }
        
        if (GUI.Button(new Rect(10, 10, 100, 50), "Resource"))
        {
            ClientSend.RequestHarvest(new HexCoordinates(0, 0));

            foreach (RessourceType resource in Enum.GetValues(typeof(RessourceType)))
            {
                Tribe t = Client.instance.Player.Tribe;
                if (t != null)
                {
                    t.tribeInventory.AddRessource(resource, 200);
                }
            }
        }
    }
}
