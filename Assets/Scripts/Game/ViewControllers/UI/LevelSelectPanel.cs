using System.Globalization;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeeHarvest {
 /// <summary>Five-column scene-owned picker. Only one page of buttons is instantiated.</summary>
 public sealed class LevelSelectPanel : BeeHarvestController {
  const int PageSize=PlaytestRoute.LevelsPerGroup,Columns=5;
  const float Gap=9;
  readonly Button[] cells=new Button[PageSize];
  readonly TMP_Text[] numbers=new TMP_Text[PageSize];
  Button[] groups;
  CanvasGroup inputGate;
  BaseInputModule module;
  MenuNavigationModel navigation;
  TMP_InputField jump;
  TMP_Text message;
  ScrollRect scroll;
  RectTransform viewport,grid;
  GridLayoutGroup layout;
  Vector2 lastSize;
  int page,visible;
  bool initialized,paused;

  public void Build(MainMenuPage owner,MenuNavigationModel state,BaseInputModule inputModule){
   navigation=state;module=inputModule;
   var font=owner.Tabs.StartLevelLabel.font;
   inputGate=gameObject.AddComponent<CanvasGroup>();inputGate.interactable=false;
   PlaytestUi.Image(PlaytestUi.Rect("DimBackground",transform,0,0,1,1),new Color(0,0,0,.72f),true);
   var content=PlaytestUi.Rect("PopupContent",transform,.043f,.10f,.957f,.90f);
   var source=owner.SettingsPopup.PopupContent.transform;
   var background=FindSprite(source,"PanelBackground");
   Skin(content,background);
   var header=PlaytestUi.Rect("TitleBanner",content,.12f,.846f,.88f,.985f);
   var banner=FindSprite(source,"TitleBanner");
   if(banner!=null){var image=PlaytestUi.Image(header,Color.white);image.sprite=banner;}
   PlaytestUi.Label("Title",header,"Level Select",font,54);
   var close=PlaytestUi.Button("CloseLevelSelect",content,"X",font,38,.865f,.895f,.966f,.972f);
   close.onClick.AddListener(()=>{if(CanInteract)this.SendCommand(new ClosePlaytestSelectorCommand());});
   PlaytestUi.Label("Mode",PlaytestUi.Rect("ModeArea",content,.06f,.795f,.94f,.852f),"PLAYTEST  /  All levels unlocked",font,29);
   var input=PlaytestUi.Rect("LevelNumber",content,.075f,.695f,.70f,.78f);
   PlaytestUi.Image(input,PlaytestUi.DeepPurple,true);
   var fieldViewport=PlaytestUi.Rect("TextArea",input,.02f,.06f,.98f,.94f);
   fieldViewport.gameObject.AddComponent<RectMask2D>();
   var value=PlaytestUi.Label("Value",fieldViewport,"0123456789",font,39);value.text="";
   var hint=PlaytestUi.Label("Placeholder",fieldViewport,"Level number",font,34);hint.color=new Color(1,1,1,.6f);
   jump=input.gameObject.AddComponent<TMP_InputField>();
   jump.textViewport=fieldViewport;jump.textComponent=value;jump.placeholder=hint;jump.targetGraphic=input.GetComponent<Image>();
   jump.pointSize=39;
   jump.contentType=TMP_InputField.ContentType.IntegerNumber;jump.lineType=TMP_InputField.LineType.SingleLine;
   jump.characterLimit=LevelOneAssets.LastLevel.ToString(CultureInfo.InvariantCulture).Length;
   jump.onSubmit.AddListener(_=>Jump());
   PlaytestUi.Button("JumpToLevel",content,"Go",font,36,.73f,.695f,.925f,.78f).onClick.AddListener(Jump);

   groups=new Button[(LevelOneAssets.LastLevel+PageSize-1)/PageSize];
   for(int i=0;i<groups.Length;i++){
    int group=i,first=i*PageSize+1,last=Mathf.Min(first+PageSize-1,LevelOneAssets.LastLevel);
    float x=.068f+.864f*i/groups.Length,edge=.068f+.864f*(i+1)/groups.Length;
    groups[i]=PlaytestUi.Button("LevelGroup"+(i+1),content,first+"-"+last,font,32,x+.006f,.605f,edge-.006f,.674f);
    groups[i].onClick.AddListener(()=>{if(CanInteract)SetPage(group,1f);});
   }

   viewport=PlaytestUi.Rect("LevelViewport",content,.072f,.085f,.928f,.58f);
   PlaytestUi.Image(viewport,new Color(0,0,0,.10f),true);
   viewport.gameObject.AddComponent<RectMask2D>();
   scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=60;
   grid=PlaytestUi.Rect("LevelGrid",viewport,0,1,1,1);grid.pivot=new Vector2(.5f,1);
   layout=grid.gameObject.AddComponent<GridLayoutGroup>();layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=Columns;
   layout.spacing=new Vector2(Gap,Gap);layout.childAlignment=TextAnchor.UpperCenter;
   scroll.viewport=viewport;scroll.content=grid;
   for(int i=0;i<PageSize;i++){
    int index=i;
    cells[i]=PlaytestUi.Button("LevelCell"+(i+1),grid,"00",font,42,0,0,1,1);
    numbers[i]=cells[i].GetComponentInChildren<TMP_Text>();
    cells[i].onClick.AddListener(()=>{if(CanInteract)Choose(page*PageSize+index+1,scroll.verticalNormalizedPosition);});
   }
   message=PlaytestUi.Label("Status",PlaytestUi.Rect("StatusArea",content,.055f,.018f,.945f,.075f),"No stamina used. Trial progress is not saved.",TMP_Settings.defaultFontAsset,27);
   initialized=true;
   ButtonPressMotion.Install(gameObject);
  }

  static Sprite FindSprite(Transform root,string name){
   var part=root.Find(name);var image=part==null?null:part.GetComponent<Image>();
   return image==null?null:image.sprite;
  }
  static void Skin(RectTransform rect,Sprite sprite){
   var frame=PlaytestUi.Image(rect,sprite==null?(Color)new Color32(248,181,56,255):Color.white);
   if(sprite!=null){frame.sprite=sprite;return;}
   var fill=PlaytestUi.Rect("PanelFill",rect,0,0,1,1);
   fill.offsetMin=new Vector2(5,5);fill.offsetMax=new Vector2(-5,-5);PlaytestUi.Image(fill,PlaytestUi.Purple);
  }
  bool CanInteract=>initialized&&isActiveAndEnabled&&!paused&&inputGate.interactable&&navigation.LevelSelectOpen;
  public void Present(bool open){
   if(!open){if(gameObject.activeSelf){jump.DeactivateInputField();gameObject.SetActive(false);}return;}
   if(gameObject.activeSelf)return;
   inputGate.interactable=false;
   gameObject.SetActive(true);transform.SetAsLastSibling();
   jump.SetTextWithoutNotify("");
   SetPage(navigation.LastPlaytestLevel>0?(navigation.LastPlaytestLevel-1)/PageSize:0,navigation.LevelSelectScroll);
  }
  void SetPage(int group,float position){
   page=group;
   visible=Mathf.Min(PageSize,LevelOneAssets.LastLevel-page*PageSize);
   for(int i=0;i<groups.Length;i++)PlaytestUi.Highlight(groups[i],i==page);
   for(int i=0;i<PageSize;i++){
    cells[i].gameObject.SetActive(i<visible);
    if(i>=visible)continue;
    int level=page*PageSize+i+1;
    numbers[i].text=level.ToString("00",CultureInfo.InvariantCulture);
    PlaytestUi.Highlight(cells[i],level==navigation.LastPlaytestLevel);
   }
   message.text="No stamina used. Trial progress is not saved.";
   Canvas.ForceUpdateCanvases();LayoutGrid();Canvas.ForceUpdateCanvases();
   scroll.StopMovement();scroll.verticalNormalizedPosition=position;
  }
  void LayoutGrid(){
   lastSize=viewport.rect.size;
   float width=Mathf.Max(1,(lastSize.x-(Columns-1)*Gap)/Columns);
   float height=Mathf.Clamp(width*.91f,65,136);
   layout.cellSize=new Vector2(width,height);
   int rows=(visible+Columns-1)/Columns;
   grid.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Max(lastSize.y,rows*height+(rows-1)*Gap));
  }
  void Jump(){
   if(!CanInteract)return;
   int level;
   if(!int.TryParse(jump.text,NumberStyles.None,CultureInfo.InvariantCulture,out level)||level<1||level>LevelOneAssets.LastLevel){
    message.text="Enter a level from 1 to "+LevelOneAssets.LastLevel;return;
   }
   float position=1f-((level-1)%PageSize/Columns)/5f;
   Choose(level,position);
  }
  void Choose(int level,float position){
   if(!CanInteract)return;
   inputGate.interactable=false;
   this.SendCommand(new SelectPlaytestLevelCommand(level,position));
  }
  void LateUpdate(){
   if(!initialized)return;
   if(viewport.rect.size!=lastSize)LayoutGrid();
   if(paused||inputGate.interactable||!navigation.LevelSelectOpen)return;
   if(module!=null){var input=module.input;if(input.touchCount>0||input.GetMouseButton(0))return;module.DeactivateModule();}
   inputGate.interactable=true;
  }
  void OnApplicationPause(bool value){
   paused=value;
   if(initialized&&value){inputGate.interactable=false;jump.DeactivateInputField();}
  }
 }
}
