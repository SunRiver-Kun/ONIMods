using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KMod;

namespace AutoMaterial {
	public class _M : UserMod2 {
		public static bool debug = false;
		public static bool ignoreCopyMaterial = true;
		public static bool isOriginCopy = false;
		public static HashSet<Tag> copyTags = new HashSet<Tag>();
		public static int curHeadIndex = 0;
		public static List<string> sortHeads = new List<string>();
		public static string curSortHead => sortHeads[curHeadIndex];
		public static Dictionary<string, List<string>> hotKeys = new Dictionary<string, List<string>>();
		public static void AddToCopyTags(Tag tag) {
			if (!tag.IsValid) { return; }
			copyTags.Add(tag);
		}

		public static bool isHotKeyDown(string name) {
			List<string> keys;
			if (!hotKeys.TryGetValue(name, out keys) || keys.Count<=0) { return false; }

			foreach (string k in keys) {
				bool isKeepKey = k.IndexOf("shift") >= 0 || k.IndexOf("ctrl") >= 0 || k.IndexOf("alt") >= 0;
				bool isInput = isKeepKey ? Input.GetKey(k) : Input.GetKeyDown(k);
				if(!isInput) { return false; }
			}
			return true;
		}

		public override void OnLoad(Harmony harmony) {
			JObject json = this.GetJson();
			if (json == null) { return; }

			debug = (bool)json["debug"];
			ignoreCopyMaterial = (bool)json["ignoreCopyMaterial"];

			foreach (var v in json["sortHeads"]) { sortHeads.Add((string)v); }
			if (sortHeads.Count == 0) { sortHeads.Add("Common"); }

			foreach (JProperty prop in json["hotKeys"]) {
				List<string> keys = new List<string>();
				foreach (var k in prop.Value) {
					keys.Add((string)k);
				}
				hotKeys.Add(prop.Name, keys);
			}


			MaterialSelectorPatches.Init(json);
			base.OnLoad(harmony);
		}

		public JObject GetJson() {
			JObject result = null;
			string path = Path.Combine(this.path, "AutoMaterialConfig.json");
			if (File.Exists(path)) {
				string jsonString = File.ReadAllText(path);
				result = JsonConvert.DeserializeObject<JObject>(jsonString);
			} else {
				File.Create(path).Dispose();
				LogError("Create new file in path of " + path);
			}
			return result;
		}

		public static void LogError(string str) {
			Debug.Log("[AutoMaterial][ERROR]: " + str);
		}

		public static void Log(string str) {
			Debug.Log("[AutoMaterial]: " + str);
		}

		public static void ShowTip(string str) {
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, str, null, Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
		}
	}

	#region 选择相关
	public class SortData {
		public List<Tag> materials = new List<Tag>();
		public HashSet<Tag> disableMaterials = new HashSet<Tag>();

	}

	public class BuildData {
		public string sortType = "";
		public int buildCount = 0;
		public string subCheck = "";
	}

	public class MaterialSelectorPatches {
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

		public static void Init(JObject json) {
			try {
				foreach (var v in json["massToBuildCount"]) {
					MassToBuildCount.Add(new KeyValuePair<float, int>((float)v["mass"], (int)v["buildCount"]));
				}
				MassToBuildCount.Sort((a, b) => a.Key.CompareTo(b.Key));

				foreach (JProperty prop in json["sortData"]) {
					var v = prop.Value;
					var data = new SortData();
					SortData.Add(prop.Name, data);

					if (v["materials"] != null) {
						foreach (var mat in v["materials"]) {
							data.materials.Add((string)mat);
						}
					}
					if (v["disableMaterials"] != null) {
						foreach (var mat in v["disableMaterials"]) {
							data.disableMaterials.Add((string)mat);
						}
					}
				}

				foreach (JProperty prop in json["spBuildData"]) {
					var v = prop.Value;
					var data = new BuildData();
					if (v["buildCount"] != null) { data.buildCount = (int)v["buildCount"]; }
					if (v["sort"] != null) { data.sortType = (string)v["sort"]; }
					if (v["subCheck"] != null) { data.subCheck = (string)v["subCheck"]; }
					SpBuildData.Add(prop.Name, data);
				}
			} catch (Exception e) {
				_M.LogError("AutoMaterialConfig has some error. " + e.ToString());
				throw e;
			}
		}

		//------------------------------------------------------------------
		[HarmonyPatch(typeof(MaterialSelector), nameof(MaterialSelector.AutoSelectAvailableMaterial))]
		public class AutoSelectAvailableMaterial {
			const float SubMassScale = 4;

			static Dictionary<Tag, float> tempCount = new Dictionary<Tag, float>();

			private static void ClearMatCountCache() {
				tempCount.Clear();
			}

			private static float GetMatCount(Tag mat) {
				float count;
				if (!tempCount.TryGetValue(mat, out count)) {
					count = ClusterManager.Instance.activeWorld.worldInventory.GetAmount(mat, true);
					tempCount.Add(mat, count);
				}
				return count;
			}

			public static Tag EnoughCheck(IEnumerable<Tag> materials, float mass) {
				if (materials == null) { return Tag.Invalid; }

				foreach (var mat in materials) {
					if (GetMatCount(mat) >= mass) {
						return mat;
					}
				}
				return Tag.Invalid;
			}

			public static Tag MaxCountCheck(IEnumerable<Tag> materials, float mass) {
				if (materials == null) { return Tag.Invalid; }

				Tag maxCountMat = Tag.Invalid;
				float maxCount = 0;
				foreach (var mat in materials) {
					var count = GetMatCount(mat);
					if (count > maxCount) {
						maxCount = count;
						maxCountMat = mat;
					}
				}
				return maxCount >= mass ? maxCountMat : Tag.Invalid;
			}

			public static void SelectMaterial(MaterialSelector inst, Recipe recipe, Tag tag) {
				UISounds.PlaySound(UISounds.Sound.Object_AutoSelected);

				if (_M.copyTags.Count > 0 && !_M.copyTags.Contains(tag)) {
					Element element = ElementLoader.GetElement(tag);
					string str;
					if (element == null) {
						GameObject prefab = Assets.GetPrefab(tag);
						str = prefab != null ? prefab.GetProperName() : tag.Name;
					} else {
						str = element.name;
					}
					_M.ShowTip(string.Format((string)STRINGS.MISC.POPFX.RESOURCE_SELECTION_CHANGED, str));
				}

				inst.OnSelectMaterial(tag, recipe, true);
			}

			public static SortData TryGetSortData(Recipe recipe, out string sortType) {
				//Json
				sortType = "";
				SortData data = null;
				BuildData buildData = null;
				if (SpBuildData.TryGetValue(recipe.Result, out buildData) && SortData.TryGetValue(buildData.sortType, out data)) {
					sortType = buildData.sortType;
					return data;
				}

				//Auto
				var buildDef = recipe.GetBuildingDef();
				if (!(buildDef != null && buildDef.MaterialCategory != null)) { return null; }

				string head = _M.curSortHead;
				string commonHead = buildDef.BaseDecor > 0 ? "Decoration" : "Common";
				if (head == "Common") { head = commonHead; }

				string postName = "";
				foreach (var v in buildDef.MaterialCategory) {
					foreach (var cate in v.Split('&')) {
						if (CategoryToSortPostName.TryGetValue(cate, out postName)) {
							sortType = head + postName;
							if (SortData.TryGetValue(sortType, out data)) { return data; }

							sortType = commonHead + postName;
							if (SortData.TryGetValue(sortType, out data)) { }
						}
					}
				}
				return data;
			}

			public static float GetCompareMass(Recipe recipe, float mass) {
				BuildData buildData;
				if (SpBuildData.TryGetValue(recipe.Result, out buildData) && buildData.buildCount > 0) {
					return mass * buildData.buildCount;
				}

				foreach (var kv in MassToBuildCount) {
					if (mass <= kv.Key) {
						return mass * kv.Value;
					}
				}
				return mass;
			}

			public static string GetSubCheck(Recipe recipe) {
				BuildData buildData;
				if (SpBuildData.TryGetValue(recipe.Result, out buildData)) {
					return buildData.subCheck != "" ? buildData.subCheck : "EnoughCheck";
				}

				return "MaxCountCheck";
			}

			public static bool Prefix(MaterialSelector __instance, ref bool __result, Recipe ___activeRecipe, float ___activeMass) {
				if (___activeRecipe == null || __instance.ElementToggles.Count == 0) { return true; }
				if (_M.debug) { _M.Log("buildId, buildName: " + ___activeRecipe.Result + "\t" + ___activeRecipe.Name); }

				ClearMatCountCache();
				string sortId;
				SortData data = TryGetSortData(___activeRecipe, out sortId);
				if (data != null) {
					HashSet<Tag> materials = new HashSet<Tag>();
					foreach (var mat in __instance.ElementToggles.Keys) {
						if (!data.disableMaterials.Contains(mat)) {
							materials.Add(mat);
						}
					}

					float mass = GetCompareMass(___activeRecipe, ___activeMass);
					var checkMaterials = data.materials.FindAll((v) => materials.Contains(v));
					Tag tag = EnoughCheck(checkMaterials, mass);
					if (!tag.IsValid) {
						float subMass = Math.Min(mass, SubMassScale * ___activeMass);
						string subCheck = GetSubCheck(___activeRecipe);
						if (subCheck == "EnoughCheck") {
							tag = EnoughCheck(checkMaterials, subMass);
						} else {
							tag = MaxCountCheck(checkMaterials, subMass);
						}
					}
					if (!tag.IsValid) { tag = MaxCountCheck(materials, 0); }

					if (_M.debug) { _M.Log($"SortId: {sortId}\tEnoughMass: {mass}\tBaseMass: {___activeMass}\tsubCheck: {GetSubCheck(___activeRecipe)}"); }
					if (tag.IsValid) {
						SelectMaterial(__instance, ___activeRecipe, tag);
						__result = true;
						return false;
					}
				} else {
					if (_M.debug) { _M.Log($"not find sortType {___activeRecipe.Name}, {_M.curSortHead};"); }
				}

				return true;
			}
		}
	}
	#endregion 选择相关
	public class MaterialSelectorPanelPatches {

		[HarmonyPatch(typeof(MaterialSelectionPanel), nameof(MaterialSelectionPanel.SelectSourcesMaterials))]
		public class SelectSourcesMaterials {
			public static bool Prefix(MaterialSelectionPanel __instance, Building building) {
				if (!_M.ignoreCopyMaterial) { return true; }
				_M.copyTags.Clear();

				if (building == null) { return true; }
				if (_M.isOriginCopy) {
					_M.ShowTip("Copy");
					return true;
				}

				PrimaryElement component1 = building.GetComponent<PrimaryElement>();
				CellSelectionObject component2 = building.GetComponent<CellSelectionObject>();
				Constructable component3 = building.GetComponent<Constructable>();
				Deconstructable component4 = building.GetComponent<Deconstructable>();

				if (component1 != null) { _M.AddToCopyTags(component1.Element.tag); }
				if (component2 != null) { _M.AddToCopyTags(component2.element.tag); }
				if (component3 != null && component3.SelectedElementsTags != null) {
					foreach (var v in component3.SelectedElementsTags) {
						_M.AddToCopyTags(v);
					}
				}
				if (component4 != null && component4.constructionElements != null) {
					foreach (var v in component4.constructionElements) {
						_M.AddToCopyTags(v);
					}
				}

				if (_M.copyTags.Count > 0) {
					__instance.AutoSelectAvailableMaterial();
					_M.copyTags.Clear();
					return false;
				}

				return true;
			}
		}
	}

	#region 按钮相关

	public class PlanScreenPathes {
		[HarmonyPatch(typeof(PlanScreen), nameof(PlanScreen.ScreenUpdate))]
		public class OnKeyDown {
			static void Postfix(PlanScreen __instance) {
				if (_M.isHotKeyDown("Copy")) {
					var method = typeof(PlanScreen).GetMethod("OnClickCopyBuilding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					if (method != null) {
						_M.isOriginCopy = true;
						method.Invoke(__instance, null);
						_M.isOriginCopy = false;
					}
				} else if (_M.isHotKeyDown("HeadUp")) {
					_M.curHeadIndex = Math.Min(_M.curHeadIndex + 1, _M.sortHeads.Count - 1);
					_M.ShowTip("AutoMaterial " + _M.curSortHead);
				} else if (_M.isHotKeyDown("HeadDown")) {
					_M.curHeadIndex = Math.Max(_M.curHeadIndex - 1, 0);
					_M.ShowTip("AutoMaterial " + _M.curSortHead);
				}
			}
		}
	}
	#endregion 按钮相关
}
