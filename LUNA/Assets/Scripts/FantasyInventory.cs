using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FantasyUI {
 public enum ItemKind { Consumable, Weapon, Armor, Material, Accessory, Quest }
 public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
 public enum EquipSlot { Weapon, Armor, Accessory }
 [CreateAssetMenu(menuName="Fantasy UI/Item",fileName="FantasyItem")]
 public class FantasyItem : ScriptableObject {
  public string itemId="unique_item_id", displayName="New Item";
  [TextArea(2,5)] public string description;
  public Sprite icon; public ItemKind kind; public ItemRarity rarity;
  [Min(0)] public int buyPrice=20,sellPrice=10;
  [Min(1)] public int maxStack=99;
  [Min(0)] public int healAmount=20, manaAmount=0, attackBonus=0, defenseBonus=0;
  public bool canBuy=true,canSell=true;
 }
 [Serializable] public class ItemStack { public string id; public int count; public ItemStack(string id,int count){this.id=id;this.count=count;} }
 [Serializable] public class SaveData {
  public int gold=200,gems=0;
  public List<ItemStack> slots=new List<ItemStack>();
  public string weapon="",armor="",accessory="";
 }
 public class FantasyInventory : MonoBehaviour {
  public static FantasyInventory Instance {get;private set;}
  public List<FantasyItem> catalog=new List<FantasyItem>();
  [Min(1)] public int capacity=30;
  public SaveData data=new SaveData();
  public event Action Changed;
  public event Action<FantasyItem> Consumed; // Hook your player HP/MP script here.
  public event Action EquipmentChanged; // Hook character stats/visuals here.
  string SavePath {get{return Path.Combine(Application.persistentDataPath,"fantasy_inventory.json");}}
  void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);Load();}
  public FantasyItem Find(string id){return catalog.Find(x=>x!=null&&x.itemId==id);}
  public ItemStack Slot(int index){return index>=0&&index<data.slots.Count?data.slots[index]:null;}
  void Normalize(){if(data==null)data=new SaveData();if(data.slots==null)data.slots=new List<ItemStack>();
   while(data.slots.Count<capacity)data.slots.Add(new ItemStack("",0));
   if(data.slots.Count>capacity)data.slots.RemoveRange(capacity,data.slots.Count-capacity);
   foreach(var s in data.slots)if(s!=null&&(s.count<=0||string.IsNullOrEmpty(s.id))){s.id="";s.count=0;}
  }
  public int Count(string id){int n=0;foreach(var s in data.slots)if(s!=null&&s.id==id)n+=s.count;return n;}
  public bool CanAdd(string id,int count){var item=Find(id);if(item==null||count<=0)return false;int space=0;
   foreach(var s in data.slots){if(s==null||s.count<=0)space+=Mathf.Max(1,item.maxStack);else if(s.id==id)space+=Mathf.Max(0,item.maxStack-s.count);}return space>=count;
  }
  public bool Add(string id,int count){var item=Find(id);if(!CanAdd(id,count))return false;
   foreach(var s in data.slots){if(count==0)break;if(s==null||s.id!=id||s.count<=0)continue;int take=Mathf.Min(count,Mathf.Max(1,item.maxStack)-s.count);s.count+=take;count-=take;}
   foreach(var s in data.slots){if(count==0)break;if(s==null||s.count>0)continue;int take=Mathf.Min(count,Mathf.Max(1,item.maxStack));s.id=id;s.count=take;count-=take;}
   Notify();return true;
  }
  public bool Remove(string id,int count){if(count<=0||Count(id)<count)return false;
   for(int i=data.slots.Count-1;i>=0&&count>0;i--){var s=data.slots[i];if(s==null||s.id!=id)continue;int take=Mathf.Min(count,s.count);s.count-=take;count-=take;if(s.count<=0){s.id="";s.count=0;}}
   if(Count(id)==0){bool changed=false;if(data.weapon==id){data.weapon="";changed=true;}if(data.armor==id){data.armor="";changed=true;}if(data.accessory==id){data.accessory="";changed=true;}if(changed)EquipmentChanged?.Invoke();}
   Notify();return true;
  }
  public bool Buy(string id,int quantity=1){var item=Find(id);if(item==null||!item.canBuy||quantity<=0||item.buyPrice<0)return false;
   long cost=(long)item.buyPrice*quantity;if(cost>data.gold||!CanAdd(id,quantity))return false;
   if(!Add(id,quantity))return false;data.gold-=(int)cost;Notify();return true;
  }
  public bool Sell(string id,int quantity=1){var item=Find(id);if(item==null||!item.canSell||quantity<=0||item.sellPrice<0||Count(id)<quantity)return false;
   long earned=(long)item.sellPrice*quantity;if(earned>int.MaxValue-data.gold)return false;
   if(!Remove(id,quantity))return false;data.gold+=(int)earned;Notify();return true;
  }
  public bool Use(string id){var item=Find(id);if(item==null||Count(id)==0)return false;
   if(item.kind==ItemKind.Weapon)return Equip(id,EquipSlot.Weapon);
   if(item.kind==ItemKind.Armor)return Equip(id,EquipSlot.Armor);
   if(item.kind==ItemKind.Accessory)return Equip(id,EquipSlot.Accessory);
   if(item.kind!=ItemKind.Consumable)return false;
   if(!Remove(id,1))return false;Consumed?.Invoke(item);return true;
  }
  public bool Equip(string id,EquipSlot slot){var item=Find(id);if(item==null||Count(id)==0)return false;
   if(slot==EquipSlot.Weapon&&item.kind!=ItemKind.Weapon)return false;
   if(slot==EquipSlot.Armor&&item.kind!=ItemKind.Armor)return false;
   if(slot==EquipSlot.Accessory&&item.kind!=ItemKind.Accessory)return false;
   if(slot==EquipSlot.Weapon)data.weapon=id;else if(slot==EquipSlot.Armor)data.armor=id;else data.accessory=id;
   EquipmentChanged?.Invoke();Notify();return true;
  }
  public void Unequip(EquipSlot slot){if(slot==EquipSlot.Weapon)data.weapon="";else if(slot==EquipSlot.Armor)data.armor="";else data.accessory="";EquipmentChanged?.Invoke();Notify();}
  public bool MoveSlot(int from,int to){if(from<0||to<0||from>=capacity||to>=capacity||from==to)return false;
   var a=data.slots[from];data.slots[from]=data.slots[to];data.slots[to]=a;Notify();return true;
  }
  public bool IsEquipped(string id){return !string.IsNullOrEmpty(id)&&(data.weapon==id||data.armor==id||data.accessory==id);}
  public void Notify(){Save();Changed?.Invoke();}
  public void Save(){try{File.WriteAllText(SavePath,JsonUtility.ToJson(data,true));}catch(Exception e){Debug.LogWarning("Inventory save: "+e.Message);}}
  public void Load(){try{if(File.Exists(SavePath)){var loaded=JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));if(loaded!=null)data=loaded;}}catch(Exception e){Debug.LogWarning("Inventory load: "+e.Message);}Normalize();Changed?.Invoke();}
  public void ResetSave(){data=new SaveData();Normalize();Notify();}
 }
}
