﻿using Android.Support.V7.Widget;
using Android.Views;
using ObjectsLibrary;
using Square.Picasso;
using System;
using System.Collections.Generic;
using Android.App;
using XamarinApp;
using XamarinApp.ViewHolders;

namespace AndroidApp
{
    public class RecipeAdapter : RecyclerView.Adapter
    {
        private readonly List<RecipeShort> _items;
        private readonly Activity _activity;
        public event EventHandler<int> ItemClick;
        public override int ItemCount => _items.Count;

        public RecipeAdapter(List<RecipeShort> recipeShorts, Activity activity)
        {
            _activity = activity;

            _items = recipeShorts;
        }

        public void AddItems(List<RecipeShort> recipes)
        {
            _items.AddRange(recipes);
            NotifyDataSetChanged();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            RecipeViewHolder vh = holder as RecipeViewHolder;

            vh.Title.Text = _items[position]?.Title;

            var urlArray = _items[position]?.Url?.Split('/');

            if (urlArray != null && urlArray.Length >= 2)
                vh.Link.Text = urlArray[2];

            var url = _items[position]?.Image?.ImageUrl;

            if (!string.IsNullOrEmpty(url))
                Picasso.With(_activity)
                    .Load(url)
                    .Into(vh.Image);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item, parent, false);
            RecipeViewHolder vh = new RecipeViewHolder(itemView, OnClick);
            return vh;
        }
        
        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }
}