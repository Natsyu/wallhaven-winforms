﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using wallpaper_forms.Enums;
using wallpaper_forms.Models;

namespace wallpaper_forms.Services
{
    public static class RequestService
    {
        public static async Task RequestImageDetails(string searchText)
        {
            try
            {
                var requestBody = CreateRequest(searchText);
                string searchChain = "?";
                foreach (PropertyInfo propertyInfo in requestBody.GetType().GetProperties())
                {
                    if (propertyInfo != null && propertyInfo.GetValue(requestBody) != null)
                    {
                        searchChain += $"{propertyInfo.CustomAttributes.ElementAt(0).ConstructorArguments.ElementAt(0).Value.ToString()}={propertyInfo.GetValue(requestBody).ToString()}&";
                    }
                }

                string searchURL = AppSettings.WallhavenUri + searchChain;

                var response = await WallhavenApiService.Get(searchURL, null);
                var responseModel = JsonSerializer.Deserialize<WallhavenResponse>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                GlobalVariables.PhotoURL = responseModel.Data[0].Path;
                GlobalVariables.ThumbnailURL = responseModel.Data[0].Thumbs.Original;
                GlobalVariables.CurrentImageId = responseModel.Data[0].Id;
                GlobalVariables.CurrentImageDetails = $"ID: {responseModel.Data[0].Id}\r\n"
                                                    + $"Resolution: {responseModel.Data[0].Resolution}\r\n"
                                                    + $"Url: { responseModel.Data[0].Url}";
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception.Message);
            }
        }

        public static WallhavenRequest CreateRequest(string searchText)
        {
            return new WallhavenRequest()
            {
                Q = searchText,
                AtLeast = AppSettings.LeastResolution,
                Categories = AppSettings.Categories,
                Purity = AppSettings.Putiry,
                Ratios = AppSettings.Ratio,
                Sorting = Sorting.Random,
                Page = AppSettings.Page,
                Seed = AppSettings.Seed
            };
        }
        public static string BuildCategoryString(CheckBox General, CheckBox Anime, CheckBox People)
        {
            int category = 0;
            category = General.Checked ? category + Categories.General : category;
            category = Anime.Checked ? category + Categories.Anime : category;
            category = People.Checked ? category + Categories.People : category;
            return category.ToString("000");
        }
        public static string BuildPurityString(CheckBox SFW, CheckBox Sketchy, CheckBox NSFW)
        {
            int purity = 0;
            purity = SFW.Checked ? purity + Purity.Sfw : purity;
            purity = Sketchy.Checked ? purity + Purity.Sketchy : purity;
            purity = NSFW.Checked ? purity + Purity.Nsfw : purity;
            return purity.ToString("000");
        }

    }
}
