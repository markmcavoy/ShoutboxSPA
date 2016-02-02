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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Entities;
using ShoutboxSpa.DataService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.XPath;

namespace ShoutboxSpa.Components
{
    /// <summary>
    /// simple controller that we use to define an expose the import/export and also
    /// the search functionality in dnn
    /// </summary>
    public class ShoutboxSpaController : ModuleSearchBase, IPortable
    {
        protected static readonly ILog Log = LoggerSource.Instance.GetLogger(typeof(ShoutboxSpaController));

        public string ExportModule(int ModuleID)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine("<shouts>");

            var repository = ShoutPostRepository.Instance;

            foreach (var item in repository.GetPosts(ModuleID).OrderBy(s => s.ItemId))
            {
                buffer.AppendLine("<shout>");
                buffer.AppendFormat("<itemId>{0}</itemId>/r/n", item.ItemId);
                buffer.AppendFormat("<message>{0}</message>/r/n", XmlUtils.XMLEncode(item.Message));
                buffer.AppendFormat("<userId>{0}</userId>/r/n", item.UserId);
                buffer.AppendFormat("<createdDate>{0}</createdDate>/r/n", item.CreatedDate.Ticks);
                buffer.AppendFormat("<voteUp>{0}</voteUp>/r/n", item.VoteUp);
                buffer.AppendFormat("<voteDown>{0}</voteDown>/r/n", item.VoteDown);
                buffer.AppendFormat("<replyTo>{0}</replyTo>/r/n", item.ReplyTo);
                buffer.AppendLine("</shout>");

                Log.DebugFormat("Item:{0} exported", item.ItemId);
            }

            buffer.AppendLine("</shouts>");

            return buffer.ToString();
        }

        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            using(var reader = new StringReader(Content))
            {
                var xpath = new XPathDocument(reader);
                var nav = xpath.CreateNavigator();

                var repo = ShoutPostRepository.Instance;
                var mapping = new Dictionary<int, int>();

                var userController = UserController.Instance;

                foreach (XPathNavigator item in nav.Select("//shout"))
                {
                    try
                    {
                        Log.Debug("Starting item import");
                        //make an object, check we have the user in this
                        //portal, otherwise convert that post to anonymous
                        var post = new ShoutPost() 
                        { 
                            ItemId = int.Parse(item.SelectSingleNode("itemId").Value),
                            CreatedDate = new DateTime(long.Parse(item.SelectSingleNode("createdDate").Value)),
                            Message = item.SelectSingleNode("message").Value,
                            ModuleId = ModuleID,
                            ReplyTo = string.IsNullOrEmpty(item.SelectSingleNode("replyTo").Value) ? null : (int?)int.Parse(item.SelectSingleNode("replyTo").Value),
                            UserId = string.IsNullOrEmpty(item.SelectSingleNode("userId").Value) ? null : (int?)int.Parse(item.SelectSingleNode("userId").Value),
                            VoteDown = int.Parse(item.SelectSingleNode("voteDown").Value),
                            VoteUp = int.Parse(item.SelectSingleNode("voteUp").Value)
                        };

                        //if the user doesn't exist we'll just have to make it an anonymous post
                        if (post.UserId.HasValue)
                        {
                            var u = userController.GetUserById(PortalSettings.Current.PortalId,
                                                        post.UserId.Value);

                            if (u == null || u.UserID <= 0)
                            {
                                post.UserId = null;
                            }
                        }

                        //is the post a reply? if so we need to map the 
                        //parent post to the correct item
                        if (post.ReplyTo.HasValue)
                        {
                            post.ReplyTo = mapping[post.ReplyTo.Value];
                        }

                        //add to db and map the new id to any replies we 
                        //need to move over
                        mapping.Add(post.ItemId, repo.AddPost(post));

                        Log.Debug("Completed item import");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Unable to import item");
                        Log.Error(ex);
                        Exceptions.LogException(ex);
                    }
                }

            }
        }

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            var searchDocuments = new List<SearchDocument>();

            var repository = ShoutPostRepository.Instance;
            var postsToAdd = repository
                                .GetPosts(moduleInfo.ModuleID)
                                .Where(s => s.CreatedDate.ToUniversalTime() >= beginDateUtc);

            foreach (var item in postsToAdd)
            {
                Log.DebugFormat("Adding item:{0} to search index", item.ItemId);

                searchDocuments.Add(new SearchDocument() 
                {
                    UniqueKey = string.Format("{0}-{1}", item.ModuleId, item.ItemId),
                    PortalId = moduleInfo.PortalID,
                    Title = item.Message,
                    Description = item.Message,
                    Body = item.Message,
                    ModifiedTimeUtc = item.CreatedDate.ToUniversalTime()
                });
            }

            return searchDocuments;
        }
    }
}