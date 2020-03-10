﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using RecipesAndroid;
using RecipesAndroid.Objects;
using RecipesAndroid.Objects.Boxes.Elements;

namespace XamarinApp
{
    [Activity(Label = "На плите!", Theme = "@style/AppTheme.NoActionBar", Icon = "@drawable/icon", MainLauncher = true)]
    
    public class MainActivity : AppCompatActivity
    {
        private ListView _listView;
        private List<RecipeShort> recipes;
        private RecipeShortAdapter adapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // TODO: Доделать спиннер: поработать над дизайном.
            // TODO: Доделать поиск.
            // TODO: Сделать автозагрузку.
            // TODO: Сделать переход по рецепту.
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_search);
            
            Spinner spinner = FindViewById <Spinner> (Resource.Id.spinner);  
            spinner.ItemSelected += new EventHandler < AdapterView.ItemSelectedEventArgs > (spinner_ItemSelected);  
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sort_array, Android.Resource.Layout.SimpleSpinnerItem);  
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);  
            spinner.Adapter = adapter;  
            
            UpdateListView();
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e) {  
            Spinner spinner = (Spinner) sender;
            var item = spinner.GetItemAtPosition(e.Position);
            
            switch (item.ToString())
            {
                case "По популярности":
                    UpdateListView("getPage?section=popular");
                    break;
                case "По случайности":
                    UpdateListView("getPage?section=random");
                    break;
                case "По новизне":
                    UpdateListView("getPage?section=new");
                    break;
            }
            

            string toast = $"Сортировка {spinner.GetItemAtPosition(e.Position).ToString().ToLower()}";  
            Toast.MakeText(this, toast, ToastLength.Long).Show();  
        } 
        
        private async Task UpdateCollectionRecipes(string query)
        {
            await Task.Run(() =>
            {
                recipes = HttpGet.GetRecipes(query);
            });
        }
        private async void UpdateListView(string query = "getPage?section=popular")
        {
            await UpdateCollectionRecipes(query);

            _listView = FindViewById<ListView>(Resource.Id.listRecipeShorts);
            
            adapter = new RecipeShortAdapter(this, recipes);

            _listView.Adapter = adapter;
        }

       
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

