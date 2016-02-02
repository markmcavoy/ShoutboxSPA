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

using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Collections;
using ShoutboxSpa.Services.ViewModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using ShoutboxSpa.Components;
using DotNetNuke.Instrumentation;
using ShoutboxSpa.DataService;

namespace ShoutboxSpa.Services
{
    [SupportedModules("ShoutboxSpa")]
    public class SettingsController : DnnApiController
    {
        protected static readonly ILog Log = LoggerSource.Instance.GetLogger(typeof(SettingsController));

        private readonly IShoutPostRepository _repository;

        public SettingsController():this(ShoutPostRepository.Instance)
        {

        }

        public SettingsController(IShoutPostRepository instance)
        {
            _repository = instance;
        }


        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
            var moduleId = Request.FindModuleId();
            var tabId = Request.FindTabId();

            Log.DebugFormat("Getting settings for moduleId:{0}, tabId:{1}",
                                moduleId,
                                tabId);

            var viewModel = new SettingsModel(new ShoutBoxModuleSettings(moduleId, tabId));

            Log.Debug(viewModel);

            var response = new
            {
                success = true,
                data = new
                {
                    results = viewModel,
                    oldShoutsCount = _repository.CountOldShouts(moduleId, 30)
                }
            };

            return this.Request.CreateResponse(response);
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveSettings(SettingsModel settings)
        {
            var moduleSettings = new ShoutBoxModuleSettings(Request.FindModuleId(), 
                                                              Request.FindTabId());

            moduleSettings.AllowAnonymous = settings.AllowAnonymous;
            moduleSettings.FloodNewPost = settings.FloodNewPost;
            moduleSettings.FloodReply = settings.FloodReply;
            moduleSettings.FloodVoting = settings.FloodVoting;
            moduleSettings.ProfileImageSource = settings.ProfileImageSource;
            moduleSettings.NumberOfPostsToReturn = settings.RecordLimit;

            var response = new
            {
                success = true
            };

            return Request.CreateResponse(response);
        }


        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public void PurgeOld()
        {
            var moduleId = Request.FindModuleId();
            _repository.DeleteOldShouts(moduleId, 30);
        }
    }
}