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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShoutboxSpa.Components;
using System.Text;

namespace ShoutboxSpa.Services.ViewModel
{
    public class SettingsModel
    {
        public SettingsModel()
        {

        }

        public SettingsModel(ShoutBoxModuleSettings settings)
        {
            AllowAnonymous = settings.AllowAnonymous;
            FloodVoting = settings.FloodVoting;
            FloodReply = settings.FloodReply;
            FloodNewPost = settings.FloodNewPost;
            ProfileImageSource = settings.ProfileImageSource;
            RecordLimit = settings.NumberOfPostsToReturn;
        }

        /// <summary>
        /// defines if we allow anonymous visitors to reply/post new items to the 
        /// module
        /// </summary>
        public bool AllowAnonymous
        {
            get;
            set;
        }

        /// <summary>
        /// time in minutes that a visitor can vote again on the same post
        /// </summary>
        public int FloodVoting
        {
            get;
            set;
        }


        /// <summary>
        /// time in minutes that a visitor must wait before they can reply to a post
        /// </summary>
        public int FloodReply { get; set; }

        /// <summary>
        /// time in minutes that a visitor must wait before posting another new topic
        /// </summary>
        public int FloodNewPost { get; set; }

        /// <summary>
        /// defines the source we use for the profile image on the
        /// post
        /// </summary>
        public ShoutBoxModuleSettings.ProfileImage ProfileImageSource { get; set; }

        /// <summary>
        /// defines the number of items to return on the module
        /// </summary>
        public int RecordLimit { get; set; }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.AppendFormat("AllowAnonymous:{0}, ", AllowAnonymous);
            buffer.AppendFormat("FloodVoting:{0}, ", FloodVoting);
            buffer.AppendFormat("FloodReply:{0}, ", FloodReply);
            buffer.AppendFormat("FloodNewPost:{0}, ", FloodNewPost);
            buffer.AppendFormat("ProfileImageSource:{0}, ", ProfileImageSource);
            buffer.AppendFormat("RecordLimit:{0}", RecordLimit);

            return buffer.ToString();
        }
    }
}