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

using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using ShoutboxSpa.Services.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShoutboxSpa.Components;

namespace ShoutboxSpa.DataService
{
    public class ShoutPostRepository : ServiceLocator<IShoutPostRepository, ShoutPostRepository>, IShoutPostRepository
    {
        /// <summary>
        /// returns all the posts for the given moduleId
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public IQueryable<ShoutPost> GetPosts(int moduleId)
        {
            Requires.NotNegative("moduleId", moduleId);

            IQueryable<ShoutPost> posts = null;

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ShoutPost>();

                posts = rep.Get(moduleId)
                                .OrderByDescending(s => s.CreatedDate) 
                                .AsQueryable();
            }

            return posts;
        }

        public int VoteUp(int itemId)
        {
            var post = this.GetPost(itemId);
            post.VoteUp = post.VoteUp + 1;
            UpdateItem(post);

            return post.VoteUp;
        }

        public int VoteDown(int itemId)
        {
            var post = this.GetPost(itemId);
            post.VoteDown = post.VoteDown + 1;
            UpdateItem(post);

            return post.VoteDown;
        }

        public void DeleteItem(int itemId)
        {
            using (var context = DataContext.Instance())
            {
                var repo = context.GetRepository<ShoutPost>();
                repo.Delete("where replyTo=@0", itemId);
                repo.Delete("where itemId=@0", itemId);
            }
        }

        public int AddPost(ShoutPost post)
        {
            using (var context = DataContext.Instance())
            {
                var repo = context.GetRepository<ShoutPost>();
                repo.Insert(post);

                return post.ItemId;
            }
        }

        private void UpdateItem(ShoutPost item)
        {
            using (var context = DataContext.Instance())
            {
                var repo = context.GetRepository<ShoutPost>();
                repo.Update(item);
            }
        }

        protected override Func<IShoutPostRepository> GetFactory()
        {
            return () => new ShoutPostRepository();
        }


        /// <summary>
        /// due to a lack of support for ResultColumnAttribute in the DNN version of PetaPOCO
        /// we need to do this for joins :( 
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public IEnumerable<ShoutPostModel> GetDisplayPosts(int moduleId, 
                                                            int numberOfPostToReturn)
        {
            using (var context = DataContext.Instance())
            {

                var sql = "select top " + numberOfPostToReturn + " s.*, u.DisplayName, u.Email from {0}[{1}ShoutboxSpa] s " +
                            "left outer join {0}[{1}Users] u on s.userId=u.userId " +
                            "where s.moduleId = @0 and s.ReplyTo is null " +
                            "order by s.CreatedDate desc";

                //add in the owner and object qualifer
                var dataProvider = DataProvider.Instance();

                sql = string.Format(sql, 
                                    dataProvider.DatabaseOwner, 
                                    dataProvider.ObjectQualifier);

                var shouts = context
                                .ExecuteQuery<ShoutPostDisplay>(System.Data.CommandType.Text,
                                                                    sql,
                                                                    moduleId)
                                .Select(s => new ShoutPostModel(s))
                                .ToList();

                if (shouts.Count > 0)
                {
                    //get any replies
                    var sqlReplies = "select s.*, u.DisplayName, u.Email from {0}[{1}ShoutboxSpa] s " +
                                        "left outer join {0}[{1}Users] u on s.userId=u.userId " +
                                        "where s.moduleId = @0 and s.ReplyTo in (" +
                                        string.Join(",", shouts.Select(s => s.ItemId).ToArray()) + ") " +
                                        "order by s.ReplyTo, s.CreatedDate";

                    sqlReplies = string.Format(sqlReplies,
                                        dataProvider.DatabaseOwner,
                                        dataProvider.ObjectQualifier);


                    var shoutReplies = context
                                        .ExecuteQuery<ShoutPostDisplay>(System.Data.CommandType.Text,
                                                                                sqlReplies,
                                                                                moduleId)
                                        .GroupBy(s => s.ReplyTo);

                    foreach (var shoutReply in shoutReplies)
                    {
                        var shout = shouts
                                         .Where(s => s.ItemId == shoutReply.Key)
                                         .FirstOrDefault();

                        if (shout != null)
                        {
                            shout.Replies = shoutReply
                                                .Select(s => new ShoutPostModel(s))
                                                .ToArray();
                        }
                    }
                }

                return shouts;
            }
        }


        public ShoutPost GetPost(int itemId)
        {
            using (var context = DataContext.Instance())
            {
                var repo = context.GetRepository<ShoutPost>();
                return repo.GetById(itemId);
            }
        }


        /// <summary>
        /// count the number of old posts
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="skip"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        public int CountOldShouts(int moduleId, int ageInDays)
        {
            using (var context = DataContext.Instance())
            {

                var sql = "select count(*) from {0}[{1}ShoutboxSpa] " +
                            "where moduleId = @0 AND datediff(d, CreatedDate, GETDATE()) >= @1;";

                //add in the owner and object qualifer
                var dataProvider = DataProvider.Instance();

                sql = string.Format(sql,
                                    dataProvider.DatabaseOwner,
                                    dataProvider.ObjectQualifier);

                return context
                          .ExecuteScalar<int>(System.Data.CommandType.Text,
                                                                    sql,
                                                                    moduleId,
                                                                    ageInDays);
            }

        }

        public void DeleteOldShouts(int moduleId, int age)
        {
            using (var context = DataContext.Instance())
            {                
                var repo = context.GetRepository<ShoutPost>();
                repo.Delete("where moduleId = @0 AND datediff(d, CreatedDate, GETDATE()) >= @1;", 
                                moduleId, 
                                age);
            }
        }
    }
}