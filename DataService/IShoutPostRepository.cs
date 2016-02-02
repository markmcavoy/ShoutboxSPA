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

using ShoutboxSpa.Services.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoutboxSpa.Components;

namespace ShoutboxSpa.DataService
{
    public interface IShoutPostRepository
    {
        IQueryable<ShoutPost> GetPosts(int moduleId);

        ShoutPost GetPost(int itemId);

        IEnumerable<ShoutPostModel> GetDisplayPosts(int moduleId, 
                                                    int numberOfPostsToReturn);

        int VoteUp(int itemId);

        int VoteDown(int itemId);

        void DeleteItem(int itemId);

        int AddPost(ShoutPost post);


        int CountOldShouts(int moduleId, int age);

        void DeleteOldShouts(int moduleId, int age);
    }
}
