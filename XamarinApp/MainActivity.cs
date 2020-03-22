﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using RecipesAndroid;
using RecipesAndroid.Objects;
using XamarinApp.Library;
using XamarinApp.Library.Objects;

namespace XamarinApp
{
    [Activity(Label = "На плите!", Theme = "@style/AppTheme.NoActionBar", Icon = "@drawable/icon", MainLauncher = true)]
    
    public class MainActivity : AppCompatActivity
    {
        private ListView _listView;
        private List<RecipeShort> recipes;
        private RecipeShortAdapter adapter;
        public static string lastUrl;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // TODO: Доделать спиннер: поработать над дизайном. <-----
           
            // TODO: Добавить менюшку.
            // TODO: Тотальный рефакторинг.
            // TODO: Добавь сохранение
            // TODO: Кнопка "Поделиться"
            // TODO: Загрузка доп.рецептов при прокрутке.
            // TODO: Отлавливать Exception + повторные запросы.
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);


            SetContentView(Resource.Layout.activity_search);
            
            _listView = FindViewById<ListView>(Resource.Id.listRecipeShorts);
            
            _listView.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                lastUrl = recipes[int.Parse(args.Id.ToString())].Url;
                Intent intent = new Intent(this, typeof(RecipeActivity));
                StartActivity(intent);
            };

            EditText edittext = FindViewById<EditText>(Resource.Id.TextFind);
            
            edittext.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    UpdateListView($"getPage?section=recipe&recipeName={edittext.Text}");
                    Toast.MakeText(this, "Загрузка...", ToastLength.Short).Show();
                    e.Handled = true;
                }
            };

            Spinner spinner = FindViewById <Spinner> (Resource.Id.spinner);  
            spinner.ItemSelected += new EventHandler < AdapterView.ItemSelectedEventArgs > (spinner_ItemSelected);  
            
            
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sort_array, Android.Resource.Layout.SimpleSpinnerItem);  
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);  
            spinner.Adapter = adapter;  
            
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

