using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace UABEAvalonia
{
    public partial class ImportBatch : Window
    {
        private AssetWorkspace workspace;
        private string directory;
        private bool ignoreListEvents;

        public ImportBatch()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //generated events
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            boxMatchingFiles.SelectionChanged += BoxMatchingFiles_SelectionChanged;
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += BtnCancel_Click;

            ignoreListEvents = false;
        }

        public ImportBatch(AssetWorkspace workspace, List<AssetContainer> selection, string directory, List<string> extensions) : this()
        {
            this.workspace = workspace;
            this.directory = directory;

            bool anyExtension = extensions.Contains("*");

            List<string> filesInDir;
            if (!anyExtension)
                filesInDir = FileUtils.GetFilesInDirectory(directory, extensions);
            else
                filesInDir = Directory.GetFiles(directory).ToList();


            List<ImportBatchDataGridItem> gridItems = new List<ImportBatchDataGridItem>();

            foreach (AssetContainer cont in selection)
            {
                AssetNameUtils.GetDisplayNameFast(workspace, cont, true, out string assetName, out string _);
                ImportBatchDataGridItem gridItem = new ImportBatchDataGridItem()
                {
                    importInfo = new ImportBatchInfo()
                    {
                        assetName = assetName,
                        assetFile = Path.GetFileName(cont.FileInstance.path),
                        pathId = cont.PathId,
                        cont = cont
                    }
                };

                List<string> matchingFiles;

                if (!anyExtension)
                    matchingFiles = filesInDir
                        .Where(f => extensions.Any(x => f.EndsWith(gridItem.GetMatchName(x))))
                        .Select(f => Path.GetFileName(f)).ToList();
                else
                    matchingFiles = filesInDir
                        .Where(f => PathUtils.GetFilePathWithoutExtension(f).EndsWith(gridItem.GetMatchName("*")))
                        .Select(f => Path.GetFileName(f)).ToList();

                gridItem.matchingFiles = matchingFiles;
                gridItem.selectedIndex = matchingFiles.Count > 0 ? 0 : -1;
                if (gridItem.matchingFiles.Count > 0)
                {
                    gridItems.Add(gridItem);
                }
            }
            dataGrid.Items = gridItems;
        }

        public ImportBatch(AssetWorkspace workspace, List<AssetContainer> selection, string directory, List<string> extensions, string type) : this()
        {
            this.workspace = workspace;
            this.directory = directory;

            bool anyExtension = extensions.Contains("*");

            List<string> filesInDir;
            if (!anyExtension)
                filesInDir = FileUtils.GetFilesInDirectory(directory, extensions);
            else
                filesInDir = Directory.GetFiles(directory).ToList();


            List<ImportBatchDataGridItem> gridItems = new List<ImportBatchDataGridItem>();

            string[] ignores = {
                "resources.assets",
                "globalgamemanagers.assets",
                "sharedassets0.assets",
                "sharedassets1.assets",
                "sharedassets2.assets",
                "sharedassets3.assets",
                "sharedassets4.assets",
                "sharedassets5.assets",
                "sharedassets6.assets",
                "sharedassets7.assets",
                "sharedassets8.assets",
                "sharedassets9.assets",
                "sharedassets10.assets",
                "sharedassets11.assets",
                "sharedassets12.assets",
                "sharedassets13.assets",
            };
            Dictionary<int, string> tmpIgnores = new Dictionary<int, string>();
            for (int i = 0; i < filesInDir.Count; i++)
            {
                for (int j = 0; j < ignores.Length; j++)
                {
                    if (!filesInDir[i].Contains(ignores[j]))
                    {

                        tmpIgnores.Add(i, filesInDir[i]);
                        filesInDir[i] = null;
                        break;
                    }
                }
            }
            filesInDir.Sort((p1, p2) =>
            {
                if (p1 == null || p2 == null) return 0;
                string str1 = p1.Split("-")[0];
                string str2 = p2.Split("-")[0];
                char cn1 = p1.Split(".")[0][^1];
                char cn2 = p2.Split(".")[0][^1];
                int n1 = Convert.ToInt32(cn1);
                int n2 = Convert.ToInt32(cn2);
                int t1 = Convert.ToInt32(p1.Split("-")[^1].Split(".")[0]);
                int t2 = Convert.ToInt32(p2.Split("-")[^1].Split(".")[0]);

                if (n1 < n2) return -1;
                else if (n1 > n2) return 1;
                else
                {
                    if (t1 < t2) return -1;
                    else if (t1 > t2) return 1;
                    else return str1.CompareTo(str2);
                }
            });
            foreach (var item in tmpIgnores)
            {
                filesInDir[item.Key] = item.Value;
            }

            List<string> files = new List<string> { };
            filesInDir.ForEach(file => files.Add(file));
            if (type == "Phasmophobia")
            {
                string newPath;
                string[] str;
                string tmpstr;
                string cmpStr;
                foreach (AssetContainer cont in selection)
                {
                    AssetNameUtils.GetDisplayNameFast(workspace, cont, true, out string assetName, out string _);
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (files[i] == null) continue;
                        str = filesInDir[i].Split("\\");
                        tmpstr = str[^1];
                        if (tmpstr.Contains('-')) cmpStr = tmpstr.Split('-')[0];
                        else cmpStr = tmpstr.Split('.')[0];
                        if (cmpStr == assetName)
                        {
                            newPath = $"{String.Join("\\", str[0..^1])}\\{assetName}-{cont.FileInstance.name}-{cont.AssetId.pathID}.png";
                            Console.WriteLine(filesInDir[i], newPath);
                            File.Move(filesInDir[i], newPath);
                            filesInDir[i] = newPath;
                            files[i] = null;
                            break;
                        }
                    }
                }
            }

            foreach (AssetContainer cont in selection)
            {
                AssetNameUtils.GetDisplayNameFast(workspace, cont, true, out string assetName, out string _);
                ImportBatchDataGridItem gridItem = new ImportBatchDataGridItem()
                {
                    importInfo = new ImportBatchInfo()
                    {
                        assetName = assetName,
                        assetFile = Path.GetFileName(cont.FileInstance.path),
                        pathId = cont.PathId,
                        cont = cont
                    }
                };

                List<string> matchingFiles;

                if (!anyExtension)
                    matchingFiles = filesInDir
                        .Where(f => extensions.Any(x => f.EndsWith(gridItem.GetMatchName(x))))
                        .Select(f => Path.GetFileName(f)).ToList();
                else
                    matchingFiles = filesInDir
                        .Where(f => PathUtils.GetFilePathWithoutExtension(f).EndsWith(gridItem.GetMatchName("*")))
                        .Select(f => Path.GetFileName(f)).ToList();

                gridItem.matchingFiles = matchingFiles;
                gridItem.selectedIndex = matchingFiles.Count > 0 ? 0 : -1;
                if (gridItem.matchingFiles.Count > 0)
                {
                    gridItems.Add(gridItem);
                }
            }
            dataGrid.Items = gridItems;
        }

        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null && dataGrid.SelectedItem is ImportBatchDataGridItem gridItem)
            {
                boxMatchingFiles.Items = gridItem.matchingFiles;
                if (gridItem.selectedIndex != -1)
                {
                    //there's gotta be a better way to do this .-. oh well
                    ignoreListEvents = true;
                    boxMatchingFiles.SelectedIndex = gridItem.selectedIndex;
                    ignoreListEvents = false;
                }
            }
        }

        private void BoxMatchingFiles_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null && dataGrid.SelectedItem is ImportBatchDataGridItem gridItem)
            {
                if (boxMatchingFiles.SelectedIndex != -1 && !ignoreListEvents)
                    gridItem.selectedIndex = boxMatchingFiles.SelectedIndex;
            }
        }

        private void BtnOk_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            List<ImportBatchInfo> importInfos = new List<ImportBatchInfo>();
            foreach (ImportBatchDataGridItem gridItem in dataGrid.Items)
            {
                if (gridItem.selectedIndex != -1)
                {
                    ImportBatchInfo importInfo = gridItem.importInfo;
                    importInfo.importFile = Path.Combine(directory, gridItem.matchingFiles[gridItem.selectedIndex]);
                    importInfos.Add(importInfo);
                }
            }

            Close(importInfos);
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(null);
        }
    }

    public class ImportBatchInfo
    {
        public AssetContainer cont;
        public string importFile;
        public string assetName;
        public string assetFile;
        public long pathId;
    }

    public class ImportBatchDataGridItem : INotifyPropertyChanged
    {
        public ImportBatchInfo importInfo;

        public string Description { get => importInfo.assetName; }
        public string File { get => importInfo.assetFile; }
        public long PathID { get => importInfo.pathId; }

        public List<string> matchingFiles;
        public int selectedIndex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string GetMatchName(string ext)
        {
            if (ext != "*")
                return $"-{File}-{PathID}.{ext}";
            else
                return $"-{File}-{PathID}";
        }
        public void Update(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
