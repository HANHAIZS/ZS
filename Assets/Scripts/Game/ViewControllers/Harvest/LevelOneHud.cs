using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest {
 public sealed class LevelOneHud:BeeHarvestController {
  public Text[] SlotLabels,QueueLabels,NextLabels;
  public Text[] ThirdLabels;
  public Button[] ThirdButtons;
  public int VisibleRows=>ThirdLabels!=null&&ThirdLabels.Length==5?3:2;
  public Image[] QueueImages;
  public Button[] QueueButtons;
  public Button[] RearButtons;
  public Button PreviousPage,NextPage;
  public HarvestBoosterBar Boosters;
  public Button Restart;
  public RectTransform[] SlotTargets;
  HarvestModel model;HarvestSystem rules;int revision=-1,generation=-1;
  bool hiveInputEnabled=true;
  Outline[] slotOutlines,queueOutlines,nextOutlines;
  public int QueuePage {get;private set;}
  public int LayoutVersion {get;private set;}
  public void Initialize(HarvestModel state,HarvestSystem system,System.Action<int> click,System.Action restart,LevelOneController level){
   model=state;rules=system;
   for(int i=0;i<5;i++){int c=i;QueueButtons[i].onClick.RemoveAllListeners();QueueButtons[i].onClick.AddListener(()=>click(DisplayedHive(c,false)));RearButtons[i].onClick.RemoveAllListeners();RearButtons[i].onClick.AddListener(()=>click(DisplayedHive(c,true)));if(VisibleRows==3){ThirdButtons[i].onClick.RemoveAllListeners();ThirdButtons[i].onClick.AddListener(()=>click(DisplayedHive(c,2)));}}
   for(int column=0;column<5;column++)for(int row=0;row<VisibleRows;row++){
    var button=row==0?QueueButtons[column]:row==1?RearButtons[column]:ThirdButtons[column];
    var feedback=button.GetComponent<HiveWaitingClickFeedback>();
    if(feedback==null)feedback=button.gameObject.AddComponent<HiveWaitingClickFeedback>();
    feedback.Initialize(this,button,column,row,click);
   }
   if(Restart!=null)Restart.onClick.AddListener(()=>restart());
   PreviousPage.onClick.AddListener(()=>ChangePage(-1));NextPage.onClick.AddListener(()=>ChangePage(1));
   var font=Font.CreateDynamicFontFromOSFont(new[]{"PingFang SC","Arial"},24);
   foreach(var t in GetComponentsInChildren<Text>(true)){t.font=font;t.color=Restart!=null&&t.transform.IsChildOf(Restart.transform)?Color.white:new Color(.27f,.15f,.065f);}
   slotOutlines=new Outline[SlotLabels.Length];queueOutlines=new Outline[5];nextOutlines=new Outline[5];
   for(int i=0;i<SlotLabels.Length;i++)slotOutlines[i]=SlotLabels[i].GetComponent<Outline>();
   for(int i=0;i<5;i++){queueOutlines[i]=QueueLabels[i].GetComponent<Outline>();nextOutlines[i]=NextLabels[i].GetComponent<Outline>();}
   Boosters.Initialize(model,rules,this,level);Sync();
  }
  public bool CanPlayWaitingFeedback(int column,int row)=>hiveInputEnabled&&!model.Paused&&!model.Won&&!model.Deadlocked&&DisplayedHive(column,row)>=0;
  public int DisplayedHive(int column,bool rear)=>DisplayedHive(column,rear?1:0);
  public int DisplayedHive(int column,int row)=>rules.WaitingAt(column,QueuePage*VisibleRows+row);
  public RectTransform RowControl(int column,int row)=>row==0?(RectTransform)QueueButtons[column].transform:RowLabel(column,row).rectTransform;
  public Text RowLabel(int column,int row)=>row==0?QueueLabels[column]:row==1?NextLabels[column]:ThirdLabels[column];
  int RowOf(int hive,int column){for(int row=0;row<VisibleRows;row++)if(DisplayedHive(column,row)==hive)return row;return 0;}
  public RectTransform WaitingControl(int hive){int c=rules.WaitingColumnOf(hive);return RowControl(c,RowOf(hive,c));}
  public Text WaitingLabel(int hive){int c=rules.WaitingColumnOf(hive);return RowLabel(c,RowOf(hive,c));}
  public void ResetQueuePage(){QueuePage=0;revision=-1;}
  int LastPage(){int count=0;for(int c=0;c<5;c++)count=Mathf.Max(count,model.WaitingColumn(c).Count);return Mathf.Max(0,(count-1)/VisibleRows);}
  void ChangePage(int direction){if(!hiveInputEnabled||model.Paused||!model.FreeSelection||model.Won||model.Deadlocked)return;QueuePage=Mathf.Clamp(QueuePage+direction,0,LastPage());revision=-1;Sync();}
  public void Sync(){
   if(model==null)return;
   if(generation!=model.Generation){generation=model.Generation;QueuePage=0;revision=-1;LayoutColumns();}
   int page=model.FreeSelection?Mathf.Min(QueuePage,LastPage()):0;
   if(page!=QueuePage){QueuePage=page;revision=-1;}
   if(revision==model.Revision)return;revision=model.Revision;
   bool available=hiveInputEnabled&&!model.Won&&!model.Deadlocked&&!model.Paused;
   for(int s=0;s<SlotLabels.Length;s++){
    bool enabled=s<model.Slots.Count;SlotLabels[s].gameObject.SetActive(enabled);SlotTargets[s].gameObject.SetActive(enabled);
    int h=enabled?model.Slots[s]:-1;SlotLabels[s].text=h<0?"":model.Hives[h].Remaining.ToString();SlotLabels[s].color=h<0?new Color(.55f,.38f,.18f):Color.white;SlotLabels[s].fontStyle=h<0?FontStyle.Normal:FontStyle.Bold;if(slotOutlines[s]!=null)slotOutlines[s].enabled=h>=0;
   }
   for(int c=0;c<5;c++){
    int h=DisplayedHive(c,false),next=DisplayedHive(c,true);
    QueueButtons[c].interactable=available&&h>=0&&rules.CanSelectHive(h);QueueImages[c].color=Color.clear;
    QueueLabels[c].color=Color.white;QueueLabels[c].text=h<0?"":model.Hives[h].Remaining.ToString();if(queueOutlines[c]!=null)queueOutlines[c].enabled=h>=0;
    RearButtons[c].interactable=available&&next>=0&&rules.CanSelectHive(next);
    NextLabels[c].text=next<0?"":model.Hives[next].Remaining.ToString();NextLabels[c].color=Color.white;NextLabels[c].fontStyle=FontStyle.Bold;
    NextLabels[c].raycastTarget=available&&next>=0;if(nextOutlines[c]!=null)nextOutlines[c].enabled=next>=0;
    if(VisibleRows==3){
     int third=DisplayedHive(c,2);var label=ThirdLabels[c];
     ThirdButtons[c].interactable=available&&third>=0&&rules.CanSelectHive(third);
     label.text=third<0?"":model.Hives[third].Remaining.ToString();label.color=Color.white;label.fontStyle=FontStyle.Bold;label.raycastTarget=available&&third>=0;
    }
   }
   bool paging=model.FreeSelection&&LastPage()>0;PreviousPage.gameObject.SetActive(paging);NextPage.gameObject.SetActive(paging);
   PreviousPage.interactable=available&&QueuePage>0;NextPage.interactable=available&&QueuePage<LastPage();
   PreviousPage.targetGraphic.raycastTarget=available;NextPage.targetGraphic.raycastTarget=available;Boosters.Sync();
  }
  void LayoutColumns(){
   for(int c=0;c<5;c++){
    bool shown=c<model.ColumnCount;QueueButtons[c].gameObject.SetActive(shown);NextLabels[c].gameObject.SetActive(shown);
    if(VisibleRows==3)ThirdLabels[c].gameObject.SetActive(shown);
    if(!shown)continue;float x=model.ColumnCount==3?.5f+(c-1)*.30f:(c+.5f)/model.ColumnCount;
    // Front labels stretch inside their buttons; rear labels are the controls themselves.
    QueueLabels[c].rectTransform.anchorMin=Vector2.zero;QueueLabels[c].rectTransform.anchorMax=Vector2.one;
    MoveColumn((RectTransform)QueueButtons[c].transform,x);
    for(int row=1;row<VisibleRows;row++)MoveColumn(RowLabel(c,row).rectTransform,x);
   }
   LayoutVersion++;
  }
  static void MoveColumn(RectTransform control,float center){
   var a=control.anchorMin;var b=control.anchorMax;a.x=center-.085f;b.x=center+.085f;control.anchorMin=a;control.anchorMax=b;
   var position=control.anchoredPosition;position.x=0;control.anchoredPosition=position;
  }
  public void SetHiveInputEnabled(bool enabled){hiveInputEnabled=enabled;foreach(var image in QueueImages)image.raycastTarget=enabled;if(Boosters!=null)Boosters.SetInputEnabled(enabled);revision=-1;Sync();}
  void OnDestroy(){if(QueueButtons!=null)foreach(var b in QueueButtons)if(b!=null)b.onClick.RemoveAllListeners();if(RearButtons!=null)foreach(var b in RearButtons)if(b!=null)b.onClick.RemoveAllListeners();if(ThirdButtons!=null)foreach(var b in ThirdButtons)if(b!=null)b.onClick.RemoveAllListeners();if(PreviousPage!=null)PreviousPage.onClick.RemoveAllListeners();if(NextPage!=null)NextPage.onClick.RemoveAllListeners();if(Restart!=null)Restart.onClick.RemoveAllListeners();}
 }
}
