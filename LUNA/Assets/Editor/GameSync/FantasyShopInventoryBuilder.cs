#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FantasyUI;

public class FantasyShopInventoryBuilder : EditorWindow {
 Font font; Sprite background; bool autoSprites=true; int capacity=30;
 [MenuItem("Tools/Fantasy UI/Shop & Inventory Builder PRO")]
 static void Open(){GetWindow<FantasyShopInventoryBuilder>("Shop Inventory PRO");}
 void OnGUI(){GUILayout.Label("FANTASY SHOP + INVENTORY PRO",EditorStyles.boldLabel);
  EditorGUILayout.HelpBox("Copy Art into Assets first, set PNGs to Sprite (2D and UI), then generate. Auto-detect uses the 69 separated UI images.",MessageType.Info);
  autoSprites=EditorGUILayout.Toggle("Auto-find UI Sprites",autoSprites);
  background=(Sprite)EditorGUILayout.ObjectField("Optional Background",background,typeof(Sprite),false);
  font=(Font)EditorGUILayout.ObjectField("Optional Font",font,typeof(Font),false);
  capacity=EditorGUILayout.IntSlider("Inventory Slots",capacity,10,100);
  if(GUILayout.Button("GENERATE SHOP + INVENTORY",GUILayout.Height(42)))Build();
  if(GUILayout.Button("Select Inventory Manager")){var m=FindObjectOfType<FantasyInventory>();if(m!=null)Selection.activeGameObject=m.gameObject;}
 }
 Sprite S(string folder,string name){if(!autoSprites)return null;string[] guids=AssetDatabase.FindAssets(name+" t:Sprite");
  foreach(var guid in guids){string p=AssetDatabase.GUIDToAssetPath(guid).Replace('\\','/');if(p.EndsWith("/"+folder+"/"+name+".png",StringComparison.OrdinalIgnoreCase))return AssetDatabase.LoadAssetAtPath<Sprite>(p);}return null;}
 GameObject N(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));if(parent!=null)go.transform.SetParent(parent,false);return go;}
 RectTransform R(GameObject go,float x,float y,float w,float h){var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
 RectTransform Stretch(GameObject go){var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 Image I(GameObject go,Sprite sprite,Color? fallback=null){var img=go.AddComponent<Image>();img.sprite=sprite;img.color=sprite!=null?Color.white:(fallback??new Color(.15f,.1f,.07f,.95f));img.type=sprite!=null&&sprite.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;return img;}
 Text T(GameObject go,string value,int size,TextAnchor anchor=TextAnchor.MiddleCenter){var t=go.AddComponent<Text>();t.font=font!=null?font:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.alignment=anchor;t.color=new Color(1,.91f,.73f);t.raycastTarget=false;return t;}
 GameObject Pic(Transform p,string name,Sprite s,float x,float y,float w,float h){var g=N(name,p);R(g,x,y,w,h);I(g,s,Color.clear);return g;}
 Button B(Transform p,string name,string caption,Sprite sprite,float x,float y,float w,float h,UnityEngine.Events.UnityAction action){var g=N(name,p);R(g,x,y,w,h);I(g,sprite,new Color(.34f,.19f,.1f));var b=g.AddComponent<Button>();b.targetGraphic=g.GetComponent<Image>();if(action!=null)b.onClick.AddListener(action);
  if(sprite==null||string.IsNullOrEmpty(caption)==false&&name.StartsWith("Tab")){var label=N("Label",g.transform);Stretch(label);T(label,caption,18);}
  return b;
 }
 void States(Button b,Sprite normal,Sprite hover,Sprite disabled){if(b==null)return;var img=b.GetComponent<Image>();if(normal!=null){img.sprite=normal;img.color=Color.white;}if(hover!=null||disabled!=null){b.transition=Selectable.Transition.SpriteSwap;var st=new SpriteState();st.highlightedSprite=hover!=null?hover:normal;st.pressedSprite=hover!=null?hover:normal;st.selectedSprite=hover!=null?hover:normal;st.disabledSprite=disabled!=null?disabled:normal;b.spriteState=st;}}
 Transform Grid(Transform p,string name,float x,float y,float w,float h){var scroll=N(name,p);R(scroll,x,y,w,h);var sr=scroll.AddComponent<ScrollRect>();sr.horizontal=false;sr.vertical=true;
  var viewport=N("Viewport",scroll.transform);Stretch(viewport);viewport.AddComponent<RectMask2D>();
  var content=N("Content",viewport.transform);var rect=content.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(0,0);
  var grid=content.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(108,108);grid.spacing=new Vector2(10,10);grid.padding=new RectOffset(10,10,10,10);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;
  var fit=content.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
  sr.content=rect;sr.viewport=viewport.GetComponent<RectTransform>();return content.transform;
 }
 void Build(){if(FindObjectOfType<EventSystem>()==null){var es=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));Undo.RegisterCreatedObjectUndo(es,"Create EventSystem");}
  var manager=FindObjectOfType<FantasyInventory>();if(manager==null){var m=new GameObject("Fantasy Inventory Manager");Undo.RegisterCreatedObjectUndo(m,"Inventory Manager");manager=m.AddComponent<FantasyInventory>();manager.capacity=capacity;}
  var root=N("Fantasy Shop Inventory Canvas",null);Undo.RegisterCreatedObjectUndo(root,"Fantasy Shop Inventory UI");var canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);root.AddComponent<GraphicRaycaster>();
  var back=N("Background",root.transform);Stretch(back);I(back,background,new Color(.05f,.04f,.04f,.86f));
  var ui=root.AddComponent<FantasyShopInventoryUI>();ui.inventory=manager;ui.pageSize=20;
  var main=N("Main Frame",root.transform);R(main,-160,10,1310,870);I(main,S("Panels","Main_Dark"));
  var detail=N("Details Frame",root.transform);R(detail,625,10,410,870);I(detail,S("Panels","Details_Parchment"));
  var shop=N("Shop Panel",main.transform);Stretch(shop);var bag=N("Inventory Panel",main.transform);Stretch(bag);ui.shopPanel=shop;ui.inventoryPanel=bag;
  Pic(shop.transform,"Shop Title",S("Titles","Shop"),0,345,370,130);
  Pic(bag.transform,"Inventory Title",S("Titles","Inventory"),0,345,370,130);
  ui.shopContent=Grid(shop.transform,"Shop Items",-80,-12,760,540);
  ui.inventoryContent=Grid(bag.transform,"Inventory Items",-80,-12,760,540);
  // The category buttons use the 7 original image sprites.
  string[] categories={"All","Weapon","Armor","Accessory","Consumable","Material","Quest"};
  for(int i=0;i<categories.Length;i++){int captured=i;var s=S("Categories",categories[i]);var b=B(root.transform,"Category "+categories[i],categories[i],s,797,295-i*80,235,68,()=>ui.SetCategory(captured));if(s!=null)b.GetComponent<Image>().preserveAspect=true;}
  var goldBar=Pic(detail.transform,"Gold Bar",S("Currency","Gold_Bar"),-8,305,315,70);
  var goldText=N("Gold Value",goldBar.transform);R(goldText,35,0,165,46);ui.goldText=T(goldText,"200",24);
  var gemBar=Pic(detail.transform,"Gem Bar",S("Currency","Gem_Bar"),-8,225,315,70);
  var gemText=N("Gem Value",gemBar.transform);R(gemText,35,0,165,46);ui.gemText=T(gemText,"0",24);
  var info=N("Item Description",detail.transform);R(info,0,-35,310,300);ui.detailsText=T(info,"Select an item",19,TextAnchor.UpperLeft);ui.detailsText.color=new Color(.27f,.16f,.1f);
  var icon=N("Selected Item Icon",detail.transform);R(icon,0,115,84,84);ui.detailsIcon=I(icon,null,Color.clear);
  var feedback=N("Status",detail.transform);R(feedback,0,-218,320,48);ui.feedbackText=T(feedback,"",17);ui.feedbackText.color=new Color(.3f,.17f,.1f);
  ui.buyButton=Action(root.transform,"Buy",-630,-450,ui.BuySelected);
  ui.sellButton=Action(root.transform,"Sell",-400,-450,ui.SellSelected);
  ui.useButton=Action(root.transform,"Use",-170,-450,ui.UseSelected);
  ui.equipButton=Action(root.transform,"Equip",60,-450,ui.EquipSelected);
  ui.unequipButton=Action(root.transform,"Unequip",290,-450,ui.UnequipSelected);
  var tabs=N("Tabs",main.transform);R(tabs,-90,-350,460,75);
  B(tabs.transform,"Tab Shop","SHOP",null,-120,0,200,60,ui.ShowShop);
  B(tabs.transform,"Tab Inventory","INVENTORY",null,120,0,200,60,ui.ShowInventory);
  B(root.transform,"Close","X",S("Controls","Close"),882,466,95,95,ui.Close);
  B(root.transform,"Previous Page","<",S("Controls","Previous_Page"),-420,-358,58,58,ui.PreviousPage);
  B(root.transform,"Next Page",">",S("Controls","Next_Page"),-75,-358,58,58,ui.NextPage);
  var pg=N("Page Indicator",root.transform);R(pg,-246,-358,200,60);I(pg,S("Controls","Page_Indicator"),Color.clear);
  var pgText=N("Page Text",pg.transform);Stretch(pgText);ui.pageText=T(pgText,"1 / 1",20);
  var search=N("Search",root.transform);R(search,-250,380,310,58);I(search,S("Controls","Search"),new Color(.22f,.16f,.11f));
  var input=search.AddComponent<InputField>();var placeholder=N("Placeholder",search.transform);Stretch(placeholder);var ph=T(placeholder,"Search items...",17);ph.color=new Color(.7f,.7f,.7f);input.placeholder=ph;
  var typed=N("Input Text",search.transform);Stretch(typed);var typedText=T(typed,"",18);input.textComponent=typedText;input.onValueChanged.AddListener(ui.OnSearchChanged);ui.searchField=input;
  var template=N("Item Slot Template",root.transform);R(template,0,0,108,108);I(template,S("Slots","Common"));var templateButton=template.AddComponent<Button>();
  var itemIcon=N("Icon",template.transform);R(itemIcon,0,10,65,65);I(itemIcon,null,Color.clear).raycastTarget=false;
  var count=N("Count",template.transform);R(count,0,-37,100,26);T(count,"",15);
  ui.slotPrefab=templateButton;template.SetActive(false);
  ui.emptySlot=S("Slots","Empty");string[] rarities={"Common","Uncommon","Rare","Epic","Legendary"};for(int i=0;i<5;i++)ui.raritySlots[i]=S("Slots",rarities[i]);
  shop.SetActive(false);bag.SetActive(true);
  Selection.activeGameObject=root;EditorGUIUtility.PingObject(root);
  Debug.Log("Shop & Inventory PRO generated. Add Item assets to Fantasy Inventory Manager > Catalog. UI images loaded: "+(S("Panels","Main_Dark")!=null));
 }
 Button Action(Transform p,string key,float x,float y,UnityEngine.Events.UnityAction action){var normal=S("Buttons",key+"_Normal");var b=B(p,key,key,normal,x,y,215,65,action);States(b,normal,S("Buttons",key+"_Hover"),S("Buttons",key+"_Disabled"));return b;}
}
#endif
