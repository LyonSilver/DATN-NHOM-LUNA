using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FantasyUI {
 public class FantasySlotDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDropHandler {
  public FantasyShopInventoryUI ui; public int index;
  static int source=-1;
  public void OnBeginDrag(PointerEventData e){source=index;}
  public void OnDrop(PointerEventData e){if(source>=0&&ui!=null)ui.MoveSlot(source,index);source=-1;}
  public void OnEndDrag(PointerEventData e){source=-1;}
 }
 public class FantasyShopInventoryUI : MonoBehaviour {
  public FantasyInventory inventory;
  public GameObject shopPanel,inventoryPanel;
  public Transform shopContent,inventoryContent;
  public Text goldText,gemText,detailsText,pageText,feedbackText;
  public Image detailsIcon;
  public InputField searchField;
  public Button slotPrefab;
  public Sprite[] raritySlots=new Sprite[5];
  public Sprite emptySlot;
  public Sprite[] categorySprites=new Sprite[7];
  public Sprite buyNormal,buyHover,buyDisabled,sellNormal,sellHover,sellDisabled,useNormal,useHover,useDisabled,equipNormal,equipHover,equipDisabled,unequipNormal,unequipHover,unequipDisabled;
  public Sprite goldBar,gemBar,searchBar;
  public Button buyButton,sellButton,useButton,equipButton,unequipButton;
  public int pageSize=20;
  public ItemKind? filter=null;
  public string selectedId="";
  public int selectedSlot=-1;
  public bool showingShop=false;
  int page=0;
  readonly List<GameObject> spawned=new List<GameObject>();
  void OnEnable(){if(inventory==null)inventory=FantasyInventory.Instance;if(inventory!=null)inventory.Changed+=Refresh;Refresh();}
  void OnDisable(){if(inventory!=null)inventory.Changed-=Refresh;Clear();}
  public void Open(){gameObject.SetActive(true);ShowInventory();}
  public void Close(){gameObject.SetActive(false);}
  public void ShowShop(){showingShop=true;page=0;Refresh();}
  public void ShowInventory(){showingShop=false;page=0;Refresh();}
  public void SetCategory(int index){filter=index==0?(ItemKind?)null:index==1?ItemKind.Weapon:index==2?ItemKind.Armor:index==3?ItemKind.Accessory:index==4?ItemKind.Consumable:index==5?ItemKind.Material:ItemKind.Quest;page=0;Refresh();}
  public void OnSearchChanged(string unused){page=0;Refresh();}
  public void NextPage(){page++;Refresh();}
  public void PreviousPage(){page=Mathf.Max(0,page-1);Refresh();}
  void Clear(){foreach(var go in spawned)if(go!=null)Destroy(go);spawned.Clear();}
  bool Matches(FantasyItem item){if(item==null)return false;if(filter.HasValue&&item.kind!=filter.Value)return false;
   string q=searchField!=null?searchField.text.Trim():"";return q.Length==0||item.displayName.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0||item.itemId.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0;
  }
  public void Refresh(){if(inventory==null)return;
   if(shopPanel!=null)shopPanel.SetActive(showingShop);if(inventoryPanel!=null)inventoryPanel.SetActive(!showingShop);
   if(goldText!=null)goldText.text=inventory.data.gold.ToString("N0");if(gemText!=null)gemText.text=inventory.data.gems.ToString("N0");
   Clear();var entries=new List<KeyValuePair<int,FantasyItem>>();
   if(showingShop){for(int i=0;i<inventory.catalog.Count;i++){var item=inventory.catalog[i];if(Matches(item)&&item.canBuy)entries.Add(new KeyValuePair<int,FantasyItem>(-1,item));}}
   else{for(int i=0;i<inventory.capacity;i++){var stack=inventory.Slot(i);var item=stack!=null&&stack.count>0?inventory.Find(stack.id):null;
     if(filter.HasValue||searchField!=null&&!string.IsNullOrWhiteSpace(searchField.text)){if(!Matches(item))continue;}
     entries.Add(new KeyValuePair<int,FantasyItem>(i,item));}}
   int size=Mathf.Max(1,pageSize),pages=Mathf.Max(1,Mathf.CeilToInt(entries.Count/(float)size));page=Mathf.Clamp(page,0,pages-1);
   if(pageText!=null)pageText.text=(page+1)+" / "+pages;
   var parent=showingShop?shopContent:inventoryContent;
   if(parent!=null)for(int i=page*size;i<Mathf.Min(entries.Count,(page+1)*size);i++){
    var entry=entries[i];int slotIndex=entry.Key;var item=entry.Value;
    int count=slotIndex>=0&&inventory.Slot(slotIndex)!=null?inventory.Slot(slotIndex).count:0;
    CreateSlot(parent,item,slotIndex,count);
   }
   Details(inventory.Find(selectedId));UpdateActions();
  }
  void CreateSlot(Transform parent,FantasyItem item,int slotIndex,int count){if(slotPrefab==null)return;
   var b=Instantiate(slotPrefab,parent);b.gameObject.SetActive(true);spawned.Add(b.gameObject);
   b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>{selectedId=item!=null?item.itemId:"";selectedSlot=slotIndex;Details(item);UpdateActions();});
   var img=b.GetComponent<Image>();if(img!=null){int rarity=item!=null?(int)item.rarity:0;img.sprite=item!=null&&raritySlots!=null&&rarity<raritySlots.Length?raritySlots[rarity]:emptySlot;img.color=Color.white;img.type=img.sprite!=null&&img.sprite.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;}
   var label=b.GetComponentInChildren<Text>();if(label!=null)label.text=item==null?"":(showingShop?item.buyPrice+" G":count>1?"x"+count:"");
   var iconTransform=b.transform.Find("Icon");if(iconTransform!=null){var icon=iconTransform.GetComponent<Image>();icon.sprite=item!=null?item.icon:null;icon.enabled=icon.sprite!=null;}
   if(!showingShop){var drag=b.GetComponent<FantasySlotDrag>();if(drag==null)drag=b.gameObject.AddComponent<FantasySlotDrag>();drag.ui=this;drag.index=slotIndex;}
  }
  void Details(FantasyItem item){if(detailsText!=null)detailsText.text=item==null?"Select an item":item.displayName+"\n"+item.rarity+" • "+item.kind+"\n\n"+item.description+"\n\nBuy: "+item.buyPrice+" Gold\nSell: "+item.sellPrice+" Gold"+(inventory.IsEquipped(item.itemId)?"\nEQUIPPED":"");
   if(detailsIcon!=null){detailsIcon.sprite=item!=null?item.icon:null;detailsIcon.enabled=detailsIcon.sprite!=null;}}
  void UpdateActions(){var item=inventory!=null?inventory.Find(selectedId):null;bool owned=item!=null&&inventory.Count(item.itemId)>0;
   if(buyButton!=null)buyButton.interactable=item!=null&&item.canBuy&&inventory.data.gold>=item.buyPrice&&inventory.CanAdd(item.itemId,1);
   if(sellButton!=null)sellButton.interactable=owned&&item.canSell;
   if(useButton!=null)useButton.interactable=owned&&item.kind==ItemKind.Consumable;
   if(equipButton!=null)equipButton.interactable=owned&&(item.kind==ItemKind.Weapon||item.kind==ItemKind.Armor||item.kind==ItemKind.Accessory);
   if(unequipButton!=null)unequipButton.interactable=owned&&inventory.IsEquipped(selectedId);
  }
  void Message(string msg){if(feedbackText!=null)feedbackText.text=msg;else Debug.Log(msg);}
  public void BuySelected(){if(inventory==null||!inventory.Buy(selectedId))Message("Cannot buy: check gold and bag space.");else Message("Purchased!");}
  public void SellSelected(){if(inventory==null||!inventory.Sell(selectedId))Message("Cannot sell this item.");else Message("Sold!");}
  public void UseSelected(){var item=inventory!=null?inventory.Find(selectedId):null;if(item==null||item.kind!=ItemKind.Consumable||!inventory.Use(selectedId))Message("Cannot use this item.");else Message("Item used.");}
  public void EquipSelected(){if(inventory==null){return;}var item=inventory.Find(selectedId);if(item==null)return;
   var slot=item.kind==ItemKind.Weapon?EquipSlot.Weapon:item.kind==ItemKind.Armor?EquipSlot.Armor:EquipSlot.Accessory;
   if(!inventory.Equip(selectedId,slot))Message("Cannot equip.");else Message("Equipped!");}
  public void UnequipSelected(){if(inventory==null)return;var item=inventory.Find(selectedId);if(item==null)return;
   if(inventory.data.weapon==selectedId)inventory.Unequip(EquipSlot.Weapon);
   else if(inventory.data.armor==selectedId)inventory.Unequip(EquipSlot.Armor);
   else if(inventory.data.accessory==selectedId)inventory.Unequip(EquipSlot.Accessory);
   Message("Unequipped.");}
  public void MoveSlot(int from,int to){if(inventory!=null)inventory.MoveSlot(from,to);}
 }
}
