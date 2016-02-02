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
using Data = DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;
using PetaPoco;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoutboxSpa.DataService
{
    [Serializable]
    [Data.TableName("ShoutboxSpa")]
    [Data.PrimaryKey("ItemID", "ItemId")]
    [Data.Scope("ModuleId")]
    public class ShoutPost
    {
        public ShoutPost()
        {
            ItemId = -1;
        }

        public int ItemId { get; set; }

        public int ModuleId { get; set; }

        public string Message { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int VoteUp { get; set; }

        public int VoteDown { get; set; }

        public int? ReplyTo { get; set; }

    }
}