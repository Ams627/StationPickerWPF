using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace StationPickerWPF
{
    class StationsViewModel : DependencyObject, INotifyPropertyChanged
    {
        private readonly string faresfile = "s:/FareLocationsRefData.xml";
        private readonly string stationfile = "s:/StationsRefData.xml";
        private bool _loaded = false;

        IEnumerable<StationInfo> _stationInfoList;

        public ObservableCollection<string> _stationMatches { get; set; }

        public StationsViewModel()
        {
            LoadStationList();
        }

        private void LoadStationList()
        {
            try
            {
                XDocument faredoc = XDocument.Load(faresfile);
                XDocument stationdoc = XDocument.Load(stationfile);

                _stationInfoList = (from station in stationdoc.Element("StationsReferenceData").Elements("Station")
                                    where (string)station.Element("UnattendedTIS") == "true" &&
                                    !string.IsNullOrWhiteSpace((string)station.Element("CRS")) &&
                                    (string)station.Element("OJPEnabled") == "true"
                                    join fare in faredoc.Element("FareLocationsReferenceData").Elements("FareLocation")
                                    on (string)station.Element("Nlc") equals (string)fare.Element("Nlc")
                                    where (string)fare.Element("UnattendedTIS") == "true"
                                    select new StationInfo
                                    {
                                        CRS = (string)station.Element("CRS"),
                                        NLC = (string)fare.Element("Nlc"),
                                        Name = (string)fare.Element("OJPDisplayName"),
                                    }).Distinct();
                _loaded = true;
                _stationMatches = new ObservableCollection<string>();
            }
            catch (Exception ex)
            {
                _loaded = false;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public ObservableCollection<string> StationMatches
        {
            get
            {
                return _stationMatches;
            }
        }

        //public ObservableCollection<string> StationMatches
        //{
        //    get { return (ObservableCollection<string>)GetValue(StationMatchesProperty); }
        //    set { SetValue(StationMatchesProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for StationMatches.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty StationMatchesProperty =
        //    DependencyProperty.Register("StationMatches", typeof(List<string>), typeof(StationsViewModel), new PropertyMetadata(null));

        public string IDMSFolder
        {
            get { return (string)GetValue(IDMSFolderProperty); }
            set { SetValue(IDMSFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IDMSFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IDMSFolderProperty =
            DependencyProperty.Register("IDMSFolder", typeof(string), typeof(StationsViewModel),
                new PropertyMetadata("s:\\", new PropertyChangedCallback(IDMSFolderChanged)));

        public static void IDMSFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as StationsViewModel).LoadStationList();
        }

        public string Typing
        {
            get { return (string)GetValue(TypingProperty); }
            set { SetValue(TypingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Typing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypingProperty =
            DependencyProperty.Register("Typing", typeof(string), typeof(StationsViewModel),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(TypingChanged)));

        public event PropertyChangedEventHandler PropertyChanged;

        public static void TypingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var t = (d as StationsViewModel);
            t._stationMatches.Clear();

            var partialname = t.Typing;

            if (partialname.Length != 0)
            {
                var pattern = $@"\b{partialname}";
                var searchresults = t._stationInfoList.Select(s =>
                {
                    bool crsMatch = false;
                    if (partialname.Length <= 3)
                    {
                        crsMatch = string.Compare(s.CRS, 0, partialname, 0, partialname.Length, StringComparison.OrdinalIgnoreCase) == 0;
                    }
                    var match = Regex.Match(s.Name, pattern, RegexOptions.IgnoreCase);

                    // if neither name nor CRS match the partial input then return null:
                    if (!match.Success && !crsMatch)
                    {
                        return null;
                    }

                    // rank for sorting stations -1 means not ranked yet:
                    int rank = -1;

                    // A full 3-letter CRS match is the highest rank:
                    if (partialname.Length == 3 && crsMatch)
                    {
                        rank = 0;
                    }
                    else if (!match.Success)
                    {
                        // here the partial input doesn't match any stations - so there is just a partial CRS match - this is the lowest rank:
                        rank = 1000;
                    }
                    return new
                    {
                        crs = s.CRS,
                        name = s.Name,
                        // rank by the position of the word match within the sentence (split word at dash or space):
                        rank = rank == -1 ? 1 + s.Name.Substring(0, match.Index).Split(new[] { '-', ' ' }).Count() : rank
                    };
                }).Where(x => x != null).OrderBy(x => x.rank).ThenBy(x => x.name);
                foreach (var result in searchresults)
                {
                    t._stationMatches.Add(result.crs + " " + result.name);
                }
            }
        }
    }
}
