using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinAllianceApp.Controllers;

namespace XamarinAllianceApp.Views
{
    public partial class CharacterList : ContentPage
    {
        private CharacterService service;

        public CharacterList()
        {
            InitializeComponent();

            service = new CharacterService();
            btnSearchIMDB.Clicked += (sender, e) => GetCharacters();

            characterList.ItemSelected += async (s, e) =>
            {
                if (characterList.SelectedItem == null)
                    return;

                var person = characterList.SelectedItem as MovieCastCrew.Cast;

                characterList.SelectedItem = null;

                await Navigation.PushAsync(new CastMemberDetails(person.id));
            };
        }

        private void CharacterList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Set syncItems to true in order to synchronize the data on startup when running in offline mode
            //await RefreshItems(true);
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#pulltorefresh
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Exception error = null;
            try
            {
                await RefreshItems(false);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                list.EndRefresh();
            }

            if (error != null)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
            }
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshItems(true);
        }

        private async Task RefreshItems(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                characterList.ItemsSource = await service.GetCharactersAsync();
            }
        }

        private async Task GetCharacters()
        {
            try
            {
                if (txtSearchQuery.Text != "")
                {
                    using (var scope = new ActivityIndicatorScope(syncIndicator, true))
                    {
                        HttpClient client = new HttpClient();
                        var response = await client.GetStringAsync("https://api.themoviedb.org/3/search/movie?api_key=b2eec0d3fe03eb1e8938782876209b54&query=" + txtSearchQuery.Text);

                        var movies = JsonConvert.DeserializeObject<MovieResult>(response);

                        if (movies.total_results > 0)
                        {
                            var castResponse = await client.GetStringAsync("https://api.themoviedb.org/3/movie/" + movies.results[0].id + "/credits?api_key=b2eec0d3fe03eb1e8938782876209b54");

                            var cast = JsonConvert.DeserializeObject<MovieCastCrew>(castResponse);

                            if (cast != null)
                                characterList.ItemsSource = cast.cast;

                            lblSearchResults.Text = "Showing cast of " + movies.results[0].title;
                        }
                        else
                        {
                            lblSearchResults.Text = "No moview by the title of " + txtSearchQuery.Text + " was found. Please try again.";
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Invaid", "Please enter a movie to search for", "Ok");
                }
            }
            catch(Exception ex){
                lblSearchResults.Text = "An error occured: " + ex.Message;
            }
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
    
    public class MovieResult
    {
        public class Result
        {
            public string poster_path { get; set; }
            public bool adult { get; set; }
            public string overview { get; set; }
            public string release_date { get; set; }
            public List<object> genre_ids { get; set; }
            public int id { get; set; }
            public string original_title { get; set; }
            public string original_language { get; set; }
            public string title { get; set; }
            public string backdrop_path { get; set; }
            public double popularity { get; set; }
            public int vote_count { get; set; }
            public bool video { get; set; }
            public double vote_average { get; set; }
        }

        public int page { get; set; }
        public List<Result> results { get; set; }
        public int total_results { get; set; }
        public int total_pages { get; set; }
    }
    
    public class MovieCastCrew
    {
        public class Cast
        {
            public int cast_id { get; set; }
            public string character { get; set; }
            public string credit_id { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int order { get; set; }
            public string profile_path { get; set; }
        }

        public class Crew
        {
            public string credit_id { get; set; }
            public string department { get; set; }
            public int id { get; set; }
            public string job { get; set; }
            public string name { get; set; }
            public string profile_path { get; set; }
        }
        
        public int id { get; set; }
        public List<Cast> cast { get; set; }
        public List<Crew> crew { get; set; }
    }
}

