using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace XamarinAllianceApp.Views
{
    public partial class CastMemberDetails : ContentPage
    { 
        public long PersonID {get; set;}

        #region Actore details

        public ImageSource _avatarURL;
        public ImageSource AvatarURL
        {
            get { return _avatarURL; }
            set { _avatarURL = value; }
        }

        public string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string _biography;
        public string Biography
        {
            get { return _biography; }
            set { _biography = value; }
        }

        public string _birthplace;
        public string BirthPlace
        {
            get { return _birthplace; }
            set { _birthplace = value; }
        }

        public string _birthday;
        public string Birthday
        {
            get { return _birthday; }
            set { _birthday = value; }
        }

        #endregion


        public CastMemberDetails(long personid)
        {
            InitializeComponent();

            PersonID = personid;

            GetDetails();
        }

        private async Task GetDetails()
        {
            try
            {
                using (var scope = new ActivityIndicatorScope(syncIndicator, true))
                {
                    HttpClient client = new HttpClient();
                    var response = await client.GetStringAsync("https://api.themoviedb.org/3/person/"+ PersonID + "?api_key=b2eec0d3fe03eb1e8938782876209b54");

                    var actorDetails = JsonConvert.DeserializeObject<Person>(response);

                    if(actorDetails != null)
                    {
                        Title = actorDetails.name;

                        lblName.Text = actorDetails.name;
                        lblBday.Text = actorDetails.birthday;
                        lblBirthplace.Text = actorDetails.place_of_birth;
                        lblBio.Text = actorDetails.biography;
                        imgAvatar.Source = (!string.IsNullOrEmpty(actorDetails.profile_path) ? ImageSource.FromUri(new Uri("https://image.tmdb.org/t/p/w185/" + actorDetails.profile_path)) : ImageSource.FromFile("profile_generic_big"));
                    }
                }
            }
            catch { }
        }

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

    }

    public class Person
    {
        public bool adult { get; set; }
        public List<string> also_known_as { get; set; }
        public string biography { get; set; }
        public string birthday { get; set; }
        public string deathday { get; set; }
        public int gender { get; set; }
        public string homepage { get; set; }
        public int id { get; set; }
        public string imdb_id { get; set; }
        public string name { get; set; }
        public string place_of_birth { get; set; }
        public double popularity { get; set; }
        public string profile_path { get; set; }
    }
}
