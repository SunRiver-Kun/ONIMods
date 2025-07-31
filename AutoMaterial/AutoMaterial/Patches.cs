using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KMod;
using TUNING;

namespace AutoMaterial
{
	public class AutoMaterialMod : UserMod2
	{
		public static AutoMaterialMod Ins;
		public bool debug = false;
		public bool ignoreCopyMaterial = true;
		public override void OnLoad(Harmony harmony)
		{

			Ins = this;
			JObject json = this.GetJson();
			if (json == null) { return; }

			debug = (bool)json["debug"];
			ignoreCopyMaterial = (bool)json["ignoreCopyMaterial"];

			MaterialSelectorPatches.Init(json);
			base.OnLoad(harmony);
		}

		public JObject GetJson()
		{
			JObject result = null;
			string path = Path.Combine(this.path, "AutoMaterialConfig.json");
			if (File.Exists(path))
			{
				string jsonString = File.ReadAllText(path);
				result = JsonConvert.DeserializeObject<JObject>(jsonString);
			}
			else
			{
				File.Create(path).Dispose();
				LogError("Create new file in path of " + path);
			}
			return result;
		}

		public static void LogError(string str)
		{
			Debug.Log("[AutoMaterial][ERROR]: " + str);
		}

		public static void Log(string str)
		{
			Debug.Log("[AutoMaterial]: " + str);
		}
	}

	public class SortData
	{
		public List<Tag> materials = new List<Tag>();
		public HashSet<Tag> disableMaterials = new HashSet<Tag>();
	}

	public class BuildData
	{
		public string sortType = "";
		public int buildCount = 0;
	}

	public class MaterialSelectorPatches
	{
		public static List<KeyValuePair<float, int>> MassToBuildCount = new List<KeyValuePair<float, int>>();
		public static Dictionary<string, SortData> SortData = new Dictionary<string, SortData>();
		public static Dictionary<Tag, BuildData> SpBuildData = new Dictionary<Tag, BuildData>();
		public static Dictionary<string, string> CategoryToSortPostName = new Dictionary<string, string>()
		{
			{"Metal", "Ore"},
			{"RefinedMetal", "Metal"},
			{"BuildableRaw", "Rock"},
			{"Plumbable", "Rock"},
			{"Transparent", "Transparent"},
		};

		public static void Init(JObject json)
		{
			try
			{
				foreach (var v in json["massToBuildCount"])
				{
					MassToBuildCount.Add(new KeyValuePair<float, int>((float)v["mass"], (int)v["buildCount"]));
				}
				MassToBuildCount.Sort((a, b) => a.Key.CompareTo(b.Key));

				foreach (JProperty prop in json["sortData"])
				{
					var v = prop.Value;
					var data = new SortData();
					SortData.Add(prop.Name, data);

					if (v["materials"] != null)
					{
						foreach (var mat in v["materials"])
						{
							data.materials.Add((string)mat);
						}
					}
					if (v["disableMaterials"] != null)
					{
						foreach (var mat in v["disableMaterials"])
						{
							data.disableMaterials.Add((string)mat);
						}
					}
				}

				foreach (JProperty prop in json["spBuildData"])
				{
					var v = prop.Value;
					var data = new BuildData();
					if(v["buildCount"] != null) { data.buildCount = (int)v["buildCount"]; }
					if(v["sort"] != null) { data.sortType = (string)v["sort"]; }
					SpBuildData.Add(prop.Name, data);
				}
			}
			catch (Exception e)
			{
				AutoMaterialMod.LogError("AutoMaterialConfig has some error. " + e.ToString());
				throw e;
			}
		}

		//------------------------------------------------------------------
		[HarmonyPatch(typeof(MaterialSelector), nameof(MaterialSelector.AutoSelectAvailableMaterial))]
		public class AutoSelectAvailableMaterial
		{
			static Dictionary<Tag, float> tempCount = new Dictionary<Tag, float>();

			private static void ClearMatCountCache()
			{
				tempCount.Clear();
			}

			private static float GetMatCount(Tag mat)
			{
				float count;
				if (!tempCount.TryGetValue(mat, out count))
				{
					count = ClusterManager.Instance.activeWorld.worldInventory.GetAmount(mat, true);
					tempCount.Add(mat, count);
				}
				return count;
			}

			public static Tag EnoughCheck(IEnumerable<Tag> materials, float mass)
			{
				if (materials == null) { return Tag.Invalid; }

				foreach (var mat in materials)
				{
					if (GetMatCount(mat) >= mass)
					{
						return mat;
					}
				}
				return Tag.Invalid;
			}

			public static Tag MaxCountCheck(IEnumerable<Tag> materials, float mass)
			{
				if (materials == null) { return Tag.Invalid; }

				Tag maxCountMat = Tag.Invalid;
				float maxCount = 0;
				foreach (var mat in materials)
				{
					var count = GetMatCount(mat);
					if (count > maxCount)
					{
						maxCount = count;
						maxCountMat = mat;
					}
				}
				return maxCount >= mass ? maxCountMat : Tag.Invalid;
			}

			public static void SelectMaterial(MaterialSelector inst, Recipe recipe, Tag tag)
			{
				UISounds.PlaySound(UISounds.Sound.Object_AutoSelected);

				var copyTag = MaterialSelectorPanelPatches.copyTag;
				if (copyTag.IsValid && copyTag != tag)
				{
					Element element = ElementLoader.GetElement(tag);
					string str;
					if (element == null)
					{
						GameObject prefab = Assets.GetPrefab(tag);
						str = prefab != null ? prefab.GetProperName() : tag.Name;
					}
					else
					{
						str = element.name;
					}

					PopFXManager.Instance.SpawnFX(
						PopFXManager.Instance.sprite_Resource,
						string.Format((string)STRINGS.MISC.POPFX.RESOURCE_SELECTION_CHANGED, str),
						null,
						Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos())
					);
				}

				inst.OnSelectMaterial(tag, recipe, true);
			}

			public static SortData TryGetSortData(Recipe recipe, out string sortType)
			{
				//Json
				sortType = "";
				SortData data = null;
				BuildData buildData = null;
				if (SpBuildData.TryGetValue(recipe.Result, out buildData) && SortData.TryGetValue(buildData.sortType, out data))
				{
					sortType = buildData.sortType;
					return data;
				}

				//Auto
				var buildDef = recipe.GetBuildingDef();
				if (!(buildDef != null && buildDef.MaterialCategory != null)) { return null; }

				foreach (var v in buildDef.MaterialCategory)
				{
					foreach (var cate in v.Split('&'))
					{
						if (CategoryToSortPostName.TryGetValue(cate, out sortType))
						{
							sortType = (buildDef.BaseDecor > 0 ? "Decoration" : "Common") + sortType;
							if (SortData.TryGetValue(sortType, out data)) { return data; }
						}
					}
				}
				return null;
			}

			public static float GetCompareMass(Recipe recipe, float mass)
			{
				BuildData buildData;
				if (SpBuildData.TryGetValue(recipe.Result, out buildData) && buildData.buildCount>0)
				{
					return mass * buildData.buildCount;
				}

				foreach (var kv in MassToBuildCount)
				{
					if (mass <= kv.Key)
					{
						return mass * kv.Value;
					}
				}
				return mass;
			}

			public static bool Prefix(MaterialSelector __instance, ref bool __result, Recipe ___activeRecipe, float ___activeMass)
			{
				if (___activeRecipe == null || __instance.ElementToggles.Count == 0) { return true; }

				if (AutoMaterialMod.Ins.debug)
				{
					AutoMaterialMod.Log("buildId, buildName: " + ___activeRecipe.Result + "\t" + ___activeRecipe.Name);
				}

				ClearMatCountCache();
				string sortId;
				SortData data = TryGetSortData(___activeRecipe, out sortId);
				if (data != null)
				{
					HashSet<Tag> materials = new HashSet<Tag>();
					foreach (var mat in __instance.ElementToggles.Keys)
					{
						if (!data.disableMaterials.Contains(mat))
						{
							materials.Add(mat);
						}
					}

					float mass = GetCompareMass(___activeRecipe, ___activeMass);
					var checkMaterials = data.materials.FindAll((v) => materials.Contains(v));
					Tag tag = EnoughCheck(checkMaterials, mass);
					if (!tag.IsValid) { tag = MaxCountCheck(checkMaterials, ___activeMass); }
					if (!tag.IsValid) { tag = MaxCountCheck(materials, 0); }

					if(AutoMaterialMod.Ins.debug) { AutoMaterialMod.Log($"SortId: {sortId}\tEnoughMass: {mass}\tBaseMass: {___activeMass}"); }
					if (tag.IsValid)
					{
						SelectMaterial(__instance, ___activeRecipe, tag);
						__result = true;
						return false;
					}
				}

				return true;
			}
		}
	}

	public class MaterialSelectorPanelPatches
	{
		public static Tag copyTag = Tag.Invalid;


		[HarmonyPatch(typeof(MaterialSelectionPanel), nameof(MaterialSelectionPanel.SelectSourcesMaterials))]
		public class SelectSourcesMaterials
		{
			public static bool Prefix(MaterialSelectionPanel __instance, Building building)
			{
				if (!AutoMaterialMod.Ins.ignoreCopyMaterial) { return true; }

				if (building == null)
				{
					copyTag = Tag.Invalid;
					return true;
				}

				PrimaryElement component1 = building.GetComponent<PrimaryElement>();
				CellSelectionObject component2 = building.GetComponent<CellSelectionObject>();

				if (component1 != null)
				{
					copyTag = component1.Element.tag;
				}
				else if (component2 != null)
				{
					copyTag = component2.element.tag;
				}
				else
				{
					copyTag = Tag.Invalid;
				}

				if (copyTag.IsValid)
				{
					__instance.AutoSelectAvailableMaterial();
					copyTag = Tag.Invalid;
					return false;
				}

				return true;
			}
		}
	}
}
