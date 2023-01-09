/*
 * Copyright (c) 2021 Alisha Taylor
 * Open source software. Licensed under the MIT license: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;

namespace Predictive
{
    public partial class MainWindow : Window
    {
        #region Initialization

        private static readonly string s_appDisplayName;
        private static readonly string s_appVersion;
        internal static readonly string s_appDataDirPath;
        private static readonly string s_historicalDataFilePath;
        private static readonly string s_notifyDataFilePath;
        private static readonly string s_learnDataFilePath;
        private static readonly string s_configFilePath;
        private static readonly string s_errorFilePath;
        private static readonly List<string[]> s_historicalDataList = new();
        private static readonly List<string> s_notifyDataList = new();
        private static List<LearnItem> s_learnDataList = new();
        private static string s_learnDataFileComment;
        private static int s_maxTags = 10;
        private static readonly SolidColorBrush _suggestButtonHighlight = Brushes.LightGreen;
        private static readonly SolidColorBrush _exportButtonHighlight = Brushes.LightGoldenrodYellow;
        private static DragDropEffects _dragDropEffects = DragDropEffects.None;

        public MainWindow()
        {
            // UI initialization.
            InitializeComponent();

            // Defaults.
            ShowInTaskbar = true;
            Title = $"{s_appDisplayName} v{s_appVersion}";
            AllowDrop = true;
            dgvSuggests.AllowDrop = true;
            BtnTagPreset1.Visibility = Visibility.Hidden;
            BtnTagPreset2.Visibility = Visibility.Hidden;
            BtnTagPreset3.Visibility = Visibility.Hidden;
            BtnTagPreset4.Visibility = Visibility.Hidden;
            BtnTagPreset5.Visibility = Visibility.Hidden;

            // Events.
            Loaded += TagUi_Loaded;
            Closed += MainWindow_Closed;
            btnSuggest.Click += BtnSuggest_Click;
            btnAppFolder.Click += BtnAppFolder_Click;
            btnExport.Click += BtnExport_Click;
            btnClear.Click += BtnClear_Click;
            dgvSuggests.DragEnter += DgvSuggest_DragEnter;
            dgvSuggests.Drop += DgvSuggest_DragDrop;
            dgvSuggests.DragLeave += DgvSuggest_DragLeave;
            dgvSuggests.DragOver += DgvSuggest_DragOver;
            dgvTags.SelectionChanged += DgvTags_SelectionChanged;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        static MainWindow()
        {
            s_appDisplayName = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            s_appVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            string appAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            s_appDataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appAssemblyName);
            s_historicalDataFilePath = Path.Combine(s_appDataDirPath, "historical-data.txt");
            s_learnDataFilePath = Path.Combine(s_appDataDirPath, "learn-data.xml");
            s_notifyDataFilePath = Path.Combine(s_appDataDirPath, "notify-data.txt");
            s_configFilePath = Path.Combine(s_appDataDirPath, "config.xml");
            s_errorFilePath = Path.Combine(s_appDataDirPath, "error.txt");
        }

        #endregion

        #region Window events

        private async void TagUi_Loaded(object sender, EventArgs e)
        {
            // Create app folder if it doesn't exist.
            Directory.CreateDirectory(s_appDataDirPath);

            ShowUiBusy(isBusy: true, message: "Initializing...");

            // Load data files starting with the fastest loading.
            await LoadConfigFileAsync();
            await LoadNotifyDataFileAsync();
            await LoadLearnDataFileAsync();
            await LoadHistoricalDataFileAsync();    // can takes several seconds to load.

            // Initialize view models.
            InitializeDgvTags();
            InitializeDgvSuggest();

            ShowUiBusy(isBusy: false, message: "");
        }

        #endregion

        #region TagViewModel

        public class TagViewModel
        {
            public ObservableCollection<Tag> TagList { get; } = new();
        }

        public new class Tag : INotifyPropertyChanged
        {
            private string text = "";
            private SolidColorBrush shade = Brushes.White;
            public event PropertyChangedEventHandler PropertyChanged;

            public Tag(string text)
            {
                Text = text;
            }

            public string Text
            {
                get => text;
                set
                {
                    if (text.EqualsXx(value))
                    {
                        return;
                    }

                    text = value;
                    OnPropertyChanged();

                    // Wipe cached suggestions if value changes.
                    SuggestCache = null;
                }
            }

            public SolidColorBrush Shade
            {
                get => shade;
                set
                {
                    if (shade.Color == value.Color)
                    {
                        return;
                    }

                    shade = value;
                    OnPropertyChanged();
                }
            }

            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public List<SuggestItem> SuggestCache { get; set; }
        }

        #endregion

        #region SuggestViewModel

        public class SuggestViewModel
        {
            public ObservableCollection<SuggestItem> SuggestList { get; } = new();
        }

        public class SuggestItem : INotifyPropertyChanged
        {
            private string text = "";
            private SolidColorBrush shade;
            public event PropertyChangedEventHandler PropertyChanged;
            public bool IsPromoted;

            public SuggestItem(string text)
            {
                Text = text;
            }

            public SuggestItem(string text, SolidColorBrush shade)
            {
                Text = text;
                Shade = shade;
            }

            public SuggestItem(string text, SolidColorBrush shade, bool isPromoted)
            {
                Text = text;
                Shade = shade;
                IsPromoted = isPromoted;
            }

            public string Text
            {
                get => text;
                set
                {
                    if (text.EqualsXx(value))
                    {
                        return;
                    }

                    text = value;
                    OnPropertyChanged();
                }
            }

            public SolidColorBrush Shade
            {
                get => shade;
                set => shade = value;
            }

            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public SuggestItem Clone()
            {
                return new SuggestItem(Text, Shade, IsPromoted);
            }
        }

        #endregion

        #region Tag DataGrid

        private void InitializeDgvTags()
        {
            // Binding prereq.
            dgvTags.DataContext = new TagViewModel();

            // Generate blank rows to allocate number of tags supported.
            for (int i = 0; i < s_maxTags; i++)
            {
                ((TagViewModel)dgvTags.DataContext).TagList.Add(new Tag(""));
            }
        }

        private void DgvTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Clear suggestions when starting to type afresh.
                ClearSuggests(clearSearch: true);

                Tag dgvTag = GetSelectedTag();
                int selIndex = dgvTags.SelectedIndex;
                if (selIndex == -1) return;

                // Set background.
                for (int i = 0; i < ((TagViewModel)dgvTags.DataContext).TagList.Count; i++)
                {
                    ((TagViewModel)dgvTags.DataContext).TagList[i].Shade = selIndex == i ? Brushes.Gold : Brushes.White;
                }

                // Check for empty cell.
                if (dgvTag.Text.IsEmpty())
                {
                    btnSuggest.Background = default;
                    return;
                }

                // Handle suggestion cache.
                if (dgvTag.SuggestCache != null && dgvTag.SuggestCache.Count > 0)
                {
                    string[] skipTags = new[] { dgvTag.Text }.Concat(s_notifyDataList).ToArray();

                    // Update suggestion listbox.
                    foreach (SuggestItem suggestion in dgvTag.SuggestCache)
                    {
                        if (suggestion.Text.EqualsAnyXx(skipTags))
                        {
                            continue;
                        }

                        ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Add(suggestion);
                    }
                    btnSuggest.Background = default;
                }
                else
                {
                    bool match = CheckTagSuggestionsExist(dgvTag.Text);
                    btnSuggest.Background = !match ? default : _suggestButtonHighlight;
                }

                // Scroll to top of suggestions on tag selection change.
                if (VisualTreeHelper.GetChild(dgvSuggests, 0) is Decorator decorator)
                {
                    ScrollViewer scrollViewer = decorator.Child as ScrollViewer;
                    scrollViewer.ScrollToTop();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TagTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Check for empty cell.
                string val = ((TextBox)sender).Text;

                if (val.IsEmpty())
                {
                    btnSuggest.Background = default;
                    return;
                }

                // Don't proceed unless the text changed event is for the selected cell.
                int selIndex = dgvTags.SelectedIndex;
                if (selIndex < 0)
                {
                    return;
                }

                // Don't proceed unless the text matches the selected cell text.
                string selectedVal = ((TagViewModel)dgvTags.DataContext).TagList[selIndex].Text;
                if (selectedVal != val)
                {
                    return;
                }

                // Check whether suggestions are available for the tag.
                bool match = CheckTagSuggestionsExist(val);

                // Highlight the button if suggestions are available.
                btnSuggest.Background = !match ? default : _suggestButtonHighlight;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TagTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;

                    // Refresh suggestions.
                    BtnSuggest_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TagTextBox_FocusChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update export string.
                UpdateExportText();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Suggest search box

        private void TxtSearchSuggest_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Not using PreviewTextInput since that won't trigger on a Backspace.
            // Not using PreviewKeyDown since that won't read updated textbox text.

            try
            {
                Tag dgvTag = GetSelectedTag();
                if (dgvTag == null || dgvTag.Text.IsEmpty() || dgvTag.SuggestCache == null || dgvTag.SuggestCache.Count == 0)
                {
                    return;
                }

                // Filter current suggestions.
                SuggestItem[] filteredSuggestList = dgvTag.SuggestCache.Where(x => x.Text.StartsWithXx(txtSearchSuggest.Text)).ToArray();

                // Add filtered items to suggestions.
                ClearSuggests();
                filteredSuggestList.ToList().ForEach(x => ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Add(x));
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TxtSearchSuggest_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSearchSuggest.Foreground = Brushes.Black;

                if (string.Equals(txtSearchSuggest.Text, "Search here"))
                {
                    txtSearchSuggest.Text = "";
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TxtSearchSuggest_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSearchSuggest.Foreground = string.Equals(txtSearchSuggest.Text, "") ? Brushes.Gray : Brushes.Black;

                if (txtSearchSuggest.Text.IsEmpty())
                {
                    txtSearchSuggest.Text = "Search here";
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private async void TxtSearchSuggest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Check for empty textbox.
                if (txtSearchSuggest.Text.IsEmpty())
                {
                    return;
                }

                // Get highlighted tag text (can be blank line).
                string tag = GetSelectedTag()?.Text;

                // Add suggestion to tag list.
                bool isAdded = AddSuggestionTag(txtSearchSuggest.Text.ToLower());

                if (isAdded)
                {
                    // If tag is blank there's nothing to associate with.
                    if (tag.IsEmpty())
                    {
                        return;
                    }

                    // Add to learning model.
                    await AddToLearnDataListAsync(tag.ToLower(), txtSearchSuggest.Text.ToLower());

                    // Refresh suggestions.
                    BtnSuggest_Click(null, null);
                }

                txtSearchSuggest.Text = "";
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void TxtSearchSuggest_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    TxtSearchSuggest_MouseDoubleClick(null, null);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Suggest DataGrid

        private void InitializeDgvSuggest()
        {
            // Binding prereq.
            dgvSuggests.DataContext = new SuggestViewModel();
        }

        private async void SuggestLabel_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Check for empty cell.
                SuggestItem suggest = GetSelectedSuggestion();
                if (suggest == null || suggest.Text.IsEmpty())
                {
                    return;
                }

                // Add suggestion to tag list.
                bool isAdded = AddSuggestionTag(suggest.Text.ToLower());

                Tag dgvTag = GetSelectedTag();
                if (dgvTag == null || dgvTag.Text.IsEmpty())
                {
                    return;
                }

                // Add to learning model.
                await AddToLearnDataListAsync(dgvTag.Text.ToLower(), suggest.Text.ToLower());

                // Refresh suggestions.
                BtnSuggest_Click(null, null);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void SuggestLabel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                string labelText = ((Label)sender).Content.ToString();
                SuggestItem suggestItem = ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.SingleOrDefault(x => x.Text.EqualsXx(labelText));
                if (!suggestItem.IsPromoted)
                {
                    // Cancel the context menu if the right-clicked suggestion isn't a color-coded suggestion.
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void SuggestLabel_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get highlighted tag.
                Tag dgvTag = GetSelectedTag();
                if (dgvTag == null || dgvTag.Text.IsEmpty())
                {
                    return;
                }

                // Get suggestion that was right-clicked.
                ContextMenu cm = (ContextMenu)sender;
                string itemText = ((Label)cm.PlacementTarget).Content.ToString();
                SuggestItem suggestion = ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.SingleOrDefault(x => string.Equals(x.Text, itemText, StringComparison.CurrentCultureIgnoreCase));
                SuggestItem cacheSuggestion = dgvTag.SuggestCache.SingleOrDefault(x => x.Text.EqualsXx(itemText));

                // Customize right-click menu.
                MenuItem mi = (MenuItem)cm.Items[0];
                mi.Header = $"Unrelate '{suggestion.Text}' and '{dgvTag.Text}'";
                mi.Click += async (s, e) =>
                {
                    // Remove suggestion from suggest list.
                    ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Remove(suggestion);

                    // Remove suggestion from suggest cache.
                    dgvTag.SuggestCache.Remove(cacheSuggestion);

                    await DeleteFromLearnDataListAsync(dgvTag.Text, suggestion.Text);
                };
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private bool AddSuggestionTag(string tag)
        {
            string[] tags = GetEnteredTagValues();

            // Don't add suggested tag if already in datagrid.
            if (tags.ContainsXx(tag))
            {
                // Indicate that tag wasn't added.
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                // Skip non-empty cells.
                if (tags[i].IsNotEmpty())
                {
                    continue;
                }

                // Add to the first empty cell.
                SetDgvTagValue(i, tag);
                break;
            }

            // Update export string.
            UpdateExportText();

            // Indicate that tag was added.
            return true;
        }

        #endregion

        #region Suggest button

        private async void BtnSuggest_Click(object sender, EventArgs e)
        {
            try
            {
                btnSuggest.Background = default;
                ClearSuggests(clearSearch: true);

                // Get highlighted tag.
                Tag dgvTag = GetSelectedTag();
                if (dgvTag == null || dgvTag.Text.IsEmpty())
                {
                    return;
                }

                List<TagMetadata> tagMetadataList = null;
                SuggestItem[] promotedSelections = null;
                try
                {
                    // Get tag for which suggestions are needed.
                    tagMetadataList = await GetAssociatedTagsAsync(dgvTag.Text);
                    promotedSelections = ReadLearnDataList(dgvTag.Text);
                }
                catch (Exception ex)
                {
                    txtExport.Text = ex.Message;
                }
                finally
                {
                }

                string[] skipTags = new[] { dgvTag.Text };

                foreach (SuggestItem promotedSelection in promotedSelections)
                {
                    if (promotedSelection.Text.EqualsAnyXx(skipTags))
                    {
                        continue;
                    }

                    ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Add(new SuggestItem(promotedSelection.Text, promotedSelection.Shade, isPromoted: true));
                }

                skipTags = skipTags.Concat(promotedSelections.Select(x => x.Text)).ToArray();

                // Update suggestion listbox with selections from historical tag data.
                foreach (TagMetadata tagMetadata in tagMetadataList)
                {
                    if (tagMetadata.Name.EqualsAnyXx(skipTags))
                    {
                        continue;
                    }

                    ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Add(new SuggestItem(tagMetadata.Name, Brushes.WhiteSmoke));
                }

                // Add to tag cache.
                List<SuggestItem> clonedSuggests = new();
                ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.ToList().ForEach(x => clonedSuggests.Add(x.Clone()));
                SetDgvTagCache(dgvTag.Text, clonedSuggests);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static async Task<List<TagMetadata>> GetAssociatedTagsAsync(string keyTag)
        {
            List<TagMetadata> tagMetadataList = new();

            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Get all tagsets with keytag.
                    string[][] tagsetMatches = s_historicalDataList.Where(x => x.ContainsXx(keyTag)).ToArray();

                    // Get all unique tags across all tagsets with keytag (associated tags).
                    string[] assocTags = tagsetMatches.SelectMany(x => x).Distinct().ToArray();

                    // Find occurrence count for each associated tag.
                    foreach (string assocTag in assocTags)
                    {
                        TagMetadata tagMetadata = new();
                        tagMetadata.Name = assocTag;
                        tagMetadata.Count = tagsetMatches.Where(x => x.ContainsXx(assocTag)).Count();
                        tagMetadataList.Add(tagMetadata);
                    }

                    // Rank suggestions by usage popularity.
                    tagMetadataList = tagMetadataList.OrderByDescending(x => x.Count).ToList();
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return tagMetadataList;
        }

        private static bool CheckTagSuggestionsExist(string tag)
        {
            bool match = s_historicalDataList.Any(x => x.ContainsXx(tag) && x.Length > 1) || s_learnDataList.Any(x => x.Tag.EqualsXx(tag));

            return match;
        }

        #endregion

        #region App folder button

        private void BtnAppFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer", s_appDataDirPath);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Clear button

        private void BtnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearTags();
                ClearSuggests(clearSearch: true);
                txtExport.Text = "";
                btnSuggest.Background = default;
                btnExport.Background = default;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Export button

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> notifyMsg = new();

                // Check for spelling errors.
                List<string> spellingErrorList = new();
                int charIdx = txtExport.Text.Length;
                while ((charIdx = txtExport.GetNextSpellingErrorCharacterIndex(charIndex: charIdx, LogicalDirection.Backward)) > -1)
                {
                    spellingErrorList.Add(txtExport.Text.Substring(charIdx, txtExport.GetSpellingErrorLength(charIdx)));
                }
                if (spellingErrorList.Any())
                {
                    notifyMsg.Add($"Spelling errors:\r\n{string.Join(", ", spellingErrorList.Select(x => $"'{x}'"))}");
                }

                // Check if any tags are in notify file.
                string[] exportTags = txtExport.Text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).OrderBy(x => x).ToArray();
                string[] notifyExportTags = exportTags.IntersectXx(s_notifyDataList);
                if (notifyExportTags.Any())
                {
                    notifyMsg.Add($"Tag(s) not recommended:\r\n{string.Join(", ", notifyExportTags.Select(x => $"'{x}'"))}");
                }

                // Prompt whether to continue with export.
                if (notifyMsg.Any())
                {
                    MessageBoxResult dr = MessageBox.Show($"{string.Join("\r\n\r\n", notifyMsg)}\r\n\r\nCopy to clipboard?", s_appDisplayName, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (dr != MessageBoxResult.OK)
                    {
                        return;
                    }
                }

                // Save tags to clipboard.
                Clipboard.SetText(txtExport.Text);

                // Add to historical data list.
                if (!s_historicalDataList.Any(x => string.Join(",", x).EqualsXx(string.Join(",", exportTags))))
                {
                    s_historicalDataList.Add(exportTags);
                }

                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Read comment lines from the data file.
                    string[] oldCommentLines = File.ReadAllText(s_historicalDataFilePath).GetCommentLines();

                    // Update the data file in the app folder, preserving comments.
                    File.WriteAllLines(s_historicalDataFilePath, oldCommentLines);
                    File.AppendAllLines(s_historicalDataFilePath, s_historicalDataList.Select(x => string.Join(", ", x)));
                });

                // Set export button color.
                btnExport.Background = default;

                // Normalize tags scores only when the export button is clicked.
                string[] tags = ((TagViewModel)dgvTags.DataContext).TagList?.Select(x => x.Text)?.ToArray();
                await NormalizeLearnDataListAsync(tags);

                // Refresh suggestions.
                BtnSuggest_Click(null, null);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Helper methods

        private void UpdateExportText()
        {
            // Update export string.
            string[] uniqueTags = GetEnteredTagValues().Where(x => x.Trim().Replace(",", "").Length > 0).Select(x => x.Trim().Replace(",", "").ToLower()).Distinct().ToArray();
            txtExport.Text = string.Join(", ", uniqueTags);
            dgvSuggests.Background = Brushes.White;

            // Set export button color.
            btnExport.Background = txtExport.Text.IsEmpty() || (Clipboard.GetText().EqualsXx(txtExport.Text)) ? default : _exportButtonHighlight;
        }

        private void SetDgvTagValue(int index, string value)
        {
            ((TagViewModel)dgvTags.DataContext).TagList[index].Text = value;
        }

        private void SetDgvTagCache(string val, List<SuggestItem> suggestCache)
        {
            int selIndex = dgvTags.SelectedIndex;
            if (selIndex > -1 && (((TagViewModel)dgvTags.DataContext).TagList[selIndex].Text.EqualsXx(val)))
            {
                ((TagViewModel)dgvTags.DataContext).TagList[selIndex].SuggestCache = suggestCache;
            }
        }

        private string[] GetEnteredTagValues()
        {
            return ((TagViewModel)dgvTags.DataContext).TagList.Select(x => x.Text).ToArray();
        }

        private Tag GetSelectedTag()
        {
            int selIndex = dgvTags.SelectedIndex;
            return selIndex < 0 ? null : ((TagViewModel)dgvTags.DataContext).TagList[selIndex];
        }

        private SuggestItem GetSelectedSuggestion()
        {
            int selIndex = dgvSuggests.SelectedIndex;
            return selIndex < 0 ? null : ((SuggestViewModel)dgvSuggests.DataContext).SuggestList[selIndex];
        }

        private void ClearTags()
        {
            // Clear DataGrid text and background.
            foreach (Tag item in ((TagViewModel)dgvTags.DataContext).TagList)
            {
                item.Text = "";
                item.Shade = Brushes.White;
            }

            dgvTags.SelectedIndex = -1;
        }

        private void ClearSuggests(bool clearSearch = false)
        {
            // Clear DataGrid text.
            ((SuggestViewModel)dgvSuggests.DataContext).SuggestList.Clear();

            // Clear suggest search box.
            if (clearSearch)
            {
                txtSearchSuggest.Text = "Search here";
                txtSearchSuggest.Foreground = Brushes.Gray;
            }

            dgvSuggests.SelectedIndex = -1;
        }

        private void ShowUiBusy(bool isBusy, string message)
        {
            // Set ui initialization method.
            if (message != null) txtExport.Text = message;

            // Handle animation.
            btnSuggest.IsEnabled = !isBusy;
        }

        private static void LogException(Exception ex)
        {
            File.AppendAllText(s_errorFilePath, $"{DateTime.Now}:\r\nMessage: {ex.Message?.Trim()}\r\nInner exception: {ex.InnerException?.ToString()?.Trim()}\r\nStackTrace: {ex.StackTrace?.Trim()}\r\n\r\n");
        }

        #endregion

        #region Modify learn data

        private static SuggestItem[] ReadLearnDataList(string keyword)
        {
            if (keyword.IsEmpty())
            {
                return Array.Empty<SuggestItem>();
            }

            LearnItem[] matches = s_learnDataList.Where(x => x.Tag.EqualsXx(keyword)).ToArray();
            if (matches.Length == 0)
            {
                return Array.Empty<SuggestItem>();
            }

            SuggestItem[] shadedSuggests = GetShadedSuggests(matches.OrderByDescending(x => x.Score).ToArray());
            return shadedSuggests;

            static SuggestItem[] GetShadedSuggests(LearnItem[] matches)
            {
                List<SuggestItem> shadedSuggests = new();

                // Get ordered grouped items (grouped by distinct priority values).
                var orderedPriorityGroups = matches.GroupBy(x => new { x.Score })
                    .OrderBy(x => x.Key.Score)
                    .ToArray();

                int distinctPriorityCount = orderedPriorityGroups.Length;

                // establish alpha step.
                int alphaMin = 20;
                int alphaMax = 150;
                int alphaStep = distinctPriorityCount > 0 ? (alphaMax - alphaMin) / distinctPriorityCount : 0;

                int priorityCounter = 0;

                // Assign same brush color to all items in a group.
                // Iterate through groups in ascending priority order.
                foreach (var priorityGroup in orderedPriorityGroups)
                {
                    // Determine alpha value for all items in this priority group.
                    int alpha = alphaMin + (priorityCounter * alphaStep);
                    priorityCounter++;

                    // Reverse alphabetize suggestions within a priority group (reversed again later).
                    LearnItem[] orderedPriorityGroup = priorityGroup.OrderByDescending(x => x.Suggest).ToArray();

                    // Set brush color for all items in this priority group.
                    foreach (LearnItem sm in orderedPriorityGroup)
                    {
                        SuggestItem suggest = new(sm.Suggest, GetPurpleShadeBrush(alpha));
                        shadedSuggests.Add(suggest);
                    }
                }

                // Set to descending priority order and same priority alphabetize items.
                shadedSuggests.Reverse();

                return shadedSuggests.ToArray();

                static SolidColorBrush GetPurpleShadeBrush(int alpha)
                {
                    SolidColorBrush brush = new(Color.FromArgb((byte)alpha, 255, 0, 255));

                    // Improve brush performance.
                    brush.Freeze();

                    return brush;
                }
            }
        }

        private static async Task AddToLearnDataListAsync(string tag, string suggest)
        {
            LearnItem match = s_learnDataList.FirstOrDefault(x => x.Tag.EqualsXx(tag) && x.Suggest.EqualsXx(suggest));
            if (match != null)
            {
                // Increment the count of the existing entry.
                match.Score++;
            }
            else
            {
                // Create a new entry (<item keyword="acrylic" selection="art" count="2"/>).
                LearnItem newSelectMetadata = new(tag, suggest, "1");
                s_learnDataList.Add(newSelectMetadata);
            }

            // Don't normalize scores here because it interferes with relative rankings.

            // Run I/O task asynchronously.
            await Task.Run(() =>
            {
                // Save learn data list to disk.
                SaveLearnDataListToFile();
            });
        }

        private static async Task NormalizeLearnDataListAsync(string[] tags)
        {
            if (tags == null || tags.Length == 0 || tags.All(x => x.IsEmpty()))
            {
                return;
            }

            // Normalize tag association scores (remove gaps).
            foreach (string tag in tags)
            {
                LearnItem[] tagSuggestions = s_learnDataList.Where(x => x.Tag.EqualsXx(tag)).ToArray();
                var orderedGroups = tagSuggestions.OrderBy(x => x.Score).GroupBy(x => new { x.Score }).ToArray();
                int normalizedScore = 0;
                foreach (var group in orderedGroups)
                {
                    normalizedScore++;
                    foreach (LearnItem item in group)
                    {
                        item.Score = normalizedScore;
                    }
                }

                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Save learn data list to disk.
                    SaveLearnDataListToFile();
                });
            }
        }

        private async Task DeleteFromLearnDataListAsync(string tag, string suggest)
        {
            LearnItem learnItem = s_learnDataList.FirstOrDefault(x => x.Tag.EqualsXx(tag) && x.Suggest.EqualsXx(suggest));
            if (learnItem == null)
            {
                return;
            }

            s_learnDataList.Remove(learnItem);

            // Run I/O task asynchronously.
            await Task.Run(() =>
            {
                // Save learn data list to disk.
                SaveLearnDataListToFile();
            });

            // Refresh suggestions.
            BtnSuggest_Click(null, null);
        }

        private static void SaveLearnDataListToFile()
        {
            // Call from async context.

            // Create xml object.
            XDocument xdoc = new(new XElement("root", new XAttribute("datatype", "learn-data"), new XAttribute("comment", s_learnDataFileComment ?? "")));
            foreach (LearnItem selectMetadata in s_learnDataList.OrderBy(x => x.Tag))
            {
                XElement element = new("item", new XAttribute("tag", selectMetadata.Tag), new XAttribute("suggest", selectMetadata.Suggest), new XAttribute("score", selectMetadata.Score));
                xdoc.Root.Add(element);
            }

            // Order items.
            IOrderedEnumerable<XElement> sortedItems = xdoc.Descendants("item").OrderBy(x => x.Attribute("tag").Value).ThenByDescending(x => int.Parse(x.Attribute("score").Value));
            xdoc.Root.ReplaceNodes(sortedItems);

            // Save to file.
            xdoc.SaveToFile(s_learnDataFilePath);
        }

        #endregion

        #region Load data files

        private static async Task LoadLearnDataFileAsync()
        {
            s_learnDataList.Clear();

            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    string content = null;

                    if (File.Exists(s_learnDataFilePath))
                    {
                        // Read the custom data file in the app folder.
                        content = File.ReadAllText(s_learnDataFilePath);
                    }
                    else
                    {
                        File.WriteAllText(s_learnDataFilePath, Properties.Resources.learn_data);

                        // Read the default data file in the Resources folder.
                        content = Properties.Resources.learn_data;
                    }

                    // Transform learn content into list.
                    s_learnDataList = GenerateLearnDataList(content);

                    // Save learn data list to disk.
                    SaveLearnDataListToFile();
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static List<LearnItem> GenerateLearnDataList(string content, bool captureComment = true)
        {
            List<LearnItem> learnDataList = new();

            try
            {
                // Remove comments and blank lines.
                string dataContent = string.Join(Environment.NewLine, content.GetDataLines());

                XDocument xdoc = XDocument.Parse(dataContent);

                List<XElement> items = xdoc.Root.Elements("item").ToList();
                if (captureComment)
                {
                    s_learnDataFileComment = xdoc.Root.Attribute("comment")?.Value;
                }
                foreach (XElement item in items)
                {
                    string tag = item.Attribute("tag").Value.ToLower();
                    string suggest = item.Attribute("suggest").Value.ToLower();
                    string count = item.Attribute("score").Value;
                    LearnItem selectMetadata = new(tag, suggest, count);
                    LearnItem existingLearnItem = learnDataList.SingleOrDefault(x => x.Tag.EqualsXx(tag) && x.Suggest.EqualsXx(suggest));
                    if (existingLearnItem == null)
                    {
                        // Add new list item.
                        learnDataList.Add(selectMetadata);
                    }
                    else
                    {
                        // Update existing list item.
                        existingLearnItem.Score += int.Parse(count);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return learnDataList;
        }

        private static async Task LoadHistoricalDataFileAsync()
        {
            s_historicalDataList.Clear();

            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    string[] dataLines = null;

                    if (File.Exists(s_historicalDataFilePath))
                    {
                        // Read the custom data file in the app folder.
                        // Remove comments and blank lines.
                        dataLines = File.ReadAllText(s_historicalDataFilePath).GetDataLines();
                    }
                    else
                    {
                        // Create a custom data file in the app folder.
                        File.WriteAllText(s_historicalDataFilePath, Properties.Resources.historical_data);

                        // Read the default data file in the Resources folder.
                        // Remove comments and blank lines.
                        dataLines = Properties.Resources.historical_data.GetDataLines();
                    }

                    foreach (string dataLine in dataLines)
                    {
                        string[] lineTags = dataLine.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).OrderBy(x => x).ToArray();

                        if (!s_historicalDataList.Any(x => string.Join(",", x).EqualsXx(string.Join(",", lineTags))))
                        {
                            s_historicalDataList.Add(lineTags);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static async Task LoadNotifyDataFileAsync()
        {
            s_notifyDataList.Clear();

            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    string[] dataLines = null;

                    if (File.Exists(s_notifyDataFilePath))
                    {
                        // Read the custom data file in the app folder.
                        dataLines = File.ReadAllText(s_notifyDataFilePath).GetDataLines();
                    }
                    else
                    {
                        // Create a custom data file in the app folder.
                        File.WriteAllText(s_notifyDataFilePath, Properties.Resources.notify_data);

                        // Read the default data file in the Resources folder.
                        dataLines = Properties.Resources.notify_data.GetDataLines();
                    }

                    s_notifyDataList.AddRange(dataLines);
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private async Task LoadConfigFileAsync()
        {
            string content = null;

            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Ensure button-data file exists and read content.
                    if (File.Exists(s_configFilePath))
                    {
                        // Read the custom data file in the app folder.
                        content = File.ReadAllText(s_configFilePath);
                    }
                    else
                    {
                        // Create a custom data file in the app folder.
                        File.WriteAllText(s_configFilePath, Properties.Resources.config);

                        // Read the default data file in the Resources folder.
                        content = Properties.Resources.config;
                    }
                });

                if (content.IsEmpty())
                {
                    return;
                }

                XDocument xdoc = XDocument.Parse(content);

                // Parse spellcheck language.
                try
                {
                    string locale = xdoc.Root?.Element("language")?.Element("spellchecker")?.Value;
                    if (locale != null)
                    {
                        dgvTags.Language = XmlLanguage.GetLanguage(locale);
                        txtExport.Language = XmlLanguage.GetLanguage(locale);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }

                // Parse presets section.
                try
                {
                    Button[] buttons = { BtnTagPreset1, BtnTagPreset2, BtnTagPreset3, BtnTagPreset4, BtnTagPreset5 };
                    List<XElement> configNodes = xdoc.Root.Element("presets").Elements("preset").ToList();
                    for (int i = 0; i < configNodes.Count; i++)
                    {
                        if (i >= 5)
                        {
                            throw new Exception("Invalid number of presets in the configuration file.");
                        }

                        string configText = configNodes[i].Attribute("text").Value;
                        List<string> configTags = configNodes[i].Elements("tag").Select(x => x.Value).Where(x => x.IsNotEmpty()).ToList();
                        if (configText.IsEmpty() || configTags.Count == 0)
                        {
                            // Set visibility.
                            buttons[i].Visibility = Visibility.Hidden;
                            continue;
                        }

                        // Set visibility.
                        buttons[i].Visibility = Visibility.Visible;

                        // Create menu item.
                        buttons[i].ToolTip = configText;

                        // Set menu item click logic.
                        buttons[i].Click += (s, e) =>
                        {
                            try
                            {
                                foreach (string configTag in configTags)
                                {
                                    AddSuggestionTag(configTag);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }

                // Parse tags section.
                try
                {
                    string tagMaxNode = xdoc.Root.Element("tags")?.Element("max")?.Value;
                    if (int.TryParse(tagMaxNode, out int maxTags) && maxTags > 0 && maxTags <= 250)
                    {
                        s_maxTags = maxTags;
                    }
                    else
                    {
                        throw new Exception("Invalid tags max in the configuration file.");
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Merge data files

        private static async Task MergeOldNewLearnDataFilesAsync(string newContent)
        {
            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Transform learn content into list.
                    List<LearnItem> newLearnDataList = GenerateLearnDataList(newContent, captureComment: false);

                    // Merge learn content lists.
                    foreach (LearnItem learnItem in newLearnDataList)
                    {
                        LearnItem existingLearnItem = s_learnDataList.SingleOrDefault(x => x.Tag.EqualsXx(learnItem.Tag) && x.Suggest.EqualsXx(learnItem.Suggest));
                        if (existingLearnItem == null)
                        {
                            // New items are added with minimal rank.
                            learnItem.Score = 1;
                            s_learnDataList.Add(learnItem);
                        }
                        // Existing items are skipped and their existing rank is preserved.
                    }

                    // Save learn data list to disk.
                    SaveLearnDataListToFile();
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static async Task MergeOldNewHistoricalDataFilesAsync(string newContent)
        {
            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Read the data file from the app folder.
                    string[] oldCommentLines = File.ReadAllText(s_historicalDataFilePath).GetCommentLines();
                    string[] newDataLines = newContent.GetDataLines();

                    // Merge new data lines 
                    foreach (string newDataLine in newDataLines)
                    {
                        string[] newDataLineTags = newDataLine.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).OrderBy(x => x).ToArray();
                        if (!s_historicalDataList.Any(x => string.Join(",", x).EqualsXx(string.Join(",", newDataLineTags))))
                        {
                            s_historicalDataList.Add(newDataLineTags);
                        }
                    }

                    // Update the data file in the app folder, preserving comments.
                    File.WriteAllLines(s_historicalDataFilePath, oldCommentLines);
                    File.AppendAllLines(s_historicalDataFilePath, s_historicalDataList.Select(x => string.Join(", ", x)));
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static async Task MergeOldNewNotifyDataFilesAsync(string newContent)
        {
            try
            {
                // Run I/O task asynchronously.
                await Task.Run(() =>
                {
                    // Read the data file from the app folder.
                    string[] oldCommentLines = File.ReadAllText(s_notifyDataFilePath).GetCommentLines();
                    string[] newDataLines = newContent.GetDataLines();

                    // Merge new data lines 
                    foreach (string newDataLine in newDataLines)
                    {
                        if (!s_notifyDataList.Any(x => x.EqualsXx(newDataLine)))
                        {
                            s_notifyDataList.Add(newDataLine);
                        }
                    }

                    // Update the data file in the app folder, preserving comments.
                    File.WriteAllLines(s_notifyDataFilePath, oldCommentLines);
                    File.AppendAllLines(s_notifyDataFilePath, s_notifyDataList);
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region Drag-drop data file

        private async void DgvSuggest_DragEnter(object sender, DragEventArgs e)
        {
            ClearSuggests(clearSearch: true);

            if ((await ValidateDropFileAsync(e)).isValid)
            {
                dgvSuggests.Background = Brushes.LightGreen;
                _dragDropEffects = DragDropEffects.Copy;
            }
            else
            {
                dgvSuggests.Background = Brushes.Tomato;
                _dragDropEffects = DragDropEffects.None;
            }
        }

        private async void DgvSuggest_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                dgvSuggests.Background = Brushes.White;

                ShowUiBusy(isBusy: true, message: null);

                // Validate data file.
                (bool isValid, string filePath, string fileType, string content) = await ValidateDropFileAsync(e);
                if (!isValid)
                {
                    return;
                }

                // Merge old and new data files and reinitialize app with the new data.
                if (fileType.EqualsXx("learn-data"))
                {
                    MessageBoxResult dr = MessageBox.Show($"Merge learn-data files?\r\n\r\nSelect Yes to merge, or No to replace.", s_appDisplayName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (dr == MessageBoxResult.Yes)
                    {
                        // Merge.
                        await MergeOldNewLearnDataFilesAsync(content);
                        txtExport.Text = $"Merged {fileType} files.";
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                        // Replace.
                        await File.WriteAllTextAsync(s_learnDataFilePath, content);
                        txtExport.Text = $"Replaced {fileType} file.";
                    }

                    // Reload data.
                    await LoadLearnDataFileAsync();

                    // Show success message.
                    txtExport.Text += $"\r\nReloaded {fileType}.";
                }
                else if (fileType.EqualsXx("historical-data"))
                {
                    MessageBoxResult dr = MessageBox.Show($"Merge historical-data files?\r\n\r\nSelect Yes to merge or No to replace.", s_appDisplayName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (dr == MessageBoxResult.Yes)
                    {
                        // Merge.
                        await MergeOldNewHistoricalDataFilesAsync(content);
                        txtExport.Text = $"Merged {fileType} files.";
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                        // Replace.
                        await File.WriteAllTextAsync(s_historicalDataFilePath, content);
                        txtExport.Text = $"Replaced {fileType} file.";
                    }

                    // Reload data.
                    await LoadHistoricalDataFileAsync();    // can takes several seconds to load.
                    txtExport.Text += $"\r\nReloaded {fileType}.";
                }
                else if (fileType.EqualsXx("notify-data"))
                {
                    MessageBoxResult dr = MessageBox.Show($"Merge notify-data files?\r\n\r\nSelect Yes to merge, or No to replace.", s_appDisplayName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (dr == MessageBoxResult.Yes)
                    {
                        // Merge.
                        await MergeOldNewNotifyDataFilesAsync(content);
                        txtExport.Text = $"Merged {fileType} files.";
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                        // Replace.
                        await File.WriteAllTextAsync(s_notifyDataFilePath, content);
                        txtExport.Text = $"Replaced {fileType} file.";
                    }

                    // Reload data.
                    await LoadNotifyDataFileAsync();
                    txtExport.Text += $"\r\nReloaded {fileType}.";
                }
                else if (fileType.EqualsXx("config"))
                {
                    // Replace.
                    await File.WriteAllTextAsync(s_configFilePath, content);
                    txtExport.Text = $"Replaced {fileType} file.";

                    // Reload data.
                    await LoadConfigFileAsync();
                    txtExport.Text += $"\r\nReloaded {fileType}.";
                }

                ShowUiBusy(isBusy: false, message: null);
            }
            catch (Exception ex)
            {
                txtExport.Text = $"Error in drag and drop operation.";
                LogException(ex);
            }
        }

        private void DgvSuggest_DragLeave(object sender, EventArgs e)
        {
            txtExport.Text = "";
            dgvSuggests.Background = Brushes.White;
        }

        private void DgvSuggest_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = _dragDropEffects;
            e.Handled = true;
        }

        private async Task<(bool isValid, string filePath, string fileType, string content)> ValidateDropFileAsync(DragEventArgs e)
        {
            txtExport.Text = $"Unrecognized file.";

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return (false, null, null, null);
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string extension = Path.GetExtension(files[0]);
            if (files.Length != 1 ||
                !File.Exists(files[0]) ||
                !(extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(files[0]).Equals(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return (false, null, null, null);
            }

            string content = await File.ReadAllTextAsync(files[0]);
            string fileType = Regex.Match(content, @"datatype\s?=\s?""(?<fileType>.*?)""").Groups["fileType"]?.Value;
            if (!(fileType.EqualsXx("learn-data") && extension.EqualsXx(".xml")) &&
                !(fileType.EqualsXx("historical-data") && extension.EqualsXx(".txt")) &&
                !(fileType.EqualsXx("notify-data") && extension.EqualsXx(".txt")) &&
                !(fileType.EqualsXx("config") && extension.EqualsXx(".xml")))
            {
                return (false, null, null, null);
            }

            txtExport.Text = $"Detected new {fileType} file.";
            return (true, files[0], fileType, content);
        }

        #endregion
    }

    #region Classes

    internal class TagMetadata
    {
        internal string Name;
        internal int Count;

        public TagMetadata()
        {
        }

        public TagMetadata(string name)
        {
            Name = name;
        }
    }

    internal class LearnItem
    {
        internal string Tag;
        internal string Suggest;
        internal int Score { get => int.Parse(scoreStr); set => scoreStr = value.ToString(); }

        private string scoreStr;

        public LearnItem(string keyTag, string suggestion, string scoreStr)
        {
            Tag = keyTag;
            Suggest = suggestion;
            this.scoreStr = scoreStr;
        }
    }

    #endregion
}
