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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Modules;

namespace ShoutboxSpa.Components
{
    /// <summary>
    /// simple class to hold our module settings and provide a strongly
    /// type access to them
    /// </summary>
    public class ShoutBoxModuleSettings
    {
        public const string ALLOW_ANONY = "allow_anonymous_posts";
        public const string FLOOD_REPLY = "flood_reply";
        public const string FLOOD_VOTE = "flood_vote";
        public const string FLOOD_POST = "flood_post";
        public const string PROFILE_IMG = "profile_img_location";
        public const string NUMBER_OF_POSTS_RETURN = "shout_post_count";

        private ModuleInfo moduleInfo;
        private int moduleId;
        private int tabId;

        /// <summary>
        /// defines the location for the image to
        /// use for the post user
        /// </summary>
        public enum ProfileImage
        {
            Gravatar = 0,
            Dnn = 1
        }

        public ShoutBoxModuleSettings(int moduleId, int tabId)
        {
            moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);
            this.moduleId = moduleId;
            this.tabId = tabId;
        }

        /// <summary>
        /// defines if we allow anonymous visitors to reply/post new items to the 
        /// module
        /// </summary>
        public bool AllowAnonymous
        {
            get
            {
                return moduleInfo.ModuleSettings.GetValueOrDefault<bool>(ALLOW_ANONY, false);
            }

            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, ALLOW_ANONY, value.ToString());
            }
        }

        /// <summary>
        /// time in minutes that a visitor can vote again on the same post
        /// </summary>
        public int FloodVoting
        {
            get
            {
                return moduleInfo.ModuleSettings.GetValueOrDefault<int>(FLOOD_VOTE, 2);
            }
            
            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, FLOOD_VOTE, value.ToString());
            }
        }


        /// <summary>
        /// time in minutes that a visitor must wait before they can reply to a post
        /// </summary>
        public int FloodReply {
            get
            {
                return moduleInfo.ModuleSettings.GetValueOrDefault<int>(FLOOD_REPLY, 2);
            }

            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, FLOOD_REPLY, value.ToString());
            }
        }

        /// <summary>
        /// time in minutes that a visitor must wait before posting another new topic
        /// </summary>
        public int FloodNewPost {
            get 
            {
                return moduleInfo.ModuleSettings.GetValueOrDefault<int>(FLOOD_POST, 1); 
            }

            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, FLOOD_POST, value.ToString());
            }
        }

        /// <summary>
        /// defines the source we use for the profile image on the
        /// post
        /// </summary>
        public ProfileImage ProfileImageSource
        {
            get
            {
                return (ProfileImage)moduleInfo.ModuleSettings.GetValueOrDefault<int>(PROFILE_IMG, (int)ProfileImage.Gravatar);
            }

            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, PROFILE_IMG, ((int)value).ToString());
            }
        }


        /// <summary>
        /// defines the number of shout posts we'll select to display. these wil be the newest ones
        /// created
        /// </summary>
        public int NumberOfPostsToReturn
        {
            get
            {
                return moduleInfo.ModuleSettings.GetValueOrDefault<int>(NUMBER_OF_POSTS_RETURN, 20);
            }

            set
            {
                ModuleController
                     .Instance
                     .UpdateModuleSetting(moduleId, NUMBER_OF_POSTS_RETURN, value.ToString());
            }
        }
    }
}