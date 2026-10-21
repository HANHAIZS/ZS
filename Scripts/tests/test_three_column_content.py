import unittest,json,collections,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from import_cube_land_levels import convert
ROOT=Path(__file__).resolve().parents[2]
class ThreeColumnContentTests(unittest.TestCase):
 def test_every_production_level_exposes_exactly_three_live_queues(self):
  for n in range(1,106):
   data=json.loads((ROOT/f'Assets/Resources/Levels/Level{n:02}.json').read_text())
   with self.subTest(level=n):
    self.assertEqual(3,data['columns']);self.assertEqual({0,1,2},{h['column'] for h in data['hives']})
    for i,h in enumerate(data['hives']):
     group=[i]+h['connected'];self.assertLessEqual(len(group),3);self.assertEqual(len(group),len({data['hives'][j]['column'] for j in group}))
 def test_cat_at_level_thirty_five_is_a_full_new_model_in_the_requested_budget(self):
  data=json.loads((ROOT/'Assets/Resources/Levels/Level35.json').read_text())
  self.assertEqual('Cat_Luci',data['sourceName']);self.assertEqual(2471,len(data['cubes']));self.assertTrue(2000<=len(data['cubes'])<=2500);self.assertEqual(3,data['columns'])
  duck=json.loads((ROOT/'Assets/Resources/Levels/Level02.json').read_text());self.assertEqual('Duck',duck['sourceName']);self.assertEqual(2190,len(duck['cubes']));self.assertEqual(30,len(duck['hives']))
  raw=json.loads((ROOT/'Assets/LevelSources/CubeLand/Cat_Luci.json').read_text());self.assertEqual(data,convert(raw,35))
  original={(p['x'],p['z'],-p['y'],layer['colorType']) for layer in raw['LayerData'] for p in layer['VoxelPositions']}
  self.assertEqual(original,{(c['x'],c['y'],c['z'],c['sourceColor']) for c in data['cubes']})
  self.assertTrue(all(c['name'].startswith('Cat_LuciV2_Voxel_') for c in data['cubes']))
  counts=collections.Counter(c['color'] for c in data['cubes']);budget=collections.Counter()
  for h in data['hives']:budget[h['color']]+=h['quota']
  self.assertEqual(counts,budget)
if __name__=='__main__':unittest.main()
