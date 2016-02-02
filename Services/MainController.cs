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
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using ShoutboxSpa.Components;
using ShoutboxSpa.DataService;
using ShoutboxSpa.Services.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using DotNetNuke.Collections;
using DotNetNuke.Instrumentation;

namespace ShoutboxSpa.Services
{
    [SupportedModules("ShoutboxSpa")]
    public class MainController : DnnApiController
    {
        protected static readonly ILog Log = LoggerSource.Instance.GetLogger(typeof(MainController));  

        private readonly IShoutPostRepository _repository;
        private string SharedResource = "~/DesktopModules/ShoutboxSpa/App_LocalResources/SharedResources.resx";

        public MainController():this(ShoutPostRepository.Instance)
        {

        }

        public MainController(IShoutPostRepository instance)
        {
            _repository = instance;
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetShouts()
        {
            int moduleId = Request.FindModuleId();
            int tabId = Request.FindTabId();
            bool allowEdit = false;
            bool allowInput = true;
            ShoutBoxModuleSettings.ProfileImage profileImg = 0;

            Log.DebugFormat("moduleId:{0}, tabId:{1}", moduleId, tabId);

            var moduleSettings = new ShoutBoxModuleSettings(moduleId, tabId);
            allowInput = moduleSettings.AllowAnonymous;
            profileImg = moduleSettings.ProfileImageSource;

            var posts = _repository
                            .GetDisplayPosts(moduleId,
                                             moduleSettings.NumberOfPostsToReturn);
            

            if (this.UserInfo != null)
            {
                //work out if the userinfo 
                //object has edit permission on this module                
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, this.Request.FindTabId(), false);
                allowEdit = ModulePermissionController
                                .HasModuleAccess(SecurityAccessLevel.Edit,
                                                    null,
                                                    moduleInfo);

         

                //if we don't allow anonymous check to see if we are auth'd
                if (!allowInput)
                {
                    allowInput = this.UserInfo != null && this.UserInfo.UserID > 0;
                }
            }

            Log.DebugFormat("Sending {0} posts to the client", posts.Count());
            
            var response = new
            {
                success = true,
                data = new
                {
                    posts = posts.ToArray(),
                    allowEdit = allowEdit,
                    allowInput = allowInput,
                    profileImage = profileImg
                }
            };

            return this.Request.CreateResponse(response);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage VoteUp(int itemId)
        {
            var floodControl = new FloodControl(this.Request.FindModuleId(),
                                                this.Request.FindTabId(),
                                                this.Request.GetIPAddress(),
                                                this.UserInfo);




            if(floodControl.AllowVote(itemId))
            {
                var currentUpVote = _repository
                                        .VoteUp(itemId);

                Log.DebugFormat("Vote up recorded, IP:{0}, itemId:{1}", 
                                    this.Request.GetIPAddress(),
                                    itemId);

                return Request.CreateResponse(new
                {
                    success = true,
                    data = new { voteUp = currentUpVote }
                });
            }
            else
            {
                Log.WarnFormat("Flood control block the vote. The IP:{0} has already voted on this item:{1} in the time limit window",
                                    this.Request.GetIPAddress(),
                                    itemId);

                return Request.CreateResponse(new
                {
                    success = false
                });
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage VoteDown(int itemId)
        {
            var floodControl = new FloodControl(this.Request.FindModuleId(),
                                                this.Request.FindTabId(),
                                                this.Request.GetIPAddress(),
                                                this.UserInfo);

            if (floodControl.AllowVote(itemId))
            {

                var currentDownVote = _repository
                                        .VoteDown(itemId);

                Log.DebugFormat("Vote down recorded, IP:{0}, itemId:{1}",
                                    this.Request.GetIPAddress(),
                                    itemId);

                return Request.CreateResponse(new
                {
                    success = true,
                    data = new { voteDown = currentDownVote }
                });
            }
            else
            {
                Log.WarnFormat("Flood control block the vote. The IP:{0} has already voted on this item:{1} in the time limit window",
                                    this.Request.GetIPAddress(),
                                    itemId);

                return Request.CreateResponse(new
                {
                    success = false
                });
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage NewReplyPost(int replyToItemId, string message)
        {
            var success = false;
            var errorMessage = string.Empty;
            var userId = -1;
            var moduleId = this.Request.FindModuleId();
            var tabId = this.Request.FindTabId();

            if (this.UserInfo != null)
            {
                userId = this.UserInfo.UserID;
            }


            var moduleSettings = new ShoutBoxModuleSettings(moduleId, tabId);

            //validate the post for profanity
            if (ValidatePostForProfanity(message))
            {

                var floodControl = new FloodControl(this.Request.FindModuleId(),
                                                this.Request.FindTabId(),
                                                this.Request.GetIPAddress(),
                                                this.UserInfo);

                if (floodControl.AllowReply(replyToItemId))
                {
                    success = true;

                    _repository.AddPost(new ShoutPost()
                    {
                        ModuleId = moduleId,
                        CreatedDate = DateTime.Now,
                        VoteDown = 0,
                        VoteUp = 0,
                        UserId = userId > 0 ? (int?)userId : null,
                        Message = message,
                        ReplyTo = replyToItemId
                    });


                    Log.DebugFormat("Reply has been saved for the item:{1}",
                                    replyToItemId);
                }
                else
                {
                    Log.WarnFormat("Flood control block the reply. The IP:{0} has already replied on this item:{1} in the time limit window",
                                    this.Request.GetIPAddress(),
                                    replyToItemId);

                    errorMessage = Localization.GetString("FloodControlReply.Text", SharedResource);
                }
            }
            else
            {
                Log.WarnFormat("The reply was not saved due to profanity. The IP:{0}",
                                    this.Request.GetIPAddress());

                errorMessage = Localization.GetString("ProfanityBanned.Text", SharedResource);
            }

            var posts = _repository
                            .GetDisplayPosts(moduleId,
                                             moduleSettings.NumberOfPostsToReturn);

            return Request.CreateResponse(new
            {
                success = success,
                message = errorMessage,
                data = new { posts = posts.ToArray() }
            });
        }



        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage NewPost(string post)
        {
            var success = false;
            var message = string.Empty;
            var userId = -1;
            var moduleId = this.Request.FindModuleId();
            var tabId = this.Request.FindTabId();


            var moduleSettings = new ShoutBoxModuleSettings(moduleId, tabId);


            if (this.UserInfo != null && this.UserInfo.UserID > 0)
            {
                userId = this.UserInfo.UserID;
            }

            //validate the post for profanity
            if (ValidatePostForProfanity(post))
            {
                var floodControl = new FloodControl(this.Request.FindModuleId(),
                                                this.Request.FindTabId(),
                                                this.Request.GetIPAddress(),
                                                this.UserInfo);

                if (floodControl.AllowNewPost())
                {
                    success = true;

                    _repository.AddPost(new ShoutPost()
                    {
                        ModuleId = moduleId,
                        CreatedDate = DateTime.Now,
                        VoteDown = 0,
                        VoteUp = 0,
                        UserId = userId > 0 ? (int?)userId : null,
                        Message = post
                    });

                    Log.Debug("New post has been saved");
                }
                else
                {
                    Log.WarnFormat("Flood control block the new post. The IP:{0} has already posted a new item in the time limit window",
                                    this.Request.GetIPAddress());

                    message = message = Localization.GetString("FloodControlNewPost.Text", SharedResource);
                }
            }
            else
            {
                Log.WarnFormat("The new post was not saved due to profanity. The IP:{0}",
                                    this.Request.GetIPAddress());

                message = Localization.GetString("ProfanityBanned.Text", SharedResource);
            }

            var posts = _repository
                            .GetDisplayPosts(moduleId,
                                             moduleSettings.NumberOfPostsToReturn);

            return Request.CreateResponse(new
            {
                success = success,
                message = message,
                data = new { posts = posts.ToArray() }
            });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage DeletePost(int itemId)
        {
            _repository.DeleteItem(itemId);

            Log.DebugFormat("ItemId:{0} deleted", itemId);

            return Request.CreateResponse(
                new { success = true }
            );
        }


        private bool ValidatePostForProfanity(string input)
        {
 
            string filename = "profanity-list.txt";
            string profanity;
            try
            {
                var profantityList = DataCache.GetCache<string[]>("shoutboxspa_profanityList");

                if (profantityList == null)
                {
                    var moduleBaseFolder = HttpContext.Current.Server.MapPath("~/DesktopModules/ShoutboxSpa");
                    using (StreamReader reader = File.OpenText(Path.Combine(moduleBaseFolder, filename)))
                    {
                        profanity = reader.ReadToEnd();
                    }

                    profantityList = profanity.Split(',');
                    DataCache.SetCache("shoutboxspa_profanityList", profantityList);
                }
                string[] message = NormaliseString(input);


                //check message for profanity
                foreach (string m in message)
                {
                    foreach (string profanityWord in profantityList)
                    {
                        if (profanityWord.Equals(m))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }        
        }

        /// <summary>
        /// tokenises a string that can be checked for profanity
        /// removes punctuation etc
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static string[] NormaliseString(string input)
        {
            var buffer = new StringBuilder();

            char[] messageArray = input.ToCharArray();
            foreach (char c in messageArray)
            {
                if (!Char.IsPunctuation(c))
                    buffer.Append(Char.ToLower(c));
            }

            return buffer.ToString().Split(' ');
        }

    }
}