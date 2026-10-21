using System;
using System.Collections.Generic;
namespace BeeHarvest {
 public static class LinkedHiveGroups {
  public static int[][] Build(HiveData[] hives) {
   int count=hives.Length;var neighbors=new List<int>[count];for(int i=0;i<count;i++)neighbors[i]=new List<int>();
   for(int i=0;i<count;i++)foreach(int other in hives[i].connected??new int[0]){
    if(other<0||other>=count||other==i)throw new ArgumentException("Invalid connected hive identity");
    neighbors[i].Add(other);neighbors[other].Add(i);
   }
   var result=new int[count][];var seen=new bool[count];
   for(int i=0;i<count;i++)if(!seen[i]){
    var group=new List<int>();var queue=new Queue<int>();queue.Enqueue(i);seen[i]=true;var columns=new HashSet<int>();
    while(queue.Count>0){int h=queue.Dequeue();group.Add(h);if(!columns.Add(hives[h].column))throw new ArgumentException("Connected hives must occupy different queue columns");foreach(int j in neighbors[h])if(!seen[j]){seen[j]=true;queue.Enqueue(j);}}
    group.Sort();var members=group.ToArray();foreach(int h in group)result[h]=members;
   }
   return result;
  }
 }
}
