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


using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShoutboxSpa.Services;

namespace ShoutboxSpa.Components
{
    /// <summary>
    /// class used to determine if the action requested by the user should be allowed
    /// or if they need to wait before the action can be completed.
    /// </summary>
    /// <remarks>this is to stop spam and other such from users just clicking and posting too quickly.
    /// If the current user has edit level access for the module then the flood control will just
    /// allow the action.</remarks>
    public class FloodControl
    {
        const string TRACKER_CACHE_KEY = "shoutboxSPA_Tracker";

        bool hasEditAccess = false;
        int moduleId;
        int tabId;
        string ipAddress;
        UserInfo user;

        int floodTimePost;
        int floodTimeReply;
        int floodTimeVote;

        /// <summary>
        /// defines the type of action we are 
        /// testing the user can complete
        /// </summary>
        internal enum ActionType
        {
            Vote = 0,
            NewPost = 1,
            Reply = 2
        }

        /// <summary>
        /// cstor
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="tabId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="user"></param>
        public FloodControl(int moduleId, int tabId, string ipAddress, UserInfo user)
        {
            this.moduleId = moduleId;
            this.tabId = tabId;
            this.ipAddress = ipAddress;
            this.user = user;

            if (user != null)
            {
                //work out if the userinfo 
                //object has edit permission on this module                
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);
                hasEditAccess = ModulePermissionController
                                    .HasModuleAccess(SecurityAccessLevel.Edit,
                                                        null,
                                                        moduleInfo);

                //load the flood control settings
                var settings = new ShoutBoxModuleSettings(moduleId, tabId);
                floodTimePost = settings.FloodNewPost;
                floodTimeReply = settings.FloodReply;
                floodTimeVote = settings.FloodVoting;
            }

            
        }


        /// <summary>
        /// works out when we last performed the action on the post for the given
        /// user in minutes
        /// </summary>
        /// <remarks>this will return null if we don't have a record in cache for that
        /// user doing the action</remarks>
        /// <param name="ipAddress"></param>
        /// <param name="action"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        private int? ActionLastComplete(ActionType action, int? postId)
        {
            var tracker = DataCache.GetCache<IDictionary<string, IList<FloodControlEvent>>>(TRACKER_CACHE_KEY) ??
                                new Dictionary<string, IList<FloodControlEvent>>();

            if (tracker.ContainsKey(ipAddress))
            {
                var actions = tracker[ipAddress];
                var lastAction = actions
                                    .Where(a => a.Action == action && a.PostId == postId)
                                    .OrderByDescending(a => a.TimeStamp)
                                    .FirstOrDefault();

                if (lastAction == null)
                {
                    return null;
                }
                else
                {
                    return (DateTime.UtcNow - lastAction.TimeStamp).Minutes;
                }
            }
            else
            {
                return null;
            }
        }

        private void RecordAction(ActionType actionType, int? postId)
        {
            var tracker = DataCache.GetCache<IDictionary<string, IList<FloodControlEvent>>>(TRACKER_CACHE_KEY) ??
                                new Dictionary<string, IList<FloodControlEvent>>();

            if (tracker.ContainsKey(ipAddress))
            {
                tracker[ipAddress].Add(new FloodControlEvent()
                { 
                    Action = actionType,
                    PostId = postId,
                    TimeStamp = DateTime.UtcNow
                });
            }
            else
            {
                tracker.Add(ipAddress, new List<FloodControlEvent>() { 
                    new FloodControlEvent(){
                        PostId = postId,
                        TimeStamp = DateTime.UtcNow,
                        Action = actionType
                    }
                });
            }

            DataCache.SetCache(TRACKER_CACHE_KEY, tracker);
        }


        /// <summary>
        /// determines if the user can vote again on the given post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public bool AllowVote(int postId)
        {
            if (hasEditAccess)
            {
                return true;
            }
            else
            {
                var lastAction = ActionLastComplete(ActionType.Vote,
                                                    postId);

                bool allow = false;

                if (lastAction.HasValue)
                {
                    allow = lastAction > floodTimeVote;
                }
                else
                {
                    allow = true;
                }


                if (allow)
                {
                    RecordAction(ActionType.Vote, postId);
                }

                return allow;
            }
        }

        /// <summary>
        /// determines if the user can add a new post
        /// </summary>
        /// <returns></returns>
        public bool AllowNewPost()
        {
            if (hasEditAccess)
            {
                return true;
            }
            else
            {
                var lastAction = ActionLastComplete(ActionType.NewPost,
                                                    null);

                bool allow = false;

                if (lastAction.HasValue)
                {
                    allow = lastAction > floodTimePost;
                }
                else
                {
                    allow = true;
                }


                if (allow)
                {
                    RecordAction(ActionType.NewPost, null);
                }

                return allow;
            }
        }

        /// <summary>
        /// determines if user can post a reply to the given post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public bool AllowReply(int postId)
        {
            if (hasEditAccess)
            {
                return true;
            }
            else
            {
                var lastAction = ActionLastComplete(ActionType.Reply,
                                                    postId);

                bool allow = false;

                if (lastAction.HasValue)
                {
                    allow = lastAction > floodTimeReply;
                }
                else
                {
                    allow = true;
                }


                if (allow)
                {
                    RecordAction(ActionType.Reply, postId);
                }

                return allow;
            }
        }

 
    }
}