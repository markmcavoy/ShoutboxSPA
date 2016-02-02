/*  Copyright 2015 Mark McAvoy
 * 
 *  This file is part of ShoutboxSPA.
 *
 *  ShoutboxSPA is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ShoutboxSPA is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ShoutboxSPA.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using ShoutboxSpa.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ShoutboxSpa.Components;

namespace ShoutboxSpa.Services.ViewModel
{
    public class ShoutPostModel
    {
        private string SharedResource = "~/DesktopModules/ShoutboxSpa/App_LocalResources/SharedResources.resx";

        private string email = null;

        public ShoutPostModel(ShoutPost post)
        {
            this.ItemId = post.ItemId;
            this.ModuleId = post.ModuleId;
            this.Message = post.Message;
            this.CreatedDate = post.CreatedDate;
            this.VoteUp = post.VoteUp;
            this.VoteDown = post.VoteDown;
            this.ReplyTo = post.ReplyTo;
            this.UserId = post.UserId;
        }


        public ShoutPostModel(ShoutPostDisplay post)
        {
            this.ItemId = post.ItemId;
            this.ModuleId = post.ModuleId;
            this.Message = post.Message;
            this.DisplayName = post.DisplayName;
            this.CreatedDate = post.CreatedDate;
            this.VoteUp = post.VoteUp;
            this.VoteDown = post.VoteDown;
            this.ReplyTo = post.ReplyTo;
            this.email = post.Email;
            this.UserId = post.UserId;
        }

        public int ItemId { get; set; }

        public int? UserId { get; set; }

        public int ModuleId { get; set; }

        public string Message { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int VoteUp { get; set; }

        public int VoteDown { get; set; }

        public int? ReplyTo { get; set; }

        public ShoutPostModel[] Replies { get; set; }

        public string PostAge 
        { 
            get 
            {
                TimeSpan age = DateTime.Now - CreatedDate;
                if (age.TotalSeconds < 120)
                {
                    return Localization.GetString("MomentsAgo.Text", SharedResource);
                }
                else if (age.TotalMinutes < 60)
                {
                    return string.Format(Localization.GetString("MinutesAgo.Text", SharedResource),
                                            Math.Round(age.TotalMinutes, 0),
                                            Math.Round(age.TotalMinutes, 0) > 1 ? "s" : "");
                }
                else if (age.TotalHours < 24)
                {
                    return string.Format(Localization.GetString("HoursAgo.Text", SharedResource),
                                            Math.Round(age.TotalHours, 0),
                                            Math.Round(age.TotalHours, 0) > 1 ? "s" : "");
                }
                else if (age.TotalDays < 365)
                {
                    return string.Format(Localization.GetString("DaysAgo.Text", SharedResource),
                                            Math.Round(age.TotalDays, 0),
                                            Math.Round(age.TotalDays, 0) > 1 ? "s" : "");
                }
                else
                {
                    return string.Format(Localization.GetString("YearsAgo.Text", SharedResource),
                                            Math.Round(age.TotalDays / 365, 0),
                                            Math.Round(age.TotalDays / 365, 0) > 1 ? "s" : "");
                }
            } 
        }

        /// <summary>
        /// read only access to the gravatar key to use
        /// to then render some image
        /// </summary>
        public string GravatarKey
        {
            get
            {
                string gravatarKey = this.DisplayName;

                if (!string.IsNullOrEmpty(this.email))
                    gravatarKey = this.email;

                if (gravatarKey == null)
                    return null;


                MD5 md5 = MD5CryptoServiceProvider.Create();
                byte[] output = md5.ComputeHash(ASCIIEncoding.UTF8.GetBytes(gravatarKey));
                StringBuilder buffer = new StringBuilder();
                foreach (byte b in output)
                {
                    buffer.AppendFormat("{0:x2}", b);
                }

                return buffer.ToString();
            }
        }
    }
}