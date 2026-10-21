"""Deterministic authoring adapter; never executes source scripts or loads bundles at runtime."""
from pathlib import Path
import argparse,json,hashlib,collections,copy


# Authoring choices are explicit; Cat_Luci uses its complete model at Level35.
SURFACE_QUEUE_LEVELS = frozenset({25, 28, 31, 34, 35, 40, 41, 42, 43, 44, 46, 47, 50, 51, 52, 53, 54, 55, 56, 57, 59, 60, 62, 63, 64, 65, 66, 69, 71, 72, 74, 75, 76, 77, 78, 79, 80, 81, 83, 84, 85, 86, 87, 88, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105})

def author_surface_queue(data):
    """Preserve source art while authoring bounded layer/color quotas for a surface-first route.

    Stable voxel names keep native prefab bindings. Logical ids favor upper cells; the
    resulting queue must additionally pass production-command replay at dt <= .1s.
    """
    result=copy.deepcopy(data)
    result['cubes'].sort(key=lambda c:(-c['y'],c['id']))
    for index,cube in enumerate(result['cubes']):cube['id']=index
    hives=[]
    for height in sorted({c['y'] for c in result['cubes']},reverse=True):
        counts=collections.Counter(c['sourceColor'] for c in result['cubes'] if c['y']==height)
        for color,count in sorted(counts.items(),key=lambda pair:(-pair[1],pair[0])):
            while count:
                quota=min(80,count);index=len(hives)
                hives.append(dict(column=index%result['columns'],quota=quota,sourceId=index,sourceColor=color,sourceRid=f'authored-surface-v1-y{height}-h{index}',color=f'Palette{color:02}',connected=[]))
                count-=quota
    result['hives']=hives
    return result


def repack_three_columns(data,order,cut_links=()):
    """Repack a proven activation order without changing voxel identity, quotas or links."""
    if data['columns']==3:return copy.deepcopy(data)
    data=copy.deepcopy(data)
    by_rid={h['sourceRid']:i for i,h in enumerate(data['hives'])}
    for left,right in cut_links:
        a,b=by_rid[left],by_rid[right]
        if b not in data['hives'][a]['connected'] or a not in data['hives'][b]['connected']:raise ValueError('Split link must exist symmetrically')
        data['hives'][a]['connected'].remove(b);data['hives'][b]['connected'].remove(a)
    count=len(data['hives'])
    if len(order)!=count or set(order)!=set(range(count)):raise ValueError('Activation order must contain every hive exactly once')
    position={old:new for new,old in enumerate(order)}
    seen=set()
    for old in range(count):
        if old in seen:continue
        group=set();pending=[old]
        while pending:
            member=pending.pop()
            if member in group:continue
            group.add(member);pending.extend(data['hives'][member]['connected'])
        seen|=group
        if len(group)>3:raise ValueError('Linked group does not fit three columns')
        ranks=sorted(position[i] for i in group)
        if ranks[-1]-ranks[0]+1!=len(group):raise ValueError('Linked group must remain consecutive in activation order')
    result=copy.deepcopy(data);result['columns']=3;result['hives']=[]
    for new,old in enumerate(order):
        hive=copy.deepcopy(data['hives'][old]);hive['column']=new%3;hive['connected']=sorted(position[i] for i in hive['connected']);result['hives'].append(hive)
    return result

# Explicit mystery-hive placement by stable original identity, after any column repacking.
MYSTERY_HIVE_RIDS = {6: {'1002','1008','1014'}, 8: {'1002','1010','1017'},
                     12: {'1001','1006','1019'}, 15: {'1001','1011','1020'},
                     18: {'1016','1008','1017'}}

def convert(raw,number,*,repack=True):
    if any(raw.get(k) for k in ['LargeVoxelData','DoubleLargeVoxelData','IceWrapperData','GiftWrapperData','PlaypenData','ShrinkingCoverData','BombData','ColorBoxData','ShieldData','FlyingPiggyData']):raise ValueError('Unsupported top-level special list')
    refs={int(r['rid']):r for r in raw['references']['RefIds']};hives=[];rid_to_index={}
    for column,col in enumerate(raw['serializedPlaceableColumn']):
        for item in col['Placeables']:
            rid=int(item['rid']);record=refs[rid];data=record['data']
            if record['type']['class']!='BlasterData' or data['Type']!=0 or any(data[k] for k in ['iceHp','isPilot','isChained','InnerBlasters','ChainedBlasters']):raise ValueError('Unsupported shooter state')
            if rid in rid_to_index:raise ValueError('Duplicate queue reference')
            rid_to_index[rid]=len(hives)
            hives.append(dict(column=column,quota=data['bulletCount'],color=f"Palette{data['color']:02}",sourceId=data['id'],sourceRid=str(rid),sourceColor=data['color'],connected=[]))
            if data.get('isHidden'):hives[-1]['isHidden']=True
    for rid,index in rid_to_index.items():
        for item in refs[rid]['data']['ConnectedBlasters']:
            other=rid_to_index[int(item['rid'])]
            if index==other:raise ValueError('Self link')
            if other not in hives[index]['connected']:hives[index]['connected'].append(other)
            if index not in hives[other]['connected']:hives[other]['connected'].append(index)
    model_name="Flower" if raw["m_Name"]=="plant_flower" else raw["m_Name"]
    cubes=[];seen=set()
    for layer in raw['LayerData']:
        for pos in layer['VoxelPositions']:
            point=(pos['x'],pos['z'],-pos['y'])
            if point in seen:raise ValueError('Duplicate voxel position')
            seen.add(point);index=len(cubes);color=layer['colorType']
            cubes.append(dict(id=index,x=point[0],y=point[1],z=point[2],name=f"{model_name}V2_Voxel_{index:04}_Palette{color:02}",color=f'Palette{color:02}',sourceColor=color,sourceDepth=layer['depth'],sourceX=pos['x'],sourceY=pos['y'],sourceZ=pos['z']))
    counts=collections.Counter(c['color'] for c in cubes);quotas=collections.Counter()
    for h in hives:
        if h['quota']<=0:raise ValueError('Nonpositive quota')
        quotas[h['color']]+=h['quota']
    if counts!=quotas:raise ValueError('Per-color quota mismatch')
    for h in hives:h['connected'].sort()
    result=dict(sourceName=raw['m_Name'],sourceContentHash=raw['contentHash'],columns=len(raw['serializedPlaceableColumn']),cubes=cubes,hives=hives)
    if number==35 or 96<=number<=105:result['columns']=3
    if number in SURFACE_QUEUE_LEVELS and any(h.get('isHidden') for h in hives):raise ValueError('Hidden source hives cannot be safely reauthored')
    result=author_surface_queue(result) if number in SURFACE_QUEUE_LEVELS else result
    if repack and result['columns']!=3:
        orders=json.loads((Path(__file__).with_name('cube_land_three_column_orders.json')).read_text())
        if str(number) not in orders:raise ValueError(f'Missing three-column recipe for Level{number:02}')
        recipe=orders[str(number)];result=repack_three_columns(result,recipe['order'],recipe.get('cutLinks',()))
    authored=MYSTERY_HIVE_RIDS.get(number,set())
    if authored and not authored.issubset({h['sourceRid'] for h in result['hives']}):raise ValueError('Missing mystery hive identity')
    for hive in result['hives']:
        if hive['sourceRid'] in authored:hive['isHidden']=True
    return result

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--source',type=Path,required=True);parser.add_argument('--output',type=Path,required=True);args=parser.parse_args();args.output.mkdir(parents=True,exist_ok=True)
    for number,name in enumerate(['plant_flower','Duck','Banana','Bee','Watermelon','Chick','Sheep','Shark','Broccoli','Cactustoy','Carrot','CatHead','Snake','Onion','DodoBird','CupHalf','casper','Rat','Mower','Crab','Santa','Hand_grips','Balloon_rotate','Gaming_microphone','Shuttle','Cat_Minti','Megaphone','World_cup','Spitfire','Hen_2','Oreo','Axe','Mustard','Robot_2','Cat_Luci','Dice','EmpireTower','Launcher','Gamepad','Rattle','Popsicle-chocolate','ChipGreen','Gas_station','Bell','BoxOpen','PizzaSlice','Camera','Earth','PizzaSliced','Paper_Towels','ChickenLeg','Shaker_salt','Hair_brush','Bowling_pins','Radio_4','Heart','Umbrella','Clock','Diamond_2','Saturn','Rolling_pin_2','Animal_Penguin','Tenis_ball','Liquid_soap','Bulb','Barrel','Waffle','Star','Chessboard','Castle','Bull','Bucket','Croissant','Lighthouse','Chocolate_2','Lifebuoy','Orange','pelican','Angel','Goomba','Cone','Avocado','Fluffy','Spices','Koala','Mami','Tractor_Shovel','Bear','Remote','Tree','Teepee','Dollars_container','Brain','Volcano','Toaster','Pigeon_2','Cola','iceberg','Wheelbarrow','Medieval_church','Ladybug','Maple','Willys','Scorpion','mouse_2'],1):
        data=convert(json.loads((args.source/f'{name}.json').read_text()),number);target=args.output/f'Level{number:02}.json';target.write_text(json.dumps(data,ensure_ascii=False,separators=(',',':'))+'\n');print(target,len(data['cubes']),len(data['hives']),data['columns'],hashlib.sha256(target.read_bytes()).hexdigest())
