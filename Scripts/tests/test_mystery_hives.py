import json,unittest,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Scripts'))
from import_cube_land_levels import convert

class MysteryHiveImportTests(unittest.TestCase):
 def test_exactly_five_levels_hide_three_hives_with_staggered_late_positions_on_six_and_eight(self):
  found=[]
  for number in range(1,106):
   data=json.loads((ROOT/f'Assets/Resources/Levels/Level{number:02}.json').read_text())
   hidden=[(i,h) for i,h in enumerate(data['hives']) if h.get('isHidden')]
   if not hidden:continue
   found.append(number);self.assertEqual(3,len(hidden))
   for i,h in hidden:
    column=[j for j,v in enumerate(data['hives']) if v['column']==h['column']]
    self.assertEqual(h['column']+2 if number in (6,8) else 1,column.index(i));self.assertFalse(h['connected'])
  self.assertEqual([6,8,12,15,18],found)
 def test_source_hidden_flag_is_preserved_without_changing_color_or_quota(self):
  source=json.loads((ROOT/'Assets/LevelSources/CubeLand/plant_flower.json').read_text())
  ref=source['references']['RefIds'][0];ref['data']['isHidden']=True
  data=convert(source,1);h=next(h for h in data['hives'] if h['sourceRid']==str(ref['rid']))
  self.assertTrue(h.get('isHidden'));self.assertEqual(ref['data']['color'],h['sourceColor']);self.assertEqual(ref['data']['bulletCount'],h['quota'])

 def test_hidden_source_cannot_be_silently_lost_when_queues_are_reauthored(self):
  source=json.loads((ROOT/'Assets/LevelSources/CubeLand/plant_flower.json').read_text())
  source['references']['RefIds'][0]['data']['isHidden']=True
  with self.assertRaisesRegex(ValueError,'Hidden source'):
   convert(source,35)

if __name__=='__main__':unittest.main()
