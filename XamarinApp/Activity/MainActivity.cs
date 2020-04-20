﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using ObjectsLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinAppLibrary;

namespace XamarinApp
{
    [Activity(Label = "На плите!", Theme = "@style/AppTheme.NoActionBar", Icon = "@drawable/icon", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private RecyclerView recyclerView;
        private RecipeShort[] recipeShorts;
        private DrawerLayout _drawer;
        private SwipeRefreshLayout swipeRefreshLayout;
        private LinearLayoutManager linearLayoutManager;

        private int page = 1;
        private string lastQuery;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // TODO: Сделать в менюшке раздел "Сохранённые рецепты"
            // TODO: Кнопка "Поделится"
            // TODO: Поиск по ингредиентам
            // TODO: Тотальный рефакторинг.
            // TODO: Загрузка доп.рецептов при прокрутке.

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_search);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.listRecipeShorts);

            swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            swipeRefreshLayout.SetColorSchemeColors(Color.Orange, Color.DarkOrange);
            swipeRefreshLayout.Refresh += delegate (object sender, System.EventArgs e)
            {
                if (lastQuery == null)
                    UpdateListView();
                else
                    UpdateListView(lastQuery);
            };


            recyclerView.HasFixedSize = false;
            linearLayoutManager = new LinearLayoutManager(this);


            var onScrollListener = new RecipeListener(linearLayoutManager);
            recyclerView.AddOnScrollListener(onScrollListener);
            onScrollListener.LoadMoreEvent += (object sender, EventArgs e) =>
            {
                string query = lastQuery.Substring(0, lastQuery.IndexOf("page=") + 5) + (++page);
                UpdateListView(query, recipeShorts);
            };

            UpdateListView();

            SetPlateRecipe();

            SetSpinner();

            _drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, _drawer, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            _drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var buttonMenu = FindViewById<Button>(Resource.Id.menu_button);
            buttonMenu.SetBackgroundResource(Resources.GetIdentifier("round_menu_24", "drawable", PackageName));
            buttonMenu.Click += SetButtonClick;
        }

        private void SetButtonClick(object sender, EventArgs args)
        {
            _drawer.OpenDrawer(GravityCompat.Start);

            var menu_buttonM = FindViewById<Button>(Resource.Id.menu_buttonM);

            menu_buttonM.Click += delegate (object sender, EventArgs args)
            {
                if (_drawer.IsDrawerOpen(GravityCompat.Start))
                    OnBackPressed();
            };

        }

        private async void UpdateListView(string query = "section=popular", RecipeShort[] recipeShorts = null)
        {
            swipeRefreshLayout.Post(() =>
            {
                swipeRefreshLayout.Refreshing = true;
                recyclerView.Clickable = false;
            });

            recyclerView.SetLayoutManager(linearLayoutManager);

            int position = 0;

            if (recipeShorts == null)
                this.recipeShorts = await UpdateCollectionRecipes(query);
            else
            {
                List<RecipeShort> localRecipes = new List<RecipeShort>();
                localRecipes.AddRange(recipeShorts);
                position = localRecipes.Count - 4;
                localRecipes.AddRange(await UpdateCollectionRecipes(query));
                this.recipeShorts = localRecipes.ToArray();
            }

            var adapter = new RecipeAdapter(this.recipeShorts, this);
            adapter.ItemClick += OnItemClick;

            
            recyclerView.SetAdapter(adapter);
            recyclerView.ScrollToPosition(position);
           
            swipeRefreshLayout.Post(() =>
            {
                swipeRefreshLayout.Refreshing = false;
                recyclerView.Clickable = true;
            });

        }

        public override void OnBackPressed()
        {
            if (_drawer.IsDrawerOpen(GravityCompat.Start))
            {
                _drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            return true;
        }

        private void SetSpinner()
        {
            var spinner = FindViewById<Spinner>(Resource.Id.spinner);
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SelectedItemSpinner);


            var spinnerAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sort_array, Resource.Layout.spinner_text);
            spinnerAdapter.SetDropDownViewResource(Resource.Layout.spinner_text);
            spinner.Adapter = spinnerAdapter;
        }

        private void SetPlateRecipe()
        {
            EditText edittext = FindViewById<EditText>(Resource.Id.TextFind);

            edittext.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;

                page = 1;

                if (e.Event.Action != KeyEventActions.Down || e.KeyCode != Keycode.Enter) return;

                lastQuery = $"section=recipe&recipeName={edittext.Text}&page={page}";
                UpdateListView(lastQuery);
                Toast.MakeText(this, "Загрузка...", ToastLength.Short).Show();


                e.Handled = true;
            };
        }

        void OnItemClick(object sender, int position)
        {
            Intent intent = new Intent(this, typeof(RecipeActivity));
            intent.PutExtra("url", recipeShorts[position].Url);
            intent.PutExtra("recipeShort", Data.RecipeToByteArray(recipeShorts[position]));

            StartActivity(intent);
        }


        private void SelectedItemSpinner(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = (Spinner)sender;
            var item = spinner.GetItemAtPosition(e.Position);

            page = 1;

            string query = null;

            switch (item.ToString())
            {
                case "По популярности":
                    query = $"section=popular&page={page}";
                    UpdateListView(query);
                    break;
                case "По случайности":
                    query = $"section=random&page={page}";
                    UpdateListView(query);
                    break;
                case "По новизне":
                    query = $"section=new&page={page}";
                    UpdateListView(query);
                    break;
            }

            lastQuery = query;
        }

        private async Task<RecipeShort[]> UpdateCollectionRecipes(string query)
        {
            return await Task.Run(function: () => HttpGet.GetPages(query));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            // Закрывает отрисовщик и возвращает, закрыт ли он или нет.
            int id = menuItem.ItemId;

            if (id == Resource.Id.nav_favorite)
            {
               Intent intent = new Intent(this, typeof(SavedRecipesActivity));
                StartActivity(intent);
            }

            return CLoseDrawer(_drawer).IsDrawerOpen(GravityCompat.Start);
        }

        private static DrawerLayout CLoseDrawer(DrawerLayout drawerLayout)
        {
            drawerLayout.CloseDrawer(GravityCompat.Start);
            return drawerLayout;
        }
    }
}