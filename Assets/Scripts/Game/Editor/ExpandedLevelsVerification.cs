using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using QFramework;
using UnityEngine;
using UnityEditor;
namespace BeeHarvest.Editor {
 /// <summary>Editor-only authoring replay through unchanged production commands, never runtime autoplay.</summary>
 public static class ExpandedLevelsVerification {
  static Replay current;
  public static string VerifyRules(int number,int strategy=0,string outputDirectory=null,string inputFile=null,string activationPlanFile=null){
   RequireStoppedEditor();current=new Replay(number,strategy,outputDirectory,inputFile,activationPlanFile);return current.Advance();
  }
  public static string ContinueRules(){RequireStoppedEditor();if(current==null)throw new InvalidOperationException("No pending authoring replay");return current.Advance();}
  static void RequireStoppedEditor(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Rule authoring verification requires stopped Play Mode");}
  [Serializable] sealed class Report {
   public string inputSha256,name,reason;public float stepSeconds=.1f;public int level,strategy,remaining,inTransit,deposited,total,steps,turns,chunks;public long wallMs;public bool won,pending;public int[] order,activationSteps;public string[] activationRids,activationMembers;public int columns;public string replayOf;
  }
  sealed class Replay {
   readonly IArchitecture app;readonly HarvestModel model;readonly HarvestSystem rules;readonly LevelData data;readonly int number,strategy,generation;readonly string hash,path;readonly System.Random random;
   readonly Report replayPlan;int planCursor;readonly Dictionary<string,int> byRid=new Dictionary<string,int>();readonly List<int> activationSteps=new List<int>();readonly List<string> activationRids=new List<string>();readonly List<string> activationMembers=new List<string>();readonly List<int> order=new List<int>();readonly Stopwatch totalWatch=Stopwatch.StartNew();int steps,previous,unchanged,turns,staleTurns,chunks;float yaw;
   public Replay(int number,int strategy,string outputDirectory,string inputFile,string activationPlanFile){
    this.number=number;this.strategy=strategy;app=BeeHarvestApp.Interface;model=app.GetModel<HarvestModel>();rules=app.GetSystem<HarvestSystem>();
    string text=string.IsNullOrEmpty(inputFile)?Resources.Load<TextAsset>("Levels/Level"+number.ToString("00")).text:File.ReadAllText(inputFile);data=JsonUtility.FromJson<LevelData>(text);if(!string.IsNullOrEmpty(activationPlanFile))replayPlan=JsonUtility.FromJson<Report>(File.ReadAllText(activationPlanFile));for(int h=0;h<data.hives.Length;h++)byRid.Add(data.hives[h].sourceRid,h);
    using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();
    string directory=string.IsNullOrEmpty(outputDirectory)?Path.GetFullPath("Temp/BeeHarvestLevelVerification"):outputDirectory;Directory.CreateDirectory(directory);path=Path.Combine(directory,"rules-"+number.ToString("00")+"-"+strategy+".json");
    int owner=(int)typeof(HarvestModel).GetField("FlightPresentationOwner",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(model);app.SendCommand(new DetachBeeFlightPresentationCommand(owner));
    app.SendCommand(new StartLevelCommand(data));generation=model.Generation;previous=model.Remaining;random=new System.Random(strategy+number*7919);
   }
   bool Finished=>model.Won||model.Deadlocked||steps>=100000||(replayPlan==null&&staleTurns>12&&model.InTransit==0);
   public string Advance(){
    if(model.Generation!=generation)throw new InvalidOperationException("Authoring replay was interrupted by another level");
    var chunk=Stopwatch.StartNew();chunks++;
    while(!Finished&&chunk.ElapsedMilliseconds<12000)Step();
    var report=new Report{inputSha256=hash,level=number,strategy=strategy,name=data.sourceName,won=model.Won,pending=!Finished,reason=model.Won?"won":model.Deadlocked?"deadlocked":steps>=100000?"step_limit":Finished?"no_progress":"chunk_yield",remaining=model.Remaining,inTransit=model.InTransit,deposited=model.Deposited,total=data.cubes.Length,steps=steps,turns=turns,chunks=chunks,wallMs=totalWatch.ElapsedMilliseconds,order=order.ToArray(),activationSteps=activationSteps.ToArray(),activationRids=activationRids.ToArray(),activationMembers=activationMembers.ToArray(),columns=data.columns,replayOf=replayPlan==null?null:replayPlan.inputSha256};
    string json=JsonUtility.ToJson(report);File.WriteAllText(path,json);return json;
   }
   void Activate(int id){
    int[] planned,slots;rules.TryPlanActivation(id,-1,out planned,out slots);var before=model.Hives.Select(h=>h.Started).ToArray();app.SendCommand(new ActivateHiveCommand(model.Hives[id].Data.column));
    if(!before[id]&&model.Hives[id].Started){activationSteps.Add(steps);activationRids.Add(model.Hives[id].Data.sourceRid);activationMembers.Add(string.Join("|",planned.Select(h=>model.Hives[h].Data.sourceRid).ToArray()));}
    for(int h=0;h<before.Length;h++)if(!before[h]&&model.Hives[h].Started)order.Add(h);
   }
   void Step(){
    if(replayPlan!=null){
     while(planCursor<replayPlan.activationSteps.Length&&replayPlan.activationSteps[planCursor]==steps){
      foreach(string rid in replayPlan.activationMembers[planCursor].Split('|')){int id=byRid[rid];if(model.Hives[id].Started)continue;int[] members,slots;if(!rules.TryPlanActivation(id,-1,out members,out slots))throw new InvalidOperationException("Scheduled hive is not available at step "+steps+": "+rid);Activate(id);if(!model.Hives[id].Started)throw new InvalidOperationException("Scheduled activation rejected");}planCursor++;
     }
    }else if((strategy<9&&steps%2==0)||(strategy>=9&&(steps==0||model.Slots.All(slot=>slot<0)||staleTurns>=8)&&model.Flights.All(f=>f.Stage==2))){
     var candidates=new List<int>();for(int c=0;c<data.columns;c++){int id=rules.Front(c);if(id>=0&&rules.CanSelectHive(id))candidates.Add(id);}
     Dictionary<string,int> visibleCounts=null;
     if(strategy==10)candidates.Sort();else if(strategy==7||strategy==8||strategy==9){visibleCounts=new Dictionary<string,int>();for(int i=0;i<model.Cubes.Count;i++)if(model.Cubes[i].Stage==0&&rules.IsCameraVisible(i)){string color=model.Cubes[i].Data.color;if(!visibleCounts.ContainsKey(color))visibleCounts[color]=0;visibleCounts[color]++;}candidates=candidates.OrderByDescending(i=>Score(model,i,visibleCounts)).ThenBy(i=>model.Hives[i].Data.sourceId).ToList();}
     else if(strategy>=6)candidates=candidates.OrderBy(i=>model.Hives[i].Data.sourceId).ToList();else if(strategy==0)candidates.Sort();else if(strategy==1)candidates=candidates.OrderByDescending(i=>rules.FindTarget(model.Hives[i].Data.color)>=0).ThenBy(i=>i).ToList();else candidates=candidates.OrderBy(i=>random.Next()).ToList();
     foreach(int id in candidates){
      int[] plannedMembers,plannedSlots;if(!rules.TryPlanActivation(id,-1,out plannedMembers,out plannedSlots))continue;
      if((strategy==2||strategy==8||strategy>=9)&&model.Slots.Any(s=>s>=0&&model.Hives[s].Data.color==model.Hives[id].Data.color))continue;
      if((strategy==7||strategy==8||strategy==9)&&Score(model,id,visibleCounts)<=0)continue;
      if(strategy==1&&rules.FindTarget(model.Hives[id].Data.color)<0&&model.Slots.Count(s=>s<0)<3)continue;
      Activate(id);if(strategy>=9)staleTurns=0;
      if(strategy==0||strategy>=6)break;
     }
    }
    if(previous==model.Remaining)unchanged++;else{previous=model.Remaining;unchanged=0;staleTurns=0;}
    if(unchanged>=60){yaw+=45;turns++;staleTurns++;unchanged=0;}
    var direction=Quaternion.Euler(0,yaw,0)*new Vector3(0,.5f,-1).normalized;app.SendCommand(new UpdateHarvestViewCommand(direction,direction*1000,true));app.SendCommand(new AdvanceHarvestCommand(.1f));steps++;
   }
  }
  static float Score(HarvestModel model,int hive,Dictionary<string,int> visible){float count=0,quota=0;foreach(int member in model.Hives[hive].Group){int found;visible.TryGetValue(model.Hives[member].Data.color,out found);count+=found;quota+=model.Hives[member].Remaining;}return count/Math.Max(1,quota);}
  public static string BuildRange(int from,int to){if(from<16||to>LevelOneAssets.LastLevel||to<from)throw new ArgumentOutOfRangeException();for(int n=from;n<=to;n++)SourceLevelSceneSetup.Apply(n);return "Built Level"+from+" through Level"+to;}
 }
}
