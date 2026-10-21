import collections,copy,json,unittest,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from import_cube_land_levels import convert,author_surface_queue
ROOT=Path(__file__).resolve().parents[2]
class SurfaceQueueAuthoringTests(unittest.TestCase):
 def setUp(self):
  self.raw=json.loads((ROOT/'Assets/LevelSources/CubeLand/Shuttle.json').read_text())
  self.original=convert(self.raw,1,repack=False)
 def test_voxel_art_and_per_color_budget_are_unchanged(self):
  before=copy.deepcopy(self.original);actual=author_surface_queue(self.original)
  self.assertEqual(before,self.original)
  def identity(c):return tuple((k,v) for k,v in c.items() if k!='id')
  self.assertEqual(sorted(map(identity,before['cubes'])),sorted(map(identity,actual['cubes'])))
  self.assertEqual(collections.Counter(c['color'] for c in before['cubes']),collections.Counter({color:sum(h['quota'] for h in actual['hives'] if h['color']==color) for color in {c['color'] for c in actual['cubes']}}))
  self.assertTrue(all(h['sourceRid'].startswith('authored-surface-v1-') for h in actual['hives']))
 def test_dense_ids_prioritize_upper_cells_and_quotas_fit_normal_tiles(self):
  data=author_surface_queue(self.original)
  self.assertEqual(list(range(len(data['cubes']))),[c['id'] for c in data['cubes']])
  self.assertEqual(sorted([c['y'] for c in data['cubes']],reverse=True),[c['y'] for c in data['cubes']])
  self.assertTrue(all(0<h['quota']<=80 and not h['connected'] for h in data['hives']))
 def test_authored_global_order_is_always_a_visible_queue_front(self):
  data=author_surface_queue(self.original);queues=[[i for i,h in enumerate(data['hives']) if h['column']==c] for c in range(data['columns'])]
  for i,h in enumerate(data['hives']):self.assertEqual(i,queues[h['column']].pop(0))
  self.assertTrue(all(not q for q in queues))
 def test_conversion_remains_deterministic_without_mutating_raw_sources(self):
  raw=copy.deepcopy(self.raw);a=author_surface_queue(convert(raw,1,repack=False));b=author_surface_queue(convert(raw,1,repack=False))
  self.assertEqual(a,b);self.assertEqual(self.raw,raw)
if __name__=='__main__':unittest.main()
