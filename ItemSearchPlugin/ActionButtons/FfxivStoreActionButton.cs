using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace ItemSearchPlugin.ActionButtons {
    public class FfxivStoreActionButton : IActionButton {

        private static ItemSearchPluginConfig _pluginConfig;
        public static Dictionary<uint, List<uint>> StoreItems = new();
        private static Dictionary<uint, Product> StoreProducts = new();

        private static Task _storeUpdateTask;
        private static CancellationTokenSource _updateCancellationToken;

        public static string UpdateStatus = string.Empty;

        public static void BeginUpdate() {
            if (_storeUpdateTask != null) {
                if (!_storeUpdateTask.IsCompleted) {
                    _updateCancellationToken.Cancel();
                    _storeUpdateTask.Wait();
                }
                _storeUpdateTask = null;
                _updateCancellationToken = null;
            }

            if (!_pluginConfig.EnableFFXIVStore) return;
            _updateCancellationToken = new CancellationTokenSource();
            _storeUpdateTask = Task.Run(UpdateTask, _updateCancellationToken.Token);
        }

        public class ProductItem {
            [JsonProperty("name")]
            public string Name;

            [JsonProperty("number")]
            public int Number;
        }

        public class Product {
            [JsonProperty("id")]
            public uint ID;

            [JsonProperty("name")]
            public string Name;

            [JsonProperty("priceText")]
            public string PriceText;

            [JsonProperty("items")]
            public List<ProductItem> Items;
        }

        public class ProductListing {
            [JsonProperty("status")]
            public int Status;

            [JsonProperty("product")]
            public Product Product;
        }

        public class ProductList {
            [JsonProperty("status")]
            public int Status;

            [JsonProperty("products")]
            public List<Product> Products;
        }


        private static void UpdateTask() {
            UpdateStatus = "Fetching Product List";
            using var wc = new WebClient();
            var json = wc.DownloadString("https://api.store.finalfantasyxiv.com/ffxivcatalog/api/products/?lang=en-us&currency=USD&limit=10000");
            if (_updateCancellationToken.IsCancellationRequested) {
                UpdateStatus = "[Cancelled] " + UpdateStatus;;
                return;
            }
            var productList = JsonConvert.DeserializeObject<ProductList>(json);
            if (productList == null) {
                UpdateStatus = "[Error] " + UpdateStatus;;
                return;
            }


            PluginLog.Debug($"Got Store Product List: {productList.Products.Count}");
            StoreItems.Clear();

            var storeProductCacheDirectory = Path.Combine(PluginInterface.GetPluginConfigDirectory(), "FFXIV Store Cache");
            Directory.CreateDirectory(storeProductCacheDirectory);

            var allItems = Data.Excel.GetSheet<Item>(Language.English);
            if (allItems == null) {
                UpdateStatus = "[Error] " + UpdateStatus;;
                return;
            }
            StoreProducts.Clear();
            for (var i = 0; i < productList.Products.Count; i++) {
                var p = productList.Products[i];
                try {
                    if (_updateCancellationToken.IsCancellationRequested) {
                        UpdateStatus = "[Cancelled] " + UpdateStatus;;
                        return;
                    }

                    string fullProductJson = null;

                    var cacheFile = Path.Combine(storeProductCacheDirectory, $"{p.ID}.json");
                    var usingCache = false;
                    if (File.Exists(cacheFile)) {
                        UpdateStatus = $"Fetching Store Items: {i}/{productList.Products.Count} [{p.ID}, Cached]";

                        usingCache = true;
                        fullProductJson = File.ReadAllText(cacheFile);
                    } else {
                        UpdateStatus = $"Fetching Store Items: {i}/{productList.Products.Count} [{p.ID}, from Store]";
                        fullProductJson = wc.DownloadString($"https://api.store.finalfantasyxiv.com/ffxivcatalog/api/products/{p.ID}?lang=en-us&currency=USD");
                    }

                    if (_updateCancellationToken.IsCancellationRequested) {
                        UpdateStatus = "[Cancelled] " + UpdateStatus;
                        return;
                    }
                    var productListing = JsonConvert.DeserializeObject<ProductListing>(fullProductJson);
                    if (productListing?.Product == null) continue;
                    if (productListing.Product.Items == null) {
                        PluginLog.Debug($"{p.Name} has no Items?");
                    } else {
                        StoreProducts.Add(p.ID, productListing.Product);

                        foreach (var item in productListing.Product.Items) {
                            var matchingItems = allItems.Where(i => i.Name.RawString == item.Name).ToList();
                            if (matchingItems.Count == 0) {
                                PluginLog.Debug($"Failed to find matching item for {item.Name}.");
                                continue;
                            }

                            if (matchingItems.Count > 1) {
                                PluginLog.Debug($"Found multiple matching items for {item.Name}.");
                            }

                            foreach (var matchedItem in matchingItems) {
                                if (!StoreItems.ContainsKey(matchedItem.RowId)) {
                                    StoreItems.Add(matchedItem.RowId, new List<uint>());
                                }

                                if (!StoreItems[matchedItem.RowId].Contains(p.ID)) {
                                    StoreItems[matchedItem.RowId].Add(p.ID);
                                }
                            }
                        }

                        if (!usingCache) {
                            PluginLog.Debug($"Cached Product Info: {p.ID}");
                            File.WriteAllText(cacheFile, fullProductJson);
                            Task.Delay(500).Wait();
                        }
                    }
                } catch (Exception ex) {
                    UpdateStatus = "[Error] " + UpdateStatus;
                    PluginLog.Error(ex, "Error in Update Task");
                    return;
                }

                UpdateStatus = string.Empty;
            }
        }


        public FfxivStoreActionButton(ItemSearchPluginConfig pluginConfig) {
            _pluginConfig = pluginConfig;
            if (StoreItems.Count <= 0) BeginUpdate();
        }

        public override string GetButtonText(Item selectedItem) {
            return "FFXIV Store";
        }

        private void DrawProductSelection() {
            var isPopupOpen = true;
            ImGui.SetNextWindowPos(productSelectionPosition, ImGuiCond.Always);
            if (ImGui.Begin("Select Store Product", ref isPopupOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.Popup)) {


                foreach (var p in productSelectionList) {
                    var product = StoreProducts[p];

                    if (ImGui.Selectable($"[{product.PriceText}] {product.Name}")) {
                        Process.Start(new ProcessStartInfo { FileName = $"https://store.finalfantasyxiv.com/ffxivstore/en-us/product/{product.ID}", UseShellExecute = true });
                        isPopupOpen = false;
                    }
                }
                if (!ImGui.IsWindowFocused()) {
                    isPopupOpen = false;
                }

                ImGui.End();
            }

            if (!isPopupOpen) PluginInterface.UiBuilder.Draw -= DrawProductSelection;

        }


        private List<uint> productSelectionList = null;
        private Vector2 productSelectionPosition;
        private Vector2 productSelectionButtonSize = Vector2.Zero;


        public override void OnButtonClicked(Item selectedItem) {
            if (!StoreItems.ContainsKey(selectedItem.RowId)) return;

            var storeItems = StoreItems[selectedItem.RowId];
            if (storeItems.Count <= 0) return;

            productSelectionList = storeItems;
            productSelectionPosition = ImGui.GetIO().MousePos;
            productSelectionButtonSize = Vector2.Zero;
            PluginInterface.UiBuilder.Draw -= DrawProductSelection;
            PluginInterface.UiBuilder.Draw += DrawProductSelection;
        }

        public override bool GetShowButton(Item selectedItem) {
            if (!_pluginConfig.EnableFFXIVStore) return false;
            return StoreItems.ContainsKey(selectedItem.RowId);
        }

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() {
            PluginInterface.UiBuilder.Draw -= DrawProductSelection;
            if (_storeUpdateTask != null) {
                _updateCancellationToken.Cancel();
                _storeUpdateTask.Wait();

                _storeUpdateTask = null;
                _updateCancellationToken = null;
            }
        }
    }
}
