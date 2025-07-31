AutoMaterial
Auto select material for building when open menu.

Total select rule:
1. Find the rule in spBuildData, if not find auto choose in sortData by metrail(Common/Decoration)
2. Exclude the disableMaterials, they will never be auto chosen.
3. Check the count of sortData.materials is not less than enoughMass(baseMass*buildCount) for sequnece.
4. If all not, select the max count of sortData.materials, no less than baseMass
5. If all not, select the max count of all valid materials exclude disableMaterials
6. If all not, use origin funcion. choose the first one or last one.

AutoMaterialConfig.json
{                       
    "debug" : true,     //Whether to log some info, such as buildId, chosenSortType, enoughMass, baseMass. see in C:/Users/Administrator/AppData/LocalLow/Klei/Oxygen Not Included/Player.log
    "ignoreCopyMaterial" : true,    //Whether to ignore the copy button and use auto material.
    "massToBuildCount" : [  //If baseMass for building not greater than mass, it need to build buildCount.  enoughMass = baseMass*buildCount
        { "mass": 5, "buildCount": 100 }, 
        { "mass": 25, "buildCount": 50 }, 
        ...
    ],
    "sortData" : {  //Defind the material rule, you can define new one and set spBuildData sort. For defualt is begin with Common, and with Decoration for this building of decoration>0
        "CommonRock": {     //most of build use rock.   decoration<=0
            "materials": ["IgneousRock", "Shale", ...],    //Check the material count is enough for enoughMass(baseMass*buildCount) for sequence.
            "disableMaterials" : ["Fossil", "Ceramic", ...]     // Nerver auto choose disableMaterials
        }, 
        "DecorationRock": {   //for the build use rock and decoration>0
            "materials": ["Granite", "SandStone", "IgneousRock", "Shale", "Obsidian", "SedimentaryRock", "WoodLog"],
            "disableMaterials" : ["Fossil", "Ceramic", "Graphite", "Isoresin", "SuperInsulator"]
        },
    },
    "spBuildData" : {   //Use special data for the build.
        "Ladder": { "sort":"CommonRock", "buildCount": 50 },    //sort is in sortData, default by materials and decoration.   buildCount is to over massToBuildCount, default to auto seek.
    }
}

总的选择材质的规则：
1. 在spBuildData里找建筑的材质选择规则，如果没找到根据材料类型在sortData里选择(Common/Decoration)
2. 在材质列表里排除disableMaterials里标出的
3. 按顺序判断materials里的材质数量是否大于等于enoughMass(baseMass*buildCount)
4. 如果没找到，查找materials里最多的且数量大于等于baseMass的材质
5. 如果还没找到，选择排除了disableMaterials后数量最多的材质
6. 如果都没找到，会使用官方的函数，一般是选择第一个或者是上一个使用的。


AutoMaterialConfig.json
{                       
    "debug" : true,     //是否打印一些数据，例如 buildId, chosenSortType, enoughMass, baseMass. 日志文件在 C:/Users/Administrator/AppData/LocalLow/Klei/Oxygen Not Included/Player.log
                                    
    "ignoreCopyMaterial" : true,    //是否忽略复制按钮的材质，并自动选择材质
                            
    "massToBuildCount" : [  //如果材料所需<=mass，则需要建筑buildCount个。 需要的总材料 enoughMass = baseMass*buildCount
        { "mass": 5, "buildCount": 100 }, 
        { "mass": 25, "buildCount": 50 }, 
        ...
    ],
                    
    "sortData" : {  //定义选择材质的规则，你可以写个新的，并在spBuildData里的sort标出。如果材质的装饰度<=0，会用Common开头的，否则用Decoration开头的
                            
        "CommonRock": {     //大部分使用常规石头（火成岩，花岗岩，砂岩等）的建筑，装饰度<=0
            "materials": ["IgneousRock", "Shale", ...],    //按顺序判断这些材质是否满足要求
            "disableMaterials" : ["Fossil", "Ceramic", ...]     //从不会自动选择的材质
        }, 
                              
        "DecorationRock": {   //使用常规石头（火成岩，花岗岩，砂岩等）的建筑，装饰度>0
            "materials": ["Granite", "SandStone", "IgneousRock", "Shale", "Obsidian", "SedimentaryRock", "WoodLog"],
            "disableMaterials" : ["Fossil", "Ceramic", "Graphite", "Isoresin", "SuperInsulator"]
        },
    },
                        
    "spBuildData" : {   //标记特殊的建筑的选择规则                                 
        "Ladder": { "sort":"CommonRock", "buildCount": 50 },    //sort在sortData里，默认根据装饰度和材质选择     buildCount是要建筑的数量，默认根据massToBuildCount来计算
    }
}


{    Rock						热导率	比热容	熔点		其他
Tag WoodLog			木材		0.22	2.3		600		隔热体，升温慢
Tag SandStone		砂岩		2.9		0.8		927		装饰度+10%
Tag Shale			页岩		1.8		0.25	927
Tag IgneousRock		火成岩		2		1		1410	过热+15°C，升温慢
Tag Granite			花岗岩		3.4		0.8		669		过热+15°C，装饰度+20%
Tag SedimentaryRock	沉积岩		2		0.2		927		热敏感
Tag Obsidian		黑曜石		2		0.2		2727	过热+15°C，热敏感
Tag Fossil			化石		2		0.91	1339
Tag Ceramic			陶瓷		0.62	0.84	1850	过热+200°C，装饰度+20%，隔热体
Tag Graphite		石墨		8		0.77	277		抗辐射
Tag MaficRock		镁铁质岩	1		0.2		1410	热敏感
Tag Isoresin		异构树液	0.17	1.3		200		隔热体，升温慢
Tag SuperInsulator	隔热质		0		5.57	3622	隔热体，升温慢
}

{	Ore
Tag Cuprite			铜矿		4.5		0.39	1084	装饰度+10%
Tag Electrum		银金矿		2		0.15	1064	热敏感
Tag AluminumOre		铝矿		20.5	0.91	1084	导热强
Tag Cinnabar		朱砂矿		4.5		0.39	583		
Tag FoolsGold		黄铁矿		4.5		0.39	1084	
Tag Cobaltite		钴矿		4		0.42	1495
Tag IronOre			铁矿		4		0.45	1535	
Tag GoldAmalgam		金汞齐		2		0.15	1064	过热+50°C，装饰度+10%，热敏感
Tag NickelOre		镍矿		3		0.45	1455	
Tag Wolframite		黑钨矿		15		0.13	2927
Tag Steel			钢			54		0.29	2427	过热+200°C，导热强
Tag Iridium			铱			170		0.13	2446	过热+500°C，装饰度+20%，导热强，热敏感，抗辐射
Tag UraniumOre		铀矿		20		1		133		导热强，升温慢
Tag Niobium			铌			54		0.27	2477	过热+500°C，装饰度+50%，导热强
Tag TempConductorSolid 导热质	220		0.62	2677	过热+900°C，导热强
}

{	Metal
Tag Copper			铜			60		0.38	1084	过热+50°C，装饰度+20%，导热强
Tag SolidMercury	固态汞		8.3		0.14	-38		357  冷却或升温液体	
Tag Lead			铅			35		0.13	328		过热-20°C，导热强，热敏感，抗辐射
Tag Cobalt			钴			100		0.42	1495	导热强
Tag Iron			铁			55		0.45	1535	过热+50°C，导热强
Tag Aluminum		铝			205		0.91	660		导热强
Tag Nickel			镍			91		0.44	1455	导热强
Tag Gold			金			60		0.13	1064	过热+50°C，装饰度+50%，导热强，热敏感，弱辐射
Tag Tungsten		钨			60		0.13	3422	过热+50°C，导热强，热敏感，弱辐射
Tag Steel			钢			54		0.49	2427	过热+200°C，导热强
Tag Iridium			铱			170		0.13	2446	过热+500°C，装饰度+20%，导热强，热敏感，抗辐射
Tag DepletedUranium	贫铀		20		1		133		导热强，升温慢，抗辐射
Tag Niobium			铌			54		0.27	2477	过热+500°C，装饰度+50%，导热强
Tag TempConductorSolid 导热质	220		0.62	2677	过热+900°C，导热强
}

{   Lucency
Tag Diamond			钻石		80		0.52	3927	过热+200°C，装饰度+100%，导热强，抗辐射
Tag Glass			玻璃		1.1		0.84	1727
Tag Amber			琥珀		0.17	1.3		95		隔热体，升温慢，高温熔化
Tag NaturalSolidResin 固态树脂	0.17	1.3		20		隔热体，升温慢
Tag SolidResin		固态树液	0.17	1.3		20		隔热体，升温慢
}

{
Tag Polypropylene	塑料		0.15	1.92	160		隔热体，升温慢，抗辐射
Tag HardPolypropylene 塑料质	0.25	1.5		1827	过热+900°C，隔热体，升温慢，抗辐射
Tag SolidViscoGel 固态粘性凝胶	0.45	1.55	-30.6	 480   冷却或升温液体
}

<link="LADDER">梯子</link> Ladder
<link="FIREPOLE">消防滑杆</link> FirePole
<link="TILE">砖块</link> Tile
<link="INSULATIONTILE">隔热砖</link> InsulationTile
<link="MESHTILE">网格砖</link> MeshTile
<link="GASPERMEABLEMEMBRANE">透气砖</link> GasPermeableMembrane
<link="METALTILE">金属砖</link> MetalTile
<link="DOOR">气动门</link> Door
<link="PRESSUREDOOR">机械气闸</link> PressureDoor
<link="MANUALPRESSUREDOOR">手动气闸</link> ManualPressureDoor
<link="STORAGELOCKER">存储箱</link> StorageLocker
<link="LIQUIDRESERVOIR">储液库</link> LiquidReservoir
<link="GASRESERVOIR">储气库</link> GasReservoir
<link="GENERATOR">煤炭发电机</link> Generator
<link="WOODGASGENERATOR">木材燃烧器</link> WoodGasGenerator
<link="HYDROGENGENERATOR">氢气发电机</link> HydrogenGenerator
<link="METHANEGENERATOR">天然气发电机</link> MethaneGenerator
<link="MANUALGENERATOR">人力发电机</link> ManualGenerator
<link="PETROLEUMGENERATOR">石油发电机</link> PetroleumGenerator
<link="STEAMTURBINE2">蒸汽涡轮机</link> SteamTurbine2
<link="STEAMTURBINE2">蒸汽涡轮机</link> SteamTurbine2
<link="SOLARPANEL">太阳能板</link> SolarPanel
<link="WIRE">电线</link> Wire
<link="WIREBRIDGE">电线桥</link> WireBridge
<link="HIGHWATTAGEWIRE">高负荷电线</link> HighWattageWire
<link="WIREREFINED">导线</link> WireRefined
<link="WIREREFINEDBRIDGE">导线桥</link> WireRefinedBridge
<link="WIREBRIDGEHIGHWATTAGE">高负荷电线接合板</link> WireBridgeHighWattage
<link="WIREREFINEDHIGHWATTAGE">高负荷导线</link> WireRefinedHighWattage
<link="WIREREFINEDBRIDGEHIGHWATTAGE">高负荷导线接合板</link> WireRefinedBridgeHighWattage
<link="BATTERY">蓄电池</link> Battery
<link="BATTERYSMART">智能蓄电池</link> BatterySmart
<link="BATTERYMEDIUM">巨型蓄电池</link> BatteryMedium
<link="POWERTRANSFORMERSMALL">变压器</link> PowerTransformerSmall
<link="POWERTRANSFORMER">大型变压器</link> PowerTransformer
<link="SWITCH">电闸</link> Switch
<link="LOGICPOWERRELAY">电力截断器</link> LogicPowerRelay
<link="LIQUIDPUMP">液泵</link> LiquidPump
<link="LIQUIDCONDUIT">液体管道</link> LiquidConduit
<link="INSULATEDLIQUIDCONDUIT">隔热液体管道</link> InsulatedLiquidConduit
<link="LIQUIDCONDUITRADIANT">导热液体管道</link> LiquidConduitRadiant
<link="LIQUIDVENT">排液口</link> LiquidVent
<link="LIQUIDCONDUITBRIDGE">液体管桥</link> LiquidConduitBridge
<link="LIQUIDCONDUITELEMENTSENSOR">液体管道元素传感器</link> LiquidConduitElementSensor
<link="LIQUIDCONDUITDISEASESENSOR">液体管道病菌传感器</link> LiquidConduitDiseaseSensor
<link="LIQUIDCONDUITDISEASESENSOR">液体管道病菌传感器</link> LiquidConduitDiseaseSensor
<link="LIQUIDCONDUITTEMPERATURESENSOR">液体管道温度传感器</link> LiquidConduitTemperatureSensor
<link="GASPUMP">气泵</link> GasPump
<link="GASFILTER">气体筛选器</link> GasFilter
<link="GASLOGICVALVE">气体截断阀</link> GasLogicValve
<link="GASVALVE">气体调节阀</link> GasValve
<link="GASCONDUIT">气体管道</link> GasConduit
<link="INSULATEDGASCONDUIT">隔热气体管道</link> InsulatedGasConduit
<link="GASCONDUITRADIANT">导热气体管道</link> GasConduitRadiant
<link="GASVENT">排气口</link> GasVent
<link="GASCONDUITBRIDGE">气体管桥</link> GasConduitBridge
<link="GASVENTHIGHPRESSURE">高压排气口</link> GasVentHighPressure
<link="GASVENTHIGHPRESSURE">高压排气口</link> GasVentHighPressure
<link="GASCONDUITELEMENTSENSOR">气体管道元素传感器</link> GasConduitElementSensor
<link="GASCONDUITDISEASESENSOR">气体管道病菌传感器</link> GasConduitDiseaseSensor
<link="GASCONDUITDISEASESENSOR">气体管道病菌传感器</link> GasConduitDiseaseSensor
<link="GASCONDUITTEMPERATURESENSOR">气体管道温度传感器</link> GasConduitTemperatureSensor
<link="METALREFINERY">金属精炼器</link> MetalRefinery
<link="KILN">窑炉</link> Kiln
<link="ROCKCRUSHER">碎石机</link> RockCrusher
<link="OILREFINERY">原油精炼器</link> OilRefinery
<link="POLYMERIZER">聚合物压塑器</link> Polymerizer
<link="GLASSFORGE">玻璃熔炉</link> GlassForge
<link="CHLORINATOR">漂白石料斗</link> Chlorinator
<link="OXYLITEREFINERY">氧石精炼炉</link> OxyliteRefinery
<link="OXYLITEREFINERY">氧石精炼炉</link> OxyliteRefinery
<link="BED">床铺</link> Bed
<link="CEILINGLIGHT">吸顶灯</link> CeilingLight
<link="FLOORLAMP">电灯</link> FloorLamp
<link="DININGTABLE">餐桌</link> DiningTable
<link="FLOWERVASEHANGINGFANCY">透明花盆</link> FlowerVaseHangingFancy
<link="SUITMARKER">气压服检查站</link> SuitMarker
<link="SUITLOCKER">气压服存放柜</link> SuitLocker
<link="OXYGENMASKMARKER">氧气面罩检查站</link> OxygenMaskMarker
<link="OXYGENMASKLOCKER">氧气面罩存放柜</link> OxygenMaskLocker
<link="OILWELLCAP">油井</link> OilWellCap
<link="LIQUIDHEATER">液体加热器</link> LiquidHeater
<link="LIQUIDCONDITIONER">液温调节器</link> LiquidConditioner
<link="AIRCONDITIONER">温度调节器</link> AirConditioner
<link="THERMALBLOCK">变温板</link> ThermalBlock
<link="LOGICWIRE">信号线</link> LogicWire
<link="LOGICWIREBRIDGE">信号线桥</link> LogicWireBridge
<link="LOGICWIRE">信号线</link> LogicWire
<link="LOGICWIREBRIDGE">信号线桥</link> LogicWireBridge
<link="LOGICRIBBON">信号线组</link> LogicRibbon
<link="LOGICRIBBON">信号线组桥</link> LogicRibbonBridge
<link="LOGICRIBBONREADER">线组读取器</link> LogicRibbonReader
<link="LOGICRIBBONWRITER">线组写入器</link> LogicRibbonWriter
<link="DUPLICANTSENSOR">复制人运动传感器</link> LogicDuplicantSensor
<link="FLOORSWITCH">压力板</link> FloorSwitch
<link="LOGICSWITCH">信号开关</link> LogicSwitch
<link="LOGICPRESSURESENSORGAS">气压传感器</link> LogicPressureSensorGas
<link="LOGICPRESSURESENSORLIQUID">液压传感器</link> LogicPressureSensorLiquid
<link="LOGICTEMPERATURESENSOR">温度传感器</link> LogicTemperatureSensor
<link="LOGICTIMEOFDAYSENSOR">周期传感器</link> LogicTimeOfDaySensor
<link="LOGICWATTSENSOR">功率传感器</link> LogicWattageSensor
<link="LOGICLIGHTSENSOR">光线传感器</link> LogicLightSensor
<link="LOGICLIGHTSENSOR">光线传感器</link> LogicLightSensor
<link="LOGICTIMERSENSOR">时间传感器</link> LogicTimerSensor
<link="LOGICDISEASESENSOR">病菌传感器</link> LogicDiseaseSensor
<link="LOGICDISEASESENSOR">病菌传感器</link> LogicDiseaseSensor
<link="LOGICELEMENTSENSORGAS">气体元素传感器</link> LogicElementSensorGas
<link="LOGICRADIATIONSENSOR">辐射传感器</link> LogicRadiationSensor
<link="LOGICCRITTERCOUNTSENSOR">小动物传感器</link> LogicCritterCountSensor
<link="LOGICELEMENTSENSORLIQUID">液体元素传感器</link> LogicElementSensorLiquid
<link="LOGICHEPSENSOR">辐射粒子传感器</link> LogicHEPSensor
<link="COMETDETECTOR">太空扫描仪</link> CometDetector
<link="LOGICCOUNTER">信号计数器</link> LogicCounter
<link="CHECKPOINT">复制人检查站</link> Checkpoint
<link="LOGIC">自动化通知器</link> LogicAlarm
<link="LOGICHAMMER">音槌</link> LogicHammer
<link="LOGICGATENOT">非门</link> LogicGateNOT
<link="LOGICINTERASTEROIDRECEIVER">信号接收器</link> LogicInterasteroidReceiver
<link="LOGICINTERASTEROIDSENDER">信号播报器</link> LogicInterasteroidSender
<link="LOGICGATEAND">与门</link> LogicGateAND
<link="LOGICGATEOR">或门</link> LogicGateOR
<link="LOGICGATEBUFFER">缓冲门</link> LogicGateBUFFER
<link="LOGICGATEFILTER">过滤门</link> LogicGateFILTER
<link="LOGICGATEXOR">异或门</link> LogicGateXOR
<link="LOGICMEMORY">锁存器</link> LogicMemory
<link="ALGAEDISTILLERY">信号分配器</link> LogicGateDemultiplexer
<link="LOGICGATEMULTIPLEXER">信号选择器</link> LogicGateMultiplexer
<link="SOLIDCONDUITDISEASESENSOR">运输轨道病菌传感器</link> SolidConduitDiseaseSensor
<link="SOLIDCONDUITDISEASESENSOR">运输轨道病菌传感器</link> SolidConduitDiseaseSensor
<link="SOLIDCONDUITTEMPERATURESENSOR">运输轨道温度传感器</link> SolidConduitTemperatureSensor
<link="SOLIDCONDUITELEMENTSENSOR">运输轨道元素传感器</link> SolidConduitElementSensor
<link="SOLIDTRANSFERARM">自动清扫器</link> SolidTransferArm
<link="SOLIDCONDUIT">运输轨道</link> SolidConduit
<link="SOLIDCONDUITBRIDGE">运输轨桥</link> SolidConduitBridge
<link="SOLIDCONDUITINBOX">运输装载器</link> SolidConduitInbox
<link="SOLIDVENT">轨道滑槽</link> SolidVent
<link="SOLIDFILTER">固体筛选器</link> SolidFilter
<link="SOLIDCONDUITOUTBOX">运输存放器</link> SolidConduitOutbox
<link="AUTOMINER">自动采矿机</link> AutoMiner
<link="SOLIDLOGICVALVE">轨道截断器</link> SolidLogicValve
<link="SOLIDLIMITVALVE">轨道计量器</link> SolidLimitValve