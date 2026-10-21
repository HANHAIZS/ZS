import copy,json,unittest,subprocess,tempfile
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from import_cube_land_levels import convert
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/LevelSources/CubeLand'
CONVERTED=ROOT/'Assets/Resources/Levels'
class ImportTests(unittest.TestCase):
 def test_cli_emits_all_one_hundred_five_explicit_levels(self):
  with tempfile.TemporaryDirectory() as folder:
   subprocess.run([sys.executable,str(ROOT/'Scripts/import_cube_land_levels.py'),'--source',str(SOURCE),'--output',folder],check=True,capture_output=True,text=True)
   self.assertEqual([f'Level{n:02}.json' for n in range(1,106)],sorted((p.name for p in Path(folder).glob('*.json')),key=lambda name:int(name[5:-5])))
 def test_banana_uses_its_own_voxel_identity(self):
  data=convert(json.loads((SOURCE/'Banana.json').read_text()),3)
  self.assertEqual(('Banana',812,20,3),(data['sourceName'],len(data['cubes']),len(data['hives']),data['columns']))
  self.assertTrue(all(c['name'].startswith('BananaV2_Voxel_') for c in data['cubes']))
 @classmethod
 def setUpClass(cls):cls.flower=json.loads((SOURCE/'plant_flower.json').read_text())
 def test_production_is_reproducible(self):
  for number,name in enumerate(['plant_flower','Duck','Banana','Bee','Watermelon','Chick','Sheep','Shark','Broccoli','Cactustoy','Carrot','CatHead','Snake','Onion','DodoBird','CupHalf','casper','Rat','Mower','Crab','Santa','Hand_grips','Balloon_rotate','Gaming_microphone','Shuttle','Cat_Minti','Megaphone','World_cup','Spitfire','Hen_2','Oreo','Axe','Mustard','Robot_2','Cat_Luci','Dice','EmpireTower','Launcher','Gamepad','Rattle','Popsicle-chocolate','ChipGreen','Gas_station','Bell','BoxOpen','PizzaSlice','Camera','Earth','PizzaSliced','Paper_Towels','ChickenLeg','Shaker_salt','Hair_brush','Bowling_pins','Radio_4','Heart','Umbrella','Clock','Diamond_2','Saturn','Rolling_pin_2','Animal_Penguin','Tenis_ball','Liquid_soap','Bulb','Barrel','Waffle','Star','Chessboard','Castle','Bull','Bucket','Croissant','Lighthouse','Chocolate_2','Lifebuoy','Orange','pelican','Angel','Goomba','Cone','Avocado','Fluffy','Spices','Koala','Mami','Tractor_Shovel','Bear','Remote','Tree','Teepee','Dollars_container','Brain','Volcano','Toaster','Pigeon_2','Cola','iceberg','Wheelbarrow','Medieval_church','Ladybug','Maple','Willys','Scorpion','mouse_2'],1):
   actual=convert(json.loads((SOURCE/f'{name}.json').read_text()),number)
   expected=json.loads((CONVERTED/f'Level{number:02}.json').read_text());self.assertEqual(expected,actual)
 def test_source_coordinates_color_and_depth_remain_traceable(self):
  data=convert(self.flower,1);raw=[(l,p) for l in self.flower['LayerData'] for p in l['VoxelPositions']]
  for cube,(layer,pos) in zip(data['cubes'],raw):self.assertEqual((pos['x'],pos['z'],-pos['y'],layer['colorType'],layer['depth']),(cube['x'],cube['y'],cube['z'],cube['sourceColor'],cube['sourceDepth']))
 def test_duplicate_voxel_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['LayerData'][0]['VoxelPositions'].append(raw['LayerData'][0]['VoxelPositions'][0]);
  with self.assertRaisesRegex(ValueError,'Duplicate voxel'):convert(raw,1)
 def test_duplicate_queue_reference_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['serializedPlaceableColumn'][0]['Placeables'].append(raw['serializedPlaceableColumn'][0]['Placeables'][0])
  with self.assertRaisesRegex(ValueError,'Duplicate queue'):convert(raw,1)
 def test_unsupported_special_list_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['IceWrapperData']=[{}]
  with self.assertRaisesRegex(ValueError,'Unsupported top-level'):convert(raw,1)
 def test_unsupported_shooter_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['references']['RefIds'][0]['data']['iceHp']=1
  with self.assertRaisesRegex(ValueError,'Unsupported shooter'):convert(raw,1)
 def test_quota_mismatch_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['references']['RefIds'][0]['data']['bulletCount']+=1
  with self.assertRaisesRegex(ValueError,'quota mismatch'):convert(raw,1)
 def test_nonpositive_quota_is_rejected(self):
  raw=copy.deepcopy(self.flower);raw['references']['RefIds'][0]['data']['bulletCount']=0
  with self.assertRaisesRegex(ValueError,'Nonpositive'):convert(raw,1)
if __name__=='__main__':unittest.main()
