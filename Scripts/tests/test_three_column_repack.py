import unittest,copy,collections,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from import_cube_land_levels import repack_three_columns
class ThreeColumnRepackTests(unittest.TestCase):
 def fixture(self):
  links=[[4],[],[5,6],[],[0],[2,6],[2,5]];columns=[4,0,1,3,2,2,4]
  hives=[dict(column=columns[i],quota=i+1,color=f'Palette{i+1:02}',sourceId=100+i,sourceRid=str(i),sourceColor=i+1,connected=link) for i,link in enumerate(links)]
  cubes=[]
  for h in hives:
   for _ in range(h['quota']):cubes.append(dict(id=len(cubes),x=len(cubes),y=0,z=0,name=f'voxel_{len(cubes)}',color=h['color']))
  return dict(columns=5,hives=hives,cubes=cubes,sourceName='Fixture')
 def test_repacking_preserves_named_voxels_quotas_and_links_while_moving_groups_to_distinct_queue_fronts(self):
  data=self.fixture();original=copy.deepcopy(data);actual=repack_three_columns(data,[1,0,4,3,2,5,6]);self.assertEqual(original,data);self.assertEqual(original['cubes'],actual['cubes']);self.assertEqual(3,actual['columns'])
  self.assertEqual(['1','0','4','3','2','5','6'],[h['sourceRid'] for h in actual['hives']]);self.assertEqual([0,1,2,0,1,2,0],[h['column'] for h in actual['hives']]);self.assertEqual([[],[2],[1],[],[5,6],[4,6],[4,5]],[h['connected'] for h in actual['hives']])
  before={h['sourceRid']:{k:v for k,v in h.items() if k not in ['column','connected']} for h in original['hives']};after={h['sourceRid']:{k:v for k,v in h.items() if k not in ['column','connected']} for h in actual['hives']};self.assertEqual(before,after)
  queues=[[i for i,h in enumerate(actual['hives']) if h['column']==c] for c in range(3)]
  for group in [[0],[1,2],[3],[4,5,6]]:
   for i in group:self.assertEqual(i,queues[actual['hives'][i]['column']].pop(0))
 def test_missing_or_duplicate_hive_cannot_silently_lose_a_color_budget(self):
  for order in [[0,1],[0,1,2,3,4,5,5]]:
   with self.assertRaisesRegex(ValueError,'exactly once'):repack_three_columns(self.fixture(),order)
 def test_a_group_cannot_be_split_across_unrelated_hives_in_the_plan(self):
  with self.assertRaisesRegex(ValueError,'consecutive'):repack_three_columns(self.fixture(),[0,1,4,3,2,5,6])
 def chain(self):return dict(columns=4,cubes=[dict(id=0,name='unchanged')],hives=[dict(column=i,quota=80,color='Palette07',sourceRid=str(i),connected=link) for i,link in enumerate([[1],[0,2],[1,3],[2]])])
 def test_transitive_four_hive_chain_is_rejected_even_when_each_hive_has_only_two_direct_links(self):
  with self.assertRaisesRegex(ValueError,'fit three'):repack_three_columns(self.chain(),[0,1,2,3])
 def test_declared_middle_cut_turns_the_four_chain_into_two_pairs_without_changing_any_quota(self):
  original=self.chain();data=repack_three_columns(original,[0,1,2,3],[('1','2')]);self.assertEqual([[1],[0],[3],[2]],[h['connected'] for h in data['hives']]);self.assertEqual([80]*4,[h['quota'] for h in data['hives']]);self.assertEqual([0,1,2,0],[h['column'] for h in data['hives']]);self.assertEqual([0,2],original['hives'][1]['connected'])
 def test_declared_link_cut_must_exist_in_both_directions(self):
  with self.assertRaisesRegex(ValueError,'symmetrically'):repack_three_columns(self.chain(),[0,1,2,3],[('0','3')])
if __name__=='__main__':unittest.main()
